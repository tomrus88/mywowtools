using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WoWPacketViewer
{
    public partial class FrmMain : Form
    {
        private delegate void AddListViewItem(ListViewItem item);

        private delegate void ListViewUpdateControl(bool end);

        private PacketViewerBase m_packetViewer;
        private FrmSearch m_searchForm;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void AddPacket(ListViewItem item)
        {
            if (_list.InvokeRequired)
            {
                Invoke(new AddListViewItem(AddPacket), item);
            }
            else
            {
                _list.Items.Add(item);
            }
        }

        private void UpdateControl(bool end)
        {
            if (_list.InvokeRequired)
            {
                Invoke(new ListViewUpdateControl(UpdateControl), end);
            }
            else
            {
                if (end)
                {
                    _list.EndUpdate();
                }
                else
                {
                    _list.BeginUpdate();
                }
            }
        }

        public void FillListView(BackgroundWorker worker)
        {
            var pkt = (from packet in m_packetViewer.Packets
                       where packet.Code == OpCodes.CMSG_AUTH_SESSION
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

            var i = 0;
            UpdateControl(false);
            foreach (var p in m_packetViewer.Packets)
            {
                AddPacket(p.Direction == Direction.Client
                                ? new ListViewItem(new[] { build.ToString(), p.Code.ToString(), string.Empty, p.Data.Length.ToString() })
                                : new ListViewItem(new[] { build.ToString(), string.Empty, p.Code.ToString(), p.Data.Length.ToString() }));
                ++i;
                worker.ReportProgress((int)((i / (float)m_packetViewer.Packets.Count) * 100f));
            }
            UpdateControl(true);
        }

        public void Search(string text, bool searchUp)
        {
            var item = FindItem(text, searchUp);
            if (item == null)
            {
                return;
            }
            item.Selected = true;
            item.EnsureVisible();
        }

        private ListViewItem FindItem(string text, bool searchUp)
        {
            if (!searchUp)
            {
                for (var i = SelectedIndex + 1; i < _list.Items.Count; i++)
                {
                    var j = _list.Items[i];
                    if (j.Text.Contains(text))
                    {
                        return j;
                    }

                    foreach (ListViewItem.ListViewSubItem subItem in j.SubItems)
                    {
                        if (subItem.Text.Contains(text))
                        {
                            return j;
                        }
                    }
                }
            }
            for (var i = SelectedIndex - 1; i != 0; --i)
            {
                var j = _list.Items[i];
                if (j.Text.Contains(text))
                {
                    return j;
                }

                foreach (ListViewItem.ListViewSubItem subItem in j.SubItems)
                {
                    if (subItem.Text.Contains(text))
                    {
                        return j;
                    }
                }
            }
            return null;
        }

        private int SelectedIndex
        {
            get
            {
                var selectedIndex = 0;
                var sic = _list.SelectedIndices;
                if (sic.Count > 0)
                {
                    selectedIndex = sic[0];
                }
                return selectedIndex;
            }
        }

        private void List_Select(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();

            var sic = _list.SelectedIndices;
            if (sic.Count != 0)
            {
                textBox1.Text = m_packetViewer.Packets[sic[0]].HexLike();
                textBox2.Text = m_packetViewer.ShowParsed(m_packetViewer.Packets[sic[0]]);
            }
        }

        private void OpenMenu_Click(object sender, EventArgs e)
        {
            var result = _openDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Clear();
                textBox2.Clear();
                _list.Items.Clear();

                _statusLabel.Text = "Loading...";
                var file = _openDialog.FileName;
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
                m_packetViewer.LoadData(_openDialog.FileName);
                _statusLabel.Text = _openDialog.FileName;
                _backgroundWorker.RunWorkerAsync(m_packetViewer.Packets.Count);
            }
        }

        private void SaveMenu_Click(object sender, EventArgs e)
        {
            if (_saveDialog.ShowDialog() == DialogResult.OK)
            {
                using (var stream = new StreamWriter(_saveDialog.OpenFile()))
                {
                    foreach (var p in m_packetViewer.Packets)
                    {
                        stream.Write(p.HexLike());
                    }
                }
            }
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void FindMenu_Click(object sender, EventArgs e)
        {
            if (m_searchForm == null || m_searchForm.IsDisposed)
            {
                m_searchForm = new FrmSearch();
            }

            if (!m_searchForm.Visible)
            {
                m_searchForm.Show(this);
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage < 100)
            {
                _progressBar.Value = e.ProgressPercentage;
            }
            else
            {
                _progressBar.Visible = false;
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            FillListView(sender as BackgroundWorker);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _statusLabel.Text = "Done.";
        }
    }
}
