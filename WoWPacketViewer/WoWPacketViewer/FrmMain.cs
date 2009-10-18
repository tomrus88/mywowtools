using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WoWPacketViewer
{
    public interface ISupportFind
    {
        void Search(string text, bool searchUp, bool ignoreCase);
    }

    public partial class FrmMain : Form, ISupportFind
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
                                ? new ListViewItem(new[] { p.UnixTime.ToString("X8"), p.TicksCount.ToString("X8"), p.Code.ToString(), string.Empty, p.Data.Length.ToString() })
                                : new ListViewItem(new[] { p.UnixTime.ToString("X8"), p.TicksCount.ToString("X8"), string.Empty, p.Code.ToString(), p.Data.Length.ToString() }));
                ++i;
                worker.ReportProgress((int)((i / (float)m_packetViewer.Packets.Count) * 100f));
            }
            UpdateControl(true);
        }

        public void Search(string text, bool searchUp, bool ignoreCase)
        {
            var item = FindItem(text, searchUp, ignoreCase);
            if (item != null)
            {
                item.Selected = true;
                item.EnsureVisible();
            }
            else
            {
                MessageBox.Show(string.Format("Can't find:'{0}'", text), "Packet Viewer", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
        }

        private ListViewItem FindItem(string text, bool searchUp, bool ignoreCase)
        {
            var comparisonType = ignoreCase
                                     ? StringComparison.InvariantCultureIgnoreCase
                                     : StringComparison.InvariantCulture;
            if (searchUp)
            {
                for (var i = SelectedIndex - 1; i >= 0; --i)
                {
                    if (ContainsText(_list.Items[i], text, comparisonType))
                    {
                        return _list.Items[i];
                    }
                }
            }
            else
            {
                for (var i = SelectedIndex + 1; i < _list.Items.Count; i++)
                {
                    if (ContainsText(_list.Items[i], text, comparisonType))
                    {
                        return _list.Items[i];
                    }
                }
            }
            return null;
        }

        private static bool ContainsText(ListViewItem item, string text, StringComparison comparisonType)
        {
            if (item.Text.IndexOf(text, comparisonType) != -1)
            {
                return true;
            }
            foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
            {
                if (subItem.Text.IndexOf(text, comparisonType) != -1)
                {
                    return true;
                }
            }
            return false;
        }

        private int SelectedIndex
        {
            get
            {
                var sic = _list.SelectedIndices;
                return sic.Count > 0 ? sic[0] : -1;
            }
        }

        private void List_Select(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();

            if (SelectedIndex != -1)
            {
                var packet = m_packetViewer.Packets[SelectedIndex];

                textBox1.Text = packet.HexLike();
                textBox2.Text = m_packetViewer.ShowParsed(packet);
            }
        }

        private void OpenMenu_Click(object sender, EventArgs e)
        {
            if (_openDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            textBox1.Clear();
            textBox2.Clear();
            _list.Items.Clear();

            _statusLabel.Text = "Loading...";
            var file = _openDialog.FileName;
            m_packetViewer = PacketViewerBase.Create(Path.GetExtension(file));
            if (m_packetViewer != null)
            {
                m_packetViewer.LoadData(file);
                //_statusLabel.Text = String.Format("Client Build: {0}", m_packetViewer.Build);
                _backgroundWorker.RunWorkerAsync(m_packetViewer.Packets.Count);
            }
        }

        private void SaveMenu_Click(object sender, EventArgs e)
        {
            if (m_packetViewer == null)
            {
                MessageBox.Show("You should load something first!");
                return;
            }

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
            _statusLabel.Text = String.Format("Done. Client Build: {0}", m_packetViewer.Build);
        }

        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                if (m_searchForm == null || m_searchForm.IsDisposed)
                {
                    m_searchForm = new FrmSearch();
                    m_searchForm.Show(this);
                }
                else
                {
                    m_searchForm.FindNext();
                }
            }
        }

        private void saveAsParsedTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_packetViewer == null)
            {
                MessageBox.Show("You should load something first!");
                return;
            }

            if (_saveDialog.ShowDialog() == DialogResult.OK)
            {
                using (var stream = new StreamWriter(_saveDialog.OpenFile()))
                {
                    foreach (var p in m_packetViewer.Packets)
                    {
                        string parsed = Parser.CreateParser(p).Parse();
                        if(String.IsNullOrEmpty(parsed))
                            continue;
                        stream.Write(parsed);
                    }
                }
            }
        }

        private void _saveDialog_FileOk(object sender, CancelEventArgs e)
        {
            //MessageBox.Show("file ok?");
        }

        private void saveWardenAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_packetViewer == null)
            {
                MessageBox.Show("You should load something first!");
                return;
            }

            _saveDialog.FileName = Path.GetFileName(_openDialog.FileName).Replace("bin", "txt");

            if (_saveDialog.ShowDialog() == DialogResult.OK)
            {
                using (var stream = new StreamWriter(_saveDialog.OpenFile()))
                {
                    foreach (var p in m_packetViewer.Packets)
                    {
                        if (p.Code != OpCodes.CMSG_WARDEN_DATA && p.Code != OpCodes.SMSG_WARDEN_DATA)
                            continue;
                        stream.Write(p.HexLike());

                        string parsed = Parser.CreateParser(p).Parse();
                        if (String.IsNullOrEmpty(parsed))
                            continue;
                        stream.Write(parsed);
                    }
                }
            }
        }
    }
}
