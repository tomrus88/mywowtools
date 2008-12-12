using System;
using System.Text;

namespace WoWPacketViewer.Parsers {
	[Parser(OpCodes.SMSG_MONSTER_MOVE)]
	[Parser(OpCodes.SMSG_MONSTER_MOVE_TRANSPORT)]
	internal class MonsterMoveParser : Parser {
		public MonsterMoveParser(Packet packet)
			: base(packet) {
		}

		public override string Parse() {
			var gr = Packet.CreateReader();

			var sb = new StringBuilder();
			sb.AppendFormat("Monster GUID: 0x{0:X16}", gr.ReadPackedGuid()).AppendLine();
			if(Packet.Code == OpCodes.SMSG_MONSTER_MOVE_TRANSPORT) {
				sb.AppendFormat("Transport GUID: 0x{0:X16}", gr.ReadPackedGuid()).AppendLine();
				sb.AppendFormat("Seat Position: 0x{0:X2}", gr.ReadByte()).AppendLine();
			}

			sb.AppendFormat("Dest Position: {0}", gr.ReadCoords3()).AppendLine();
			sb.AppendFormat("Ticks Count: 0x{0:X8}", gr.ReadUInt32()).AppendLine();

			var movementType = gr.ReadByte(); // 0-4
			sb.AppendFormat("MovementType: 0x{0:X2}", movementType).AppendLine();

			switch(movementType) {
			case 1:
				break;
			case 2:
				sb.AppendFormat("Target Position: {0}", gr.ReadCoords3()).AppendLine();
				break;
			case 3:
				sb.AppendFormat("Target GUID: 0x{0:X16}", gr.ReadUInt64()).AppendLine();
				break;
			case 4:
				sb.AppendFormat("Target Rotation: {0}", gr.ReadSingle()).AppendLine();
				break;
			default:
				break;
			}

			if(movementType != 1) {
				#region Block1

				/// <summary>
				/// block1
				/// </summary>
				var movementFlags = gr.ReadUInt32();
				sb.AppendFormat("Movement Flags: 0x{0:X8}", movementFlags).AppendLine();

				if((movementFlags & 0x400000) != 0) {
					var unk_0x400000 = gr.ReadByte();
					var unk_0x400000_ms_time = gr.ReadUInt32();

					sb.AppendFormat("Flags 0x400000: byte 0x{0:X8} and int 0x{1:X8}", unk_0x400000,
										 unk_0x400000_ms_time).AppendLine();
				}

				var moveTime = gr.ReadUInt32();
				sb.AppendFormat("Movement Time: 0x{0:X8}", moveTime).AppendLine();

				if((movementFlags & 0x8) != 0) {
					var unk_float_0x8 = gr.ReadSingle();
					var unk_int_0x8 = gr.ReadUInt32();

					sb.AppendFormat("Flags 0x8: float {0} and int 0x{1:X8}", unk_float_0x8, unk_int_0x8).AppendLine();
				}

				var splinesCount = gr.ReadUInt32();
				sb.AppendFormat("Splines Count: {0}", splinesCount).AppendLine();

				#endregion

				#region Block2

				/// <summary>
				/// block2
				/// </summary>
				if(!((movementFlags & 0x80200) != 0)) {
					var startPos = gr.ReadCoords3();
					sb.AppendFormat("Current position: {0}", startPos).AppendLine();

					if(splinesCount > 1) {
						for(uint i = 0; i < splinesCount - 1; ++i) {
							var packedOffset = gr.ReadInt32();
							sb.AppendFormat("Packed Offset: 0x{0:X8}", packedOffset).AppendLine();

							#region UnpackOffset

							var x = ((packedOffset & 0x7FF) << 21 >> 21) * 0.25f;
							var y = ((((packedOffset >> 11) & 0x7FF) << 21) >> 21) * 0.25f;
							var z = ((packedOffset >> 22 << 22) >> 22) * 0.25f;
							sb.AppendFormat("Path Point {0}: {1}, {2}, {3}", i, startPos.X + x, startPos.Y + y, startPos.Z + z);
							sb.AppendLine();

							#endregion
						}
					}
				}
				else {
					var startPos = gr.ReadCoords3();
					sb.AppendFormat("Splines Start Point: {0}", startPos).AppendLine();

					if(splinesCount > 1) {
						for(uint i = 0; i < splinesCount - 1; ++i) {
							sb.AppendFormat("Spline Point {0}: {1}", i, gr.ReadCoords3()).AppendLine();
						}
					}
				}

				#endregion
			}

			if(gr.BaseStream.Position != gr.BaseStream.Length) {
				throw new Exception("Packet structure changed!");
			}

			return sb.ToString();
		}
	}
}
