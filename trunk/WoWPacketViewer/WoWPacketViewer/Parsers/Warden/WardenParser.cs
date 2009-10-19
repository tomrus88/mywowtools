using System.Collections.Generic;
using WoWPacketViewer.Parsers.Warden;

namespace WoWPacketViewer.Parsers
{
	[Parser(OpCodes.CMSG_WARDEN_DATA)]
	[Parser(OpCodes.SMSG_WARDEN_DATA)]
	internal abstract class MsgWardenData : Parser
	{
		protected static Dictionary<byte, CheckType> s_checkTypes = new Dictionary<byte, CheckType>();
		protected static List<CheckInfo> s_lastCheckInfo = new List<CheckInfo>();
		protected static FrmWardenDebug s_wardenDebugForm;

		protected MsgWardenData(Packet packet)
			: base(packet)
		{
		}

		public static void InitCheckTypes(Dictionary<byte, CheckType> checkTypes)
		{
			s_checkTypes = checkTypes;
		}
	}
}