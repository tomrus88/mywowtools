using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WoWPacketViewer
{
    public abstract class PacketViewerBase
    {
        //TODO: сделать по аналогии с update парсером
        protected static List<Packet> m_packets = new List<Packet>();

        public static List<Packet> Packets
        {
            get { return m_packets; }
        }

        public abstract int LoadData(string file);

        public ListViewItem[] ListPackets()
        {
            var pkt = (from packet in m_packets
                       where packet.Opcode == OpCodes.CMSG_AUTH_SESSION
                       select packet).FirstOrDefault();

            uint build;
            try
            {
                build = BitConverter.ToUInt32(pkt.Data, 0);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                return null;
            }

            var items = new List<ListViewItem>();
            foreach (Packet p in m_packets)
            {
                ListViewItem item;
                if (p.Direction == Direction.CLIENT)
                    item = new ListViewItem(new string[] { build.ToString(), p.Opcode.ToString(), string.Empty, p.Data.Length.ToString() });
                else
                    item = new ListViewItem(new string[] { build.ToString(), string.Empty, p.Opcode.ToString(), p.Data.Length.ToString() });
                items.Add(item);
            }
            return items.ToArray();
        }

        public void ShowHex(TextBox tb, int index)
        {
            Packet pkt = m_packets[index];

            tb.Text = HexLike(pkt);
        }

        public string HexLike(Packet pkt)
        {

            var length = pkt.Data.Length;
            var dir = (pkt.Direction == Direction.CLIENT) ? "C->S" : "S->C";

            var result = new StringBuilder();
            result.AppendFormat("Packet {0}, {1} ({2}), len {3}", dir, pkt.Opcode, (ushort)pkt.Opcode, length);
            result.AppendLine();

            if (length == 0)
            { // empty packet (no data provided)
                result.AppendLine("0000: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- | ................");
            }
            else
            {
                int offset = 0;
                for (int i = 0; i < length; i += 0x10)
                {
                    var bytes = new StringBuilder();
                    var chars = new StringBuilder();

                    for (int j = 0; j < 0x10; ++j)
                    {
                        if (offset < length)
                        {
                            int c = pkt.Data[offset];
                            offset++;

                            bytes.AppendFormat("{0,-3:X2}", c);
                            chars.Append((c >= 0x20 && c < 0x80) ? (char)c : '.');
                        }
                        else
                        {
                            bytes.Append("-- ");
                            chars.Append('.');
                        }
                    }

                    result.AppendLine(i.ToString("X4") + ": " + bytes.ToString() + "| " + chars.ToString());
                }
            }

            result.AppendLine();

            return result.ToString();
        }

        public void ShowParsed(TextBox tb, int index)
        {

        }
    }
}
