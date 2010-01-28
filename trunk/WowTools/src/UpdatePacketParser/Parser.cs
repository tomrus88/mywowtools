using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip.Compression;
using WoWObjects;
using WowTools.Core;

namespace UpdatePacketParser
{
    internal class Parser
    {
        private readonly Dictionary<ulong, WoWObject> objects = new Dictionary<ulong, WoWObject>();

        public Parser(IPacketReader reader)
        {
            foreach (var packet in reader.ReadPackets())
            {
                var gr = packet.CreateReader();
                var code = packet.Code;
                if (code == OpCodes.SMSG_COMPRESSED_UPDATE_OBJECT)
                {
                    code = OpCodes.SMSG_UPDATE_OBJECT;
                    Decompress(ref gr);
                }
                if (code == OpCodes.SMSG_UPDATE_OBJECT)
                {
                    ParseRest(gr);
                    CheckPacket(gr);
                }

                gr.Close();
            }
        }

        //public Parser(BinaryReader reader, OpCodes opcode)
        //{
        //    if (opcode == OpCodes.SMSG_COMPRESSED_UPDATE_OBJECT)
        //    {
        //        opcode = OpCodes.SMSG_UPDATE_OBJECT;
        //        Decompress(ref reader);
        //    }
        //    if (opcode == OpCodes.SMSG_UPDATE_OBJECT)
        //    {
        //        ParseRest(reader);
        //        CheckPacket(reader);
        //    }
        //}

        private WoWObject GetWoWObject(ulong guid)
        {
            WoWObject obj;
            objects.TryGetValue(guid, out obj);
            return obj;
        }

        private static void CheckPacket(BinaryReader gr)
        {
            if (gr.BaseStream.Position != gr.BaseStream.Length)
                MessageBox.Show(String.Format("Packet parsing error, diff {0}", gr.BaseStream.Length - gr.BaseStream.Position));
        }

        private void ParseRest(BinaryReader gr)
        {
            var objectsCount = gr.ReadUInt32();

            for (var i = 0; i < objectsCount; i++)
            {
                var updateType = (UpdateTypes)gr.ReadByte();
                switch (updateType)
                {
                    case UpdateTypes.UPDATETYPE_VALUES:
                        ParseValues(gr);
                        break;
                    case UpdateTypes.UPDATETYPE_MOVEMENT:
                        ParseMovement(gr);
                        break;
                    case UpdateTypes.UPDATETYPE_CREATE_OBJECT:
                    case UpdateTypes.UPDATETYPE_CREATE_OBJECT2:
                        ParseCreateObjects(gr);
                        break;
                    case UpdateTypes.UPDATETYPE_OUT_OF_RANGE_OBJECTS:
                        ParseOutOfRangeObjects(gr);
                        break;
                    case UpdateTypes.UPDATETYPE_NEAR_OBJECTS:
                        ParseNearObjects(gr);
                        break;
                    default:
                        Console.WriteLine("Unknown updatetype {0}", updateType);
                        break;
                }
            }
        }

        private void ParseValues(BinaryReader gr)
        {
            ulong guid = gr.ReadPackedGuid();

            var woWObjectUpdate = WoWObjectUpdate.Read(gr);

            var wowobj = GetWoWObject(guid);
            if (wowobj != null)
                wowobj.AddUpdate(woWObjectUpdate);
            else
                MessageBox.Show("Boom!");
        }

        private static void ParseMovement(BinaryReader gr)
        {
            var guid = gr.ReadPackedGuid();

            var mInfo = MovementInfo.Read(gr);

            // is it even used by blizzard?
        }

        private void ParseCreateObjects(BinaryReader gr)
        {
            var guid = gr.ReadPackedGuid();

            var objectTypeId = (ObjectTypes)gr.ReadByte();

            var movement = MovementInfo.Read(gr);

            // values part
            var update = WoWObjectUpdate.Read(gr);

            var obj = GetWoWObject(guid);
            if (obj == null)
                objects.Add(guid, new WoWObject(objectTypeId, movement, update.Data));
            else
            {
                objects.Remove(guid);
                objects.Add(guid, new WoWObject(objectTypeId, movement, update.Data));
            }
        }

        private static void ParseOutOfRangeObjects(BinaryReader gr)
        {
            var count = gr.ReadUInt32();
            var guids = new ulong[count];
            for (var i = 0; i < count; ++i)
                guids[i] = gr.ReadPackedGuid();
        }

