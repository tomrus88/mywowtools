using System;
using System.Text;

namespace WoWPacketViewer.Parsers {
	[Parser(OpCodes.SMSG_AURA_UPDATE)]
	[Parser(OpCodes.SMSG_AURA_UPDATE_ALL)]
	internal class AuraUpdateParser : Parser {
		[Flags]
		private enum AuraFlags : byte {
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

		public AuraUpdateParser(Packet packet)
			: base(packet) {
		}

		public override string Parse() {
			var gr = Packet.CreateReader();

			var sb = new StringBuilder();
			sb.AppendFormat("GUID: {0:X16}", gr.ReadPackedGuid()).AppendLine();

			sb.AppendLine();

			while(gr.BaseStream.Position < gr.BaseStream.Length) {
				sb.AppendFormat("Slot: {0:X2}", gr.ReadByte()).AppendLine();

				var spellId = gr.ReadUInt32();
				sb.AppendFormat("Spell: {0:X8}", spellId).AppendLine();

				if(spellId > 0) {
					var af = (AuraFlags)gr.ReadByte();
					sb.AppendFormat("Flags: {0}", af).AppendLine();

					sb.AppendFormat("Level: {0:X2}", gr.ReadByte()).AppendLine();

					sb.AppendFormat("Charges: {0:X2}", gr.ReadByte()).AppendLine();

					if(!((af & AuraFlags.NotOwner) != AuraFlags.None)) {
						sb.AppendFormat("GUID2: {0:X16}", gr.ReadPackedGuid()).AppendLine();
					}

					if((af & AuraFlags.Duration) != AuraFlags.None) {
						sb.AppendFormat("Full duration: {0:X8}", gr.ReadUInt32()).AppendLine();

						sb.AppendFormat("Rem. duration: {0:X8}", gr.ReadUInt32()).AppendLine();
					}
				}
				sb.AppendLine();
			}

			if(gr.BaseStream.Position != gr.BaseStream.Length) {
				throw new Exception("Packet structure changed!");
			}

			return sb.ToString();
		}
	}
}
