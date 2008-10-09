using System;
using System.Windows.Forms;
using System.IO;

namespace WoWPacketViewer
{
    public partial class Form1 : Form
    {
        PacketViewerBase m_packetViewer;
        SearchForm m_searchForm;

        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Clear();
                textBox2.Clear();
                listView1.Items.Clear();

                toolStripStatusLabel1.Text = "Loading...";
                var file = openFileDialog1.FileName;
                switch (Path.GetExtension(file))
                {
                    case ".bin":
                        m_packetViewer = new WowCorePacketViewer();
                        break;
                    case ".sqlite":
                        m_packetViewer = new SqLitePacketViewer();
                        break;
                    default:
                        break;
                }
                m_packetViewer.LoadData(openFileDialog1.FileName);
                toolStripStatusLabel1.Text = openFileDialog1.FileName;
                listView1.Items.AddRange(m_packetViewer.ListPackets());
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();

            ListView.SelectedIndexCollection sic = listView1.SelectedIndices;
            if (sic.Count == 0)
                return;
            m_packetViewer.ShowHex(textBox1, sic[0]);
            m_packetViewer.ShowParsed(textBox2, sic[0]);
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_searchForm == null || m_searchForm.IsDisposed)
                m_searchForm = new SearchForm();

            if (m_searchForm.Visible)
                return;

            m_searchForm.Show();
        }

        public void Search(string opcode, bool direction)
        {
            if (String.IsNullOrEmpty(opcode))
                return;

            int selectedIndex = 0;
            ListView.SelectedIndexCollection sic = listView1.SelectedIndices;
            if (sic.Count != 0)
                selectedIndex = sic[0];

            if (!direction)
            {
                ListViewItem i = listView1.FindItemWithText(opcode, true, selectedIndex + 1);
                if (i != null)
                {
                    i.Selected = true;
                    i.EnsureVisible();
                }
            }
            else
            {
                // не пашет :(
                for (int i = listView1.Items.Count - 1; i != 0; --i)
                {
                    ListViewItem j = listView1.Items[i];
                    if (j.Text.Contains(opcode))
                    {
                        j.Selected = true;
                        j.EnsureVisible();
                        break;
                    }

                    System.Collections.IEnumerator gg = j.SubItems.GetEnumerator();
                    bool exit = false;
                    while (gg.MoveNext())
                    {
                        ListViewItem.ListViewSubItem k = (ListViewItem.ListViewSubItem)gg.Current;
                        if(k.Text.Contains(opcode))
                        {
                            j.Selected = true;
                            j.EnsureVisible();
                            exit = true;
                            break;
                        }
                    }

                    if (exit)
                        break;
                }
            }
        }

        private void saveAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamWriter stream = new StreamWriter(saveFileDialog1.OpenFile());
                foreach (Packet p in PacketViewerBase.Packets)
                {
                    stream.Write(m_packetViewer.HexLike(p));
                }
                stream.Flush();
                stream.Close();
            }
        }
    }
}
