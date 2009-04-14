using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

using WoWReader;
using UpdateFields;
using WoWObjects;

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
                        break;
                    case 502:
                        Decompress(ref gr);
                        ParseRest(gr);
                        break;
                    default:
                        break;
                }
                gr.Close();
            }
        }

        private void ParseRest(GenericReader gr)
        {
            uint objects_count = gr.ReadUInt32();

            for (int i = 0; i < objects_count; i++)
            {
                UpdateTypes updateType = (UpdateTypes)gr.ReadByte();
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
            ulong guid = gr.ReadPackedGuid();
            byte blocks_count = gr.ReadByte();
            int[] updatemask = new int[blocks_count];
            for (int i = 0; i < updatemask.Length; ++i)
            {
                updatemask[i] = gr.ReadInt32();
            }

            BitArray mask = new BitArray(updatemask);

            int masklen = mask.Count;

            Dictionary<int, uint> values = new Dictionary<int, uint>();

            for (int i = 0; i < masklen; ++i)
            {
                if (mask[i])
                {
                    values[i] = gr.ReadUInt32();
                }
            }

            WoWObject wowobj = GetWoWObject(guid);
            if (wowobj == null)
                return;

            WoWObjectUpdate update = new WoWObjectUpdate(mask, values);
            wowobj.AddUpdate(update);
        }

        private void ParseMovement(GenericReader gr)
        {
            ulong guid = gr.ReadPackedGuid();
            UpdateFlags updateflags = (UpdateFlags)gr.ReadUInt16();
            MovementFlags movementFlags = MovementFlags.MOVEMENTFLAG_NONE;

            // 0x20
            if ((updateflags & UpdateFlags.UPDATEFLAG_LIVING) != 0)
            {
                movementFlags = (MovementFlags)gr.ReadUInt32();
                ushort unk1 = gr.ReadUInt16();
                uint time1 = gr.ReadUInt32();

                float x = gr.ReadSingle();
                float y = gr.ReadSingle();
                float z = gr.ReadSingle();
                float o = gr.ReadSingle();

                if ((movementFlags & MovementFlags.MOVEMENTFLAG_ONTRANSPORT) != 0)
                {
                    ulong t_guid = gr.ReadPackedGuid();
                    float t_x = gr.ReadSingle();
                    float t_y = gr.ReadSingle();
                    float t_z = gr.ReadSingle();
                    float t_o = gr.ReadSingle();
                    uint t_time = gr.ReadUInt32();
                    byte t_unk = gr.ReadByte();
                }

                if (((movementFlags & (MovementFlags.MOVEMENTFLAG_SWIMMING | MovementFlags.MOVEMENTFLAG_UNK5)) != 0) || ((unk1 & 0x20) != 0))
                {
                    float unk = gr.ReadSingle();
                }

                uint time2 = gr.ReadUInt32();

                if ((movementFlags & MovementFlags.MOVEMENTFLAG_JUMPING) != 0)
                {
                    float unk = gr.ReadSingle();
                    float sin = gr.ReadSingle();
                    float cos = gr.ReadSingle();
                    float spd = gr.ReadSingle();
                }

                if ((movementFlags & MovementFlags.MOVEMENTFLAG_SPLINE) != 0)
                {
                    float unk = gr.ReadSingle();
                }

                float walk_speed = gr.ReadSingle();
                float run_speed = gr.ReadSingle();
                float swim_back = gr.ReadSingle();
                float swin_speed = gr.ReadSingle();
                float walk_back = gr.ReadSingle();
                float fly_speed = gr.ReadSingle();
                float fly_back = gr.ReadSingle();
                float turn_speed = gr.ReadSingle();
                float unk_speed = gr.ReadSingle();

                if ((movementFlags & MovementFlags.MOVEMENTFLAG_SPLINE2) != 0)
                {
                    SplineFlags sf = (SplineFlags)gr.ReadUInt16();

                    if ((sf & SplineFlags.POINT) != 0)
                    {
                        float x2 = gr.ReadSingle();
                        float y2 = gr.ReadSingle();
                        float z2 = gr.ReadSingle();
                    }

                    if ((sf & SplineFlags.TARGET) != 0)
                    {
                        ulong s_guid = gr.ReadUInt64();
                    }

                    if ((sf & SplineFlags.ORIENT) != 0)
                    {
                        float o2 = gr.ReadSingle();
                    }

                    byte b1 = gr.ReadByte();
                    byte b2 = gr.ReadByte();

                    uint cur_time = gr.ReadUInt32();
                    uint full_time = gr.ReadUInt32();
                    uint unk = gr.ReadUInt32();

                    float u1 = gr.ReadSingle();
                    float u2 = gr.ReadSingle();
                    float u3 = gr.ReadSingle();

                    gr.ReadUInt32();

                    uint count = gr.ReadUInt32();

                    for (uint i = 0; i < count; ++i)
                    {
                        float x3 = gr.ReadSingle();
                        float y3 = gr.ReadSingle();
                        float z3 = gr.ReadSingle();
                    }

                    byte b3 = gr.ReadByte();  // added in 3.0.8

                    float end_x = gr.ReadSingle();
                    float end_y = gr.ReadSingle();
                    float end_z = gr.ReadSingle();
                }
            }
            else
            {
                // 0x100
                if ((updateflags & UpdateFlags.UPDATEFLAG_UNK1) != 0)
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
                        float x = gr.ReadSingle();
                        float y = gr.ReadSingle();
                        float z = gr.ReadSingle();
                        float o = gr.ReadSingle();
                    }
                }
            }

            if ((updateflags & UpdateFlags.UPDATEFLAG_LOWGUID) != 0)
            {
                uint l_guid = gr.ReadUInt32();
            }

            if ((updateflags & UpdateFlags.UPDATEFLAG_HIGHGUID) != 0)
            {
                uint h_guid = gr.ReadUInt32();
            }

            if ((updateflags & UpdateFlags.UPDATEFLAG_TARGET_GUID) != 0)
            {
                ulong f_guid = gr.ReadPackedGuid();
            }

            if ((updateflags & UpdateFlags.UPDATEFLAG_TRANSPORT) != 0)
            {
                uint t_time = gr.ReadUInt32();
            }

            // WotLK
            if ((updateflags & UpdateFlags.UPDATEFLAG_VEHICLE) != 0)
            {
                uint unk1 = gr.ReadUInt32();
                float unk2 = gr.ReadSingle();
            }

            // 3.1
            if ((updateflags & UpdateFlags.UPDATEFLAG_UNK2) != 0)
            {
                ulong unk1 = gr.ReadUInt64();
            }
        }

        private void ParseCreateObjects(GenericReader gr)
        {
            // Variables
            MovementInfo movementInfo = new MovementInfo(0);

            // Guid
            ulong guid = gr.ReadPackedGuid();

            // Object Type
            ObjectTypes objectTypeId = (ObjectTypes)gr.ReadByte();

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
                    movementInfo.m_splineInfo.m_splineFlags = (SplineFlags)gr.ReadUInt16();

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

                    movementInfo.m_splineInfo.m_unk1 = gr.ReadByte();
                    movementInfo.m_splineInfo.m_unk1 = gr.ReadByte();

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
                if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_UNK1) != 0)
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

            if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_UNK2) != 0)
            {
                movementInfo.m_0x200_guid = gr.ReadUInt64();
            }

            // values part
            byte blocks_count = gr.ReadByte();
            int[] updatemask = new int[blocks_count];
            for (int i = 0; i < updatemask.Length; ++i)
            {
                updatemask[i] = gr.ReadInt32();
            }

            BitArray mask = new BitArray(updatemask);

            int masklen = mask.Count;

            Dictionary<int, uint> values = new Dictionary<int, uint>();

            for (int i = 0; i < masklen; ++i)
            {
                if (mask[i])
                {
                    values[i] = gr.ReadUInt32();
                }
            }

            if (!m_objects.ContainsKey(guid))
            {
                WoWObject wowobj = new WoWObject(0, objectTypeId);
                wowobj.UpdateMask = mask;
                wowobj.Initialize(values);
                wowobj.SetPosition(movementInfo);
                m_objects.Add(guid, wowobj);
            }
        }

        private void ParseOORObjects(GenericReader gr)
        {
            uint count = gr.ReadUInt32();
            ulong[] GUIDS = new ulong[count];
            for (uint i = 0; i < count; ++i)
            {
                GUIDS[i] = gr.ReadPackedGuid();
            }
        }

        private void ParseNearObjects(GenericReader gr)
        {
            uint count = gr.ReadUInt32();
            ulong[] GUIDS = new ulong[count];
            for (uint i = 0; i < count; ++i)
            {
                GUIDS[i] = gr.ReadPackedGuid();
            }
        }

        public static void Decompress(ref GenericReader gr)
        {
            int uncompressedLength = gr.ReadInt32();
            byte[] input = gr.ReadBytes((int)gr.Remaining);
            gr.Close();
            byte[] output = new byte[uncompressedLength];
            DecompressZLib(input, output);
            gr = new GenericReader(new MemoryStream(output));
        }

        public static void DecompressZLib(byte[] input, byte[] output)
        {
            Inflater item = new Inflater();
            item.SetInput(input, 0, input.Length);
            item.Inflate(output, 0, output.Length);
        }

        public void PrintObjects(ListBox listBox)
        {
            listBox.Items.Clear();

            foreach (KeyValuePair<ulong, WoWObject> pair in m_objects)
            {
                ulong guid = pair.Key;
                ObjectTypes type = pair.Value.TypeId;

                string final = String.Format("{0:X16} {1}", guid, type);
                listBox.Items.Add(final);
            }
        }

        public void PrintObjectsType(ListBox listBox, ObjectTypeMask mask, CustomFilterMask customMask)
        {
            listBox.Items.Clear();

            foreach (KeyValuePair<ulong, WoWObject> pair in m_objects)
            {
                if ((pair.Value.GetType() & mask) != ObjectTypeMask.TYPEMASK_NONE)
                    continue;

                if (customMask != CustomFilterMask.CUSTOM_FILTER_NONE)
                {
                    uint highGUID = (pair.Value.GetGUIDHigh() >> 16);
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

                ulong guid = pair.Key;
                ObjectTypes type = pair.Value.TypeId;

                string final = String.Format("{0:X16} {1}", guid, type);
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
            WoWObject obj = m_objects[guid];
            ObjectTypes type = obj.TypeId;

            for (int i = 0; i < obj.UpdateMask.Count; i++)
            {
                if (obj.UpdateMask[i])
                {
                    UpdateField uf = UpdateFieldsLoader.GetUpdateField(type, i);
                    Object value = GetValueBaseOnType(obj.UInt32Values[i], uf.Type);
                    ListViewItem item = new ListViewItem(new string[] { uf.Name, value.ToString() });
                    listView.Items.Add(item);
                }
            }
        }

        public void PrintObjectUpdatesInfo(ulong guid, ListView listView)
        {
            WoWObject obj = m_objects[guid];
            ObjectTypes type = obj.TypeId;
            int c = 1;

            foreach (WoWObjectUpdate update in obj.Updates)
            {
                string updatenum = String.Format("Update {0}:", c);
                ListViewGroup group = new ListViewGroup(updatenum);
                listView.Groups.Add(group);
                c++;

                for (int i = 0; i < update.updatemask.Count; i++)
                {
                    if (update.updatemask[i])
                    {
                        UpdateField uf = UpdateFieldsLoader.GetUpdateField(type, i);
                        Object value = GetValueBaseOnType(update.updatedata[i], uf.Type);
                        ListViewItem item = new ListViewItem(new string[] { uf.Name, value.ToString() }, group);
                        listView.Items.Add(item);
                    }
                }
            }
        }

        public void PrintObjectMovementInfo(ulong guid, RichTextBox richTextBox)
        {
            WoWObject obj = m_objects[guid];
            MovementInfo mInfo = obj.MovementInfo;
            List<string> strings = new List<string>();

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

                    strings.Add(String.Format("Spline byte1: {0:X2}", mInfo.m_splineInfo.m_unk1));
                    strings.Add(String.Format("Spline byte2: {0:X2}", mInfo.m_splineInfo.m_unk2));

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
                if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_UNK1) != UpdateFlags.UPDATEFLAG_NONE)
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

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_UNK2) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("0x200 guid: {0:X16}", mInfo.m_0x200_guid));
            }

            richTextBox.Lines = strings.ToArray();
        }

        private Object GetValueBaseOnType(Object value, uint type)
        {
            Object result = 0;
            byte[] bytes;
            switch (type)
            {
                case 1:
                    result = (uint)value;
                    return result;
                case 2:
                    bytes = BitConverter.GetBytes((uint)value);
                    ushort first = BitConverter.ToUInt16(bytes, 0);
                    ushort second = BitConverter.ToUInt16(bytes, 2);
                    result = String.Format("{0} {1}", first, second);
                    return result;
                case 3:
                    bytes = BitConverter.GetBytes((uint)value);
                    result = BitConverter.ToSingle(bytes, 0);
                    return result;
                case 4:
                    result = (uint)value;
                    return result;
                case 5:
                    string str = ((uint)value).ToString("X8");
                    result = str;
                    return result;
                default:
                    return result;
            }
        }

        public void Close()
        {
            m_objects.Clear();
        }
    }
}
