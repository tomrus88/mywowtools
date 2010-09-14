using System;
using System.Data;
using System.Windows.Forms;

namespace DBC_Viewer
{
    public partial class FilterForm : Form
    {
        public FilterForm()
        {
            InitializeComponent();
        }

        private void FilterForm_Load(object sender, EventArgs e)
        {
            var dt = ((MainForm)Owner).DataTable;

            for (var i = 0; i < dt.Columns.Count; ++i)
            {
                comboBox1.Items.Add(dt.Columns[i].ColumnName);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var owner = ((MainForm)Owner);
            var dt = owner.DataTable;
            var col = (string)comboBox1.SelectedItem;
            var op = (string)comboBox2.SelectedItem;

            if (op == null || col == null)
            {
                MessageBox.Show("Select something first");
                return;
            }

            var val = 0;

            try
            {
                val = Convert.ToInt32(textBox1.Text);
            }
            catch
            {
                MessageBox.Show("Invalid value entered!");
                return;
            }

            switch (op)
            {
                case "&":
                    owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<int>(col) & val) != 0).AsDataView());
                    break;
                case "~&":
                    owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<int>(col) & val) == 0).AsDataView());
                    break;
                case "==":
                    owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<int>(col) == val)).AsDataView());
                    break;
                case "!=":
                    owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<int>(col) != val)).AsDataView());
                    break;
                case "<":
                    owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<int>(col) < val)).AsDataView());
                    break;
                case ">":
                    owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<int>(col) > val)).AsDataView());
                    break;
            }
        }

        private void FilterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                Owner.Activate();
            }
        }
    }
}
