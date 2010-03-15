using System.IO;

namespace WowTools.Core
{
    public class TransportInfo
    {
        public ulong Guid { get; private set; }
        public Coords4 Position { get; private set; }
        public uint Time { get; private set; }
        public byte Seat { get; private set; }
        public uint Time2 { get; private set; }

        public static TransportInfo Read(BinaryReader reader, MovementFlags2 flags2)
        {
            var tInfo = new TransportInfo();
            tInfo.Guid = reader.ReadPackedGuid();
            tInfo.Position = reader.ReadCoords4();
            tInfo.Time = reader.ReadUInt32();
            tInfo.Seat = reader.ReadByte();
            if ((flags2 & MovementFlags2.InterpolatedPlayerMovement) != MovementFlags2.None)
                tInfo.Time2 = reader.ReadUInt32();
            return tInfo;
        }
    }
}
