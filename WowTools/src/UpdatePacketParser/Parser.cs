using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip.Compression;
using UpdateFields;
using WoWObjects;
using WowTools.Core;

namespace UpdatePacketParser
{
    class Parser
    {
        Dictionary<ulong, WoWObject> m_objects = new Dictionary<ulong, WoWObject>();

        public Dictionary<ulong, WoWObject> Objects
        {
            get { return m_objects; }
            set { m_objects = value; }
        }

        private WoWObject GetWoWObject(ulong guid)
        {
            WoWObject obj;
            m_objects.TryGetValue(guid, out obj);
            return obj;
        }

        public Parser(PacketReaderBase reader)
        {
            Packet packet;
            while ((packet = reader.ReadPacket()) != null)
            {
                var gr = new GenericReader(new MemoryStream(packet.Data));
                switch (packet.Code)
                {
                    case 169:
                        ParseRest(gr);
                        CheckPacket(gr);
                        break;
                    case 502:
                        Decompress(ref gr);
                        ParseRest(gr);
                        CheckPacket(gr);
                        break;
                    default:
                        break;
                }

                gr.Close();
            }
        }

        private static void CheckPacket(BinaryReader gr)
        {
            if (gr.BaseStream.Position != gr.BaseStream.Length)
                MessageBox.Show(String.Format("Packet parsing error, diff {0}", gr.BaseStream.Length - gr.BaseStream.Position));
        }

