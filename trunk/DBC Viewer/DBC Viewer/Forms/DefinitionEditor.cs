using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace DBCViewer
{
    public partial class DefinitionEditor : Form
    {
        private ListViewItem.ListViewSubItem m_listViewSubItem;
        private readonly string[] comboBoxItems1 = new string[] { "long", "ulong", "int", "uint", "short", "ushort", "sbyte", "byte", "float", "double", "string" };
        private readonly string[] comboBoxItems2 = new string[] { "True", "False" };
        private string m_name;

        public DefinitionEditor()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WriteXml();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void WriteXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine((Owner as MainForm).WorkingFolder, "dbclayout.xml"));

            XmlNode oldnode = null; // nodes.Count == 0

            XmlNodeList nodes = doc["DBFilesClient"].GetElementsByTagName(m_name);

            if (nodes.Count == 1)
                oldnode = nodes[0];
            else if (nodes.Count > 1)
            {
                int index = (Owner as MainForm).DefinitionIndex;
                oldnode = nodes[index];
            }

            XmlElement newnode = doc.CreateElement(m_name);
            newnode.SetAttributeNode("build", "").Value = oldnode == null ? "0" : oldnode.Attributes["build"].Value;

            foreach (ListViewItem item in listView1.Items)
            {
                if (item.SubItems[2].Text == "True")
                {
                    XmlElement index = doc.CreateElement("index");
                    XmlNode primary = index.AppendChild(doc.CreateElement("primary"));
                    primary.InnerText = item.SubItems[0].Text;
                    newnode.AppendChild(index);
                }

                XmlElement ele = doc.CreateElement("field");
                ele.SetAttributeNode("type", "").Value = item.SubItems[1].Text;
                ele.SetAttributeNode("name", "").Value = item.SubItems[0].Text;
                newnode.AppendChild(ele);
            }

            if (oldnode == null)
                doc["DBFilesClient"].AppendChild(newnode);
            else
                doc["DBFilesClient"].ReplaceChild(newnode, oldnode);

            doc.Save(Path.Combine((Owner as MainForm).WorkingFolder, "dbclayout.xml"));
        }

        public void InitDefinitions()
        {
            m_name = (Owner as MainForm).DBCName;

            XmlElement def = (Owner as MainForm).Definition;

            if (def == null)
                return;

            XmlNodeList fields = def.GetElementsByTagName("field");
            XmlNodeList indexes = def.GetElementsByTagName("index");

            foreach (XmlNode field in fields)
            {
                listView1.Items.Add(new ListViewItem(new string[] {
                    field.Attributes["name"].Value,
                    field.Attributes["type"].Value,
                    IsIndexColumn(field.Attributes["name"].Value, indexes).ToString()}));
            }
        }

        private bool IsIndexColumn(string name, XmlNodeList indexes)
        {
            foreach (XmlNode index in indexes)
                if (index["primary"].InnerText == name)
                    return true;
            return false;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            Point point = listView1.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = listView1.GetItemAt(point.X, point.Y);

            if (dragToItem == null)
                return;

            ListViewItem dragFromItem = listView1.SelectedItems[0];

            var dragToIndex = dragToItem.Index;
            var dragFromIndex = dragFromItem.Index;

            if (dragToIndex == dragFromIndex)
                return;

            if (dragFromIndex < dragToIndex)
                dragToIndex++;

            ListViewItem insertItem = (ListViewItem)dragFromItem.Clone();
            listView1.Items.Insert(dragToIndex, insertItem);
            listView1.Items.Remove(dragFromItem);
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            var item = listView1.GetItemAt(e.X, e.Y);
            if (item != null)
            {
                var subItem = item.GetSubItemAt(e.X, e.Y);
                if (item.SubItems.IndexOf(subItem) != 0)
                    item.Selected = true;
            }

            if (e.Button == MouseButtons.Right && listView1.SelectedItems.Count > 0)
                listView1.DoDragDrop(listView1.SelectedItems[0], DragDropEffects.Move);
            else if (e.Button == MouseButtons.Left && comboBox1.Visible)
                comboBox1.Visible = false;
        }

        private void listView1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            m_listViewSubItem.Text = comboBox1.Text;        // Set text of ListView item

            comboBox1.Visible = false;                      // Hide the combobox
        }

        private void comboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)                   // Check if user pressed "ESC"
                comboBox1.Visible = false;                  // Hide the combobox
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var item = listView1.GetItemAt(e.X, e.Y);       // Get the item that was clicked
            m_listViewSubItem = item.GetSubItemAt(e.X, e.Y);// Get the sub item that was clicked

            var column = item.SubItems.IndexOf(m_listViewSubItem);

            switch (column)
            {
                case 0:                                     // name column without combobox
                    item.BeginEdit();
                    return;
                case 1:
                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange(comboBoxItems1);
                    break;
                case 2:
                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange(comboBoxItems2);
                    break;
                default:
                    break;
            }

            if (m_listViewSubItem != null)                  // Check if an actual item was clicked
            {
                Rectangle ClickedItem = m_listViewSubItem.Bounds;   // Get the bounds of the item clicked

                // Adjust the top and left of the control
                ClickedItem.X += listView1.Left;
                ClickedItem.Y += listView1.Top;

                comboBox1.Bounds = ClickedItem;             // Set combobox bounds to match calculation

                comboBox1.Text = m_listViewSubItem.Text;    // Set the default text for the combobox to be the clicked item's text

                comboBox1.Visible = true;                   // Show the combobox
                comboBox1.BringToFront();                   // Make sure it is on top
                comboBox1.Focus();                          // Give focus to the combobox
            }
        }

        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete || listView1.SelectedItems.Count == 0)
                return;

            var index = listView1.SelectedItems[0].Index;
            listView1.Items.RemoveAt(index);

            index = listView1.Items.Count > index ? index : listView1.Items.Count - 1;
            if (index == -1)
                return;

            listView1.Items[index].Selected = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                listView1.Items.Insert(listView1.SelectedItems[0].Index + 1, new ListViewItem(new string[] { "newField", "int", "False" }));
            else
                listView1.Items.Add(new ListViewItem(new string[] { "newField", "int", "False" }));
        }

        private void DefinitionEditor_Load(object sender, EventArgs e)
        {
            InitDefinitions();
        }
    }
}