        private static void ParseNearObjects(BinaryReader gr)
        {
            var count = gr.ReadUInt32();
            var guids = new ulong[count];
            for (var i = 0; i < count; ++i)
                guids[i] = gr.ReadPackedGuid();
        }

        private static void Decompress(ref BinaryReader gr)
        {
            var uncompressedLength = gr.ReadInt32();
            var input = gr.ReadBytes((int)(gr.BaseStream.Length - gr.BaseStream.Position));
            gr.Close();
            var output = new byte[uncompressedLength];
            var inflater = new Inflater();
            inflater.SetInput(input, 0, input.Length);
            inflater.Inflate(output, 0, output.Length);
            gr = new BinaryReader(new MemoryStream(output));
        }

        public void PrintObjects(ListBox listBox)
        {
            listBox.Items.Clear();

            foreach (var pair in objects)
            {
                var guid = pair.Key;
                var type = pair.Value.TypeId;

                var final = String.Format("{0:X16} {1}", guid, type);
                listBox.Items.Add(final);
            }
        }

        public void PrintObjectsType(ListBox listBox, ObjectTypeMask mask, CustomFilterMask customMask)
        {
            listBox.Items.Clear();

            foreach (var pair in objects)
            {
                if ((pair.Value.GetType() & mask) != ObjectTypeMask.TYPEMASK_NONE)
                    continue;

                if (customMask != CustomFilterMask.CUSTOM_FILTER_NONE)
                {
                    var highGUID = (pair.Value.GetGUIDHigh() >> 16);
                    if ((customMask & CustomFilterMask.CUSTOM_FILTER_UNITS) != CustomFilterMask.CUSTOM_FILTER_NONE &&
                        (highGUID == 0xF130 || highGUID == 0xF530))
                        continue;
                    if ((customMask & CustomFilterMask.CUSTOM_FILTER_PETS) != CustomFilterMask.CUSTOM_FILTER_NONE &&
                        (highGUID == 0xF140 || highGUID == 0xF540))
                        continue;
                    if ((customMask & CustomFilterMask.CUSTOM_FILTER_VEHICLES) != CustomFilterMask.CUSTOM_FILTER_NONE &&
                        (highGUID == 0xF150 || highGUID == 0xF550))
                        continue;
                    if ((customMask & CustomFilterMask.CUSTOM_FILTER_OBJECTS) != CustomFilterMask.CUSTOM_FILTER_NONE &&
                        (highGUID == 0xF110 || highGUID == 0xF510))
                        continue;
                    if ((customMask & CustomFilterMask.CUSTOM_FILTER_TRANSPORT) != CustomFilterMask.CUSTOM_FILTER_NONE &&
                        (highGUID == 0xF120 || highGUID == 0xF520))
                        continue;
                    if ((customMask & CustomFilterMask.CUSTOM_FILTER_MO_TRANSPORT) != CustomFilterMask.CUSTOM_FILTER_NONE &&
                        (highGUID == 0x1FC0))
                        continue;
                }

                var guid = pair.Key;
                var type = pair.Value.TypeId;

                var final = String.Format("{0:X16} {1}", guid, type);
                listBox.Items.Add(final);
            }
        }

        public void PrintObjectInfo(ulong guid, ListView listView)
        {
            var obj = objects[guid];
            var type = obj.TypeId;

            foreach (var kvp in obj.Data)
            {
                var uf = UpdateFieldsLoader.GetUpdateField(type, kvp.Key);
                var value = GetValueBaseOnType(kvp.Value, uf.Type);
                listView.Items.Add(new ListViewItem(new[] { uf.Name, value }));
            }
        }

        public void PrintObjectUpdatesInfo(ulong guid, ListView listView)
        {
            var obj = objects[guid];

            // make a temp copy of original values
            var objData = new Dictionary<int, uint>();
            foreach (var v in obj.Data)
                objData[v.Key] = v.Value;

            var c = 0;
            foreach (var update in obj.Updates)
            {
                c++;
                var group = new ListViewGroup(String.Format("Update {0}:", c));
                listView.Groups.Add(group);

                foreach (var kvp in update.Data)
                {
                    var uf = UpdateFieldsLoader.GetUpdateField(obj.TypeId, kvp.Key);
                    var oldValue = GetValueBaseOnType((uint)0, uf.Type); // default value 0

                    if (objData.ContainsKey(kvp.Key))
                    {
                        oldValue = GetValueBaseOnType(objData[kvp.Key], uf.Type);
                        objData[kvp.Key] = kvp.Value;
                    }

                    var newValue = GetValueBaseOnType(kvp.Value, uf.Type);
                    listView.Items.Add(new ListViewItem(new[] { uf.Name, oldValue, newValue }, group));
                }
            }
        }

