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
    }
}
