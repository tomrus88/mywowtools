using System;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace bin_parser
{
    public partial class Form1 : Form
    {
        uint packet = 1;

        [Flags]
        enum Flags
        {
            flag01 = 0x00000001,
            flag02 = 0x00000002,
            flag03 = 0x00000004,
            flag04 = 0x00000008,
            flag05 = 0x00000010,
            flag06 = 0x00000020,
            flag07 = 0x00000040,
            flag08 = 0x00000080,
            flag09 = 0x00000100, // running
            flag10 = 0x00000200, // taxi
            flag11 = 0x00000400,
            flag12 = 0x00000800,
            flag13 = 0x00001000,
            flag14 = 0x00002000,
            flag15 = 0x00004000,
            flag16 = 0x00008000,
            flag17 = 0x00010000,
            flag18 = 0x00020000,
            flag19 = 0x00040000,
            flag20 = 0x00080000,
            flag21 = 0x00100000,
            flag22 = 0x00200000,
            flag23 = 0x00400000,
            flag24 = 0x00800000,
            flag25 = 0x01000000,
            flag26 = 0x02000000,
            flag27 = 0x04000000,
            flag28 = 0x08000000,
            flag29 = 0x10000000,
            flag30 = 0x20000000,
            flag31 = 0x40000000
        };

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
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
                    try
                    {
                        if (ParseHeader(gr, sw, swe, data))
                            packet++;
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.ToString());
                        swe.WriteLine("error in pos " + gr.BaseStream.Position.ToString("X16"));
                    }
                }

                sw.Close();
                swe.Close();
                data.Close();
                gr.Close();
            }
            MessageBox.Show("Done!", "BIN parser", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);

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
                case 0x00DD:
                    //ParseMonsterMoveOpcode(gr, gr2, sb, swe);
                    break;
                case 0x012A:
                    //ParseInitialSpellsOpcode(gr, gr2, sb, swe);
                    break;
                case 0x025C:
                    //ParseAuctionListResultOpcode(gr, gr2, sb, swe);
                    break;
                case 0x007E:
                    //ParsePartyMemberStatsOpcode(gr, gr2, sb, swe);
                    break;
                case 0x00A9:
                    ParseUpdatePacket(gr, gr2, sb, swe);
                    break;
                default:
                    break;
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
