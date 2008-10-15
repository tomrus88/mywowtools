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
        protected List<Packet> m_packets = new List<Packet>();

        public List<Packet> Packets
        {
            get { return m_packets; }
        }

        public abstract int LoadData(string file);

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
    }
}
