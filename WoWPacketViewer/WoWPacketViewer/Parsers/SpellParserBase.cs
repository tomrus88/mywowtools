using System.IO;

namespace WoWPacketViewer.Parsers
{
	internal abstract class SpellParserBase : Parser
	{
		protected SpellParserBase(Packet packet)
			: base(packet)
		{
		}

		public SpellStartParser.TargetFlags ReadTargets(BinaryReader br)
		{
			var tf = (SpellStartParser.TargetFlags)br.ReadUInt32();
			AppendFormatLine("TargetFlags: {0}", tf);

			if ((tf & (SpellStartParser.TargetFlags.TARGET_FLAG_UNIT |
			           SpellStartParser.TargetFlags.TARGET_FLAG_PVP_CORPSE |
			           SpellStartParser.TargetFlags.TARGET_FLAG_OBJECT |
			           SpellStartParser.TargetFlags.TARGET_FLAG_CORPSE |
			           SpellStartParser.TargetFlags.TARGET_FLAG_UNK2)) != SpellStartParser.TargetFlags.TARGET_FLAG_SELF)
			{
				AppendFormatLine("ObjectTarget: 0x{0:X16}", br.ReadPackedGuid());
			}

			if ((tf & (SpellStartParser.TargetFlags.TARGET_FLAG_ITEM |
			           SpellStartParser.TargetFlags.TARGET_FLAG_TRADE_ITEM)) != SpellStartParser.TargetFlags.TARGET_FLAG_SELF)
			{
				AppendFormatLine("ItemTarget: 0x{0:X16}", br.ReadPackedGuid());
			}

			if ((tf & SpellStartParser.TargetFlags.TARGET_FLAG_SOURCE_LOCATION) != SpellStartParser.TargetFlags.TARGET_FLAG_SELF)
			{
				AppendFormatLine("SrcTarget: {0} {1} {2}", br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
			}

			if ((tf & SpellStartParser.TargetFlags.TARGET_FLAG_DEST_LOCATION) != SpellStartParser.TargetFlags.TARGET_FLAG_SELF)
			{
				AppendFormatLine("DstTargetGuid: {0}", br.ReadPackedGuid().ToString("X16"));
				AppendFormatLine("DstTarget: {0} {1} {2}", br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
			}

			if ((tf & SpellStartParser.TargetFlags.TARGET_FLAG_STRING) != SpellStartParser.TargetFlags.TARGET_FLAG_SELF)
			{
				AppendFormatLine("StringTarget: {0}", br.ReadCString());
			}

			return tf;
		}
	}
}