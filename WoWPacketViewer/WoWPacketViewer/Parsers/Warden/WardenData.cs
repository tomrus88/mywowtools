using System;
using System.Collections.Generic;
using System.Windows.Forms;

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

			TextBox[] textBoxes = wardenDebugForm.GetTextBoxes();
			if (textBoxes.Length != 0)
			{
				foreach (TextBox tb in textBoxes)
				{
					if (tb.Name == "textBox1")
					{
						tb.Text = String.Empty;
						foreach (string str in strings)
						{
							tb.Text += str;
							tb.Text += "\r\n";
						}
						tb.Text += Utility.PrintHex(checks, 0, checks.Length);
						continue;
					}

					byte val = 0;
					if (GetByteForCheckType((CheckType) tb.TabIndex, ref val))
						tb.Text = String.Format("{0:X2}", val);
				}
			}

			if (!wardenDebugForm.Visible)
			{
				wardenDebugForm.Show();
			}
		}

		private static bool GetByteForCheckType(CheckType checkType, ref byte val)
		{
			foreach (var temp in CheckTypes)
			{
				if (temp.Value == checkType)
				{
					val = temp.Key;
					return true;
				}
			}
			return false;
		}
	}
}