using System;
using System.Collections.Generic;

namespace WoWPacketViewer {
	public abstract class PacketViewerBase {
		protected List<Packet> m_packets = new List<Packet>();

		public List<Packet> Packets {
			get { return m_packets; }
		}

		public abstract int LoadData(string file);

		public string ShowParsed(Packet pkt) {
			return Parser.CreateParser(pkt).Parse();
		}
	}
}
