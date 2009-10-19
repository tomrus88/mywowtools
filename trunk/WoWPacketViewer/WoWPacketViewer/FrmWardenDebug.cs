using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WoWPacketViewer.Parsers;

namespace WoWPacketViewer
{
    public partial class FrmWardenDebug : Form
    {
        public FrmWardenDebug()
        {
            InitializeComponent();
        }

        public TextBox[] GetTextBoxes()
        {
            List<TextBox> temp = new List<TextBox>();
            foreach (var control in Controls)
            {
                if (control is TextBox)
                    temp.Add(control as TextBox);
            }

            return temp.OrderBy(t => t.TabIndex).ToArray();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Dictionary<byte, CheckType> checkTypes = new Dictionary<byte, CheckType>();

            var textBoxes = GetTextBoxes();

            foreach (var tb in textBoxes)
            {
                if (tb.Name == "textBox1")
                    continue;

                if (tb.Text != String.Empty)
                    checkTypes.Add(Convert.ToByte(tb.Text, 16), (CheckType)tb.TabIndex);
            }

            MSG_WARDEN_DATA.InitCheckTypes(checkTypes);

            Hide();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if(textBox1.SelectedText != String.Empty)
                textBox2.Text = textBox1.SelectedText.Trim();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText != String.Empty)
                textBox3.Text = textBox1.SelectedText.Trim();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText != String.Empty)
                textBox4.Text = textBox1.SelectedText.Trim();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText != String.Empty)
                textBox5.Text = textBox1.SelectedText.Trim();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText != String.Empty)
                textBox6.Text = textBox1.SelectedText.Trim();
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText != String.Empty)
                textBox7.Text = textBox1.SelectedText.Trim();
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText != String.Empty)
                textBox8.Text = textBox1.SelectedText.Trim();
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText != String.Empty)
                textBox9.Text = textBox1.SelectedText.Trim();
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText != String.Empty)
                textBox10.Text = textBox1.SelectedText.Trim();
        }
    }
}