        public void PrintObjectMovementInfo(ulong guid, RichTextBox richTextBox)
        {
            var obj = objects[guid];
            var mInfo = obj.Movement;
            var strings = new List<string>();

            strings.Add(String.Format("Update Flags: {0}", mInfo.UpdateFlags));

            if ((mInfo.UpdateFlags & UpdateFlags.UPDATEFLAG_LIVING) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Movement Flags: {0}", mInfo.Flags));
                strings.Add(String.Format("Unknown Flags: {0:X4}", mInfo.Unknown1));
                strings.Add(String.Format("Timestamp: {0:X8}", mInfo.TimeStamp));

                strings.Add(String.Format("Position: {0}", mInfo.Position));

                if ((mInfo.Flags & MovementFlags.MOVEMENTFLAG_ONTRANSPORT) != MovementFlags.MOVEMENTFLAG_NONE)
                {
                    strings.Add(String.Format("Transport GUID: {0:X16}", mInfo.Transport.Guid));
                    strings.Add(String.Format("Transport POS: {0}", mInfo.Transport.Position));
                    strings.Add(String.Format("Transport Time: {0:X8}", mInfo.Transport.Time));
                    strings.Add(String.Format("Transport Seat: {0:X2}", mInfo.Transport.Seat));
                }

                if (((mInfo.Flags & (MovementFlags.MOVEMENTFLAG_SWIMMING | MovementFlags.MOVEMENTFLAG_UNK5)) !=
                     MovementFlags.MOVEMENTFLAG_NONE) || ((mInfo.Unknown1 & 0x20) != 0))
                {
                    strings.Add(String.Format("Swimming Pitch: {0}", mInfo.SwimPitch));
                }

                strings.Add(String.Format("Fall Time: {0:X8}", mInfo.FallTime));

                if ((mInfo.Flags & MovementFlags.MOVEMENTFLAG_JUMPING) != MovementFlags.MOVEMENTFLAG_NONE)
                {
                    strings.Add(String.Format("Jumping Unk: {0}", mInfo.JumpUnk1));
                    strings.Add(String.Format("Jumping Sin: {0}", mInfo.JumpSinAngle));
                    strings.Add(String.Format("Jumping Cos: {0}", mInfo.JumpCosAngle));
                    strings.Add(String.Format("Jumping Speed: {0}", mInfo.JumpXySpeed));
                }

                if ((mInfo.Flags & MovementFlags.MOVEMENTFLAG_SPLINE) != MovementFlags.MOVEMENTFLAG_NONE)
                {
                    strings.Add(String.Format("Unknown (spline?): {0}", mInfo.Unknown2));
                }

                for (byte i = 0; i < mInfo.Speeds.Length; ++i)
                    strings.Add(String.Format("Speed{0}: {1}", i, mInfo.Speeds[i]));

                if ((mInfo.Flags & MovementFlags.MOVEMENTFLAG_SPLINE2) != MovementFlags.MOVEMENTFLAG_NONE)
                {
                    strings.Add(String.Format("Spline Flags: {0}", mInfo.Spline.Flags));

                    if ((mInfo.Spline.Flags & SplineFlags.POINT) != SplineFlags.NONE)
                    {
                        strings.Add(String.Format("Spline Point: {0}", mInfo.Spline.Point));
                    }

                    if ((mInfo.Spline.Flags & SplineFlags.TARGET) != SplineFlags.NONE)
                    {
                        strings.Add(String.Format("Spline GUID: {0:X16}", mInfo.Spline.Guid));
                    }

                    if ((mInfo.Spline.Flags & SplineFlags.ORIENT) != SplineFlags.NONE)
                    {
                        strings.Add(String.Format("Spline Orient: {0}", mInfo.Spline.Rotation));
                    }

                    strings.Add(String.Format("Spline CurrTime: {0:X8}", mInfo.Spline.CurrentTime));
                    strings.Add(String.Format("Spline FullTime: {0:X8}", mInfo.Spline.FullTime));
                    strings.Add(String.Format("Spline Unk: {0:X8}", mInfo.Spline.Unknown1));

                    strings.Add(String.Format("Spline float1: {0}", mInfo.Spline.UnknownFloat1));
                    strings.Add(String.Format("Spline float2: {0}", mInfo.Spline.UnknownFloat2));
                    strings.Add(String.Format("Spline float3: {0}", mInfo.Spline.UnknownFloat3));

                    strings.Add(String.Format("Spline uint1: {0:X8}", mInfo.Spline.Unknown2));

                    strings.Add(String.Format("Spline Count: {0:X8}", mInfo.Spline.Count));

                    for (uint i = 0; i < mInfo.Spline.Count; ++i)
                    {
                        strings.Add(String.Format("Splines_{0}: {1}", i, mInfo.Spline.Splines[(int)i]));
                    }

                    strings.Add(String.Format("Spline byte3: {0:X2}", mInfo.Spline.Unknown3));

                    strings.Add(String.Format("Spline End Point: {0}", mInfo.Spline.EndPoint));
                }
            }
            else
            {
                if ((mInfo.UpdateFlags & UpdateFlags.UPDATEFLAG_GO_POSITION) != UpdateFlags.UPDATEFLAG_NONE)
                {
                    strings.Add(String.Format("GUID 0x100: {0:X16}", mInfo.Guid0X100));
                    strings.Add(String.Format("Position 0x100: {0}", mInfo.Pos0X100));
                    strings.Add(String.Format("Position2 0x100: {0}", mInfo.Pos20X100));
                    strings.Add(String.Format("Unkf 0x100: {0}", mInfo.UnkFloat0X100));
                }
                else
                {
                    if ((mInfo.UpdateFlags & UpdateFlags.UPDATEFLAG_HAS_POSITION) != UpdateFlags.UPDATEFLAG_NONE)
                    {
                        strings.Add(String.Format("Position: {0}", mInfo.Position));
                    }
                }
            }

            if ((mInfo.UpdateFlags & UpdateFlags.UPDATEFLAG_LOWGUID) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Low GUID: {0:X8}", mInfo.LowGuid));
            }

            if ((mInfo.UpdateFlags & UpdateFlags.UPDATEFLAG_HIGHGUID) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("High GUID: {0:X8}", mInfo.HighGuid));
            }

