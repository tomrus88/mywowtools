using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using WoWReader;
using UpdateFields;
using A9parser;
using WoWObjects;

namespace bin_parser
{
    internal class Binparser
    {
        private NumberFormatInfo nfi;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Binparser bp = new Binparser();
            bp.nfi = new CultureInfo("en-us", false).NumberFormat;
            bp.nfi.NumberDecimalSeparator = ".";

            UpdateFieldsLoader.LoadUpdateFields();

            DateTime starttime = DateTime.Now;

            Console.WriteLine("Starting at {0}", starttime);

            DirectoryInfo di = new DirectoryInfo("."); // DirectoryInfo
            //FileInfo[] fi = di.GetFiles("tcp*.log.dump", SearchOption.AllDirectories); // Get file list
            FileInfo[] fi = di.GetFiles("*.dump", SearchOption.AllDirectories); // Get file list

            Console.WriteLine("Found {0} files to parse", fi.Length);

            foreach (FileInfo f in fi)
            {
                ParseFile(f);
            }

            TimeSpan worktime = DateTime.Now - starttime;
            //MessageBox.Show("Done in " + worktime.ToString() + "!", "BIN parser", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
            Console.WriteLine("Done in " + worktime.ToString() + "!");
            Console.ReadKey();
        }

        static uint packet = 1;

        // we must use clear dictionary for each new file...
        public static Dictionary<ulong, ObjectTypes> m_objects = new Dictionary<ulong, ObjectTypes>();
        //public static Dictionary<ulong, WoWObject> m_objects2 = new Dictionary<ulong, WoWObject>();

        private static bool ParseFile(FileInfo f)
        {
            DateTime filestarttime = DateTime.Now;

            Console.Write("Parsing {0}...", f.Name);

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

            TimeSpan fileworktime = DateTime.Now - filestarttime;

            Console.WriteLine(" Parsed in {0}", fileworktime);

            return true;
        }

        /// <summary>
        /// Packet header parser.
        /// </summary>
        /// <param name="gr">Main stream reader.</param>
        /// <param name="sw">Data stream writer.</param>
        /// <param name="swe">Error logger writer.</param>
        /// <param name="data">Data logger writer.</param>
        /// <returns>Successful</returns>
        private static bool ParseHeader(GenericReader gr, StreamWriter sw, StreamWriter swe, StreamWriter data)
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
            {
                ms.Close();
                gr2.Close();
                return false;
            }

            switch (opcode)
            {
                /*case 0x00DD:  // SMSG_MONSTER_MOVE
                    OpcodeParsers.OpcodeParsers.ParseMonsterMoveOpcode(gr, gr2, sb, swe);
                    break;*/
                /*case 0x012A:  // SMSG_INITIAL_SPELLS
                    OpcodeParsers.OpcodeParsers.ParseInitialSpellsOpcode(gr, gr2, sb, swe);
                    break;*/
                /*case 0x025C:  // SMSG_AUCTION_LIST_RESULT
                    OpcodeParsers.OpcodeParsers.ParseAuctionListResultOpcode(gr, gr2, sb, swe);
                    break;*/
                /*case 0x007E:  // SMSG_PARTY_MEMBER_STATS
                case 0x02F2:  // SMSG_PARTY_MEMBER_STATS_FULL
                    OpcodeParsers.OpcodeParsers.ParsePartyMemberStatsOpcode(gr, gr2, sb, swe);
                    break;*/
                case 0x00A9:    // SMSG_UPDATE_OBJECT
                case 0x01F6:    // SMSG_COMPRESSED_UPDATE_OBJECT
                    if(opcode == 0x01F6)
                        gr2 = A9.Decompress(gr2);
                    A9.ParseUpdatePacket(gr, gr2, sb, swe);
                    break;
                /*case 0x0042: // SMSG_LOGIN_SETTIMESPEED
                    OpcodeParsers.OpcodeParsers.ParseLoginSetTimeSpeedOpcode(gr, gr2, sb, swe);
                    break;/*
                /*case 0x01B1: // SMSG_TRAINER_LIST
                    OpcodeParsers.OpcodeParsers.ParseTrainerListOpcode(gr, gr2, sb, swe);
                    break;*/
                /*case 0x014A: // SMSG_ATTACKERSTATEUPDATE
                    OpcodeParsers.OpcodeParsers.ParseAttackerStateUpdateOpcode(gr, gr2, sb, swe);
                    break;*/
                default:    // unhandled opcode
                    return false;
            }

            if (sb.ToString().Length != 0)
                sw.WriteLine(sb.ToString());

            ms.Close();
            gr2.Close();

            return true;
        }
    }
}
