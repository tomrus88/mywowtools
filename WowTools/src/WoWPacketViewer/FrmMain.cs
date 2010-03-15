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
        private ListViewItem[] myCache;                     // array to cache items for the virtual list
        private int firstItem;                              // stores the index of the first item in the cache
        private bool searchUp;
        private bool ignoreCase;

        public FrmMain()
        {
            InitializeComponent();
        }

        private int SelectedIndex
        {
            get
            {
                var sic = _list.SelectedIndices;
                return sic.Count > 0 ? sic[0] : 0;
            }
        }

        #region ISupportFind Members

        public void Search(string text, bool searchUp, bool ignoreCase)
        {
            if (!Loaded())
                return;

            this.searchUp = searchUp;
            this.ignoreCase = ignoreCase;

            var item = _list.FindItemWithText(text, true, SelectedIndex, true);
            if (item != null)
            {
                _list.EnsureVisible(item.Index);
                _list.SelectedIndices.Clear();
                _list.SelectedIndices.Add(item.Index);
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

        private void List_Select(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();

            var packet = packets[SelectedIndex];

            textBox1.Text = Utility.HexLike(packet);
            var parser = ParserFactory.CreateParser(packet);
            textBox2.Text = parser.Parse();
        }

        private void OpenMenu_Click(object sender, EventArgs e)
        {
            if (_openDialog.ShowDialog() != DialogResult.OK)
                return;

            textBox1.Clear();
            textBox2.Clear();
            _list.Items.Clear();

            _statusLabel.Text = "Loading...";
            var file = _openDialog.FileName;
            packetViewer = PacketReaderFactory.Create(Path.GetExtension(file));

            if (!Loaded())
                return;

            packets = packetViewer.ReadPackets(file).ToList();

            _list.VirtualMode = true;
            _list.VirtualListSize = packets.Count;

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

            if (_saveDialog.ShowDialog() != DialogResult.OK)
                return;

            using (var stream = new StreamWriter(_saveDialog.OpenFile()))
            {
                foreach (var p in packets)
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
                searchForm = new FrmSearch();

            if (!searchForm.Visible)
                searchForm.Show(this);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage < 100)
                _progressBar.Value = e.ProgressPercentage;
            else
                _progressBar.Visible = false;
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //FillListView(sender as BackgroundWorker);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _statusLabel.Text = String.Format("Done. Client Build: {0}", packetViewer.Build);
        }

        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F3)
                return;

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

            if (_saveDialog.ShowDialog() != DialogResult.OK)
                return;

            using (var stream = new StreamWriter(_saveDialog.OpenFile()))
            {
                foreach (var p in packets)
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

            if (_saveDialog.ShowDialog() != DialogResult.OK)
                return;

            using (var stream = new StreamWriter(_saveDialog.OpenFile()))
            {
                foreach (var p in packets)
                {
                    if (p.Code != OpCodes.CMSG_WARDEN_DATA && p.Code != OpCodes.SMSG_WARDEN_DATA)
                        continue;
                    //stream.Write(Utility.HexLike(p));

                    var parsed = ParserFactory.CreateParser(p).Parse();
                    if (String.IsNullOrEmpty(parsed))
                        continue;
                    stream.Write(parsed);
                }
            }
        }

        private void _list_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            // check to see if the requested item is currently in the cache
            if (myCache != null && e.ItemIndex >= firstItem && e.ItemIndex < firstItem + myCache.Length)
            {
                // A cache hit, so get the ListViewItem from the cache instead of making a new one.
                e.Item = myCache[e.ItemIndex - firstItem];
            }
            else
            {
                // A cache miss, so create a new ListViewItem and pass it back.
                var p = packets[e.ItemIndex];

                e.Item = p.Direction == Direction.Client
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
                        });
            }
        }

        private void _list_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
        {
            var comparisonType = ignoreCase
                                    ? StringComparison.InvariantCultureIgnoreCase
                                    : StringComparison.InvariantCulture;

            if (searchUp)
            {
                for (var i = SelectedIndex - 1; i >= 0; --i)
                {
                    var op = packets[i].Code.ToString();
                    if (op.IndexOf(e.Text, comparisonType) != -1)
                    {
                        e.Index = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = SelectedIndex + 1; i < _list.Items.Count; ++i)
                {
                    var op = packets[i].Code.ToString();
                    if (op.IndexOf(e.Text, comparisonType) != -1)
                    {
                        e.Index = i;
                        break;
                    }
                }
            }
        }

        private void _list_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            // We've gotten a request to refresh the cache.
            // First check if it's really necessary.
            if (myCache != null && e.StartIndex >= firstItem && e.EndIndex <= firstItem + myCache.Length)
            {
                // If the newly requested cache is a subset of the old cache, 
                // no need to rebuild everything, so do nothing.
                return;
            }

            // Now we need to rebuild the cache.
            firstItem = e.StartIndex;
            int length = e.EndIndex - e.StartIndex + 1; // indexes are inclusive
            myCache = new ListViewItem[length];

            // Fill the cache with the appropriate ListViewItems.
            for (int i = 0; i < length; i++)
            {
                var p = packets[e.StartIndex + i];

                myCache[i] = p.Direction == Direction.Client
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
                        });
            }
        }
    }
}
