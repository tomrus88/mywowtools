using System;
using System.IO;
using System.Text;

namespace WoWPacketViewer {
	public class Packet {
		public Direction Direction { get; private set; }

		public OpCodes Code { get; private set; }

		public byte[] Data { get; private set; }

		public Packet(Direction direction, OpCodes opcode, byte[] data) {
			Direction = direction;
			Code = opcode;
			Data = data;
		}

		public virtual BinaryReader CreateReader() {
			return new BinaryReader(new MemoryStream(Data));
		}

		public string HexLike() {
			var length = Data.Length;
			var dir = (Direction == Direction.Client) ? "C->S" : "S->C";

			var result = new StringBuilder();
			result.AppendFormat("Packet {0}, {1} ({2}), len {3}", dir, Code, (ushort)Code, length);
			result.AppendLine();

			if(length == 0) {
				result.AppendLine("0000: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- : ................");
			}
			else {
				var offset = 0;
				for(var i = 0; i < length; i += 0x10) {
					var bytes = new StringBuilder();
					var chars = new StringBuilder();

					for(var j = 0; j < 0x10; ++j) {
						if(offset < length) {
							int c = Data[offset];
							offset++;

							bytes.AppendFormat("{0,-3:X2}", c);
							chars.Append((c >= 0x20 && c < 0x80) ? (char)c : '.');
						}
						else {
							bytes.Append("-- ");
							chars.Append('.');
						}
					}

					result.AppendLine(i.ToString("X4") + ": " + bytes + ": " + chars);
				}
			}

			result.AppendLine();

			return result.ToString();
		}
	}
}