            if ((mInfo.UpdateFlags & UpdateFlags.UPDATEFLAG_TARGET_GUID) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Target GUID: {0:X16}", mInfo.FullGuid));
            }

            if ((mInfo.UpdateFlags & UpdateFlags.UPDATEFLAG_TRANSPORT) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Transport Time: {0:X8}", mInfo.TransportTime));
            }

            if ((mInfo.UpdateFlags & UpdateFlags.UPDATEFLAG_VEHICLE) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Vehicle Id: {0:X8}", mInfo.VehicleId));
                strings.Add(String.Format("Facing Adjustement: {0}", mInfo.FacingAdjustement));
            }

            if ((mInfo.UpdateFlags & UpdateFlags.UPDATEFLAG_GO_ROTATION) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("GO rotation: {0}", mInfo.Guid0X200.ToString("X16")));
            }

            richTextBox.Lines = strings.ToArray();
        }

        private static string GetValueBaseOnType(Object value, uint type)
        {
            var bytes = BitConverter.GetBytes((uint)value);

            switch (type)
            {
                case 1:
                    return String.Format("int: {0:D}", BitConverter.ToInt32(bytes, 0));
                case 2:
                    return String.Format("2 x ushort: {0} {1}", BitConverter.ToUInt16(bytes, 0), BitConverter.ToUInt16(bytes, 2));
                case 3:
                    return String.Format("float: {0}", BitConverter.ToSingle(bytes, 0));
                case 4:
                    return String.Format("ulong part: 0x{0:X8}", (uint)value);
                case 5:
                    return String.Format("bytes: 0x{0:X8}", (uint)value);
                case 6:
                    return String.Format("ushort: {0}, byte {1}, byte {2}", BitConverter.ToUInt16(bytes, 0), bytes[2], bytes[3]);
                default:
                    return "0";
            }
        }

        public void Close()
        {
            objects.Clear();
        }
    }
}
