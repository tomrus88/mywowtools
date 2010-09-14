using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using dbc2sql;

namespace DBC_Viewer
{
    public partial class MainForm : Form
    {
        DataTable m_dataTable;
        public DataTable DataTable { get { return m_dataTable; } }
        DataView m_dataView;
        public DataView DataView { get { return m_dataView; } }
        IWowClientDBReader m_reader;
        FilterForm m_filterForm;

        public MainForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            dataGridView1.VirtualMode = false;
            dataGridView1.DataSource = null;

            toolStripProgressBar1.Visible = true;

            backgroundWorker1.RunWorkerAsync(openFileDialog1.FileName);
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

            int val = Convert.ToInt32(m_dataView[e.RowIndex][e.ColumnIndex]);

            var sb = new StringBuilder();
            sb.AppendFormat(new BinaryFormatter(), "Integer: {0:D}{1}", val, Environment.NewLine);
            sb.AppendFormat(new BinaryFormatter(), "HEX: {0:X}{1}", val, Environment.NewLine);
            sb.AppendFormat(new BinaryFormatter(), "BIN: {0:B}{1}", val, Environment.NewLine);
            sb.AppendFormat(new BinaryFormatter(), "Float: {0}{1}", BitConverter.ToSingle(BitConverter.GetBytes(val), 0), Environment.NewLine);
            sb.AppendFormat(new BinaryFormatter(), "String: {0}{1}", m_reader.StringTable[val], Environment.NewLine);
            e.ToolTipText = sb.ToString();
        }

        private void dataGridView1_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell != null)
                label1.Text = String.Format("Current Cell: {0}x{1}", dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex);
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var col = dataGridView1.Columns[e.ColumnIndex].Name;

            if (dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection != SortOrder.Ascending)
                m_dataView.Sort = String.Format("[{0}] asc", col);
            else
                m_dataView.Sort = String.Format("[{0}] desc", col);

            SetDataView(m_dataView);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var file = (string)e.Argument;

            if (Path.GetExtension(file).ToLowerInvariant() == ".dbc")
                m_reader = new DBCReader(file);
            else
                m_reader = new DB2Reader(file);

            m_dataTable = new DataTable();

            // Add columns
            for (var i = 0; i < m_reader.FieldsCount; ++i)
            {
                var colName = String.Format("Col {0}", i);
                m_dataTable.Columns.Add(colName, typeof(int));
            }

            // Add rows
            for (var i = 0; i < m_reader.RecordsCount; ++i)
            {
                var br = m_reader[i];

                var dataRow = m_dataTable.NewRow();

                // Add cells
                for (var j = 0; j < m_reader.FieldsCount; ++j)
                {
                    var colName = String.Format("Col {0}", j);
                    dataRow[colName] = br.ReadInt32();
                }

                m_dataTable.Rows.Add(dataRow);

                int percent = (int)((float)m_dataTable.Rows.Count / (float)m_reader.RecordsCount * 100.0f);
                (sender as BackgroundWorker).ReportProgress(percent);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripProgressBar1.Visible = false;
            toolStripProgressBar1.Value = 0;

            SetDataView(m_dataTable.DefaultView);
            dataGridView1.VirtualMode = true;
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

            SetDataView(m_dataTable.DefaultView);
        }

        public void SetDataView(DataView dataView)
        {
            m_dataView = dataView;
            dataGridView1.DataSource = m_dataView;
        }
    }
}
