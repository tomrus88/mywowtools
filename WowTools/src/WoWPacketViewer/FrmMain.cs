using System;
using System.IO;
using System.Windows.Forms;
using WowTools.Core;

namespace WoWPacketViewer
{
    public partial class FrmMain : Form
    {
        private FrmSearch searchForm;

        private FrmView SelectedView
        {
            get { return (FrmView)ActiveMdiChild; }
        }

        public FrmMain()
        {
            InitializeComponent();
        }

        private void OpenMenu_Click(object sender, EventArgs e)
        {
            if (_openDialog.ShowDialog() != DialogResult.OK)
                return;

            _statusLabel.Text = "Loading...";
            var file = _openDialog.FileName;

            FrmView view = new FrmView();
            view.File = file;
            view.Text = Path.GetFileName(file);
            view.MdiParent = this;
            view.Show();

            _statusLabel.Text = String.Format("Done.");
        }

        private void SaveMenu_Click(object sender, EventArgs e)
        {
            if (SelectedView == null)
                return;

            if (!SelectedView.Loaded)
            {
                MessageBox.Show("You should load something first!");
                return;
            }

            _saveDialog.FileName = Path.GetFileName(_openDialog.FileName).Replace("bin", "txt");

            if (_saveDialog.ShowDialog() != DialogResult.OK)
                return;

            using (var stream = new StreamWriter(_saveDialog.OpenFile()))
            {
                foreach (var p in SelectedView.Packets)
                {
                    stream.Write(p.HexLike());
                }
            }
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void FindMenu_Click(object sender, EventArgs e)
        {
            if (SelectedView == null)
                return;

            CreateSearchFormIfNeed();

            if (!searchForm.Visible)
                searchForm.Show();
        }

        private bool CreateSearchFormIfNeed()
        {
            if (searchForm == null || searchForm.IsDisposed)
            {
                searchForm = new FrmSearch();
                searchForm.Owner = SelectedView;
                searchForm.Show();
                return true;
            }
            return false;
        }

        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (SelectedView == null)
                return;

            if (e.KeyCode != Keys.F3)
                return;

            if (!CreateSearchFormIfNeed())
                searchForm.FindNext();
        }

        private void saveAsParsedTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedView == null)
                return;

            if (!SelectedView.Loaded)
            {
                MessageBox.Show("You should load something first!");
                return;
            }

            if (_saveDialog.ShowDialog() != DialogResult.OK)
                return;

            using (var stream = new StreamWriter(_saveDialog.OpenFile()))
            {
                foreach (var p in SelectedView.Packets)
                {
                    string parsed = ParserFactory.CreateParser(p).ToString();
                    if (String.IsNullOrEmpty(parsed))
                        continue;
                    stream.Write(parsed);
                }
            }
        }

        private void saveWardenAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedView == null)
                return;

            if (!SelectedView.Loaded)
            {
                MessageBox.Show("You should load something first!");
                return;
            }

            _saveDialog.FileName = Path.GetFileName(_openDialog.FileName).Replace("bin", "txt");

            if (_saveDialog.ShowDialog() != DialogResult.OK)
                return;

            using (var stream = new StreamWriter(_saveDialog.OpenFile()))
            {
                foreach (var p in SelectedView.Packets)
                {
                    if (p.Code != OpCodes.CMSG_WARDEN_DATA && p.Code != OpCodes.SMSG_WARDEN_DATA)
                        continue;
                    //stream.Write(Utility.HexLike(p));

                    var parsed = ParserFactory.CreateParser(p).ToString();
                    if (String.IsNullOrEmpty(parsed))
                        continue;
                    stream.Write(parsed);
                }
            }
        }

        private void reloadDefinitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParserFactory.ReInit();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            ParserFactory.Init();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Tag != null)
                (tabControl1.SelectedTab.Tag as Form).Select();
        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            for (int i = 0; i < tabControl1.TabCount; i++)
            {
                var r = tabControl1.GetTabRect(i);
                if (r.Contains(e.Location))
                {
                    closeTabToolStripMenuItem.Tag = tabControl1.TabPages[i].Tag;
                    closeAllButThisToolStripMenuItem.Tag = tabControl1.TabPages[i];
                    contextMenuStrip1.Show(tabControl1, e.Location);
                    break;
                }
            }
        }

        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((FrmView)((ToolStripMenuItem)sender).Tag).Close();
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            while (tabControl1.HasChildren)
            {
                ((FrmView)tabControl1.TabPages[0].Tag).Close();
            }
        }

        private void closeAllButThisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var thisTab = ((ToolStripMenuItem)sender).Tag;

            int index = 0;
            while (tabControl1.TabPages.Count != 1)
            {
                if (tabControl1.TabPages[index] == thisTab)
                {
                    index++;
                    continue;
                }

                ((FrmView)tabControl1.TabPages[index].Tag).Close();
            }
        }

        private void FrmMain_MdiChildActivate(object sender, EventArgs e)
        {
            var child = (FrmView)ActiveMdiChild;

            if (child == null)
            {
                tabControl1.Visible = false;
            }
            else
            {
                ActiveMdiChild.Width = 0;
                ActiveMdiChild.WindowState = FormWindowState.Maximized;

                if (ActiveMdiChild.Tag == null)
                {
                    TabPage tp = new TabPage();
                    tp.Parent = tabControl1;
                    tp.Text = ActiveMdiChild.Text;
                    tp.Tag = ActiveMdiChild;
                    tp.Show();

                    ActiveMdiChild.Tag = tp;
                    ActiveMdiChild.FormClosing += new FormClosingEventHandler(ActiveMdiChild_FormClosing);
                }

                tabControl1.SelectedTab = (ActiveMdiChild.Tag as TabPage);

                if (!tabControl1.Visible)
                    tabControl1.Visible = true;

                if (searchForm != null)
                    searchForm.Owner = ActiveMdiChild;
            }
        }

        void ActiveMdiChild_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((sender as Form).Tag as TabPage).Dispose();
        }
    }
}
