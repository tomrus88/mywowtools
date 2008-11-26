using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WoWPacketViewer
{
    public abstract class PacketViewerBase
    {
        protected List<Packet> m_packets = new List<Packet>();

        public List<Packet> Packets
        {
            get { return m_packets; }
        }

        public abstract int LoadData(string file);

        public string HexLike(Packet pkt)
        {
            var length = pkt.Data.Length;
            var dir = (pkt.Direction == Direction.Client) ? "C->S" : "S->C";

            var result = new StringBuilder();
            result.AppendFormat("Packet {0}, {1} ({2}), len {3}", dir, pkt.Opcode, (ushort)pkt.Opcode, length);
            result.AppendLine();

            if (length == 0)
            { // empty packet (no data provided)
                result.AppendLine("0000: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- | ................");
            }
            else
            {
                int offset = 0;
                for (int i = 0; i < length; i += 0x10)
                {
                    var bytes = new StringBuilder();
                    var chars = new StringBuilder();

                    for (int j = 0; j < 0x10; ++j)
                    {
                        if (offset < length)
                        {
                            int c = pkt.Data[offset];
                            offset++;

                            bytes.AppendFormat("{0,-3:X2}", c);
                            chars.Append((c >= 0x20 && c < 0x80) ? (char)c : '.');
                        }
                        else
                        {
                            bytes.Append("-- ");
                            chars.Append('.');
                        }
                    }

                    result.AppendLine(i.ToString("X4") + ": " + bytes.ToString() + "| " + chars.ToString());
                }
            }

            result.AppendLine();

            return result.ToString();
        }

        public string ShowParsed(Packet pkt)
        {
            GenericReader gr = new GenericReader(new MemoryStream(pkt.Data));
            StringBuilder sb = new StringBuilder();

            switch (pkt.Opcode)
            {
                case OpCodes.SMSG_AURA_UPDATE:
                case OpCodes.SMSG_AURA_UPDATE_ALL:
                    ParseAuraUpdateOpcode(pkt, gr, sb);
                    break;
                case OpCodes.SMSG_INITIAL_SPELLS:
                    ParseInitialSpellsOpcode(pkt, gr, sb);
                    break;
                case OpCodes.SMSG_PET_SPELLS:
                    ParsePetSpellsOpcode(pkt, gr, sb);
                    break;
                case OpCodes.SMSG_MONSTER_MOVE:
                case OpCodes.SMSG_MONSTER_MOVE_TRANSPORT:
                    ParseMonsterMoveOpcode(pkt, gr, sb);
                    break;
            }

            return sb.ToString();
        }

        private static void ParseAuraUpdateOpcode(Packet pkt, GenericReader gr, StringBuilder sb)
        {
            ulong guid = gr.ReadPackedGuid();
            sb.AppendFormat("GUID: {0}", guid.ToString("X16"));
            sb.AppendLine();
            sb.AppendLine();

            while (gr.BaseStream.Position < gr.BaseStream.Length)
            {
                byte slot = gr.ReadByte();
                sb.AppendFormat("Slot: {0}", slot.ToString("X2"));
                sb.AppendLine();

                uint spellid = gr.ReadUInt32();
                sb.AppendFormat("Spell: {0}", spellid.ToString("X8"));
                sb.AppendLine();

                if (spellid > 0)
                {
                    AuraFlags af = (AuraFlags)gr.ReadByte();
                    sb.AppendFormat("Flags: {0}", af);
                    sb.AppendLine();

                    byte auraLevel = gr.ReadByte();
                    sb.AppendFormat("Level: {0}", auraLevel.ToString("X2"));
                    sb.AppendLine();

                    byte auraApplication = gr.ReadByte();
                    sb.AppendFormat("Charges: {0}", auraApplication.ToString("X2"));
                    sb.AppendLine();

                    if (!((af & AuraFlags.AURA_FLAG_NOT_OWNER) != AuraFlags.AURA_FLAG_NONE))
                    {
                        ulong guid2 = gr.ReadPackedGuid();
                        sb.AppendFormat("GUID2: {0}", guid2.ToString("X16"));
                        sb.AppendLine();
                    }

                    if ((af & AuraFlags.AURA_FLAG_DURATION) != AuraFlags.AURA_FLAG_NONE)
                    {
                        uint durationFull = gr.ReadUInt32();
                        sb.AppendFormat("Full duration: {0}", durationFull.ToString("X8"));
                        sb.AppendLine();

                        uint durationRemaining = gr.ReadUInt32();
                        sb.AppendFormat("Rem. duration: {0}", durationRemaining.ToString("X8"));
                        sb.AppendLine();
                    }
                }
                sb.AppendLine();
            }

            if (gr.BaseStream.Position != gr.BaseStream.Length)
                MessageBox.Show(String.Format("Packet structure changed! Opcode {0}", pkt.Opcode));
        }

        private static void ParseInitialSpellsOpcode(Packet pkt, GenericReader gr, StringBuilder sb)
        {
            byte unk = gr.ReadByte();
            sb.AppendFormat("Unk: {0}", unk);
            sb.AppendLine();

            ushort spellsCount = gr.ReadUInt16();
            sb.AppendFormat("Spells count: {0}", spellsCount);
            sb.AppendLine();

            for (ushort i = 0; i < spellsCount; ++i)
            {
                ushort spellId = gr.ReadUInt16();
                ushort spellSlot = gr.ReadUInt16();

                sb.AppendFormat("Spell: id {0}, slot {1}", spellId, spellSlot);
                sb.AppendLine();
            }

            ushort cooldownsCount = gr.ReadUInt16();
            sb.AppendFormat("Cooldowns count: {0}", cooldownsCount);
            sb.AppendLine();

            for (ushort i = 0; i < cooldownsCount; ++i)
            {
                ushort spellId = gr.ReadUInt16();
                ushort itemId = gr.ReadUInt16();
                ushort category = gr.ReadUInt16();
                uint coolDown1 = gr.ReadUInt32();
                uint coolDown2 = gr.ReadUInt32();

                sb.AppendFormat("Cooldown: spell {0}, item {1}, cat {2}, time1 {3}, time2 {4}", spellId, itemId, category, coolDown1, coolDown2);
                sb.AppendLine();
            }

            if (gr.BaseStream.Position != gr.BaseStream.Length)
                MessageBox.Show(String.Format("Packet structure changed! Opcode {0}", pkt.Opcode));
        }

        private static void ParsePetSpellsOpcode(Packet pkt, GenericReader gr, StringBuilder sb)
        {
            ulong guid = gr.ReadUInt64();
            sb.AppendFormat("GUID: {0}", guid.ToString("X16"));
            sb.AppendLine();

            uint petFamily = gr.ReadUInt32();
            sb.AppendFormat("Pet family: {0}", petFamily);
            sb.AppendLine();

            uint unk1 = gr.ReadUInt32();
            uint unk2 = gr.ReadUInt32();
            sb.AppendFormat("Unk1: {0}, Unk2: {1}", unk1, unk2.ToString("X8"));
            sb.AppendLine();

            for (ushort i = 0; i < 10; ++i)
            {
                ushort spellOrAction = gr.ReadUInt16();
                ushort type = gr.ReadUInt16();

                sb.AppendFormat("SpellOrAction: id {0}, type {1}", spellOrAction, type.ToString("X4"));
                sb.AppendLine();
            }

            byte spellsCount = gr.ReadByte();
            sb.AppendFormat("Spells count: {0}", spellsCount);
            sb.AppendLine();

            for (ushort i = 0; i < spellsCount; ++i)
            {
                ushort spellId = gr.ReadUInt16();
                ushort active = gr.ReadUInt16();

                sb.AppendFormat("Spell {0}, active {1}", spellId, active.ToString("X4"));
                sb.AppendLine();
            }

            byte cooldownsCount = gr.ReadByte();
            sb.AppendFormat("Cooldowns count: {0}", cooldownsCount);
            sb.AppendLine();

            for (byte i = 0; i < cooldownsCount; ++i)
            {
                ushort spell = gr.ReadUInt16();
                ushort category = gr.ReadUInt16();
                uint cooldown = gr.ReadUInt32();
                uint categoryCooldown = gr.ReadUInt32();

                sb.AppendFormat("Cooldown: spell {0}, category {1}, cooldown {2}, categoryCooldown {3}", spell, category, cooldown, categoryCooldown);
                sb.AppendLine();
            }

            if (gr.BaseStream.Position != gr.BaseStream.Length)
                MessageBox.Show(String.Format("Packet structure changed! Opcode {0}", pkt.Opcode));
        }

        private static void ParseMonsterMoveOpcode(Packet pkt, GenericReader gr, StringBuilder sb)
        {
            ulong monsterGuid = gr.ReadPackedGuid();
            sb.AppendFormat("Monster GUID: 0x{0}", monsterGuid.ToString("X16"));
            sb.AppendLine();

            if (pkt.Opcode == OpCodes.SMSG_MONSTER_MOVE_TRANSPORT)
            {
                ulong transportGuid = gr.ReadPackedGuid();
                sb.AppendFormat("Transport GUID: 0x{0}", transportGuid.ToString("X16"));
                sb.AppendLine();

                byte seatPos = gr.ReadByte();
                sb.AppendFormat("Seat Position: 0x{0}", seatPos.ToString("X2"));
                sb.AppendLine();
            }

            Coords3 position = gr.ReadCoords3();
            sb.AppendFormat("Dest Position: {0}", position.GetCoords());
            sb.AppendLine();

            uint ticksCount = gr.ReadUInt32();
            sb.AppendFormat("Ticks Count: 0x{0}", ticksCount.ToString("X8"));
            sb.AppendLine();

            byte movementType = gr.ReadByte(); // 0-4
            sb.AppendFormat("MovementType: 0x{0}", movementType.ToString("X2"));
            sb.AppendLine();

            switch (movementType)
            {
                case 1:
                    break;
                case 2:
                    Coords3 targetPos = gr.ReadCoords3();
                    sb.AppendFormat("TargetPosition: {0}", targetPos.GetCoords());
                    sb.AppendLine();
                    break;
                case 3:
                    ulong targetGuid = gr.ReadUInt64();
                    sb.AppendFormat("Target GUID: 0x{0}", targetGuid.ToString("X16"));
                    sb.AppendLine();
                    break;
                case 4:
                    float targetRot = gr.ReadSingle();
                    sb.AppendFormat("Target Rotation: {0}", targetRot);
                    sb.AppendLine();
                    break;
                default:
                    break;
            }

            if (movementType != 1)
            {
                #region Block1
                /// <summary>
                /// block1
                /// </summary>
                uint movementFlags = gr.ReadUInt32();
                sb.AppendFormat("Movement Flags: 0x{0}", movementFlags.ToString("X8"));
                sb.AppendLine();

                if ((movementFlags & 0x400000) != 0)
                {
                    byte unk_0x400000 = gr.ReadByte();
                    uint unk_0x400000_ms_time = gr.ReadUInt32();

                    sb.AppendFormat("Flags 0x400000: byte 0x{0} and int 0x{1}", unk_0x400000.ToString("X8"), unk_0x400000_ms_time.ToString("X8"));
                    sb.AppendLine();
                }

                uint moveTime = gr.ReadUInt32();
                sb.AppendFormat("Movement Time: 0x{0}", moveTime.ToString("X8"));
                sb.AppendLine();

                if ((movementFlags & 0x8) != 0)
                {
                    float unk_float_0x8 = gr.ReadSingle();
                    uint unk_int_0x8 = gr.ReadUInt32();

                    sb.AppendFormat("Flags 0x8: float {0} and int 0x{1}", unk_float_0x8, unk_int_0x8.ToString("X8"));
                    sb.AppendLine();
                }

                uint splinesCount = gr.ReadUInt32();
                sb.AppendFormat("Splines Count: {0}", splinesCount);
                sb.AppendLine();
                #endregion

                #region Block2
                /// <summary>
                /// block2
                /// </summary>
                if (!((movementFlags & 0x80200) != 0))
                {
                    Coords3 startPos = gr.ReadCoords3();
                    sb.AppendFormat("Current position: {0}", startPos.GetCoords());
                    sb.AppendLine();

                    if (splinesCount > 1)
                    {
                        for (uint i = 0; i < splinesCount - 1; ++i)
                        {
                            int packedOffset = gr.ReadInt32();
                            sb.AppendFormat("Packed Offset: 0x{0}", packedOffset.ToString("X8"));
                            sb.AppendLine();

                            #region UnpackOffset
                            float x = (float)((packedOffset & 0x7FF) << 21 >> 21) * 0.25f;
                            float y = (float)((((packedOffset >> 11) & 0x7FF) << 21) >> 21) * 0.25f;
                            float z = (float)((packedOffset >> 22 << 22) >> 22) * 0.25f;
                            sb.AppendFormat("Path Point {0}: {1}, {2}, {3}", i, startPos.X + x, startPos.Y + y, startPos.Z + z);
                            sb.AppendLine();
                            #endregion
                        }
                    }
                }
                else
                {
                    Coords3 startPos = gr.ReadCoords3();
                    sb.AppendFormat("Splines Start Point: {0}", startPos.GetCoords());
                    sb.AppendLine();

                    if (splinesCount > 1)
                    {
                        for (uint i = 0; i < splinesCount - 1; ++i)
                        {
                            Coords3 spline = gr.ReadCoords3();
                            sb.AppendFormat("Spline Point {0}: {1}", i, spline.GetCoords());
                            sb.AppendLine();
                        }
                    }
                }
                #endregion
            }

            if (gr.BaseStream.Position != gr.BaseStream.Length)
                MessageBox.Show(String.Format("Packet structure changed! Opcode {0}", pkt.Opcode));
        }
    }
}
