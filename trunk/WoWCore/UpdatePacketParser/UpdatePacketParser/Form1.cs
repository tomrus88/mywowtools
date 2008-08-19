using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WoWObjects;

namespace UpdatePacketParser
{
    public partial class Form1 : Form
    {
        Parser m_parser;    // active parser

        public Form1()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhanledExceptionHandler);
        }

        private void UnhanledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            MessageBox.Show(args.ExceptionObject.ToString());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listView1.Items.Clear();
            listView2.Items.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(1);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            ListView.SelectedListViewItemCollection slvic = listView1.SelectedItems;
            for (int i = 0; i < slvic.Count; i++)
            {
                ListViewItem.ListViewSubItemCollection lvsic = slvic[i].SubItems;
                if (lvsic.Count != 2)
                    return;
                sb.AppendLine(lvsic[0].Text + '\t' + lvsic[1].Text);
            }

            string temp = sb.ToString();
            if (String.IsNullOrEmpty(temp))
                return;
            Clipboard.SetText(temp);
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            ListView.SelectedListViewItemCollection slvic = listView2.SelectedItems;
            for (int i = 0; i < slvic.Count; i++)
            {
                ListViewItem.ListViewSubItemCollection lvsic = slvic[i].SubItems;
                if (lvsic.Count != 2)
                    return;
                sb.AppendLine(lvsic[0].Text + '\t' + lvsic[1].Text);
            }

            string temp = sb.ToString();
            if (String.IsNullOrEmpty(temp))
                return;
            Clipboard.SetText(temp);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
                LoadFile(openFileDialog1.FileName);
        }

        private void LoadFile(string filename)
        {
            if (m_parser != null)
            {
                m_parser.Close();
                m_parser = null;
            }
            listBox1.Items.Clear();
            listView1.Items.Clear();
            listView2.Items.Clear();
            m_parser = new Parser(filename);
            m_parser.PrintObjects(listBox1);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            m_parser.PrintObjectInfo(listBox1.SelectedIndex, listView1);

            // process updates
            listView2.Items.Clear();
            m_parser.PrintObjectUpdatesInfo(listBox1.SelectedIndex, listView2);

            // process movement info
            richTextBox1.Clear();
            m_parser.PrintObjectMovementInfo(listBox1.SelectedIndex, richTextBox1);
        }
    }
}
