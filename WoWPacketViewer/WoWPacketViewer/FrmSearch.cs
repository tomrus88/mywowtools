using System;
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
            var frmMain = ((FrmMain)Owner);
            var opcode = textBox1.Text;
            if (!String.IsNullOrEmpty(opcode))
            {
                frmMain.Search(opcode, radioButton1.Checked);
            }
        }
    }
}
