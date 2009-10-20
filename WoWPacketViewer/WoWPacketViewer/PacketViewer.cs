using System.Collections.Generic;

namespace WoWPacketViewer
{
	public abstract class PacketViewerBase
	{
		protected uint build;

		public uint Build
		{
			get { return build; }
		}

		public static PacketViewerBase Create(string extension)
		{
			switch (extension)
			{
				case ".bin":
					return new WowCorePacketViewer();
				case ".sqlite":
					return new SqLitePacketViewer();
				case ".xml":
					return new SniffitztPacketViewer();
				default:
					return null;
			}
		}

		public abstract IEnumerable<Packet> ReadPackets(string file);
	}
}