using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WoWPacketViewer
{
    public partial class FrmSearch : Form
    {
        public FrmSearch()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ((FrmMain)Owner).Search(textBox1.Text, radioButton1.Checked ? true : false);
        }
    }
}
