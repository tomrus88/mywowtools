using System;
using System.Text;

namespace WoWPacketViewer.Parsers {
	[Parser(OpCodes.SMSG_INITIAL_SPELLS)]
	internal class InitialSpellsParser : Parser {
		public InitialSpellsParser(Packet packet)
			: base(packet) {
		}

		public override string Parse() {
			var gr = Packet.CreateReader();

			var sb = new StringBuilder();

			sb.AppendFormat("Unk: {0}", gr.ReadByte()).AppendLine();

			var spellsCount = gr.ReadUInt16();
			sb.AppendFormat("Spells count: {0}", spellsCount).AppendLine();

			for(ushort i = 0; i < spellsCount; ++i) {
				var spellId = gr.ReadUInt16();
				var spellSlot = gr.ReadUInt16();

				sb.AppendFormat("Spell: id {0}, slot {1}", spellId, spellSlot).AppendLine();
			}

			var cooldownsCount = gr.ReadUInt16();
			sb.AppendFormat("Cooldowns count: {0}", cooldownsCount).AppendLine();

			for(ushort i = 0; i < cooldownsCount; ++i) {
				var spellId = gr.ReadUInt16();
				var itemId = gr.ReadUInt16();
				var category = gr.ReadUInt16();
				var coolDown1 = gr.ReadUInt32();
				var coolDown2 = gr.ReadUInt32();

				sb.AppendFormat("Cooldown: spell {0}, item {1}, cat {2}, time1 {3}, time2 {4}", spellId, itemId, category, coolDown1,
									 coolDown2).AppendLine();
			}

			if(gr.BaseStream.Position != gr.BaseStream.Length) {
				throw new Exception("Packet structure changed!");
			}

			return sb.ToString();
		}
	}
}
