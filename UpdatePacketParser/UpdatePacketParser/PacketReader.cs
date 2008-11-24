using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpdatePacketParser
{
    public class Packet
    {
        public int Size { get; set; }
        public int Code { get; set; }
        public byte[] Data { get; set; }
    }

    public abstract class PacketReaderBase
    {
        public abstract Packet ReadPacket();
    }
}
