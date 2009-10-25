using System.IO;
using WowTools.Core;

namespace WoWObjects
{
    public class TransportInfo
    {
        public ulong Guid { get; private set; }
        public Coords4 Position { get; private set; }
        public uint Time { get; private set; }
        public byte Seat { get; private set; }

        public static TransportInfo Read(BinaryReader reader)
        {
            return new TransportInfo
            {
                Guid = reader.ReadPackedGuid(),
                Position = reader.ReadCoords4(),
                Time = reader.ReadUInt32(),
                Seat = reader.ReadByte()
            };
        }
    }
}
