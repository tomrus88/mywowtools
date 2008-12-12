using System;
using System.Collections.Generic;

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

        public string ShowParsed(Packet pkt)
        {
            return Parser.CreateParser(pkt).Parse();
        }

        public static PacketViewerBase Create(string extension)
        {
            switch (extension)
            {
            case ".bin":
                return new WowCorePacketViewer();
            case ".sqlite":
                return new SqLitePacketViewer();
            case ".xml":
                return new SniffitztPacketViewer();
            default:
                return null;
            }
        }
    }
}