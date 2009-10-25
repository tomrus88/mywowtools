using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WowTools.Core;

namespace WoWPacketViewer
{
    public partial class FrmMain : Form, ISupportFind
    {
        private IPacketReader packetViewer;
        private FrmSearch searchForm;
        private List<Packet> packets;

        public FrmMain()
        {
            InitializeComponent();
        }

        private int SelectedIndex
        {
            get
            {
                ListView.SelectedIndexCollection sic = _list.SelectedIndices;
                return sic.Count > 0 ? sic[0] : -1;
            }
        }

        #region ISupportFind Members

        public void Search(string text, bool searchUp, bool ignoreCase)
        {
            ListViewItem item = FindItem(text, searchUp, ignoreCase);
            if (item != null)
            {
                item.Selected = true;
                item.EnsureVisible();
            }
            else
            {
                MessageBox.Show(string.Format("Can't find:'{0}'", text),
                                "Packet Viewer",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
        }

        #endregion

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
            Packet pkt = (from packet in packets
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

            int i = 0;
            UpdateControl(false);
            foreach (Packet p in packets)
            {
                AddPacket(p.Direction == Direction.Client
                    ? new ListViewItem(new[]
                    {
                        p.UnixTime.ToString("X8"), 
                        p.TicksCount.ToString("X8"), 
                        p.Code.ToString(),
                        String.Empty,
                        p.Data.Length.ToString()
                    })
                    : new ListViewItem(new[]
                    {
                        p.UnixTime.ToString("X8"), 
                        p.TicksCount.ToString("X8"), 
                        String.Empty,
                        p.Code.ToString(), 
                        p.Data.Length.ToString()
                    }));
                ++i;
                worker.ReportProgress((int)((i / (float)packets.Count) * 100f));
            }
            UpdateControl(true);
        }

        private ListViewItem FindItem(string text, bool searchUp, bool ignoreCase)
        {
            StringComparison comparisonType = ignoreCase
                                                ? StringComparison.InvariantCultureIgnoreCase
                                                : StringComparison.InvariantCulture;
            if (searchUp)
            {
                for (int i = SelectedIndex - 1; i >= 0; --i)
                {
                    if (ContainsText(_list.Items[i], text, comparisonType))
                    {
                        return _list.Items[i];
                    }
                }
            }
            else
            {
                for (int i = SelectedIndex + 1; i < _list.Items.Count; i++)
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

        private void List_Select(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();

            if (SelectedIndex == -1) return;

            Packet packet = packets[SelectedIndex];

            textBox1.Text = Utility.HexLike(packet);
            textBox2.Text = ParserFactory.CreateParser(packet).Parse();
        }

        private void OpenMenu_Click(object sender, EventArgs e)
        {
            if (_openDialog.ShowDialog() != DialogResult.OK) return;

            textBox1.Clear();
            textBox2.Clear();
            _list.Items.Clear();

            _statusLabel.Text = "Loading...";
            string file = _openDialog.FileName;
            packetViewer = PacketReaderFactory.Create(Path.GetExtension(file));

            if (!Loaded()) return;

            packets = packetViewer.ReadPackets(file).ToList();

            _backgroundWorker.RunWorkerAsync(packets.Count);
        }

        private bool Loaded()
        {
            return packetViewer != null;
        }

        private void SaveMenu_Click(object sender, EventArgs e)
        {
            if (!Loaded())
            {
                MessageBox.Show("You should load something first!");
                return;
            }

            if (_saveDialog.ShowDialog() != DialogResult.OK) return;

            using (var stream = new StreamWriter(_saveDialog.OpenFile()))
            {
                foreach (Packet p in packets)
                {
                    stream.Write(Utility.HexLike(p));
                }
            }
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void FindMenu_Click(object sender, EventArgs e)
        {
            if (searchForm == null || searchForm.IsDisposed)
            {
                searchForm = new FrmSearch();
            }

            if (!searchForm.Visible)
            {
                searchForm.Show(this);
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
            _statusLabel.Text = String.Format("Done. Client Build: {0}", packetViewer.Build);
        }

        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F3) return;

            if (searchForm == null || searchForm.IsDisposed)
            {
                searchForm = new FrmSearch();
                searchForm.Show(this);
            }
            else
            {
                searchForm.FindNext();
            }
        }

        private void saveAsParsedTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Loaded())
            {
                MessageBox.Show("You should load something first!");
                return;
            }

            if (_saveDialog.ShowDialog() != DialogResult.OK) return;

            using (var stream = new StreamWriter(_saveDialog.OpenFile()))
            {
                foreach (Packet p in packets)
                {
                    string parsed = ParserFactory.CreateParser(p).Parse();
                    if (String.IsNullOrEmpty(parsed))
                        continue;
                    stream.Write(parsed);
                }
            }
        }

        private void saveWardenAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Loaded())
            {
                MessageBox.Show("You should load something first!");
                return;
            }

            _saveDialog.FileName = Path.GetFileName(_openDialog.FileName).Replace("bin", "txt");

            if (_saveDialog.ShowDialog() != DialogResult.OK) return;

            using (var stream = new StreamWriter(_saveDialog.OpenFile()))
            {
                foreach (Packet p in packets)
                {
                    if (p.Code != OpCodes.CMSG_WARDEN_DATA && p.Code != OpCodes.SMSG_WARDEN_DATA)
                        continue;
                    stream.Write(Utility.HexLike(p));

                    string parsed = ParserFactory.CreateParser(p).Parse();
                    if (String.IsNullOrEmpty(parsed))
                        continue;
                    stream.Write(parsed);
                }
            }
        }

        #region Nested type: AddListViewItem

        private delegate void AddListViewItem(ListViewItem item);

        #endregion

        #region Nested type: ListViewUpdateControl

        private delegate void ListViewUpdateControl(bool end);

        #endregion
    }
}
