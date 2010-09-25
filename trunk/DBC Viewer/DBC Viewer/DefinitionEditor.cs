using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;

namespace DBCViewer
{
    public partial class DefinitionEditor : Form
    {
        public DefinitionEditor()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Ascending);
            WriteXml("test");
            Close();
        }

        private void WriteXml(string name)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("dbclayout.xml");

            XmlNode oldnode = doc["DBFilesClient"][name];
            XmlElement newnode = doc.CreateElement(name);

            if (oldnode == null)
                doc["DBFilesClient"].AppendChild(newnode);
            else
                doc["DBFilesClient"].ReplaceChild(newnode, oldnode);

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[1].Value != null && row.Cells[2].Value != null && row.Cells[3].Value != null)
                {
                    if ((string)row.Cells[1].Value == String.Empty)
                        continue;

                    if ((bool)row.Cells[3].Value)
                    {
                        XmlElement index = doc.CreateElement("index");
                        XmlNode primary = index.AppendChild(doc.CreateElement("primary"));
                        primary.InnerText = (string)row.Cells[1].Value;
                        doc["DBFilesClient"][name].AppendChild(index);
                    }

                    XmlElement ele = doc.CreateElement("field");
                    ele.SetAttributeNode("type", "").Value = (string)row.Cells[2].Value;
                    ele.SetAttributeNode("name", "").Value = (string)row.Cells[1].Value;
                    doc["DBFilesClient"][name].AppendChild(ele);
                }
            }
            doc.Save("dbclayout_test.xml");
        }

        public void SetDefinitions(XmlElement def)
        {
            XmlNodeList fields = def.GetElementsByTagName("field");
            XmlNodeList indexes = def.GetElementsByTagName("index");
            foreach (XmlNode field in fields)
            {
                var row = dataGridView1.Rows.Add();
                ((DataGridViewCell)dataGridView1[0, row]).Value = row;
                ((DataGridViewCell)dataGridView1[1, row]).Value = field.Attributes["name"].Value;
                ((DataGridViewComboBoxCell)dataGridView1[2, row]).Value = field.Attributes["type"].Value;
                ((DataGridViewCheckBoxCell)dataGridView1[3, row]).Value = IsIndexColumn(field.Attributes["name"].Value, indexes);
            }
        }

        private bool IsIndexColumn(string name, XmlNodeList indexes)
        {
            foreach (XmlNode index in indexes)
                if (index["primary"].InnerText == name)
                    return true;
            return false;
        }

        private void dataGridView1_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[0].Value = dataGridView1.Rows.Count;
            e.Row.Cells[1].Value = "";
            e.Row.Cells[2].Value = "int";
            e.Row.Cells[3].Value = false;
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 0 || e.RowIndex == -1)
                return;

            try
            {
                string val = Convert.ToString(dataGridView1[e.ColumnIndex, e.RowIndex].Value);
                dataGridView1[e.ColumnIndex, e.RowIndex].Value = int.Parse(val);
            }
            catch
            {
                dataGridView1[e.ColumnIndex, e.RowIndex].Value = null;
            }
        }
    }
}
