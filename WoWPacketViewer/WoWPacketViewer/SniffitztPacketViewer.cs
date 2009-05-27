using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace WoWPacketViewer
{
    public class SniffitztPacketViewer : PacketViewerBase
    {
        public override int LoadData(string file)
        {
            var uri = new Uri(Path.GetFullPath(file)).ToString();
            var doc = XDocument.Load(uri);
            foreach (var packet in doc.XPathSelectElements("*/packet"))
            {
                var direction = (string)packet.Attribute("direction") == "S2C"
                                          ? Direction.Server
                                          : Direction.Client;
                var opcode = (OpCodes)(uint)packet.Attribute("opcode");
                m_packets.Add(new Packet(direction, opcode, Utility.HexStringToBinary(packet.Value), 0, 0));
            }
            return m_packets.Count;
        }
    }
}
