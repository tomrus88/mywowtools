using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

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
            var dir = (pkt.Direction == Direction.Client) ? "C->S" : "S->C";

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

        public string ShowParsed(Packet pkt)
        {
            GenericReader gr = new GenericReader(new MemoryStream(pkt.Data));
            StringBuilder sb = new StringBuilder();

            switch (pkt.Opcode)
            {
                case OpCodes.SMSG_AURA_UPDATE:
                case OpCodes.SMSG_AURA_UPDATE_ALL:
                    {
                        ulong guid = gr.ReadPackedGuid();
                        sb.AppendFormat("GUID: {0}", guid.ToString("X16"));
                        sb.AppendLine();
                        sb.AppendLine();

                        while (gr.BaseStream.Position < gr.BaseStream.Length)
                        {
                            byte slot = gr.ReadByte();
                            sb.AppendFormat("Slot: {0}", slot.ToString("X2"));
                            sb.AppendLine();

                            uint spellid = gr.ReadUInt32();
                            sb.AppendFormat("Spell: {0}", spellid.ToString("X8"));
                            sb.AppendLine();

                            if (spellid > 0)
                            {
                                AuraFlags af = (AuraFlags)gr.ReadByte();
                                sb.AppendFormat("Flags: {0}", af);
                                sb.AppendLine();

                                byte auraLevel = gr.ReadByte();
                                sb.AppendFormat("Level: {0}", auraLevel.ToString("X2"));
                                sb.AppendLine();

                                byte auraApplication = gr.ReadByte();
                                sb.AppendFormat("Charges: {0}", auraApplication.ToString("X2"));
                                sb.AppendLine();

                                if (!((af & AuraFlags.AURA_FLAG_08) != AuraFlags.AURA_FLAG_00))
                                {
                                    ulong guid2 = gr.ReadPackedGuid();
                                    sb.AppendFormat("GUID2: {0}", guid2.ToString("X16"));
                                    sb.AppendLine();
                                }

                                if ((af & AuraFlags.AURA_FLAG_20) != AuraFlags.AURA_FLAG_00)
                                {
                                    uint durationFull = gr.ReadUInt32();
                                    sb.AppendFormat("Full duration: {0}", durationFull.ToString("X8"));
                                    sb.AppendLine();

                                    uint durationRemaining = gr.ReadUInt32();
                                    sb.AppendFormat("Rem. duration: {0}", durationRemaining.ToString("X8"));
                                    sb.AppendLine();
                                }
                            }
                            sb.AppendLine();
                        }

                        if (gr.BaseStream.Position != gr.BaseStream.Length)
                            MessageBox.Show(String.Format("Packet structure changed! Opcode {0}", pkt.Opcode));
                    }
                    break;
            }

            return sb.ToString();
        }
    }
}
