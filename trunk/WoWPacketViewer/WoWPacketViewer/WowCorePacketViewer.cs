using System;
using System.IO;
using System.Text;

namespace WoWPacketViewer
{
    public class WowCorePacketViewer : PacketViewerBase
    {
        public override int LoadData(string file)
        {
            var gr = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read), Encoding.ASCII);
            gr.ReadBytes(3); // PKT
            gr.ReadBytes(2); // 0x02, 0x02
            gr.ReadByte();      // 0x06
            Build = gr.ReadUInt16(); // build
            gr.ReadBytes(4);    // client locale
            gr.ReadBytes(20); // packet key
            gr.ReadBytes(64);   // realm name

            while (gr.PeekChar() >= 0)
            {
                var direction = gr.ReadByte() == 0xff ? Direction.Server : Direction.Client;
                var unixtime = gr.ReadUInt32();
                var tickcount = gr.ReadUInt32();
                var size = gr.ReadUInt32();
                var opcode = (direction == Direction.Client) ? (OpCodes)gr.ReadUInt32() : (OpCodes)gr.ReadUInt16();
                var data = gr.ReadBytes((int)size
                                                - ((direction == Direction.Client) ? 4 : 2)
                     );

                m_packets.Add(new Packet(direction, opcode, data, unixtime, tickcount));
            }

            gr.Close();
            return m_packets.Count;
        }
    }
}
