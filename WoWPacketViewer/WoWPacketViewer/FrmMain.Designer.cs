namespace WoWPacketViewer
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			  this._list = new System.Windows.Forms.ListView();
			  this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			  this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			  this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			  this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			  this.textBox1 = new System.Windows.Forms.TextBox();
			  this.textBox2 = new System.Windows.Forms.TextBox();
			  this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			  this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			  this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			  this.saveAsTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			  this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			  this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			  this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			  this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			  this._statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			  this._progressBar = new System.Windows.Forms.ToolStripProgressBar();
			  this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			  this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			  this._openDialog = new System.Windows.Forms.OpenFileDialog();
			  this._saveDialog = new System.Windows.Forms.SaveFileDialog();
			  this._backgroundWorker = new System.ComponentModel.BackgroundWorker();
			  this.menuStrip1.SuspendLayout();
			  this.statusStrip1.SuspendLayout();
			  this.splitContainer1.Panel1.SuspendLayout();
			  this.splitContainer1.Panel2.SuspendLayout();
			  this.splitContainer1.SuspendLayout();
			  this.splitContainer2.Panel1.SuspendLayout();
			  this.splitContainer2.Panel2.SuspendLayout();
			  this.splitContainer2.SuspendLayout();
			  this.SuspendLayout();
			  // 
			  // _list
			  // 
			  this._list.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							  | System.Windows.Forms.AnchorStyles.Left)
							  | System.Windows.Forms.AnchorStyles.Right)));
			  this._list.BackColor = System.Drawing.SystemColors.WindowText;
			  this._list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
			  this._list.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			  this._list.ForeColor = System.Drawing.Color.MediumSeaGreen;
			  this._list.FullRowSelect = true;
			  this._list.GridLines = true;
			  this._list.HideSelection = false;
			  this._list.Location = new System.Drawing.Point(3, 3);
			  this._list.MultiSelect = false;
			  this._list.Name = "_list";
			  this._list.Size = new System.Drawing.Size(872, 195);
			  this._list.TabIndex = 0;
			  this._list.UseCompatibleStateImageBehavior = false;
			  this._list.View = System.Windows.Forms.View.Details;
			  this._list.SelectedIndexChanged += new System.EventHandler(this.List_Select);
			  // 
			  // columnHeader1
			  // 
			  this.columnHeader1.Text = "Build";
			  this.columnHeader1.Width = 65;
			  // 
			  // columnHeader2
			  // 
			  this.columnHeader2.Text = "Client Opcode";
			  this.columnHeader2.Width = 350;
			  // 
			  // columnHeader3
			  // 
			  this.columnHeader3.Text = "Server Opcode";
			  this.columnHeader3.Width = 350;
			  // 
			  // columnHeader4
			  // 
			  this.columnHeader4.Text = "Packet Size";
			  this.columnHeader4.Width = 100;
			  // 
			  // textBox1
			  // 
			  this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							  | System.Windows.Forms.AnchorStyles.Left)
							  | System.Windows.Forms.AnchorStyles.Right)));
			  this.textBox1.BackColor = System.Drawing.SystemColors.WindowText;
			  this.textBox1.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			  this.textBox1.ForeColor = System.Drawing.SystemColors.Window;
			  this.textBox1.Location = new System.Drawing.Point(3, 0);
			  this.textBox1.Multiline = true;
			  this.textBox1.Name = "textBox1";
			  this.textBox1.ReadOnly = true;
			  this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			  this.textBox1.Size = new System.Drawing.Size(481, 169);
			  this.textBox1.TabIndex = 1;
			  // 
			  // textBox2
			  // 
			  this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							  | System.Windows.Forms.AnchorStyles.Left)
							  | System.Windows.Forms.AnchorStyles.Right)));
			  this.textBox2.BackColor = System.Drawing.SystemColors.WindowText;
			  this.textBox2.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			  this.textBox2.ForeColor = System.Drawing.SystemColors.Window;
			  this.textBox2.Location = new System.Drawing.Point(3, 0);
			  this.textBox2.Multiline = true;
			  this.textBox2.Name = "textBox2";
			  this.textBox2.ReadOnly = true;
			  this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			  this.textBox2.Size = new System.Drawing.Size(381, 169);
			  this.textBox2.TabIndex = 2;
			  // 
			  // menuStrip1
			  // 
			  this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
			  this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			  this.menuStrip1.Name = "menuStrip1";
			  this.menuStrip1.Size = new System.Drawing.Size(878, 24);
			  this.menuStrip1.TabIndex = 3;
			  this.menuStrip1.Text = "menuStrip1";
			  // 
			  // fileToolStripMenuItem
			  // 
			  this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveAsTextToolStripMenuItem,
            this.exitToolStripMenuItem});
			  this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			  this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			  this.fileToolStripMenuItem.Text = "File";
			  // 
			  // openToolStripMenuItem
			  // 
			  this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			  this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			  this.openToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
			  this.openToolStripMenuItem.Text = "&Open...";
			  this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenMenu_Click);
			  // 
			  // saveAsTextToolStripMenuItem
			  // 
			  this.saveAsTextToolStripMenuItem.Name = "saveAsTextToolStripMenuItem";
			  this.saveAsTextToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			  this.saveAsTextToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
			  this.saveAsTextToolStripMenuItem.Text = "&Save As Text...";
			  this.saveAsTextToolStripMenuItem.Click += new System.EventHandler(this.SaveMenu_Click);
			  // 
			  // exitToolStripMenuItem
			  // 
			  this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			  this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			  this.exitToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
			  this.exitToolStripMenuItem.Text = "E&xit";
			  this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitMenu_Click);
			  // 
			  // editToolStripMenuItem
			  // 
			  this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findToolStripMenuItem});
			  this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			  this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			  this.editToolStripMenuItem.Text = "Edit";
			  // 
			  // findToolStripMenuItem
			  // 
			  this.findToolStripMenuItem.Name = "findToolStripMenuItem";
			  this.findToolStripMenuItem.ShortcutKeyDisplayString = "";
			  this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F3)));
			  this.findToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			  this.findToolStripMenuItem.Text = "Find...";
			  this.findToolStripMenuItem.Click += new System.EventHandler(this.FindMenu_Click);
			  // 
			  // statusStrip1
			  // 
			  this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._statusLabel,
            this._progressBar});
			  this.statusStrip1.Location = new System.Drawing.Point(0, 406);
			  this.statusStrip1.Name = "statusStrip1";
			  this.statusStrip1.Size = new System.Drawing.Size(878, 22);
			  this.statusStrip1.TabIndex = 4;
			  this.statusStrip1.Text = "statusStrip1";
			  // 
			  // _statusLabel
			  // 
			  this._statusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			  this._statusLabel.Name = "_statusLabel";
			  this._statusLabel.Size = new System.Drawing.Size(39, 17);
			  this._statusLabel.Text = "Ready";
			  // 
			  // _progressBar
			  // 
			  this._progressBar.Name = "_progressBar";
			  this._progressBar.Size = new System.Drawing.Size(100, 16);
			  // 
			  // splitContainer1
			  // 
			  this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							  | System.Windows.Forms.AnchorStyles.Left)
							  | System.Windows.Forms.AnchorStyles.Right)));
			  this.splitContainer1.Location = new System.Drawing.Point(0, 27);
			  this.splitContainer1.Name = "splitContainer1";
			  this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			  // 
			  // splitContainer1.Panel1
			  // 
			  this.splitContainer1.Panel1.Controls.Add(this._list);
			  this.splitContainer1.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			  // 
			  // splitContainer1.Panel2
			  // 
			  this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			  this.splitContainer1.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			  this.splitContainer1.Size = new System.Drawing.Size(878, 376);
			  this.splitContainer1.SplitterDistance = 197;
			  this.splitContainer1.TabIndex = 5;
			  // 
			  // splitContainer2
			  // 
			  this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							  | System.Windows.Forms.AnchorStyles.Left)
							  | System.Windows.Forms.AnchorStyles.Right)));
			  this.splitContainer2.Location = new System.Drawing.Point(0, 3);
			  this.splitContainer2.Name = "splitContainer2";
			  // 
			  // splitContainer2.Panel1
			  // 
			  this.splitContainer2.Panel1.Controls.Add(this.textBox1);
			  // 
			  // splitContainer2.Panel2
			  // 
			  this.splitContainer2.Panel2.Controls.Add(this.textBox2);
			  this.splitContainer2.Size = new System.Drawing.Size(878, 172);
			  this.splitContainer2.SplitterDistance = 487;
			  this.splitContainer2.TabIndex = 3;
			  // 
			  // _openDialog
			  // 
			  this._openDialog.Filter = "WoWBinary Files|*.bin|SQLite Files|*.sqlite|Sniffitzt XML Files|*.xml";
			  // 
			  // _backgroundWorker
			  // 
			  this._backgroundWorker.WorkerReportsProgress = true;
			  this._backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
			  this._backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
			  this._backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
			  // 
			  // FrmMain
			  // 
			  this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			  this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			  this.ClientSize = new System.Drawing.Size(878, 428);
			  this.Controls.Add(this.splitContainer1);
			  this.Controls.Add(this.statusStrip1);
			  this.Controls.Add(this.menuStrip1);
			  this.MainMenuStrip = this.menuStrip1;
			  this.Name = "FrmMain";
			  this.Text = "Packet Viewer";
			  this.menuStrip1.ResumeLayout(false);
			  this.menuStrip1.PerformLayout();
			  this.statusStrip1.ResumeLayout(false);
			  this.statusStrip1.PerformLayout();
			  this.splitContainer1.Panel1.ResumeLayout(false);
			  this.splitContainer1.Panel2.ResumeLayout(false);
			  this.splitContainer1.ResumeLayout(false);
			  this.splitContainer2.Panel1.ResumeLayout(false);
			  this.splitContainer2.Panel1.PerformLayout();
			  this.splitContainer2.Panel2.ResumeLayout(false);
			  this.splitContainer2.Panel2.PerformLayout();
			  this.splitContainer2.ResumeLayout(false);
			  this.ResumeLayout(false);
			  this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog _openDialog;
        private System.Windows.Forms.ToolStripStatusLabel _statusLabel;
        private System.Windows.Forms.ToolStripProgressBar _progressBar;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsTextToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog _saveDialog;
        public System.Windows.Forms.ListView _list;
        private System.ComponentModel.BackgroundWorker _backgroundWorker;
    }
}
