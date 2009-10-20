namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_MONSTER_MOVE)]
    [Parser(OpCodes.SMSG_MONSTER_MOVE_TRANSPORT)]
    internal class MonsterMoveParser : Parser
    {
        public MonsterMoveParser(Packet packet)
            : base(packet)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            AppendFormatLine("Monster GUID: 0x{0:X16}", gr.ReadPackedGuid());

            if (Packet.Code == OpCodes.SMSG_MONSTER_MOVE_TRANSPORT)
            {
                AppendFormatLine("Transport GUID: 0x{0:X16}", gr.ReadPackedGuid());
                AppendFormatLine("Seat Position: 0x{0:X2}", gr.ReadByte());
            }

            AppendFormatLine("Monster unk byte: {0}", gr.ReadByte());

            AppendFormatLine("Dest Position: {0}", gr.ReadCoords3());
            AppendFormatLine("Ticks Count: 0x{0:X8}", gr.ReadUInt32());

            var movementType = gr.ReadByte(); // 0-4
            AppendFormatLine("MovementType: 0x{0:X2}", movementType);

            switch (movementType)
            {
                case 1:
                    // client sets following values to:
                    // movementFlags = 0x1000;
                    // moveTime = 0;
                    // splinesCount = 1;
                    break;
                case 2:
                    AppendFormatLine("Target Position: {0}", gr.ReadCoords3());
                    break;
                case 3:
                    AppendFormatLine("Target GUID: 0x{0:X16}", gr.ReadUInt64());
                    break;
                case 4:
                    AppendFormatLine("Target Rotation: {0}", gr.ReadSingle());
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
                var movementFlags = gr.ReadUInt32();
                AppendFormatLine("Movement Flags: 0x{0:X8}", movementFlags);

                if ((movementFlags & 0x200000) != 0)
                {
                    var unk_0x200000 = gr.ReadByte();
                    var unk_0x200000_ms_time = gr.ReadUInt32();

                    AppendFormatLine("Flags 0x200000: byte 0x{0:X8} and int 0x{1:X8}", unk_0x200000, unk_0x200000_ms_time);
                }

                var moveTime = gr.ReadUInt32();
                AppendFormatLine("Movement Time: 0x{0:X8}", moveTime);

                if ((movementFlags & 0x800) != 0)
                {
                    var unk_float_0x800 = gr.ReadSingle();
                    var unk_int_0x800 = gr.ReadUInt32();

                    AppendFormatLine("Flags 0x800: float {0} and int 0x{1:X8}", unk_float_0x800, unk_int_0x800);
                }

                var splinesCount = gr.ReadUInt32();
                AppendFormatLine("Splines Count: {0}", splinesCount);

                #endregion

                #region Block2

                /// <summary>
                /// block2
                /// </summary>
                if ((movementFlags & 0x42000) != 0)
                {
                    var startPos = gr.ReadCoords3();
                    AppendFormatLine("Splines Start Point: {0}", startPos);

                    if (splinesCount > 1)
                    {
                        for (var i = 0; i < splinesCount - 1; ++i)
                        {
                            AppendFormatLine("Spline Point {0}: {1}", i, gr.ReadCoords3());
                        }
                    }
                }
                else
                {
                    var startPos = gr.ReadCoords3();
                    AppendFormatLine("Current position: {0}", startPos);

                    if (splinesCount > 1)
                    {
                        for (var i = 0; i < splinesCount - 1; ++i)
                        {
                            var packedOffset = gr.ReadInt32();
                            AppendFormatLine("Packed Offset: 0x{0:X8}", packedOffset);

                            #region UnpackOffset

                            var x = ((packedOffset & 0x7FF) << 21 >> 21) * 0.25f;
                            var y = ((((packedOffset >> 11) & 0x7FF) << 21) >> 21) * 0.25f;
                            var z = ((packedOffset >> 22 << 22) >> 22) * 0.25f;
                            AppendFormatLine("Path Point {0}: {1}, {2}, {3}", i, startPos.X + x, startPos.Y + y, startPos.Z + z);

                            #endregion

                            /*#region PackingTest

                            var packed = 0;
                            packed |= (int) (x/0.25f) & 0x7FF;
                            packed |= ((int) (y/0.25f) & 0x7FF) << 11;
                            packed |= ((int) (z/0.25f) & 0x3FF) << 22;
                            AppendFormatLine("Test packing 0x{0:X8}", packed);

                            if(packedOffset != packed)
                                MessageBox.Show("Not equal!");

                            #endregion*/
                        }
                    }
                }

                #endregion
            }

            CheckPacket(gr);

            return GetParsedString();
        }
    }
}
