using System;
using System.Text;
using System.IO;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_SPELL_GO)]
    internal class SpellGoParser : SpellParserBase
    {
        public SpellGoParser(Packet packet)
            : base(packet)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            AppendFormatLine("Caster: 0x{0:X16}", gr.ReadPackedGuid());
            AppendFormatLine("Target: 0x{0:X16}", gr.ReadPackedGuid());
            AppendFormatLine("Pending Cast: {0}", gr.ReadByte());
            AppendFormatLine("Spell Id: {0}", gr.ReadUInt32());
            var cf = (SpellStartParser.CastFlags)gr.ReadUInt32();
            AppendFormatLine("Cast Flags: {0}", cf);
            AppendFormatLine("TicksCount: {0}", gr.ReadUInt32());

            readSpellGoTargets(gr);

            var tf = ReadTargets(gr);

            if ((cf & SpellStartParser.CastFlags.CAST_FLAG_12) != SpellStartParser.CastFlags.CAST_FLAG_00)
            {
                AppendFormatLine("PredictedPower: {0}", gr.ReadUInt32());
            }

            if ((cf & SpellStartParser.CastFlags.CAST_FLAG_22) != SpellStartParser.CastFlags.CAST_FLAG_00)
            {
                var v1 = gr.ReadByte();
                AppendFormatLine("Cooldowns Before: {0}", (SpellStartParser.CooldownMask)v1);
                var v2 = gr.ReadByte();
                AppendFormatLine("Cooldowns Now: {0}", (SpellStartParser.CooldownMask)v2);

                for (var i = 0; i < 6; ++i)
                {
                    var v3 = (1 << i);

                    if ((v3 & v1) != 0)
                    {
                        if (!((v3 & v2) != 0))
                        {
                            var v4 = gr.ReadByte();
                            AppendFormatLine("Cooldown for {0} is {1}", (SpellStartParser.CooldownMask)v3, v4);
                        }
                    }
                }
            }

            if ((cf & SpellStartParser.CastFlags.CAST_FLAG_18) != SpellStartParser.CastFlags.CAST_FLAG_00)
            {
                AppendFormatLine("0x20000: Unk float {0}, unk int {1}", gr.ReadSingle(), gr.ReadUInt32());
            }

            if ((cf & SpellStartParser.CastFlags.CAST_FLAG_06) != SpellStartParser.CastFlags.CAST_FLAG_00)
            {
                AppendFormatLine("Projectile displayid {0}, inventoryType {1}", gr.ReadUInt32(), gr.ReadUInt32());
            }

            if ((cf & SpellStartParser.CastFlags.CAST_FLAG_20) != SpellStartParser.CastFlags.CAST_FLAG_00)
            {
                AppendFormatLine("cast flags & 0x80000: Unk int {0}, uint int {1}", gr.ReadUInt32(), gr.ReadUInt32());
            }

            if ((tf & SpellStartParser.TargetFlags.TARGET_FLAG_DEST_LOCATION) != SpellStartParser.TargetFlags.TARGET_FLAG_SELF)
            {
                AppendFormatLine("targetFlags & 0x40: byte {0}", gr.ReadByte());
            }

            CheckPacket(gr);

            return GetParsedString();
        }

        public void readSpellGoTargets(BinaryReader br)
        {
            var hitCount = br.ReadByte();

            for (var i = 0; i < hitCount; ++i)
            {
                AppendFormatLine("GO Hit Target {0}: 0x{0:X16}", i, br.ReadUInt64());
            }

            var missCount = br.ReadByte();

            for (var i = 0; i < missCount; ++i)
            {
                AppendFormatLine("GO Miss Target {0}: 0x{0:X16}", i, br.ReadUInt64());
                var missReason = br.ReadByte();
                AppendFormatLine("GO Miss Reason {0}: {1}", i, missReason);
                if (missReason == 11) // reflect
                {
                    AppendFormatLine("GO Reflect Reason {0}: {1}", i, br.ReadByte());
                }
            }
        }
    }
}
