using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WoWPacketViewer {
	public struct Packet {
		Direction m_direction;
		OpCodes m_opcode;
		byte[] m_data;

		public Direction Direction {
			get { return m_direction; }
			set { m_direction = value; }
		}

		public OpCodes Opcode {
			get { return m_opcode; }
			set { m_opcode = value; }
		}

		public byte[] Data {
			get { return m_data; }
			set { m_data = value; }
		}

		public Packet(Direction direction, OpCodes opcode, byte[] data) {
			m_direction = direction;
			m_opcode = opcode;
			m_data = data;
		}

		public string HexLike() {
			int length = m_data.Length;

			string dir = string.Empty;

			if(m_direction == Direction.CLIENT)
				dir = "C->S";
			else
				dir = "S->C";

			StringBuilder result = new StringBuilder();

			result.AppendLine(string.Format("Packet {0}, {1} ({2}), len {3}", dir, m_opcode, (ushort)m_opcode, length));

			int byteIndex = 0;

			int full_lines = length >> 4;
			int rest = length & 0x0F;

			int offset = 0;

			if((full_lines == 0) && (rest == 0)) // empty packet (no data provided)
            {
				result.AppendLine("0000: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- | ................");
			}
			else {
				for(int i = 0; i < full_lines; ++i, byteIndex += 0x10) {
					StringBuilder bytes = new StringBuilder();
					StringBuilder chars = new StringBuilder();

					for(int j = 0; j < 0x10; ++j) {
						int c = m_data[offset];
						offset++;

						bytes.Append(c.ToString("X2"));

						bytes.Append(' ');

						if(c >= 0x20 && c < 0x80)
							chars.Append((char)c);
						else
							chars.Append('.');
					}

					result.AppendLine(byteIndex.ToString("X4") + ": " + bytes.ToString() + "| " + chars.ToString());
				}

				if(rest != 0) {
					StringBuilder bytes = new StringBuilder();
					StringBuilder chars = new StringBuilder(rest);

					for(int j = 0; j < 0x10; ++j) {
						if(j < rest) {
							int c = m_data[offset];
							offset++;

							bytes.Append(c.ToString("X2"));

							bytes.Append(' ');

							if(c >= 0x20 && c < 0x80)
								chars.Append((char)c);
							else
								chars.Append('.');
						}
						else {
							bytes.Append("-- ");
							chars.Append('.');
						}
					}
					result.AppendLine(byteIndex.ToString("X4") + ": " + bytes.ToString() + "| " + chars.ToString());
				}
			}

			result.AppendLine();

			return result.ToString();
		}
	}
}
