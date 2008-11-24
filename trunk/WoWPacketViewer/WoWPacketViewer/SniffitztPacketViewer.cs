using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace WoWPacketViewer
{
    public class SniffitztPacketViewer : PacketViewerBase
    {
        public override int LoadData(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            XmlNodeList packets = doc.GetElementsByTagName("packet");

            foreach (XmlElement packet in packets)
            {
                var direction = packet.Attributes["direction"].Value == "S2C" ? Direction.Server : Direction.Client;
                var opcode = (OpCodes)Convert.ToUInt32(packet.Attributes["opcode"].Value);
                var data = packet.InnerText;

                int len = data.Length / 2;
                byte[] bytes = new byte[len];

                for (int i = 0; i < len; ++i)
                {
                    int pos = i * 2;
                    string str = data[pos].ToString();
                    str += data[pos + 1];
                    bytes[i] = byte.Parse(str, System.Globalization.NumberStyles.HexNumber);
                }

                m_packets.Add(new Packet(direction, opcode, bytes));
            }

            return m_packets.Count;
        }
    }
}
