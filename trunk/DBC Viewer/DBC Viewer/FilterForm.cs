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
            var colName = (string)comboBox1.SelectedItem;
            var op = (string)comboBox2.SelectedItem;

            var col = dt.Columns[colName];

            if (op == null || colName == null)
            {
                MessageBox.Show("Select something first!");
                return;
            }

            string val = textBox1.Text;

            if (val == String.Empty)
            {
                MessageBox.Show("Enter something first!");
                return;
            }

            if (col.DataType == typeof(string))
            {
                switch (op)
                {
                    case "==":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<string>(colName) == (string)val)).AsDataView());
                        break;
                    case "!=":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<string>(colName) != (string)val)).AsDataView());
                        break;
                    default:
                        MessageBox.Show("Unsupported operation for selected column!");
                        return;
                }
            }
            else if (col.DataType == typeof(long))
            {
                var locVal = Convert.ToInt64(val);

                switch (op)
                {
                    case "&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<long>(colName) & locVal) != 0).AsDataView());
                        break;
                    case "~&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<long>(colName) & locVal) == 0).AsDataView());
                        break;
                    case "==":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<long>(colName) == locVal)).AsDataView());
                        break;
                    case "!=":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<long>(colName) != locVal)).AsDataView());
                        break;
                    case "<":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<long>(colName) < locVal)).AsDataView());
                        break;
                    case ">":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<long>(colName) > locVal)).AsDataView());
                        break;
                    default:
                        MessageBox.Show("Unsupported operation for selected column!");
                        return;
                }
            }
            else if (col.DataType == typeof(ulong))
            {
                var locVal = Convert.ToUInt64(val);

                switch (op)
                {
                    case "&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<ulong>(colName) & locVal) != 0).AsDataView());
                        break;
                    case "~&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<ulong>(colName) & locVal) == 0).AsDataView());
                        break;
                    case "==":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<ulong>(colName) == locVal)).AsDataView());
                        break;
                    case "!=":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<ulong>(colName) != locVal)).AsDataView());
                        break;
                    case "<":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<ulong>(colName) < locVal)).AsDataView());
                        break;
                    case ">":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<ulong>(colName) > locVal)).AsDataView());
                        break;
                    default:
                        MessageBox.Show("Unsupported operation for selected column!");
                        return;
                }
            }
            else if (col.DataType == typeof(int))
            {
                var locVal = Convert.ToInt32(val);

                switch (op)
                {
                    case "&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<int>(colName) & locVal) != 0).AsDataView());
                        break;
                    case "~&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<int>(colName) & locVal) == 0).AsDataView());
                        break;
                    case "==":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<int>(colName) == locVal)).AsDataView());
                        break;
                    case "!=":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<int>(colName) != locVal)).AsDataView());
                        break;
                    case "<":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<int>(colName) < locVal)).AsDataView());
                        break;
                    case ">":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<int>(colName) > locVal)).AsDataView());
                        break;
                    default:
                        MessageBox.Show("Unsupported operation for selected column!");
                        return;
                }
            }
            else if (col.DataType == typeof(uint))
            {
                var locVal = Convert.ToUInt32(val);

                switch (op)
                {
                    case "&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<uint>(colName) & locVal) != 0).AsDataView());
                        break;
                    case "~&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<uint>(colName) & locVal) == 0).AsDataView());
                        break;
                    case "==":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<uint>(colName) == locVal)).AsDataView());
                        break;
                    case "!=":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<uint>(colName) != locVal)).AsDataView());
                        break;
                    case "<":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<uint>(colName) < locVal)).AsDataView());
                        break;
                    case ">":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<uint>(colName) > locVal)).AsDataView());
                        break;
                    default:
                        MessageBox.Show("Unsupported operation for selected column!");
                        return;
                }
            }
            else if (col.DataType == typeof(short))
            {
                var locVal = Convert.ToInt16(val);

                switch (op)
                {
                    case "&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<short>(colName) & locVal) != 0).AsDataView());
                        break;
                    case "~&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<short>(colName) & locVal) == 0).AsDataView());
                        break;
                    case "==":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<short>(colName) == locVal)).AsDataView());
                        break;
                    case "!=":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<short>(colName) != locVal)).AsDataView());
                        break;
                    case "<":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<short>(colName) < locVal)).AsDataView());
                        break;
                    case ">":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<short>(colName) > locVal)).AsDataView());
                        break;
                    default:
                        MessageBox.Show("Unsupported operation for selected column!");
                        return;
                }
            }
            else if (col.DataType == typeof(ushort))
            {
                var locVal = Convert.ToUInt16(val);

                switch (op)
                {
                    case "&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<ushort>(colName) & locVal) != 0).AsDataView());
                        break;
                    case "~&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<ushort>(colName) & locVal) == 0).AsDataView());
                        break;
                    case "==":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<ushort>(colName) == locVal)).AsDataView());
                        break;
                    case "!=":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<ushort>(colName) != locVal)).AsDataView());
                        break;
                    case "<":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<ushort>(colName) < locVal)).AsDataView());
                        break;
                    case ">":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<ushort>(colName) > locVal)).AsDataView());
                        break;
                    default:
                        MessageBox.Show("Unsupported operation for selected column!");
                        return;
                }
            }
            else if (col.DataType == typeof(sbyte))
            {
                var locVal = Convert.ToSByte(val);

                switch (op)
                {
                    case "&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<sbyte>(colName) & locVal) != 0).AsDataView());
                        break;
                    case "~&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<sbyte>(colName) & locVal) == 0).AsDataView());
                        break;
                    case "==":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<sbyte>(colName) == locVal)).AsDataView());
                        break;
                    case "!=":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<sbyte>(colName) != locVal)).AsDataView());
                        break;
                    case "<":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<sbyte>(colName) < locVal)).AsDataView());
                        break;
                    case ">":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<sbyte>(colName) > locVal)).AsDataView());
                        break;
                    default:
                        MessageBox.Show("Unsupported operation for selected column!");
                        return;
                }
            }
            else if (col.DataType == typeof(byte))
            {
                var locVal = Convert.ToByte(val);

                switch (op)
                {
                    case "&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<byte>(colName) & locVal) != 0).AsDataView());
                        break;
                    case "~&":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<byte>(colName) & locVal) == 0).AsDataView());
                        break;
                    case "==":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<byte>(colName) == locVal)).AsDataView());
                        break;
                    case "!=":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<byte>(colName) != locVal)).AsDataView());
                        break;
                    case "<":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<byte>(colName) < locVal)).AsDataView());
                        break;
                    case ">":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<byte>(colName) > locVal)).AsDataView());
                        break;
                    default:
                        MessageBox.Show("Unsupported operation for selected column!");
                        return;
                }
            }
            else if (col.DataType == typeof(float))
            {
                var locVal = Convert.ToSingle(val);

                switch (op)
                {
                    case "==":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<float>(colName) == locVal)).AsDataView());
                        break;
                    case "!=":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<float>(colName) != locVal)).AsDataView());
                        break;
                    case "<":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<float>(colName) < locVal)).AsDataView());
                        break;
                    case ">":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<float>(colName) > locVal)).AsDataView());
                        break;
                    default:
                        MessageBox.Show("Unsupported operation for selected column!");
                        return;
                }
            }
            else if (col.DataType == typeof(double))
            {
                var locVal = Convert.ToDouble(val);

                switch (op)
                {
                    case "==":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<double>(colName) == locVal)).AsDataView());
                        break;
                    case "!=":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<double>(colName) != locVal)).AsDataView());
                        break;
                    case "<":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<double>(colName) < locVal)).AsDataView());
                        break;
                    case ">":
                        owner.SetDataView(dt.AsEnumerable().Where(tr => (tr.Field<double>(colName) > locVal)).AsDataView());
                        break;
                    default:
                        MessageBox.Show("Unsupported operation for selected column!");
                        return;
                }
            }
            else
            {
                MessageBox.Show("Unhandled type?");
            }

            //dt.AsEnumerable().Where(tr => tr[colName] == val).AsDataView();
            //dt.AsEnumerable().Where(tr => tr[colName] & val).AsDataView();
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
