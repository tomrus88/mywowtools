using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using dbc2sql;
using PluginInterface;

namespace DBCViewer
{
    public partial class MainForm : Form
    {
        // Fields
        DataTable m_dataTable;
        DataView m_dataView;
        IWowClientDBReader m_reader;
        FilterForm m_filterForm;
        DefinitionSelect m_selector;
        XmlDocument m_definitions;
        XmlNodeList m_fields;
        DirectoryCatalog m_catalog;
        XmlElement m_definition;
        string m_dbcName;
        string m_dbcFile;
        DateTime m_startTime;
        string m_workingFolder;

        // Properties
        public DataTable DataTable { get { return m_dataTable; } }
        public DataView DataView { get { return m_dataView; } }
        public string WorkingFolder { get { return m_workingFolder; } }
        public XmlElement Definition { get { return m_definition; } }
        public string DBCName { get { return m_dbcName; } }
        public int DefinitionIndex { get { return m_selector != null ? m_selector.DefinitionIndex : 0; } }

        // Delegates
        delegate void SetDataViewDelegate(DataView view);

        [ImportMany(AllowRecomposition = true)]
        List<IPlugin> Plugins { get; set; }

        public MainForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            LoadFile(openFileDialog1.FileName);
        }

        private void LoadFile(string file)
        {
            m_dbcFile = file;
            Text = "DBC Viewer";
            dataGridView1.DataSource = null;

            if (m_filterForm != null)
                m_filterForm.Dispose();

            m_dbcName = Path.GetFileNameWithoutExtension(file);

            LoadDefinitions(); // reload in case of modification

            m_definition = GetDefinition();

            if (m_definition == null)
            {
                StartEditor();
                return;
            }

            toolStripProgressBar1.Visible = true;
            toolStripStatusLabel1.Text = "Loading...";

            m_startTime = DateTime.Now;
            backgroundWorker1.RunWorkerAsync(file);
        }

        private void CloseFile()
        {
            Text = "DBC Viewer";
            dataGridView1.DataSource = null;

            if (m_filterForm != null)
                m_filterForm.Dispose();

            m_definition = null;
            m_dataTable = null;
            m_dataView = null;
            columnsFilterToolStripMenuItem.DropDownItems.Clear();
        }

        private void StartEditor()
        {
            DefinitionEditor editor = new DefinitionEditor();
            var result = editor.ShowDialog(this);
            if (result == DialogResult.OK)
                LoadFile(m_dbcFile);
            else
                MessageBox.Show("Editor cancelled!");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void dataGridView1_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            e.Value = m_dataView[e.RowIndex][e.ColumnIndex];
        }

        private void dataGridView1_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            int val = 0;

