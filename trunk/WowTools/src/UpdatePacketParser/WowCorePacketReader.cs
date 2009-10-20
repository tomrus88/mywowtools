using System.Collections.Generic;
using System.IO;
using System.Text;
using UpdateFields;
using WowTools.Core;

namespace UpdatePacketParser
{
    public class WowCorePacketReader : IPacketReader
    {
        private readonly BinaryReader _reader;

        public WowCorePacketReader(string filename)
        {
			_reader = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read), Encoding.ASCII);
            _reader.ReadBytes(3);                    // PKT
            _reader.ReadBytes(2);                    // 0x02, 0x02
            _reader.ReadByte();                      // 0x06
            var build = _reader.ReadUInt16();     // build
            _reader.ReadBytes(4);                    // client locale
            _reader.ReadBytes(20);                   // packet key
            _reader.ReadBytes(64);                   // realm name

            UpdateFieldsLoader.LoadUpdateFields(build);
        }

        public Packet ReadPacket()
        {
            if (_reader.PeekChar() < 0)
                return null;

            var direction = _reader.ReadByte();
            var unixtime = _reader.ReadUInt32();
            var tickcount = _reader.ReadUInt32();

            var packet = new Packet();
            packet.Size = _reader.ReadInt32() - (direction == 0xFF ? 2 : 4);
            packet.Code = (OpCodes) (direction == 0xFF ? _reader.ReadInt16() : _reader.ReadInt32());
            packet.Data = _reader.ReadBytes(packet.Size);
            return packet;
        }

    	public virtual IEnumerable<Packet> ReadPackets()
    	{
    		var packets = new List<Packet>();
    		Packet packet;
    		while ((packet = ReadPacket()) != null)
    		{
    			packets.Add(packet);
    		}
    		return packets;
    	}
    }
}
