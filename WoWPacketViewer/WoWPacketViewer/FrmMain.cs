using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace WoWPacketViewer
{
    public partial class FrmMain : Form
    {
        delegate void AddListViewItem(ListViewItem item);
        delegate void ListViewUpdateControl(bool end);
        private PacketViewerBase m_packetViewer;
        private FrmSearch m_searchForm;

        public FrmMain()
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
                    case ".xml":
                        m_packetViewer = new SniffitztPacketViewer();
                        break;
                    default:
                        break;
                }
                m_packetViewer.LoadData(openFileDialog1.FileName);
                toolStripStatusLabel1.Text = openFileDialog1.FileName;
                backgroundWorker1.RunWorkerAsync(m_packetViewer.Packets.Count);
                //FillListView();
            }
        }

        private void AddPacket(ListViewItem item)
        {
            if (listView1.InvokeRequired)
            {
                AddListViewItem d = new AddListViewItem(AddPacket);
                Invoke(d, new object[] { item });
            }
            else
                listView1.Items.Add(item);
        }

        private void UpdateControl(bool end)
        {
            if (listView1.InvokeRequired)
            {
                ListViewUpdateControl lvuc = new ListViewUpdateControl(UpdateControl);
                Invoke(lvuc, new object[] { end });
            }
            else
            {
                if (end)
                    listView1.EndUpdate();
                else
                    listView1.BeginUpdate();
            }
        }

        public void FillListView(BackgroundWorker worker)
        {
            var pkt = (from packet in m_packetViewer.Packets
                       where packet.Opcode == OpCodes.CMSG_AUTH_SESSION
                       select packet).FirstOrDefault();

            uint build;
            try
            {
                build = BitConverter.ToUInt32(pkt.Data, 0);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                return;
            }

            int i = 0;
            UpdateControl(false);
            foreach (Packet p in m_packetViewer.Packets)
            {
                ListViewItem item;
                if (p.Direction == Direction.Client)
                    item = new ListViewItem(new string[] { build.ToString(), p.Opcode.ToString(), string.Empty, p.Data.Length.ToString() });
                else
                    item = new ListViewItem(new string[] { build.ToString(), string.Empty, p.Opcode.ToString(), p.Data.Length.ToString() });
                AddPacket(item);
                ++i;
                worker.ReportProgress(i / (m_packetViewer.Packets.Count / 100));
            }
            UpdateControl(true);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();

            ListView.SelectedIndexCollection sic = listView1.SelectedIndices;
            if (sic.Count == 0)
                return;

            textBox1.Text = m_packetViewer.HexLike(m_packetViewer.Packets[sic[0]]);
            textBox2.Text = m_packetViewer.ShowParsed(m_packetViewer.Packets[sic[0]]);
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_searchForm == null || m_searchForm.IsDisposed)
                m_searchForm = new FrmSearch();

            if (m_searchForm.Visible)
                return;

            m_searchForm.Show(this);
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
                for (int i = selectedIndex - 1; i != 0; --i)
                {
                    ListViewItem j = listView1.Items[i];
                    if (j.Text.Contains(opcode))
                    {
                        j.Selected = true;
                        j.EnsureVisible();
                        break;
                    }

                    bool exit = false;
                    foreach (ListViewItem.ListViewSubItem si in j.SubItems)
                    {
                        if (si.Text.Contains(opcode))
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
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamWriter stream = new StreamWriter(saveFileDialog1.OpenFile());
                foreach (Packet p in m_packetViewer.Packets)
                {
                    stream.Write(m_packetViewer.HexLike(p));
                }
                stream.Flush();
                stream.Close();
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage <= 100)
                toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            FillListView(worker);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            toolStripStatusLabel1.Text = "Done.";
        }
    }
}