            if (m_dataTable.Columns[e.ColumnIndex].DataType != typeof(string))
            {
                if (m_dataTable.Columns[e.ColumnIndex].DataType == typeof(int))
                    val = Convert.ToInt32(m_dataView[e.RowIndex][e.ColumnIndex]);
                else if (m_dataTable.Columns[e.ColumnIndex].DataType == typeof(float))
                    val = (int)BitConverter.ToUInt32(BitConverter.GetBytes((float)m_dataView[e.RowIndex][e.ColumnIndex]), 0);
                else
                    val = (int)Convert.ToUInt32(m_dataView[e.RowIndex][e.ColumnIndex]);
            }
            else
                val = (from k in m_reader.StringTable where string.Compare(k.Value, (string)m_dataView[e.RowIndex][e.ColumnIndex], true) == 0 select k.Key).FirstOrDefault();

            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "Integer: {0:D}{1}", val, Environment.NewLine);
            sb.AppendFormat(new BinaryFormatter(), "HEX: {0:X}{1}", val, Environment.NewLine);
            sb.AppendFormat(new BinaryFormatter(), "BIN: {0:B}{1}", val, Environment.NewLine);
            sb.AppendFormat(CultureInfo.InvariantCulture, "Float: {0}{1}", BitConverter.ToSingle(BitConverter.GetBytes(val), 0), Environment.NewLine);
            //sb.AppendFormat(CultureInfo.InvariantCulture, "Double: {0}{1}", BitConverter.ToDouble(BitConverter.GetBytes(val), 0), Environment.NewLine);
            sb.AppendFormat(CultureInfo.InvariantCulture, "String: {0}{1}", m_reader.StringTable[(int)val], Environment.NewLine);
            e.ToolTipText = sb.ToString();
        }

        private void dataGridView1_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell != null)
                label1.Text = String.Format("Current Cell: {0}x{1}", dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var file = (string)e.Argument;

            if (Path.GetExtension(file).ToLowerInvariant() == ".dbc")
                m_reader = new DBCReader(file);
            else
                m_reader = new DB2Reader(file);

            m_fields = m_definition.GetElementsByTagName("field");

            if (GetFieldsCount(m_fields) != m_reader.FieldsCount)
            {
                var msg = String.Format("{0} has invalid definition!\nFields count mismatch: got {1}, expected {2}", Path.GetFileName(file), m_fields.Count, m_reader.FieldsCount);
                ShowErrorMessageBox(msg);
                e.Cancel = true;
                return;
            }

            m_dataTable = new DataTable(Path.GetFileName(file));

            CreateColumns();                                // Add columns

            CreateIndexes();                                // Add indexes

            string[] types = new string[m_fields.Count];

            for (var j = 0; j < m_fields.Count; ++j)
                types[j] = m_fields[j].Attributes["type"].Value;

            for (var i = 0; i < m_reader.RecordsCount; ++i) // Add rows
            {
                var dataRow = m_dataTable.NewRow();

                #region Test
                //var bytes = m_reader.GetRowAsByteArray(i);
                //unsafe
                //{
                //    fixed (void* b = bytes)
                //    {
                //        IntPtr ptr = new IntPtr(b);

                //        int offset = 0;

                //        for (var j = 0; j < m_fields.Count; ++j)    // Add cells
                //        {
                //            switch (types[j])
                //            {
                //                case "long":
                //                    dataRow[j] = *(long*)(ptr + offset);
                //                    offset += 8;
                //                    break;
                //                case "ulong":
                //                    dataRow[j] = *(ulong*)(ptr + offset);
                //                    offset += 8;
                //                    break;
                //                case "int":
                //                    dataRow[j] = *(int*)(ptr + offset);
                //                    offset += 4;
                //                    break;
                //                case "uint":
                //                    dataRow[j] = *(uint*)(ptr + offset);
                //                    offset += 4;
                //                    break;
                //                case "short":
                //                    dataRow[j] = *(short*)(ptr + offset);
                //                    offset += 2;
                //                    break;
                //                case "ushort":
                //                    dataRow[j] = *(ushort*)(ptr + offset);
                //                    offset += 2;
                //                    break;
                //                case "sbyte":
                //                    dataRow[j] = *(sbyte*)(ptr + offset);
                //                    offset += 1;
                //                    break;
                //                case "byte":
                //                    dataRow[j] = *(byte*)(ptr + offset);
                //                    offset += 1;
                //                    break;
                //                case "float":
                //                    dataRow[j] = *(float*)(ptr + offset);
                //                    offset += 4;
                //                    break;
                //                case "double":
                //                    dataRow[j] = *(double*)(ptr + offset);
                //                    offset += 8;
                //                    break;
                //                case "string":
                //                    dataRow[j] = m_reader.StringTable[*(int*)(ptr + offset)];
                //                    offset += 4;
                //                    break;
                //                default:
                //                    throw new Exception(String.Format("Unknown field type {0}!", m_fields[j].Attributes["type"].Value));
                //            }
                //        }
                //    }
                //}
                #endregion
                var br = m_reader[i];

                for (var j = 0; j < m_fields.Count; ++j)    // Add cells
                {
                    switch (types[j])
                    {
                        case "long":
                            dataRow[j] = br.ReadInt64();
                            break;
                        case "ulong":
                            dataRow[j] = br.ReadUInt64();
                            break;
                        case "int":
                            dataRow[j] = br.ReadInt32();
                            break;
                        case "uint":
                            dataRow[j] = br.ReadUInt32();
                            break;
                        case "short":
                            dataRow[j] = br.ReadInt16();
                            break;
                        case "ushort":
                            dataRow[j] = br.ReadUInt16();
                            break;
                        case "sbyte":
                            dataRow[j] = br.ReadSByte();
                            break;
                        case "byte":
                            dataRow[j] = br.ReadByte();
                            break;
                        case "float":
                            dataRow[j] = br.ReadSingle();
                            break;
                        case "double":
                            dataRow[j] = br.ReadDouble();
                            break;
                        case "string":
                            dataRow[j] = m_reader.StringTable[br.ReadInt32()];
                            break;
                        default:
                            throw new Exception(String.Format("Unknown field type {0}!", m_fields[j].Attributes["type"].Value));
                    }
                }

                m_dataTable.Rows.Add(dataRow);

                int percent = (int)((float)m_dataTable.Rows.Count / (float)m_reader.RecordsCount * 100.0f);
                (sender as BackgroundWorker).ReportProgress(percent);
            }

            if (dataGridView1.InvokeRequired)
            {
                SetDataViewDelegate d = new SetDataViewDelegate(SetDataView);
                Invoke(d, new object[] { m_dataTable.DefaultView });
            }
            else
            {
                SetDataView(m_dataTable.DefaultView);
            }

            e.Result = file;
        }

        private XmlElement GetDefinition()
        {
            XmlNodeList definitions = m_definitions["DBFilesClient"].GetElementsByTagName(m_dbcName);

            if (definitions.Count == 0)
            {
                var msg = String.Format("{0} missing definition!", m_dbcName);
                ShowErrorMessageBox(msg);
                return null;
            }
            else if (definitions.Count == 1)
            {
                return ((XmlElement)definitions[0]);
            }
            else
            {
                m_selector = new DefinitionSelect();
                m_selector.SetDefinitions(definitions);
                var result = m_selector.ShowDialog(this);
                if (result != DialogResult.OK || m_selector.DefinitionIndex == -1)
                    return null;
                return ((XmlElement)definitions[m_selector.DefinitionIndex]);
            }
        }

        private void ShowErrorMessageBox(string format, params object[] args)
        {
            var msg = String.Format(format, args);
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void CreateIndexes()
        {
            XmlNodeList indexes = m_definition.GetElementsByTagName("index");
            var columns = new DataColumn[indexes.Count];
            var idx = 0;
            foreach (XmlElement index in indexes)
                columns[idx++] = m_dataTable.Columns[index["primary"].InnerText];
            m_dataTable.PrimaryKey = columns;
        }

        private void CreateColumns()
        {
            foreach (XmlElement field in m_fields)
            {
                var colName = field.Attributes["name"].Value;

                switch (field.Attributes["type"].Value)
                {
                    case "long":
                        m_dataTable.Columns.Add(colName, typeof(long));
                        break;
                    case "ulong":
                        m_dataTable.Columns.Add(colName, typeof(ulong));
                        break;
                    case "int":
                        m_dataTable.Columns.Add(colName, typeof(int));
                        break;
                    case "uint":
                        m_dataTable.Columns.Add(colName, typeof(uint));
                        break;
                    case "short":
                        m_dataTable.Columns.Add(colName, typeof(short));
                        break;
                    case "ushort":
                        m_dataTable.Columns.Add(colName, typeof(ushort));
                        break;
                    case "sbyte":
                        m_dataTable.Columns.Add(colName, typeof(sbyte));
                        break;
                    case "byte":
                        m_dataTable.Columns.Add(colName, typeof(byte));
                        break;
                    case "float":
                        m_dataTable.Columns.Add(colName, typeof(float));
                        break;
                    case "double":
                        m_dataTable.Columns.Add(colName, typeof(double));
                        break;
                    case "string":
                        m_dataTable.Columns.Add(colName, typeof(string));
                        break;
                    default:
                        throw new Exception(String.Format("Unknown field type {0}!", field.Attributes["type"].Value));
                }
            }
        }

        private void columnsFilterEventHandler(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            dataGridView1.Columns[item.Name].Visible = !item.Checked;

            ((ToolStripMenuItem)item.OwnerItem).ShowDropDown();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripProgressBar1.Visible = false;
            toolStripProgressBar1.Value = 0;

            if (e.Error != null)
            {
                ShowErrorMessageBox(e.Error.ToString());
                toolStripStatusLabel1.Text = "Error.";
            }
            else if (e.Cancelled == true)
            {
                toolStripStatusLabel1.Text = "Error in definitions.";
                StartEditor();
            }
            else
            {
                TimeSpan total = DateTime.Now - m_startTime;
                toolStripStatusLabel1.Text = String.Format(CultureInfo.InvariantCulture, "Ready. Loaded in {0} sec", total.TotalSeconds);
                Text = String.Format("DBC Viewer - {0}", e.Result.ToString());
                InitColumnsFilter();
            }
        }

        private void InitColumnsFilter()
        {
            columnsFilterToolStripMenuItem.DropDownItems.Clear();

            foreach (XmlElement field in m_fields)
            {
                var colName = field.Attributes["name"].Value;
                var type = field.Attributes["type"].Value;
                var format = field.Attributes["format"] != null ? field.Attributes["format"].Value : String.Empty;
                var visible = field.Attributes["visible"] != null ? field.Attributes["visible"].Value == "true" : true;
                var width = field.Attributes["width"] != null ? Convert.ToInt32(field.Attributes["width"].Value) : 100;

                var item = new ToolStripMenuItem(colName);
                item.Click += new EventHandler(columnsFilterEventHandler);
                item.CheckOnClick = true;
                item.Name = colName;
                item.Checked = !visible;
                columnsFilterToolStripMenuItem.DropDownItems.Add(item);

                dataGridView1.Columns[colName].Visible = visible;
                dataGridView1.Columns[colName].Width = width;
                dataGridView1.Columns[colName].AutoSizeMode = GetColumnAutoSizeMode(type, format);
                dataGridView1.Columns[colName].SortMode = DataGridViewColumnSortMode.Automatic;
            }
        }

        private DataGridViewAutoSizeColumnMode GetColumnAutoSizeMode(string type, string format)
        {
            switch (type)
            {
                case "string":
                    return DataGridViewAutoSizeColumnMode.NotSet;
                default:
                    break;
            }

            if (format == String.Empty)
                return DataGridViewAutoSizeColumnMode.DisplayedCells;

            switch (format.Substring(0, 1).ToLower())
            {
                case "X":
                case "B":
                case "O":
                    return DataGridViewAutoSizeColumnMode.DisplayedCells;
                default:
                    return DataGridViewAutoSizeColumnMode.ColumnHeader;
            }
        }

        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_dataTable == null)
                return;

            if (m_filterForm == null || m_filterForm.IsDisposed)
                m_filterForm = new FilterForm();

            if (!m_filterForm.Visible)
                m_filterForm.Show(this);
        }

        private void resetFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_dataTable == null)
                return;

            if (m_filterForm != null)
                m_filterForm.ResetFilters();

            SetDataView(m_dataTable.DefaultView);
        }

        public void SetDataView(DataView dataView)
        {
            m_dataView = dataView;
            dataGridView1.DataSource = m_dataView;

            label2.Text = String.Format("Rows Displayed: {0}", m_dataView.Count);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            WindowState = Properties.Settings.Default.WindowState;
            Size = Properties.Settings.Default.WindowSize;
            Location = Properties.Settings.Default.WindowLocation;

            m_workingFolder = Application.StartupPath;

            LoadDefinitions();
            Compose();

            var cmds = Environment.GetCommandLineArgs();
            if (cmds.Length > 1)
            {
                LoadFile(cmds[1]);
            }
        }

        private void LoadDefinitions()
        {
            m_definitions = new XmlDocument();
            m_definitions.Load(Path.Combine(m_workingFolder, "dbclayout.xml"));
        }

        private void Compose()
        {
            m_catalog = new DirectoryCatalog(Environment.CurrentDirectory);
            var container = new CompositionContainer(m_catalog);
            container.ComposeParts(this);
        }

        private void runPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_dataTable == null)
            {
                ShowErrorMessageBox("Nothing loaded yet!");
                return;
            }

            m_catalog.Refresh();

            if (Plugins.Count == 0)
            {
                ShowErrorMessageBox("No plugins found!");
                return;
            }

            PluginsForm selector = new PluginsForm();
            selector.SetPlugins(Plugins);
            var result = selector.ShowDialog(this);
            if (result != DialogResult.OK || selector.PluginIndex == -1)
            {
                ShowErrorMessageBox("No plugin selected!");
                return;
            }

            Plugins[selector.PluginIndex].Run(m_dataTable);
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var attribute = m_fields[e.ColumnIndex].Attributes["format"];

            if (attribute == null)
                return;

            var fmtStr = "{0:" + attribute.Value + "}";
            e.Value = String.Format(new BinaryFormatter(), fmtStr, e.Value);
            e.FormattingApplied = true;
        }

        private void resetColumnsFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.Visible = true;
                ((ToolStripMenuItem)columnsFilterToolStripMenuItem.DropDownItems[col.Name]).Checked = false;
            }
        }

        private static int GetFieldsCount(XmlNodeList fields)
        {
            int count = 0;
            foreach (XmlElement field in fields)
            {
                switch (field.Attributes["type"].Value)
                {
                    case "long":
                    case "ulong":
                    case "double":
                        count += 2;
                        break;
                    default:
                        count++;
                        break;
                }
            }
            return count;
        }

        private void autoSizeColumnsModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var control = (ToolStripMenuItem)sender;
            foreach (ToolStripMenuItem item in autoSizeModeToolStripMenuItem.DropDownItems)
            {
                if (item != control)
                    item.Checked = false;
            }

            var index = (int)columnContextMenuStrip.Tag;
            dataGridView1.Columns[index].AutoSizeMode = (DataGridViewAutoSizeColumnMode)Enum.Parse(typeof(DataGridViewAutoSizeColumnMode), (string)control.Tag);
        }

        private void hideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var index = (int)columnContextMenuStrip.Tag;
            dataGridView1.Columns[index].Visible = false;
            ((ToolStripMenuItem)columnsFilterToolStripMenuItem.DropDownItems[index]).Checked = true;
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                return;

            DataGridView.HitTestInfo hit = dataGridView1.HitTest(e.X, e.Y);

            if (hit.Type == DataGridViewHitTestType.ColumnHeader)
            {
                columnContextMenuStrip.Tag = hit.ColumnIndex;

                foreach (ToolStripMenuItem item in autoSizeModeToolStripMenuItem.DropDownItems)
                {
                    if (item.Tag.ToString() == dataGridView1.Columns[hit.ColumnIndex].AutoSizeMode.ToString())
                        item.Checked = true;
                    else
                        item.Checked = false;
                }

                columnContextMenuStrip.Show((Control)sender, e.Location.X, e.Location.Y);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseFile();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.WindowState = WindowState;

            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowSize = Size;
                Properties.Settings.Default.WindowLocation = Location;
            }
            else
            {
                Properties.Settings.Default.WindowSize = RestoreBounds.Size;
                Properties.Settings.Default.WindowLocation = RestoreBounds.Location;
            }

            Properties.Settings.Default.Save();
        }

        private void difinitionEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_dbcName == null)
                return;

            StartEditor();
        }

        private void reloadDefinitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadDefinitions();
        }
    }
}
