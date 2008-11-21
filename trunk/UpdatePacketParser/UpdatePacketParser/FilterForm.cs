using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using UpdateFields;

namespace UpdatePacketParser
{
    [Flags]
    public enum CustomFilterMask
    {
        CUSTOM_FILTER_NONE = 0,
        CUSTOM_FILTER_UNITS = 1,
        CUSTOM_FILTER_PETS = 2,
        CUSTOM_FILTER_VEHICLES = 4,
        CUSTOM_FILTER_TRANSPORT = 8,
        CUSTOM_FILTER_MO_TRANSPORT = 16
    };

    public partial class FilterForm : Form
    {
        public FilterForm()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ObjectTypeMask mask = ObjectTypeMask.TYPEMASK_NONE;

            if (checkBox1.Checked)
                mask |= ObjectTypeMask.TYPEMASK_ITEM;

            if (checkBox2.Checked)
                mask |= ObjectTypeMask.TYPEMASK_CONTAINER;

            if (checkBox3.Checked)
                mask |= ObjectTypeMask.TYPEMASK_UNIT;

            if (checkBox4.Checked)
                mask |= ObjectTypeMask.TYPEMASK_PLAYER;

            if (checkBox5.Checked)
                mask |= ObjectTypeMask.TYPEMASK_GAMEOBJECT;

            if (checkBox6.Checked)
                mask |= ObjectTypeMask.TYPEMASK_DYNAMICOBJECT;

            if (checkBox7.Checked)
                mask |= ObjectTypeMask.TYPEMASK_CORPSE;

            CustomFilterMask customMask = CustomFilterMask.CUSTOM_FILTER_NONE;

            if (checkBox8.Checked)
                customMask |= CustomFilterMask.CUSTOM_FILTER_TRANSPORT;

            if (checkBox9.Checked)
                customMask |= CustomFilterMask.CUSTOM_FILTER_PETS;

            if (checkBox10.Checked)
                customMask |= CustomFilterMask.CUSTOM_FILTER_VEHICLES;

            if (checkBox11.Checked)
                customMask |= CustomFilterMask.CUSTOM_FILTER_MO_TRANSPORT;

            if (checkBox12.Checked)
                customMask |= CustomFilterMask.CUSTOM_FILTER_UNITS;

            ((FrmMain)Owner).PrintObjectType(mask, customMask);
        }
    }
}
