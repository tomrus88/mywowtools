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

        protected uint m_build;

        public uint Build
        {
            get { return m_build; }
            set { m_build = value; }
        }

        public abstract int LoadData(string file);

        public string ShowParsed(Packet pkt)
        {
            return ParserFactory.CreateParser(pkt).Parse();
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
