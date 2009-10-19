using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WoWPacketViewer.Parsers.Warden;

namespace WoWPacketViewer
{
	internal partial class FrmWardenDebug : Form
    {
        public FrmWardenDebug()
        {
            InitializeComponent();
        }

        public TextBox[] GetTextBoxes()
        {
            var temp = new List<TextBox>();
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
            var checkTypes = new Dictionary<byte, CheckType>();

        	foreach (var tb in GetTextBoxes())
            {
                if (tb.Name == "textBox1")
                    continue;

                if (tb.Text != String.Empty)
                    checkTypes.Add(Convert.ToByte(tb.Text, 16), (CheckType)tb.TabIndex);
            }

            WardenData.InitCheckTypes(checkTypes);

            Hide();
        }

        private void toolStripMenuItem_Click(object sender, EventArgs e)
        {
        	if (tbInfo.SelectedText == String.Empty) return;

        	foreach (var tb in GetTextBoxes())
        	{
        		if (tb.Name == "textBox1")
        			continue;

        		if(tb.TabIndex == Convert.ToInt32(((ToolStripMenuItem) sender).Tag))
        		{
        			tb.Text = tbInfo.SelectedText.Trim();
        			break;
        		}
        	}
        }

		public void SetInfo(string value)
    	{
    		tbInfo.Text = value;
    	}

		public void SetCheckTypes(IDictionary<byte, CheckType> checkTypes)
		{
			foreach (TextBox tb in GetTextBoxes())
			{
				if (tb.Name == "textBox1")
					continue;

				byte val = 0;
				if (GetByteForCheckType((CheckType) tb.TabIndex, ref val, checkTypes))
					tb.Text = String.Format("{0:X2}", val);
			}
		}

		private static bool GetByteForCheckType(CheckType checkType, ref byte val, IEnumerable<KeyValuePair<byte, CheckType>> dictionary)
    	{
    		foreach (var temp in dictionary)
    		{
    			if (temp.Value == checkType)
    			{
    				val = temp.Key;
    				return true;
    			}
    		}
    		return false;
    	}
    }
}
