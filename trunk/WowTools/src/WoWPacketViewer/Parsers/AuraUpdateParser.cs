using System;
using System.Text;
using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_AURA_UPDATE)]
    [Parser(OpCodes.SMSG_AURA_UPDATE_ALL)]
    internal class AuraUpdateParser : Parser
    {
        [Flags]
        private enum AuraFlags : byte
        {
            None = 0x00,
            Index1 = 0x01,
            Index2 = 0x02,
            Index3 = 0x04,
            NotOwner = 0x08,
            Positive = 0x10,
            Duration = 0x20,
            Unk1 = 0x40,
            Negative = 0x80,
        }

        public override void Parse()
        {
            AppendFormatLine("GUID: {0:X16}", Reader.ReadPackedGuid());

            while (Reader.BaseStream.Position < Reader.BaseStream.Length)
            {
                AppendFormatLine("Slot: {0:X2}", Reader.ReadByte());

                var spellId = Reader.ReadUInt32();
                AppendFormatLine("Spell: {0:X8}", spellId);

                if (spellId > 0)
                {
                    var af = (AuraFlags)Reader.ReadByte();
                    AppendFormatLine("Flags: {0}", af);

                    AppendFormatLine("Level: {0:X2}", Reader.ReadByte());

                    AppendFormatLine("Charges: {0:X2}", Reader.ReadByte());

                    if (!af.HasFlag(AuraFlags.NotOwner))
                    {
                        AppendFormatLine("GUID2: {0:X16}", Reader.ReadPackedGuid());
                    }

                    if (af.HasFlag(AuraFlags.Duration))
                    {
                        AppendFormatLine("Full duration: {0:X8}", Reader.ReadUInt32());

                        AppendFormatLine("Rem. duration: {0:X8}", Reader.ReadUInt32());
                    }
                }
                AppendLine();
            }
        }
    }
}
