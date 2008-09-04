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
	}
}
