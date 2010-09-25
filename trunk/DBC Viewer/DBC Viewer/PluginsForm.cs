using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PluginInterface;

namespace DBCViewer
{
    public partial class PluginsForm : Form
    {
        public int PluginIndex { get; private set; }

        public PluginsForm()
        {
            InitializeComponent();
        }

        public void SetPlugins(List<IPlugin> plugins)
        {
            foreach (IPlugin plugin in plugins)
            {
                var item = String.Format("{0}", plugin.GetType().Name);
                listBox1.Items.Add(item);
            }

            listBox1.SelectedIndex = 0;
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PluginIndex = listBox1.SelectedIndex;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
