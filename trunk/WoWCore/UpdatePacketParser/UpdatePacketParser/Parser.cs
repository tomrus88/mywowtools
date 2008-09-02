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

		  public Parser(PacketReaderBase reader) {
			  Packet packet;
			  while((packet = reader.ReadPacket()) != null) {
				  var gr = new GenericReader(new MemoryStream(packet.Data));
				  switch(packet.Code) {
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
            //byte hasTransport = gr.ReadByte();

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
            UpdateFlags updateflags = (UpdateFlags)gr.ReadByte();
            MovementFlags movementFlags = MovementFlags.MOVEMENTFLAG_NONE;

            // 0x20
            if ((updateflags & UpdateFlags.UPDATEFLAG_LIVING) != 0)
            {
                movementFlags = (MovementFlags)gr.ReadUInt32();
                ushort unk = gr.ReadUInt16();
                uint time = gr.ReadUInt32();
            }

            // 0x40
            if ((updateflags & UpdateFlags.UPDATEFLAG_HASPOSITION) != 0)
            {
                float x = gr.ReadSingle();
                float y = gr.ReadSingle();
                float z = gr.ReadSingle();
                float o = gr.ReadSingle();
            }

            // 0x20
            if ((updateflags & UpdateFlags.UPDATEFLAG_LIVING) != 0)
            {
                if ((movementFlags & MovementFlags.MOVEMENTFLAG_ONTRANSPORT) != 0)
                {
                    ulong t_guid = gr.ReadUInt64();
                    float t_x = gr.ReadSingle();
                    float t_y = gr.ReadSingle();
                    float t_z = gr.ReadSingle();
                    float t_o = gr.ReadSingle();
                    uint t_time = gr.ReadUInt32();
                    byte t_unk = gr.ReadByte();
                }

                if ((movementFlags & (MovementFlags.MOVEMENTFLAG_SWIMMING | MovementFlags.MOVEMENTFLAG_UNK5)) != 0)
                {
                    float unk = gr.ReadSingle();
                }

                uint time = gr.ReadUInt32();

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
                    SplineFlags sf = (SplineFlags)gr.ReadUInt32();

                    if ((sf & SplineFlags.POINT) != 0)
                    {
                        float x = gr.ReadSingle();
                        float y = gr.ReadSingle();
                        float z = gr.ReadSingle();
                    }

                    if ((sf & SplineFlags.TARGET) != 0)
                    {
                        ulong s_guid = gr.ReadUInt64();
                    }

                    if ((sf & SplineFlags.ORIENT) != 0)
                    {
                        float o = gr.ReadSingle();
                    }

                    uint cur_time = gr.ReadUInt32();
                    uint full_time = gr.ReadUInt32();
                    uint unk = gr.ReadUInt32();
                    uint count = gr.ReadUInt32();

                    for (uint i = 0; i < count; ++i)
                    {
                        float x = gr.ReadSingle();
                        float y = gr.ReadSingle();
                        float z = gr.ReadSingle();
                    }

                    float end_x = gr.ReadSingle();
                    float end_y = gr.ReadSingle();
                    float end_z = gr.ReadSingle();
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
            if((updateflags & UpdateFlags.UPDATEFLAG_UNKNOWN1) != 0)
            {
                uint unk1 = gr.ReadUInt32();
                float unk2 = gr.ReadSingle();
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
            movementInfo.m_updateFlags = (UpdateFlags)gr.ReadByte();

            // 0x20
            if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_LIVING) != 0)
            {
                movementInfo.m_movementFlags = (MovementFlags)gr.ReadUInt32();
                movementInfo.m_unknown1 = gr.ReadUInt16();
                movementInfo.m_timeStamp = gr.ReadUInt32();
            }

            // 0x40
            if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_HASPOSITION) != 0)
            {
                movementInfo.m_position = gr.ReadCoords4();
            }

            // 0x20
            if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_LIVING) != 0)
            {
                if ((movementInfo.m_movementFlags & MovementFlags.MOVEMENTFLAG_ONTRANSPORT) != 0)
                {
                    movementInfo.m_transportInfo.m_transportGuid = gr.ReadUInt64();
                    movementInfo.m_transportInfo.m_transportPos = gr.ReadCoords4();
                    movementInfo.m_transportInfo.m_transportTime = gr.ReadUInt32();
                    movementInfo.m_transportInfo.m_transportUnk = gr.ReadByte();
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
                    movementInfo.m_splineInfo.m_splineCount = gr.ReadUInt32();

                    for (uint i = 0; i < movementInfo.m_splineInfo.m_splineCount; ++i)
                    {
                        movementInfo.m_splineInfo.m_splines.Add(gr.ReadCoords3());
                    }

                    movementInfo.m_splineInfo.m_splineEndPoint = gr.ReadCoords3();
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

            if ((movementInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_UNKNOWN1) != 0)
            {
                movementInfo.m_wotlkUnknown1 = gr.ReadUInt32();
                movementInfo.m_wotlkUnknown2 = gr.ReadSingle();
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
            foreach (KeyValuePair<ulong, WoWObject> pair in m_objects)
            {
                ulong guid = pair.Key;
                ObjectTypes type = pair.Value.TypeId;

                string final = String.Format("{0}   {1}", guid.ToString("X16"), type);
                listBox.Items.Add(final);
            }
        }

        private WoWObject GetObjectAtIndex(int index)
        {
            // TODO: handle possible exception in case invalid index
            //KeyValuePair<ulong, WoWObject> pair = m_objects.ElementAt(index);
            return m_objects.ElementAt(index).Value;
        }

        public void PrintObjectInfo(int index, ListView listView)
        {
            WoWObject obj = GetObjectAtIndex(index);
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

        public void PrintObjectUpdatesInfo(int index, ListView listView)
        {
            WoWObject obj = GetObjectAtIndex(index);
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

        public void PrintObjectMovementInfo(int index, RichTextBox richTextBox)
        {
            WoWObject obj = GetObjectAtIndex(index);
            MovementInfo mInfo = obj.MovementInfo;
            List<string> strings = new List<string>();

            strings.Add(String.Format("Update Flags: {0}", mInfo.m_updateFlags));

            if((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_LIVING) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Movement Flags: {0}", mInfo.m_movementFlags));
                strings.Add(String.Format("Unknown Flags: {0}", mInfo.m_unknown1.ToString("X4")));
                strings.Add(String.Format("Timestamp: {0}", mInfo.m_timeStamp.ToString("X8")));
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_HASPOSITION) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Position: {0}", mInfo.m_position.GetCoordsAsString()));
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_LIVING) != UpdateFlags.UPDATEFLAG_NONE)
            {
                if ((mInfo.m_movementFlags & MovementFlags.MOVEMENTFLAG_ONTRANSPORT) != MovementFlags.MOVEMENTFLAG_NONE)
                {
                    strings.Add(String.Format("Transport GUID: {0}", mInfo.m_transportInfo.m_transportGuid.ToString("X16")));
                    strings.Add(String.Format("Transport POS: {0}", mInfo.m_transportInfo.m_transportPos.GetCoordsAsString()));
                    strings.Add(String.Format("Transport Time: {0}", mInfo.m_transportInfo.m_transportTime.ToString("X8")));
                    strings.Add(String.Format("Transport Unk: {0}", mInfo.m_transportInfo.m_transportUnk.ToString("X2")));
                }

                if (((mInfo.m_movementFlags & (MovementFlags.MOVEMENTFLAG_SWIMMING | MovementFlags.MOVEMENTFLAG_UNK5)) != MovementFlags.MOVEMENTFLAG_NONE) || ((mInfo.m_unknown1 & 0x20) != 0))
                {
                    strings.Add(String.Format("Swimming Pitch: {0}", mInfo.m_swimPitch));
                }

                strings.Add(String.Format("Fall Time: {0}", mInfo.m_fallTime.ToString("X8")));

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
                        strings.Add(String.Format("Spline Point: {0}", mInfo.m_splineInfo.m_splinePoint.GetCoords()));
                    }

                    if ((mInfo.m_splineInfo.m_splineFlags & SplineFlags.TARGET) != SplineFlags.NONE)
                    {
                        strings.Add(String.Format("Spline GUID: {0}", mInfo.m_splineInfo.m_splineGuid.ToString("X16")));
                    }

                    if ((mInfo.m_splineInfo.m_splineFlags & SplineFlags.ORIENT) != SplineFlags.NONE)
                    {
                        strings.Add(String.Format("Spline Orient: {0}", mInfo.m_splineInfo.m_splineRotation));
                    }

                    strings.Add(String.Format("Spline CurrTime: {0}", mInfo.m_splineInfo.m_splineCurTime.ToString("X8")));
                    strings.Add(String.Format("Spline FullTime: {0}", mInfo.m_splineInfo.m_splineFullTime.ToString("X8")));
                    strings.Add(String.Format("Spline Unk: {0}", mInfo.m_splineInfo.m_splineUnk1.ToString("X8")));
                    strings.Add(String.Format("Spline Count: {0}", mInfo.m_splineInfo.m_splineCount.ToString("X8")));

                    for (uint i = 0; i < mInfo.m_splineInfo.m_splineCount; ++i)
                    {
                        strings.Add(String.Format("Splines_{0}: {1}", i, mInfo.m_splineInfo.m_splines[(int)i].GetCoords()));
                    }

                    strings.Add(String.Format("Spline End Point: {0}", mInfo.m_splineInfo.m_splineEndPoint.GetCoords()));
                }
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_LOWGUID) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Low GUID: {0}", mInfo.m_lowGuid.ToString("X8")));
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_HIGHGUID) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("High GUID: {0}", mInfo.m_highGuid.ToString("X8")));
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_TARGET_GUID) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Target GUID: {0}", mInfo.m_fullGuid.ToString("X16")));
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_TRANSPORT) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Transport Time: {0}", mInfo.m_transportTime.ToString("X8")));
            }

            if ((mInfo.m_updateFlags & UpdateFlags.UPDATEFLAG_UNKNOWN1) != UpdateFlags.UPDATEFLAG_NONE)
            {
                strings.Add(String.Format("Wotlk Unk1: {0}", mInfo.m_wotlkUnknown1.ToString("X8")));
                strings.Add(String.Format("Wotlk Unk2: {0}", mInfo.m_wotlkUnknown2));
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
