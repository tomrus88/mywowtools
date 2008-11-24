using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpdateFields;
using System.Xml;

namespace UpdatePacketParser
{
    public class SniffitztPacketReader : PacketReaderBase
    {
        private XmlDocument _document;
        private XmlNodeList _packets;
        private int _readPackets = 0;

        public SniffitztPacketReader(string filename)
        {
            _document = new XmlDocument();
            _document.Load(filename);

            uint build = Convert.ToUInt32(_document.GetElementsByTagName("header")[0].Attributes["clientBuild"].Value);

            _packets = _document.GetElementsByTagName("packet");

            UpdateFieldsLoader.LoadUpdateFields(build);
        }

        public override Packet ReadPacket()
        {
            if (_readPackets >= _packets.Count)
            {
                return null;
            }

            XmlNode element = _packets[_readPackets];

            var data = element.InnerText;

            int len = data.Length / 2;

            byte[] bytes = new byte[len];

            for (int i = 0; i < len; ++i)
            {
                int pos = i * 2;
                string str = data[pos].ToString();
                str += data[pos + 1];
                bytes[i] = byte.Parse(str, System.Globalization.NumberStyles.HexNumber);
            }

            var packet = new Packet();
            packet.Size = len;
            packet.Code = Convert.ToInt32(element.Attributes["opcode"].Value);
            packet.Data = bytes;

            _readPackets++;
            return packet;
        }
    }
}
