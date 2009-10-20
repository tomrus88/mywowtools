namespace WoWPacketViewer
{
	public static class PacketReaderFactory
	{
		public static IPacketReader Create(string extension)
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
	}
}