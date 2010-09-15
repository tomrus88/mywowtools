using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DBC_Viewer
{
    public partial class FilterForm : Form
    {
        EnumerableRowCollection<DataRow> m_filter;
        ComboBox.ObjectCollection decimalOperators;
        ComboBox.ObjectCollection stringOperators;
        ComboBox.ObjectCollection floatOperators;

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

            decimalOperators = new ComboBox.ObjectCollection(comboBox2) { "&", "~&", "==", "!=", "<", ">" };
            stringOperators = new ComboBox.ObjectCollection(comboBox2) { "==", "!=", "*__", "__*", "_*_" };
            floatOperators = new ComboBox.ObjectCollection(comboBox2) { "==", "!=", "<", ">" };

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var owner = ((MainForm)Owner);
            var dt = owner.DataTable;
            //var dv = owner.DataView;
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
                textBox1.Focus();
                return;
            }

            if (m_filter == null)
                m_filter = dt.AsEnumerable();

            if (!checkBox1.Checked)
                m_filter = dt.AsEnumerable();

            bool result = false;
            if (col.DataType == typeof(string))
                result = FilterString(colName, op, val);
            else if (col.DataType == typeof(long))
                result = FilterInt64(colName, op, val);
            else if (col.DataType == typeof(ulong))
                result = FilterUInt64(colName, op, val);
            else if (col.DataType == typeof(int))
                result = FilterInt32(colName, op, val);
            else if (col.DataType == typeof(uint))
                result = FilterUInt32(colName, op, val);
            else if (col.DataType == typeof(short))
                result = FilterInt16(colName, op, val);
            else if (col.DataType == typeof(ushort))
                result = FilterUInt16(colName, op, val);
            else if (col.DataType == typeof(sbyte))
                result = FilterInt8( colName, op, val);
            else if (col.DataType == typeof(byte))
                result = FilterUInt8(colName, op, val);
            else if (col.DataType == typeof(float))
                result = FilterSingle(colName, op, val);
            else if (col.DataType == typeof(double))
                result = FilterDouble(colName, op, val);
            else
                MessageBox.Show("Unhandled type?");

            if (result)
                owner.SetDataView(m_filter.AsDataView());
            else
                MessageBox.Show("Unhandled type?");

            //dt.AsEnumerable().Where(tr => tr[colName] == val).AsDataView();
            //dt.AsEnumerable().Where(tr => tr[colName] & val).AsDataView();
            //var fmt = Convert.ChangeType(val, col.DataType);
        }

        private bool FilterDouble(string colName, string op, string val)
        {
            var locVal = Convert.ToDouble(val);

            switch (op)
            {
                case "==":
                    m_filter = m_filter.Where(tr => (tr.Field<double>(colName) == locVal));
                    break;
                case "!=":
                    m_filter = m_filter.Where(tr => (tr.Field<double>(colName) != locVal));
                    break;
                case "<":
                    m_filter = m_filter.Where(tr => (tr.Field<double>(colName) < locVal));
                    break;
                case ">":
                    m_filter = m_filter.Where(tr => (tr.Field<double>(colName) > locVal));
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool FilterSingle(string colName, string op, string val)
        {
            var locVal = Convert.ToSingle(val);

            switch (op)
            {
                case "==":
                    m_filter = m_filter.Where(tr => (tr.Field<float>(colName) == locVal));
                    break;
                case "!=":
                    m_filter = m_filter.Where(tr => (tr.Field<float>(colName) != locVal));
                    break;
                case "<":
                    m_filter = m_filter.Where(tr => (tr.Field<float>(colName) < locVal));
                    break;
                case ">":
                    m_filter = m_filter.Where(tr => (tr.Field<float>(colName) > locVal));
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool FilterUInt8(string colName, string op, string val)
        {
            var locVal = Convert.ToByte(val);

            switch (op)
            {
                case "&":
                    m_filter = m_filter.Where(tr => (tr.Field<byte>(colName) & locVal) != 0);
                    break;
                case "~&":
                    m_filter = m_filter.Where(tr => (tr.Field<byte>(colName) & locVal) == 0);
                    break;
                case "==":
                    m_filter = m_filter.Where(tr => (tr.Field<byte>(colName) == locVal));
                    break;
                case "!=":
                    m_filter = m_filter.Where(tr => (tr.Field<byte>(colName) != locVal));
                    break;
                case "<":
                    m_filter = m_filter.Where(tr => (tr.Field<byte>(colName) < locVal));
                    break;
                case ">":
                    m_filter = m_filter.Where(tr => (tr.Field<byte>(colName) > locVal));
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool FilterInt8(string colName, string op, string val)
        {
            var locVal = Convert.ToSByte(val);

            switch (op)
            {
                case "&":
                    m_filter = m_filter.Where(tr => (tr.Field<sbyte>(colName) & locVal) != 0);
                    break;
                case "~&":
                    m_filter = m_filter.Where(tr => (tr.Field<sbyte>(colName) & locVal) == 0);
                    break;
                case "==":
                    m_filter = m_filter.Where(tr => (tr.Field<sbyte>(colName) == locVal));
                    break;
                case "!=":
                    m_filter = m_filter.Where(tr => (tr.Field<sbyte>(colName) != locVal));
                    break;
                case "<":
                    m_filter = m_filter.Where(tr => (tr.Field<sbyte>(colName) < locVal));
                    break;
                case ">":
                    m_filter = m_filter.Where(tr => (tr.Field<sbyte>(colName) > locVal));
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool FilterUInt16(string colName, string op, string val)
        {
            var locVal = Convert.ToUInt16(val);

            switch (op)
            {
                case "&":
                    m_filter = m_filter.Where(tr => (tr.Field<ushort>(colName) & locVal) != 0);
                    break;
                case "~&":
                    m_filter = m_filter.Where(tr => (tr.Field<ushort>(colName) & locVal) == 0);
                    break;
                case "==":
                    m_filter = m_filter.Where(tr => (tr.Field<ushort>(colName) == locVal));
                    break;
                case "!=":
                    m_filter = m_filter.Where(tr => (tr.Field<ushort>(colName) != locVal));
                    break;
                case "<":
                    m_filter = m_filter.Where(tr => (tr.Field<ushort>(colName) < locVal));
                    break;
                case ">":
                    m_filter = m_filter.Where(tr => (tr.Field<ushort>(colName) > locVal));
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool FilterInt16(string colName, string op, string val)
        {
            var locVal = Convert.ToInt16(val);

            switch (op)
            {
                case "&":
                    m_filter = m_filter.Where(tr => (tr.Field<short>(colName) & locVal) != 0);
                    break;
                case "~&":
                    m_filter = m_filter.Where(tr => (tr.Field<short>(colName) & locVal) == 0);
                    break;
                case "==":
                    m_filter = m_filter.Where(tr => (tr.Field<short>(colName) == locVal));
                    break;
                case "!=":
                    m_filter = m_filter.Where(tr => (tr.Field<short>(colName) != locVal));
                    break;
                case "<":
                    m_filter = m_filter.Where(tr => (tr.Field<short>(colName) < locVal));
                    break;
                case ">":
                    m_filter = m_filter.Where(tr => (tr.Field<short>(colName) > locVal));
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool FilterUInt32(string colName, string op, string val)
        {
            var locVal = Convert.ToUInt32(val);

            switch (op)
            {
                case "&":
                    m_filter = m_filter.Where(tr => (tr.Field<uint>(colName) & locVal) != 0);
                    break;
                case "~&":
                    m_filter = m_filter.Where(tr => (tr.Field<uint>(colName) & locVal) == 0);
                    break;
                case "==":
                    m_filter = m_filter.Where(tr => (tr.Field<uint>(colName) == locVal));
                    break;
                case "!=":
                    m_filter = m_filter.Where(tr => (tr.Field<uint>(colName) != locVal));
                    break;
                case "<":
                    m_filter = m_filter.Where(tr => (tr.Field<uint>(colName) < locVal));
                    break;
                case ">":
                    m_filter = m_filter.Where(tr => (tr.Field<uint>(colName) > locVal));
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool FilterInt32(string colName, string op, string val)
        {
            var locVal = Convert.ToInt32(val);

            switch (op)
            {
                case "&":
                    m_filter = m_filter.Where(tr => (tr.Field<int>(colName) & locVal) != 0);
                    break;
                case "~&":
                    m_filter = m_filter.Where(tr => (tr.Field<int>(colName) & locVal) == 0);
                    break;
                case "==":
                    m_filter = m_filter.Where(tr => (tr.Field<int>(colName) == locVal));
                    break;
                case "!=":
                    m_filter = m_filter.Where(tr => (tr.Field<int>(colName) != locVal));
                    break;
                case "<":
                    m_filter = m_filter.Where(tr => (tr.Field<int>(colName) < locVal));
                    break;
                case ">":
                    m_filter = m_filter.Where(tr => (tr.Field<int>(colName) > locVal));
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool FilterUInt64(string colName, string op, string val)
        {
            var locVal = Convert.ToUInt64(val);

            switch (op)
            {
                case "&":
                    m_filter = m_filter.Where(tr => (tr.Field<ulong>(colName) & locVal) != 0);
                    break;
                case "~&":
                    m_filter = m_filter.Where(tr => (tr.Field<ulong>(colName) & locVal) == 0);
                    break;
                case "==":
                    m_filter = m_filter.Where(tr => (tr.Field<ulong>(colName) == locVal));
                    break;
                case "!=":
                    m_filter = m_filter.Where(tr => (tr.Field<ulong>(colName) != locVal));
                    break;
                case "<":
                    m_filter = m_filter.Where(tr => (tr.Field<ulong>(colName) < locVal));
                    break;
                case ">":
                    m_filter = m_filter.Where(tr => (tr.Field<ulong>(colName) > locVal));
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool FilterInt64(string colName, string op, string val)
        {
            var locVal = Convert.ToInt64(val);

            switch (op)
            {
                case "&":
                    m_filter = m_filter.Where(tr => (tr.Field<long>(colName) & locVal) != 0);
                    break;
                case "~&":
                    m_filter = m_filter.Where(tr => (tr.Field<long>(colName) & locVal) == 0);
                    break;
                case "==":
                    m_filter = m_filter.Where(tr => (tr.Field<long>(colName) == locVal));
                    break;
                case "!=":
                    m_filter = m_filter.Where(tr => (tr.Field<long>(colName) != locVal));
                    break;
                case "<":
                    m_filter = m_filter.Where(tr => (tr.Field<long>(colName) < locVal));
                    break;
                case ">":
                    m_filter = m_filter.Where(tr => (tr.Field<long>(colName) > locVal));
                    break;
                default:
                    return false;
            }
            return true;
        }

        private bool FilterString(string colName, string op, string val)
        {
            StringComparison cmp = checkBox2.Checked ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

            switch (op)
            {
                case "==":
                    m_filter = m_filter.Where(tr => (tr.Field<string>(colName) == val));
                    break;
                case "!=":
                    m_filter = m_filter.Where(tr => (tr.Field<string>(colName) != val));
                    break;
                case "*__":
                    m_filter = m_filter.Where(tr => (tr.Field<string>(colName).StartsWith(val, cmp)));
                    break;
                case "__*":
                    m_filter = m_filter.Where(tr => (tr.Field<string>(colName).EndsWith(val, cmp)));
                    break;
                case "_*_":
                    if (checkBox2.Checked)
                        m_filter = m_filter.Where(tr => (tr.Field<string>(colName).ToLowerInvariant().Contains(val.ToLowerInvariant())));
                    else
                        m_filter = m_filter.Where(tr => (tr.Field<string>(colName).Contains(val)));
                    break;
                default:
                    return false;
            }
            return true;
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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var owner = ((MainForm)Owner);
            var dt = owner.DataTable;
            var colName = (string)comboBox1.SelectedItem;
            var col = dt.Columns[colName];

            if (col.DataType == typeof(string))
                checkBox2.Visible = true;
            else
                checkBox2.Visible = false;

            comboBox2.Items.Clear();

            if (col.DataType == typeof(string))
                AddComboBoxItems(stringOperators);
            else if (col.DataType == typeof(long))
                AddComboBoxItems(decimalOperators);
            else if (col.DataType == typeof(ulong))
                AddComboBoxItems(decimalOperators);
            else if (col.DataType == typeof(int))
                AddComboBoxItems(decimalOperators);
            else if (col.DataType == typeof(uint))
                AddComboBoxItems(decimalOperators);
            else if (col.DataType == typeof(short))
                AddComboBoxItems(decimalOperators);
            else if (col.DataType == typeof(ushort))
                AddComboBoxItems(decimalOperators);
            else if (col.DataType == typeof(sbyte))
                AddComboBoxItems(decimalOperators);
            else if (col.DataType == typeof(byte))
                AddComboBoxItems(decimalOperators);
            else if (col.DataType == typeof(float))
                AddComboBoxItems(floatOperators);
            else if (col.DataType == typeof(double))
                AddComboBoxItems(floatOperators);
            else
                MessageBox.Show("Unhandled type?");

            comboBox2.SelectedIndex = 0;
        }

        private void AddComboBoxItems(ComboBox.ObjectCollection items)
        {
            foreach (var item in items)
                comboBox2.Items.Add(item);
        }
    }
}
