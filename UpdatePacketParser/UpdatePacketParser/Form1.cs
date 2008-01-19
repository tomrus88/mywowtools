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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listView1.Items.Clear();
            listView2.Items.Clear();
            UpdateFields.UpdateFieldsLoader.LoadUpdateFields();
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
            string temp = listView1.SelectedItems.ToString();
            if (String.IsNullOrEmpty(temp))
                return;
            Clipboard.SetText(temp);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if(result == DialogResult.OK)
                LoadFile(openFileDialog1.FileName);
        }

        private void LoadFile(string filename)
        {
            if(m_parser != null)
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

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string temp = listView2.SelectedItems.ToString();
            if (String.IsNullOrEmpty(temp))
                return;
            Clipboard.SetText(temp);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            m_parser.PrintObjectInfo(listBox1.SelectedIndex, listView1);

            // process updates also
            listView2.Items.Clear();
            m_parser.PrintObjectUpdatesInfo(listBox1.SelectedIndex, listView2);
        }
    }
}
