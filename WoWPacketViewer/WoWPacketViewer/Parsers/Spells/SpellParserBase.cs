using System.IO;

namespace WoWPacketViewer.Parsers.Spells
{
    internal abstract class SpellParserBase : Parser
    {
        protected SpellParserBase(Packet packet)
            : base(packet)
        {
        }

        public TargetFlags ReadTargets(BinaryReader br)
        {
            var tf = (TargetFlags)br.ReadUInt32();
            AppendFormatLine("TargetFlags: {0}", tf);

            if ((tf & (TargetFlags.TARGET_FLAG_UNIT |
                       TargetFlags.TARGET_FLAG_PVP_CORPSE |
                       TargetFlags.TARGET_FLAG_OBJECT |
                       TargetFlags.TARGET_FLAG_CORPSE |
                       TargetFlags.TARGET_FLAG_UNK2)) != TargetFlags.TARGET_FLAG_SELF)
            {
                AppendFormatLine("ObjectTarget: 0x{0:X16}", br.ReadPackedGuid());
            }

            if ((tf & (TargetFlags.TARGET_FLAG_ITEM |
                       TargetFlags.TARGET_FLAG_TRADE_ITEM)) != TargetFlags.TARGET_FLAG_SELF)
            {
                AppendFormatLine("ItemTarget: 0x{0:X16}", br.ReadPackedGuid());
            }

            if ((tf & TargetFlags.TARGET_FLAG_SOURCE_LOCATION) != TargetFlags.TARGET_FLAG_SELF)
            {
                AppendFormatLine("SrcTarget: {0} {1} {2}", br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            }

            if ((tf & TargetFlags.TARGET_FLAG_DEST_LOCATION) != TargetFlags.TARGET_FLAG_SELF)
            {
                AppendFormatLine("DstTargetGuid: {0}", br.ReadPackedGuid().ToString("X16"));
                AppendFormatLine("DstTarget: {0} {1} {2}", br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            }

            if ((tf & TargetFlags.TARGET_FLAG_STRING) != TargetFlags.TARGET_FLAG_SELF)
            {
                AppendFormatLine("StringTarget: {0}", br.ReadCString());
            }

            return tf;
        }
    }
}