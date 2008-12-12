using System;
using System.Text;
using System.IO;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_SPELL_GO)]
    internal class SpellGoParser : Parser
    {
        public SpellGoParser(Packet packet)
            : base(packet)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            var sb = new StringBuilder();

            sb.AppendFormat("Caster: 0x{0:X16}", gr.ReadPackedGuid()).AppendLine();
            sb.AppendFormat("Target: 0x{0:X16}", gr.ReadPackedGuid()).AppendLine();
            sb.AppendFormat("Pending Cast: {0}", gr.ReadByte()).AppendLine();
            sb.AppendFormat("Spell Id: {0}", gr.ReadUInt32()).AppendLine();
            SpellStartParser.CastFlags cf = (SpellStartParser.CastFlags)gr.ReadUInt32();
            sb.AppendFormat("Cast Flags: {0}", cf).AppendLine();
            sb.AppendFormat("TicksCount: {0}", gr.ReadUInt32()).AppendLine();

            readSpellGoTargets(gr, sb);

            SpellStartParser.TargetFlags tf = SpellStartParser.ReadTargets(gr, sb);

            if ((cf & SpellStartParser.CastFlags.CAST_FLAG_12) != SpellStartParser.CastFlags.CAST_FLAG_00)
            {
                sb.AppendFormat("PredictedPower: {0}", gr.ReadUInt32()).AppendLine();
            }

            if ((cf & SpellStartParser.CastFlags.CAST_FLAG_22) != SpellStartParser.CastFlags.CAST_FLAG_00)
            {
                var v1 = gr.ReadByte();
                sb.AppendFormat("Cooldowns Before: {0}", (SpellStartParser.CooldownMask)v1).AppendLine();
                var v2 = gr.ReadByte();
                sb.AppendFormat("Cooldowns Now: {0}", (SpellStartParser.CooldownMask)v2).AppendLine();

                for (int i = 0; i < 6; ++i)
                {
                    var v3 = (i << i);

                    if ((v3 & v1) != 0)
                    {
                        if (!((v3 & v2) != 0))
                        {
                            var v4 = gr.ReadByte();
                            sb.AppendFormat("Cooldown for {0} is {1}", (SpellStartParser.CooldownMask)v3, v4).AppendLine();
                        }
                    }
                }
            }

            if ((cf & SpellStartParser.CastFlags.CAST_FLAG_18) != SpellStartParser.CastFlags.CAST_FLAG_00)
            {
                sb.AppendFormat("0x20000: Unk float {0}, unk int {1}", gr.ReadSingle(), gr.ReadUInt32()).AppendLine();
            }

            if ((cf & SpellStartParser.CastFlags.CAST_FLAG_06) != SpellStartParser.CastFlags.CAST_FLAG_00)
            {
                sb.AppendFormat("Projectile displayid {0}, inventoryType {1}", gr.ReadUInt32(), gr.ReadUInt32()).AppendLine();
            }

            if ((cf & SpellStartParser.CastFlags.CAST_FLAG_20) != SpellStartParser.CastFlags.CAST_FLAG_00)
            {
                sb.AppendFormat("cast flags & 0x80000: Unk int {0}, uint int {1}", gr.ReadUInt32(), gr.ReadUInt32()).AppendLine();
            }

            if ((tf & SpellStartParser.TargetFlags.TARGET_FLAG_DEST_LOCATION) != SpellStartParser.TargetFlags.TARGET_FLAG_SELF)
            {
                sb.AppendFormat("targetFlags & 0x40: byte {0}", gr.ReadByte()).AppendLine();
            }

            if (gr.BaseStream.Position != gr.BaseStream.Length)
            {
                throw new Exception("Packet structure changed!");
            }

            return sb.ToString();
        }

        public static void readSpellGoTargets(BinaryReader br, StringBuilder sb)
        {
            byte hitCount = br.ReadByte();

            for (byte i = 0; i < hitCount; ++i)
            {
                sb.AppendFormat("GO Hit Target {0}: 0x{0:X16}", i, br.ReadUInt64()).AppendLine();
            }

            byte missCount = br.ReadByte();

            for (byte i = 0; i < missCount; ++i)
            {
                sb.AppendFormat("GO Miss Target {0}: 0x{0:X16}", i, br.ReadUInt64()).AppendLine();
                byte missReason = br.ReadByte();
                sb.AppendFormat("GO Miss Reason {0}: {1}", i, missReason).AppendLine();
                if (missReason == 11) // reflect
                {
                    sb.AppendFormat("GO Reflect Reason {0}: {1}", i, br.ReadByte()).AppendLine();
                }
            }
        }
    }
}
