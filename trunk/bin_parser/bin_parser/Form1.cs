using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace bin_parser
{
    public partial class Form1 : Form
    {
        uint packet = 1;

        // we must use clear dictionary for each new file...
        public static Dictionary<ulong, ObjectTypes> m_objects = new Dictionary<ulong,ObjectTypes>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime starttime = DateTime.Now;

            DirectoryInfo di = new DirectoryInfo("."); // DirectoryInfo
            FileInfo[] fi = di.GetFiles("tcp*.log.dump", SearchOption.AllDirectories); // Get file list

            foreach (FileInfo f in fi)
            {
                GenericReader gr = new GenericReader(f.FullName, Encoding.ASCII);

                string error_log = f.FullName + ".errors.txt";
                StreamWriter swe = new StreamWriter(error_log);

                string database_log = f.FullName + ".data.txt";
                StreamWriter data = new StreamWriter(database_log);

                string ofn = f.FullName + ".data_out.txt";
                StreamWriter sw = new StreamWriter(ofn);

                sw.AutoFlush = true;
                swe.AutoFlush = true;
                data.AutoFlush = true;

                while (gr.PeekChar() >= 0)
                {
                    //try
                    //{
                        if (ParseHeader(gr, sw, swe, data))
                            packet++;
                    //}
                    //catch (Exception exc)
                    //{
                    //    MessageBox.Show(exc.ToString());
                    //    swe.WriteLine("error in pos " + gr.BaseStream.Position.ToString("X16"));
                    //}
                }

                // clear objects list...
                m_objects.Clear();

                sw.Close();
                swe.Close();
                data.Close();
                gr.Close();
            }

            TimeSpan worktime = DateTime.Now - starttime;
            MessageBox.Show("Done in " + worktime.ToString() + "!", "BIN parser", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
        }

        /// <summary>
        /// Packet header parser.
        /// </summary>
        /// <param name="gr">Main stream reader.</param>
        /// <param name="sw">Data stream writer.</param>
        /// <param name="swe">Error logger writer.</param>
        /// <param name="data">Data logger writer.</param>
        /// <returns>Successful</returns>
        private bool ParseHeader(GenericReader gr, StreamWriter sw, StreamWriter swe, StreamWriter data)
        {
            StringBuilder sb = new StringBuilder();

            int datasize = gr.ReadInt32();

            //sb.AppendLine("Packet offset " + (gr.BaseStream.Position - 4).ToString("X2"));

            //sb.AppendLine("Packet number: " + packet);

            //sb.AppendLine("Data size " + datasize);

            byte[] temp = gr.ReadBytes(datasize);
            MemoryStream ms = new MemoryStream(temp);
            GenericReader gr2 = new GenericReader(ms);

            ushort opcode = 0;

            if (gr2.PeekChar() >= 0)
                opcode = gr2.ReadUInt16();
            else
                return false;

            switch (opcode)
            {
                /*case 0x00DD:
                    //ParseMonsterMoveOpcode(gr, gr2, sb, swe);
                    break;*/
                /*case 0x012A:
                    //ParseInitialSpellsOpcode(gr, gr2, sb, swe);
                    break;*/
                /*case 0x025C:
                    //ParseAuctionListResultOpcode(gr, gr2, sb, swe);
                    break;*/
                /*case 0x007E:
                    //ParsePartyMemberStatsOpcode(gr, gr2, sb, swe);
                    break;*/
                case 0x00A9:
                    ParseUpdatePacket(gr, gr2, sb, swe);
                    break;
                default:    // unhandled opcode
                    return false;
            }

            if (sb.ToString().Length != 0)
                sw.WriteLine(sb.ToString());

            return true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateFields.LoadUpdateFields();
        }
    }
}
