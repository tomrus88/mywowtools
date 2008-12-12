using System;
using System.Text;

namespace WoWPacketViewer.Parsers {
	[Parser(OpCodes.SMSG_PET_SPELLS)]
	internal class PetSpellsParser : Parser {
		public PetSpellsParser(Packet packet)
			: base(packet) {
		}

		public override string Parse() {
			var gr = Packet.CreateReader();

			var sb = new StringBuilder();
			sb.AppendFormat("GUID: {0:X16}", gr.ReadUInt64()).AppendLine();

			sb.AppendFormat("Pet family: {0}", gr.ReadUInt32()).AppendLine();

			var unk1 = gr.ReadUInt32();
			var unk2 = gr.ReadUInt32();
			sb.AppendFormat("Unk1: {0}, Unk2: {1:X8}", unk1, unk2).AppendLine();

			for(ushort i = 0; i < 10; ++i) {
				var spellOrAction = gr.ReadUInt16();
				var type = gr.ReadUInt16();

				sb.AppendFormat("SpellOrAction: id {0}, type {1:X4}", spellOrAction, type).AppendLine();
			}

			var spellsCount = gr.ReadByte();
			sb.AppendFormat("Spells count: {0}", spellsCount).AppendLine();

			for(ushort i = 0; i < spellsCount; ++i) {
				var spellId = gr.ReadUInt16();
				var active = gr.ReadUInt16();

				sb.AppendFormat("Spell {0}, active {1:X4}", spellId, active).AppendLine();
			}

			var cooldownsCount = gr.ReadByte();
			sb.AppendFormat("Cooldowns count: {0}", cooldownsCount).AppendLine();

			for(byte i = 0; i < cooldownsCount; ++i) {
				var spell = gr.ReadUInt16();
				var category = gr.ReadUInt16();
				var cooldown = gr.ReadUInt32();
				var categoryCooldown = gr.ReadUInt32();

				sb.AppendFormat("Cooldown: spell {0}, category {1}, cooldown {2}, categoryCooldown {3}", spell, category, cooldown,
											categoryCooldown).AppendLine();
			}

			if(gr.BaseStream.Position != gr.BaseStream.Length) {
				throw new Exception("Packet structure changed!");
			}

			return sb.ToString();
		}
	}
}
