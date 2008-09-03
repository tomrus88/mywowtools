namespace CharacterConverter.Gui
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
			  this.label1 = new System.Windows.Forms.Label();
			  this.label2 = new System.Windows.Forms.Label();
			  this.label3 = new System.Windows.Forms.Label();
			  this.label4 = new System.Windows.Forms.Label();
			  this.label5 = new System.Windows.Forms.Label();
			  this.button1 = new System.Windows.Forms.Button();
			  this.textBox1 = new System.Windows.Forms.TextBox();
			  this.textBox2 = new System.Windows.Forms.TextBox();
			  this.textBox3 = new System.Windows.Forms.TextBox();
			  this.textBox4 = new System.Windows.Forms.TextBox();
			  this.textBox5 = new System.Windows.Forms.TextBox();
			  this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			  this.SuspendLayout();
			  // 
			  // label1
			  // 
			  this.label1.AutoSize = true;
			  this.label1.Location = new System.Drawing.Point(1, 5);
			  this.label1.Name = "label1";
			  this.label1.Size = new System.Drawing.Size(32, 13);
			  this.label1.TabIndex = 0;
			  this.label1.Text = "Host:";
			  // 
			  // label2
			  // 
			  this.label2.AutoSize = true;
			  this.label2.Location = new System.Drawing.Point(1, 31);
			  this.label2.Name = "label2";
			  this.label2.Size = new System.Drawing.Size(29, 13);
			  this.label2.TabIndex = 1;
			  this.label2.Text = "Port:";
			  // 
			  // label3
			  // 
			  this.label3.AutoSize = true;
			  this.label3.Location = new System.Drawing.Point(1, 56);
			  this.label3.Name = "label3";
			  this.label3.Size = new System.Drawing.Size(56, 13);
			  this.label3.TabIndex = 2;
			  this.label3.Text = "Database:";
			  // 
			  // label4
			  // 
			  this.label4.AutoSize = true;
			  this.label4.Location = new System.Drawing.Point(1, 82);
			  this.label4.Name = "label4";
			  this.label4.Size = new System.Drawing.Size(36, 13);
			  this.label4.TabIndex = 3;
			  this.label4.Text = "Login:";
			  // 
			  // label5
			  // 
			  this.label5.AutoSize = true;
			  this.label5.Location = new System.Drawing.Point(1, 108);
			  this.label5.Name = "label5";
			  this.label5.Size = new System.Drawing.Size(56, 13);
			  this.label5.TabIndex = 4;
			  this.label5.Text = "Password:";
			  // 
			  // button1
			  // 
			  this.button1.Location = new System.Drawing.Point(4, 131);
			  this.button1.Name = "button1";
			  this.button1.Size = new System.Drawing.Size(159, 23);
			  this.button1.TabIndex = 5;
			  this.button1.Text = "Go!";
			  this.button1.UseVisualStyleBackColor = true;
			  this.button1.Click += new System.EventHandler(this.button1_Click);
			  // 
			  // textBox1
			  // 
			  this.textBox1.Location = new System.Drawing.Point(63, 2);
			  this.textBox1.Name = "textBox1";
			  this.textBox1.Size = new System.Drawing.Size(100, 20);
			  this.textBox1.TabIndex = 6;
			  this.textBox1.Text = "localhost";
			  // 
			  // textBox2
			  // 
			  this.textBox2.Location = new System.Drawing.Point(63, 28);
			  this.textBox2.Name = "textBox2";
			  this.textBox2.Size = new System.Drawing.Size(100, 20);
			  this.textBox2.TabIndex = 7;
			  this.textBox2.Text = "3306";
			  // 
			  // textBox3
			  // 
			  this.textBox3.Location = new System.Drawing.Point(63, 54);
			  this.textBox3.Name = "textBox3";
			  this.textBox3.Size = new System.Drawing.Size(100, 20);
			  this.textBox3.TabIndex = 8;
			  // 
			  // textBox4
			  // 
			  this.textBox4.Location = new System.Drawing.Point(63, 79);
			  this.textBox4.Name = "textBox4";
			  this.textBox4.Size = new System.Drawing.Size(100, 20);
			  this.textBox4.TabIndex = 9;
			  // 
			  // textBox5
			  // 
			  this.textBox5.Location = new System.Drawing.Point(63, 105);
			  this.textBox5.Name = "textBox5";
			  this.textBox5.PasswordChar = '*';
			  this.textBox5.Size = new System.Drawing.Size(100, 20);
			  this.textBox5.TabIndex = 10;
			  // 
			  // FrmMain
			  // 
			  this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			  this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			  this.ClientSize = new System.Drawing.Size(168, 154);
			  this.Controls.Add(this.textBox5);
			  this.Controls.Add(this.textBox4);
			  this.Controls.Add(this.textBox3);
			  this.Controls.Add(this.textBox2);
			  this.Controls.Add(this.textBox1);
			  this.Controls.Add(this.button1);
			  this.Controls.Add(this.label5);
			  this.Controls.Add(this.label4);
			  this.Controls.Add(this.label3);
			  this.Controls.Add(this.label2);
			  this.Controls.Add(this.label1);
			  this.Name = "FrmMain";
			  this.Text = "Converter";
			  this.ResumeLayout(false);
			  this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}

