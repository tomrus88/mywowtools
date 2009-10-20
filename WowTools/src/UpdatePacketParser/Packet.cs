namespace UpdatePacketParser
{
	public class Packet
	{
		public int Size { get; set; }
		public int Code { get; set; }
		public byte[] Data { get; set; }
	}
}