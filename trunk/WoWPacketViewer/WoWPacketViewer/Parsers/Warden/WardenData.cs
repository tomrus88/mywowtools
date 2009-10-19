using System.Collections.Generic;
using System.Text;

namespace WoWPacketViewer.Parsers.Warden
{
	internal static class WardenData
	{
		public static IList<CheckInfo> CheckInfos = new List<CheckInfo>();
		public static IDictionary<byte, CheckType> CheckTypes = new Dictionary<byte, CheckType>();
		private static FrmWardenDebug wardenDebugForm;

		public static void InitCheckTypes(IDictionary<byte, CheckType> checkTypes)
		{
			CheckTypes = checkTypes;
		}

		public static void ShowForm(IEnumerable<string> strings, byte[] checks)
		{
			if (wardenDebugForm == null || wardenDebugForm.IsDisposed)
			{
				wardenDebugForm = new FrmWardenDebug();
			}

			wardenDebugForm.XorByte = checks[checks.Length - 1];

			wardenDebugForm.SetInfo(CreateTextInfo(strings, checks));

			wardenDebugForm.SetCheckTypes(CheckTypes);

			if (!wardenDebugForm.Visible)
			{
				wardenDebugForm.Show();
			}
		}

		private static string CreateTextInfo(IEnumerable<string> strings, byte[] checks)
		{
			var sb = new StringBuilder();
			foreach (string s in strings)
			{
				sb.AppendLine(s);
			}
			sb.AppendLine();
			sb.AppendLine(Utility.PrintHex(checks, 0, checks.Length));
			return sb.ToString();
		}
	}
}