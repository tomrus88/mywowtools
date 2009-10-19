using System.Collections.Generic;

namespace WoWPacketViewer.Parsers.Warden
{
	internal static class WardenData
	{
		public static IList<CheckInfo> CheckInfos = new List<CheckInfo>();
		public static IDictionary<byte, CheckType> CheckTypes = new Dictionary<byte, CheckType>();
		public static FrmWardenDebug WardenDebugForm;

		public static void InitCheckTypes(IDictionary<byte, CheckType> checkTypes)
		{
			CheckTypes = checkTypes;
		}
	}
}