        private void ParseRest(GenericReader gr)
        {
            var objects_count = gr.ReadUInt32();

            for (var i = 0; i < objects_count; i++)
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
                        ParseOORObjects(gr);
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

        private void ParseValues(GenericReader gr)
        {
            var guid = gr.ReadPackedGuid();
            var blocks_count = gr.ReadByte();
            var updatemask = new int[blocks_count];
            for (var i = 0; i < updatemask.Length; ++i)
            {
                updatemask[i] = gr.ReadInt32();
            }

            var mask = new BitArray(updatemask);

            var masklen = mask.Count;

            var values = new Dictionary<int, uint>();

            for (var i = 0; i < masklen; ++i)
            {
                if (mask[i])
                {
                    values[i] = gr.ReadUInt32();
                }
            }

            var wowobj = GetWoWObject(guid);
            if (wowobj == null)
                return;

            var update = new WoWObjectUpdate(mask, values);
            wowobj.AddUpdate(update);
        }

        private static void ParseMovement(GenericReader gr)
        {
            var guid = gr.ReadPackedGuid();
            var updateflags = (UpdateFlags)gr.ReadUInt16();

            // 0x20
            if ((updateflags & UpdateFlags.UPDATEFLAG_LIVING) != 0)
            {
                var movementFlags = (MovementFlags)gr.ReadUInt32();
                var unk1 = gr.ReadUInt16();
                var time1 = gr.ReadUInt32();

                var x = gr.ReadSingle();
                var y = gr.ReadSingle();
                var z = gr.ReadSingle();
                var o = gr.ReadSingle();

                if ((movementFlags & MovementFlags.MOVEMENTFLAG_ONTRANSPORT) != 0)
                {
                    var t_guid = gr.ReadPackedGuid();
                    var t_x = gr.ReadSingle();
                    var t_y = gr.ReadSingle();
                    var t_z = gr.ReadSingle();
                    var t_o = gr.ReadSingle();
                    var t_time = gr.ReadUInt32();
                    var t_unk = gr.ReadByte();
                }

                if (((movementFlags & (MovementFlags.MOVEMENTFLAG_SWIMMING | MovementFlags.MOVEMENTFLAG_UNK5)) != 0) || ((unk1 & 0x20) != 0))
                {
                    var unk = gr.ReadSingle();
                }

                var time2 = gr.ReadUInt32();

                if ((movementFlags & MovementFlags.MOVEMENTFLAG_JUMPING) != 0)
                {
                    var unk = gr.ReadSingle();
                    var sin = gr.ReadSingle();
                    var cos = gr.ReadSingle();
                    var spd = gr.ReadSingle();
                }

                if ((movementFlags & MovementFlags.MOVEMENTFLAG_SPLINE) != 0)
                {
                    var unk = gr.ReadSingle();
                }

                var walk_speed = gr.ReadSingle();
                var run_speed = gr.ReadSingle();
                var swim_back = gr.ReadSingle();
                var swin_speed = gr.ReadSingle();
                var walk_back = gr.ReadSingle();
                var fly_speed = gr.ReadSingle();
                var fly_back = gr.ReadSingle();
                var turn_speed = gr.ReadSingle();
                var unk_speed = gr.ReadSingle();

                if ((movementFlags & MovementFlags.MOVEMENTFLAG_SPLINE2) != 0)
                {
                    var sf = (SplineFlags)gr.ReadUInt32();

                    if ((sf & SplineFlags.POINT) != 0)
                    {
                        var x2 = gr.ReadSingle();
                        var y2 = gr.ReadSingle();
                        var z2 = gr.ReadSingle();
                    }

                    if ((sf & SplineFlags.TARGET) != 0)
                    {
                        var s_guid = gr.ReadUInt64();
                    }

                    if ((sf & SplineFlags.ORIENT) != 0)
                    {
                        var o2 = gr.ReadSingle();
                    }

                    var cur_time = gr.ReadUInt32();
                    var full_time = gr.ReadUInt32();
                    var unk = gr.ReadUInt32();

                    var u1 = gr.ReadSingle();
                    var u2 = gr.ReadSingle();
                    var u3 = gr.ReadSingle();

                    var u4 = gr.ReadUInt32();

                    var count = gr.ReadUInt32();

                    for (uint i = 0; i < count; ++i)
                    {
                        var x3 = gr.ReadSingle();
                        var y3 = gr.ReadSingle();
                        var z3 = gr.ReadSingle();
                    }

                    var b3 = gr.ReadByte();  // added in 3.0.8

                    var end_x = gr.ReadSingle();
                    var end_y = gr.ReadSingle();
                    var end_z = gr.ReadSingle();
                }
            }
            else
            {
                // 0x100
                if ((updateflags & UpdateFlags.UPDATEFLAG_GO_POSITION) != 0)
                {
                    gr.ReadPackedGuid();
                    gr.ReadCoords3();
                    gr.ReadCoords4();
                    gr.ReadSingle();
                }
                else
                {
                    // 0x40
                    if ((updateflags & UpdateFlags.UPDATEFLAG_HAS_POSITION) != 0)
                    {
                        var x = gr.ReadSingle();
                        var y = gr.ReadSingle();
                        var z = gr.ReadSingle();
                        var o = gr.ReadSingle();
                    }
                }
            }

            if ((updateflags & UpdateFlags.UPDATEFLAG_LOWGUID) != 0)
            {
                var l_guid = gr.ReadUInt32();
            }

            if ((updateflags & UpdateFlags.UPDATEFLAG_HIGHGUID) != 0)
            {
                var h_guid = gr.ReadUInt32();
            }

            if ((updateflags & UpdateFlags.UPDATEFLAG_TARGET_GUID) != 0)
            {
                var f_guid = gr.ReadPackedGuid();
            }

            if ((updateflags & UpdateFlags.UPDATEFLAG_TRANSPORT) != 0)
            {
                var t_time = gr.ReadUInt32();
            }

            // WotLK
            if ((updateflags & UpdateFlags.UPDATEFLAG_VEHICLE) != 0)
            {
                var unk1 = gr.ReadUInt32();
                var unk2 = gr.ReadSingle();
            }

            // 3.1
            if ((updateflags & UpdateFlags.UPDATEFLAG_GO_ROTATION) != 0)
            {
                var unk1 = gr.ReadUInt64();
            }
        }

        private void ParseCreateObjects(GenericReader gr)
        {
            // Variables
            var movementInfo = new MovementInfo(0);

            // Guid
            var guid = gr.ReadPackedGuid();

            // Object Type
            var objectTypeId = (ObjectTypes)gr.ReadByte();

            // Update Flags
            movementInfo.m_updateFlags = (UpdateFlags)gr.ReadUInt16();

            // 0x20
            if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_LIVING) != 0)
            {
                movementInfo.m_movementFlags = (MovementFlags)gr.ReadUInt32();
                movementInfo.m_unknown1 = gr.ReadUInt16();
                movementInfo.m_timeStamp = gr.ReadUInt32();

                movementInfo.m_position = gr.ReadCoords4();

                if ((movementInfo.m_movementFlags & MovementFlags.MOVEMENTFLAG_ONTRANSPORT) != 0)
                {
                    movementInfo.m_transportInfo.m_transportGuid = gr.ReadPackedGuid();
                    movementInfo.m_transportInfo.m_transportPos = gr.ReadCoords4();
                    movementInfo.m_transportInfo.m_transportTime = gr.ReadUInt32();
                    movementInfo.m_transportInfo.m_transportSeat = gr.ReadByte();
                }

                if (((movementInfo.m_movementFlags & (MovementFlags.MOVEMENTFLAG_SWIMMING | MovementFlags.MOVEMENTFLAG_UNK5)) != 0) || ((movementInfo.m_unknown1 & 0x20) != 0))
                {
                    movementInfo.m_swimPitch = gr.ReadSingle();
                }

                movementInfo.m_fallTime = gr.ReadUInt32();

                if ((movementInfo.m_movementFlags & MovementFlags.MOVEMENTFLAG_JUMPING) != 0)
                {
                    movementInfo.m_jump_Unk1 = gr.ReadSingle();
                    movementInfo.m_jump_sinAngle = gr.ReadSingle();
                    movementInfo.m_jump_cosAngle = gr.ReadSingle();
                    movementInfo.m_jump_xySpeed = gr.ReadSingle();
                }

                if ((movementInfo.m_movementFlags & MovementFlags.MOVEMENTFLAG_SPLINE) != 0)
                {
                    movementInfo.m_unknown2 = gr.ReadSingle();
                }

                for (byte i = 0; i < movementInfo.m_speeds.Length; ++i)
                    movementInfo.m_speeds[i] = gr.ReadSingle();

                if ((movementInfo.m_movementFlags & MovementFlags.MOVEMENTFLAG_SPLINE2) != 0)
                {
                    movementInfo.m_splineInfo.m_splineFlags = (SplineFlags)gr.ReadUInt32();

                    if ((movementInfo.m_splineInfo.m_splineFlags & SplineFlags.POINT) != 0)
                    {
                        movementInfo.m_splineInfo.m_splinePoint = gr.ReadCoords3();
                    }

                    if ((movementInfo.m_splineInfo.m_splineFlags & SplineFlags.TARGET) != 0)
                    {
                        movementInfo.m_splineInfo.m_splineGuid = gr.ReadUInt64();
                    }

                    if ((movementInfo.m_splineInfo.m_splineFlags & SplineFlags.ORIENT) != 0)
                    {
                        movementInfo.m_splineInfo.m_splineRotation = gr.ReadSingle();
                    }

                    movementInfo.m_splineInfo.m_splineCurTime = gr.ReadUInt32();
                    movementInfo.m_splineInfo.m_splineFullTime = gr.ReadUInt32();
                    movementInfo.m_splineInfo.m_splineUnk1 = gr.ReadUInt32();

                    movementInfo.m_splineInfo.m_uf1 = gr.ReadSingle();
                    movementInfo.m_splineInfo.m_uf2 = gr.ReadSingle();
                    movementInfo.m_splineInfo.m_uf3 = gr.ReadSingle();

                    movementInfo.m_splineInfo.m_ui1 = gr.ReadUInt32();

                    movementInfo.m_splineInfo.m_splineCount = gr.ReadUInt32();

                    for (uint i = 0; i < movementInfo.m_splineInfo.m_splineCount; ++i)
                    {
                        movementInfo.m_splineInfo.m_splines.Add(gr.ReadCoords3());
                    }

                    movementInfo.m_splineInfo.m_unk3 = gr.ReadByte();  // added in 3.0.8

                    movementInfo.m_splineInfo.m_splineEndPoint = gr.ReadCoords3();
                }
            }
            else
            {
                // 0x100
                if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_GO_POSITION) != 0)
                {
                    movementInfo.m_0x100_guid = gr.ReadPackedGuid();
                    movementInfo.m_0x100_pos = gr.ReadCoords3();
                    movementInfo.m_0x100_pos2 = gr.ReadCoords4();
                    movementInfo.m_0x100_unkf = gr.ReadSingle();
                }
                else
                {
                    // 0x40
                    if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_HAS_POSITION) != 0)
                    {
                        movementInfo.m_position = gr.ReadCoords4();
                    }
                }
            }

            if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_LOWGUID) != 0)
            {
                movementInfo.m_lowGuid = gr.ReadUInt32();
            }

            if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_HIGHGUID) != 0)
            {
                movementInfo.m_highGuid = gr.ReadUInt32();
            }

            if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_TARGET_GUID) != 0)
            {
                movementInfo.m_fullGuid = gr.ReadPackedGuid();
            }

            if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_TRANSPORT) != 0)
            {
                movementInfo.m_transportTime = gr.ReadUInt32();
            }

            if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_VEHICLE) != 0)
            {
                movementInfo.m_vehicleId = gr.ReadUInt32();
                movementInfo.m_facingAdjustement = gr.ReadSingle();
            }

            if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_GO_ROTATION) != 0)
            {
                movementInfo.m_0x200_guid = gr.ReadUInt64();
            }

            // values part
            var blocks_count = gr.ReadByte();
            var updatemask = new int[blocks_count];
            for (var i = 0; i < updatemask.Length; ++i)
            {
                updatemask[i] = gr.ReadInt32();
            }

            var mask = new BitArray(updatemask);

            var masklen = mask.Count;

            var values = new Dictionary<int, uint>();

            for (var i = 0; i < masklen; ++i)
            {
                if (mask[i])
                {
                    values[i] = gr.ReadUInt32();
                }
            }

            if (m_objects.ContainsKey(guid))
            {
                // add create as update for already added objects
                var obj = GetWoWObject(guid);
                if (obj == null)
                    return;

                var update = new WoWObjectUpdate(mask, values);
                obj.AddUpdate(update);
                return;
            }

            var wowobj = new WoWObject(0, objectTypeId);
            wowobj.UpdateMask = mask;
            wowobj.Initialize(values);
            wowobj.SetPosition(movementInfo);
            m_objects.Add(guid, wowobj);
        }

        private static void ParseOORObjects(GenericReader gr)
        {
            var count = gr.ReadUInt32();
            var GUIDS = new ulong[count];
            for (uint i = 0; i < count; ++i)
            {
                GUIDS[i] = gr.ReadPackedGuid();
            }
        }

        private static void ParseNearObjects(GenericReader gr)
        {
            var count = gr.ReadUInt32();
            var GUIDS = new ulong[count];
            for (uint i = 0; i < count; ++i)
            {
                GUIDS[i] = gr.ReadPackedGuid();
            }
        }

        public static void Decompress(ref GenericReader gr)
        {
            var uncompressedLength = gr.ReadInt32();
            var input = gr.ReadBytes((int)(gr.BaseStream.Length - gr.BaseStream.Position));
            gr.Close();
            var output = new byte[uncompressedLength];
            DecompressZLib(input, output);
            gr = new GenericReader(new MemoryStream(output));
        }

        public static void DecompressZLib(byte[] input, byte[] output)
        {
            var item = new Inflater();
            item.SetInput(input, 0, input.Length);
            item.Inflate(output, 0, output.Length);
        }

        public void PrintObjects(ListBox listBox)
        {
            listBox.Items.Clear();

            foreach (var pair in m_objects)
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

            foreach (var pair in m_objects)
            {
                if ((pair.Value.GetType() & mask) != ObjectTypeMask.TYPEMASK_NONE)
                    continue;

                if (customMask != CustomFilterMask.CUSTOM_FILTER_NONE)
                {
                    var highGUID = (pair.Value.GetGUIDHigh() >> 16);
                    if ((customMask & CustomFilterMask.CUSTOM_FILTER_UNITS) != CustomFilterMask.CUSTOM_FILTER_NONE && (highGUID == 0xF130 || highGUID == 0xF530))
                        continue;
                    if ((customMask & CustomFilterMask.CUSTOM_FILTER_PETS) != CustomFilterMask.CUSTOM_FILTER_NONE && (highGUID == 0xF140 || highGUID == 0xF540))
                        continue;
                    if ((customMask & CustomFilterMask.CUSTOM_FILTER_VEHICLES) != CustomFilterMask.CUSTOM_FILTER_NONE && (highGUID == 0xF150 || highGUID == 0xF550))
                        continue;
                    if ((customMask & CustomFilterMask.CUSTOM_FILTER_OBJECTS) != CustomFilterMask.CUSTOM_FILTER_NONE && (highGUID == 0xF110 || highGUID == 0xF510))
                        continue;
                    if ((customMask & CustomFilterMask.CUSTOM_FILTER_TRANSPORT) != CustomFilterMask.CUSTOM_FILTER_NONE && (highGUID == 0xF120 || highGUID == 0xF520))
                        continue;
                    if ((customMask & CustomFilterMask.CUSTOM_FILTER_MO_TRANSPORT) != CustomFilterMask.CUSTOM_FILTER_NONE && (highGUID == 0x1FC0))
                        continue;
                }

                var guid = pair.Key;
                var type = pair.Value.TypeId;

                var final = String.Format("{0:X16} {1}", guid, type);
                listBox.Items.Add(final);
            }
        }

        private WoWObject GetObjectAtIndex(int index)
        {
            // TODO: handle possible exception in case invalid index
            return m_objects.ElementAt(index).Value;
        }

        public void PrintObjectInfo(ulong guid, ListView listView)
        {
            var obj = m_objects[guid];
            var type = obj.TypeId;

            for (var i = 0; i < obj.UpdateMask.Count; i++)
            {
                if (!obj.UpdateMask[i])
                    continue;

                var uf = UpdateFieldsLoader.GetUpdateField(type, i);
                var value = GetValueBaseOnType(obj.UInt32Values[i], uf.Type);
                var item = new ListViewItem(new string[] { uf.Name, value.ToString() });
                listView.Items.Add(item);
            }
        }

        public void PrintObjectUpdatesInfo(ulong guid, ListView listView)
        {
            var obj = m_objects[guid];
            var type = obj.TypeId;
            var c = 1;

            foreach (var update in obj.Updates)
            {
                var updatenum = String.Format("Update {0}:", c);
                var group = new ListViewGroup(updatenum);
                listView.Groups.Add(group);
                c++;

                for (var i = 0; i < update.updatemask.Count; i++)
                {
                    if (!update.updatemask[i])
                        continue;

                    var uf = UpdateFieldsLoader.GetUpdateField(type, i);
                    var value = GetValueBaseOnType(update.updatedata[i], uf.Type);
                    var item = new ListViewItem(new string[] { uf.Name, value.ToString() }, group);
                    listView.Items.Add(item);
                }
            }
        }

        public void PrintObjectMovementInfo(ulong guid, RichTextBox richTextBox)
        {
            var obj = m_objects[guid];
            var mInfo = obj.MovementInfo;
            var strings = new List<string>();

            strings.Add(String.Format("Update Flags: {0}", mInfo.m_updateFlags));

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_LIVING) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Movement Flags: {0}", mInfo.m_movementFlags));
                strings.Add(String.Format("Unknown Flags: {0:X4}", mInfo.m_unknown1));
                strings.Add(String.Format("Timestamp: {0:X8}", mInfo.m_timeStamp));

                strings.Add(String.Format("Position: {0}", mInfo.m_position));

                if ((mInfo.m_movementFlags & MovementFlags.MOVEMENTFLAG_ONTRANSPORT) != MovementFlags.MOVEMENTFLAG_NONE)
                {
                    strings.Add(String.Format("Transport GUID: {0:X16}", mInfo.m_transportInfo.m_transportGuid));
                    strings.Add(String.Format("Transport POS: {0}", mInfo.m_transportInfo.m_transportPos));
                    strings.Add(String.Format("Transport Time: {0:X8}", mInfo.m_transportInfo.m_transportTime));
                    strings.Add(String.Format("Transport Seat: {0:X2}", mInfo.m_transportInfo.m_transportSeat));
                }

                if (((mInfo.m_movementFlags & (MovementFlags.MOVEMENTFLAG_SWIMMING | MovementFlags.MOVEMENTFLAG_UNK5)) != MovementFlags.MOVEMENTFLAG_NONE) || ((mInfo.m_unknown1 & 0x20) != 0))
                {
                    strings.Add(String.Format("Swimming Pitch: {0}", mInfo.m_swimPitch));
                }

                strings.Add(String.Format("Fall Time: {0:X8}", mInfo.m_fallTime));

                if ((mInfo.m_movementFlags & MovementFlags.MOVEMENTFLAG_JUMPING) != MovementFlags.MOVEMENTFLAG_NONE)
                {
                    strings.Add(String.Format("Jumping Unk: {0}", mInfo.m_jump_Unk1));
                    strings.Add(String.Format("Jumping Sin: {0}", mInfo.m_jump_sinAngle));
                    strings.Add(String.Format("Jumping Cos: {0}", mInfo.m_jump_cosAngle));
                    strings.Add(String.Format("Jumping Speed: {0}", mInfo.m_jump_xySpeed));
                }

                if ((mInfo.m_movementFlags & MovementFlags.MOVEMENTFLAG_SPLINE) != MovementFlags.MOVEMENTFLAG_NONE)
                {
                    strings.Add(String.Format("Unknown (spline?): {0}", mInfo.m_unknown2));
                }

                for (byte i = 0; i < mInfo.m_speeds.Length; ++i)
                    strings.Add(String.Format("Speed{0}: {1}", i, mInfo.m_speeds[i]));

                if ((mInfo.m_movementFlags & MovementFlags.MOVEMENTFLAG_SPLINE2) != MovementFlags.MOVEMENTFLAG_NONE)
                {
                    strings.Add(String.Format("Spline Flags: {0}", mInfo.m_splineInfo.m_splineFlags));

                    if ((mInfo.m_splineInfo.m_splineFlags & SplineFlags.POINT) != SplineFlags.NONE)
                    {
                        strings.Add(String.Format("Spline Point: {0}", mInfo.m_splineInfo.m_splinePoint));
                    }

                    if ((mInfo.m_splineInfo.m_splineFlags & SplineFlags.TARGET) != SplineFlags.NONE)
                    {
                        strings.Add(String.Format("Spline GUID: {0:X16}", mInfo.m_splineInfo.m_splineGuid));
                    }

                    if ((mInfo.m_splineInfo.m_splineFlags & SplineFlags.ORIENT) != SplineFlags.NONE)
                    {
                        strings.Add(String.Format("Spline Orient: {0}", mInfo.m_splineInfo.m_splineRotation));
                    }

                    strings.Add(String.Format("Spline CurrTime: {0:X8}", mInfo.m_splineInfo.m_splineCurTime));
                    strings.Add(String.Format("Spline FullTime: {0:X8}", mInfo.m_splineInfo.m_splineFullTime));
                    strings.Add(String.Format("Spline Unk: {0:X8}", mInfo.m_splineInfo.m_splineUnk1));

                    strings.Add(String.Format("Spline float1: {0}", mInfo.m_splineInfo.m_uf1));
                    strings.Add(String.Format("Spline float2: {0}", mInfo.m_splineInfo.m_uf2));
                    strings.Add(String.Format("Spline float3: {0}", mInfo.m_splineInfo.m_uf3));

                    strings.Add(String.Format("Spline uint1: {0:X8}", mInfo.m_splineInfo.m_ui1));

                    strings.Add(String.Format("Spline Count: {0:X8}", mInfo.m_splineInfo.m_splineCount));

                    for (uint i = 0; i < mInfo.m_splineInfo.m_splineCount; ++i)
                    {
                        strings.Add(String.Format("Splines_{0}: {1}", i, mInfo.m_splineInfo.m_splines[(int)i]));
                    }

                    strings.Add(String.Format("Spline byte3: {0:X2}", mInfo.m_splineInfo.m_unk3));

                    strings.Add(String.Format("Spline End Point: {0}", mInfo.m_splineInfo.m_splineEndPoint));
                }
            }
            else
            {
                if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_GO_POSITION) != UpdateFlags.UPDATEFLAG_NONE)
                {
                    strings.Add(String.Format("GUID 0x100: {0:X16}", mInfo.m_0x100_guid));
                    strings.Add(String.Format("Position 0x100: {0}", mInfo.m_0x100_pos));
                    strings.Add(String.Format("Position2 0x100: {0}", mInfo.m_0x100_pos2));
                    strings.Add(String.Format("Unkf 0x100: {0}", mInfo.m_0x100_unkf));
                }
                else
                {
                    if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_HAS_POSITION) != UpdateFlags.UPDATEFLAG_NONE)
                    {
                        strings.Add(String.Format("Position: {0}", mInfo.m_position));
                    }
                }
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_LOWGUID) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Low GUID: {0:X8}", mInfo.m_lowGuid));
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_HIGHGUID) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("High GUID: {0:X8}", mInfo.m_highGuid));
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_TARGET_GUID) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Target GUID: {0:X16}", mInfo.m_fullGuid));
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_TRANSPORT) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Transport Time: {0:X8}", mInfo.m_transportTime));
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_VEHICLE) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Vehicle Id: {0:X8}", mInfo.m_vehicleId));
                strings.Add(String.Format("Facing Adjustement: {0}", mInfo.m_facingAdjustement));
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_GO_ROTATION) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("GO rotation: {0}", mInfo.m_0x200_guid.ToString("X16")));
            }

            richTextBox.Lines = strings.ToArray();
        }

        private static Object GetValueBaseOnType(Object value, uint type)
        {
            switch (type)
            {
                case 1:
                    return (uint)value;
                case 2:
                    var bytes = BitConverter.GetBytes((uint)value);
                    var first = BitConverter.ToUInt16(bytes, 0);
                    var second = BitConverter.ToUInt16(bytes, 2);
                    return String.Format("{0} {1}", first, second);
                case 3:
                    return BitConverter.ToSingle(BitConverter.GetBytes((uint)value), 0);
                case 4:
                    return (uint)value;
                case 5:
                    return ((uint)value).ToString("X8");
                default:
                    return 0;
            }
        }

        public void Close()
        {
            m_objects.Clear();
        }
    }
}
