namespace tcp_log_parser
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.up = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cfg = new System.Windows.Forms.CheckBox();
            this.chat = new System.Windows.Forms.CheckBox();
            this.q = new System.Windows.Forms.CheckBox();
            this.ping = new System.Windows.Forms.CheckBox();
            this.wd = new System.Windows.Forms.CheckBox();
            this.mm = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Parse";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // up
            // 
            this.up.AutoSize = true;
            this.up.Checked = true;
            this.up.CheckState = System.Windows.Forms.CheckState.Checked;
            this.up.Location = new System.Drawing.Point(6, 19);
            this.up.Name = "up";
            this.up.Size = new System.Drawing.Size(103, 17);
            this.up.TabIndex = 1;
            this.up.Text = "Update Packets";
            this.up.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cfg);
            this.groupBox1.Controls.Add(this.chat);
            this.groupBox1.Controls.Add(this.q);
            this.groupBox1.Controls.Add(this.ping);
            this.groupBox1.Controls.Add(this.wd);
            this.groupBox1.Controls.Add(this.mm);
            this.groupBox1.Controls.Add(this.up);
            this.groupBox1.Location = new System.Drawing.Point(12, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(174, 183);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filters";
            // 
            // cfg
            // 
            this.cfg.AutoSize = true;
            this.cfg.Location = new System.Drawing.Point(6, 157);
            this.cfg.Name = "cfg";
            this.cfg.Size = new System.Drawing.Size(77, 17);
            this.cfg.TabIndex = 3;
            this.cfg.Text = "Use config";
            this.cfg.UseVisualStyleBackColor = true;
            // 
            // chat
            // 
            this.chat.AutoSize = true;
            this.chat.Checked = true;
            this.chat.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chat.Location = new System.Drawing.Point(6, 134);
            this.chat.Name = "chat";
            this.chat.Size = new System.Drawing.Size(48, 17);
            this.chat.TabIndex = 3;
            this.chat.Text = "Chat";
            this.chat.UseVisualStyleBackColor = true;
            // 
            // q
            // 
            this.q.AutoSize = true;
            this.q.Checked = true;
            this.q.CheckState = System.Windows.Forms.CheckState.Checked;
            this.q.Location = new System.Drawing.Point(6, 111);
            this.q.Name = "q";
            this.q.Size = new System.Drawing.Size(62, 17);
            this.q.TabIndex = 3;
            this.q.Text = "Queries";
            this.q.UseVisualStyleBackColor = true;
            // 
            // ping
            // 
            this.ping.AutoSize = true;
            this.ping.Checked = true;
            this.ping.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ping.Location = new System.Drawing.Point(6, 88);
            this.ping.Name = "ping";
            this.ping.Size = new System.Drawing.Size(47, 17);
            this.ping.TabIndex = 3;
            this.ping.Text = "Ping";
            this.ping.UseVisualStyleBackColor = true;
            // 
            // wd
            // 
            this.wd.AutoSize = true;
            this.wd.Checked = true;
            this.wd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.wd.Location = new System.Drawing.Point(6, 65);
            this.wd.Name = "wd";
            this.wd.Size = new System.Drawing.Size(90, 17);
            this.wd.TabIndex = 3;
            this.wd.Text = "Warden Data";
            this.wd.UseVisualStyleBackColor = true;
            // 
            // mm
            // 
            this.mm.AutoSize = true;
            this.mm.Checked = true;
            this.mm.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mm.Location = new System.Drawing.Point(6, 42);
            this.mm.Name = "mm";
            this.mm.Size = new System.Drawing.Size(99, 17);
            this.mm.TabIndex = 2;
            this.mm.Text = "Monster Moves";
            this.mm.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(198, 236);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "tcp.log parser";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox up;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox wd;
        private System.Windows.Forms.CheckBox mm;
        private System.Windows.Forms.CheckBox ping;
        private System.Windows.Forms.CheckBox q;
        private System.Windows.Forms.CheckBox chat;
        private System.Windows.Forms.CheckBox cfg;
    }
}

