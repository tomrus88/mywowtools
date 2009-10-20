using System.Collections.Generic;

namespace WoWPacketViewer
{
	public abstract class PacketViewerBase : IPacketReader
	{
		protected uint build;

		public uint Build
		{
			get { return build; }
		}

		public abstract IEnumerable<Packet> ReadPackets(string file);
	}
}