using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WoWPacketViewer {
	public abstract class PacketViewerBase {
		protected List<Packet> m_packets = new List<Packet>();

		public abstract int LoadData(string file);

		public ListViewItem[] ListPackets() {
			IEnumerable<Packet> pkts =
				from packet in m_packets
				where packet.Opcode == OpCodes.CMSG_AUTH_SESSION
				select packet;

			Packet pkt;
			try {
				pkt = pkts.First();
			} catch(Exception exc) {
				MessageBox.Show(exc.Message);
				return null;
			}

			uint build;
			try {
				build = BitConverter.ToUInt32(pkt.Data, 0);
			} catch(Exception exc) {
				MessageBox.Show(exc.Message);
				return null;
			}

			ListViewItem[] items = new ListViewItem[m_packets.Count];
			int i = 0;
			foreach(Packet p in m_packets) {
				ListViewItem item;
				if(p.Direction == Direction.CLIENT)
					item = new ListViewItem(new string[] { build.ToString(), p.Opcode.ToString(), "", p.Data.Length.ToString() });
				else
					item = new ListViewItem(new string[] { build.ToString(), "", p.Opcode.ToString(), p.Data.Length.ToString() });
				items[i] = item;
				i++;
			}

			return items;
		}

		public void ShowHex(TextBox tb, int index) {
			Packet pkt = m_packets[index];

			tb.Text = pkt.HexLike();
		}

		public void ShowParsed(TextBox tb, int index) {

		}
	}
}
