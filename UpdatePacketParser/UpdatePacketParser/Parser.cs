using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
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
            if (m_objects.ContainsKey(guid))
                return m_objects[guid];
            else
                return null;
        }

        public Parser(string filename)
        {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + filename);
            SQLiteCommand command = new SQLiteCommand(connection);
            connection.Open();
            command.CommandText = "SELECT opcode, data FROM packets WHERE opcode=169 OR opcode=502 ORDER BY id;";
            command.Prepare();
            SQLiteDataReader reader = command.ExecuteReader();

            MemoryStream ms = new MemoryStream();

            while (reader.Read())
            {
                ushort opcode = (ushort)reader.GetInt16(0);
                byte[] data_ = (byte[])reader.GetValue(1);

                uint size = sizeof(ushort) + (uint)data_.Length;
                byte[] sz = BitConverter.GetBytes(size);
                byte[] op = BitConverter.GetBytes(opcode);

                ms.Write(sz, 0, sz.Length);
                ms.Write(op, 0, op.Length);
                ms.Write(data_, 0, data_.Length);
            }

            reader.Close();
            connection.Close();

            GenericReader gr = new GenericReader(ms, Encoding.ASCII);
            gr.BaseStream.Position = 0;

            while (gr.PeekChar() >= 0)
                ParseHeader(gr);

            gr.Close();
        }

        private void ParseHeader(GenericReader gr)
        {
            int size = gr.ReadInt32();
            byte[] temp = gr.ReadBytes(size);
            MemoryStream ms = new MemoryStream(temp);
            GenericReader gr2 = new GenericReader(ms);

            ushort opcode = gr2.ReadUInt16();

            switch (opcode)
            {
                case 169:   // not compressed
                    break;
                case 502:   // compressed
                    Decompress(ref gr2);
                    break;
                default:
                    break;  // unknown
            }

            ParseRest(gr2);
        }

        private void ParseRest(GenericReader gr)
        {
            uint objects_count = gr.ReadUInt32();
            byte hasTransport = gr.ReadByte();

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
                byte unk = gr.ReadByte();
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

            if ((updateflags & UpdateFlags.UPDATEFLAG_FULLGUID) != 0)
            {
                ulong f_guid = gr.ReadPackedGuid();
            }

            if ((updateflags & UpdateFlags.UPDATEFLAG_TRANSPORT) != 0)
            {
                uint t_time = gr.ReadUInt32();
            }
        }

        private void ParseCreateObjects(GenericReader gr)
        {
            ulong guid = gr.ReadPackedGuid();
            ObjectTypes objectTypeId = (ObjectTypes)gr.ReadByte();
            float x = 0, y = 0, z = 0, o = 0;

            // movement part

            UpdateFlags updateflags = (UpdateFlags)gr.ReadByte();
            MovementFlags movementFlags = MovementFlags.MOVEMENTFLAG_NONE;

            // 0x20
            if ((updateflags & UpdateFlags.UPDATEFLAG_LIVING) != 0)
            {
                movementFlags = (MovementFlags)gr.ReadUInt32();
                byte unk = gr.ReadByte();
                uint time = gr.ReadUInt32();
            }

            // 0x40
            if ((updateflags & UpdateFlags.UPDATEFLAG_HASPOSITION) != 0)
            {
                x = gr.ReadSingle();
                y = gr.ReadSingle();
                z = gr.ReadSingle();
                o = gr.ReadSingle();
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

                if ((movementFlags & MovementFlags.MOVEMENTFLAG_SPLINE2) != 0)
                {
                    SplineFlags sf = (SplineFlags)gr.ReadUInt32();

                    if ((sf & SplineFlags.POINT) != 0)
                    {
                        float s_x = gr.ReadSingle();
                        float s_y = gr.ReadSingle();
                        float s_z = gr.ReadSingle();
                    }

                    if ((sf & SplineFlags.TARGET) != 0)
                    {
                        ulong s_guid = gr.ReadUInt64();
                    }

                    if ((sf & SplineFlags.ORIENT) != 0)
                    {
                        float s_o = gr.ReadSingle();
                    }

                    uint cur_time = gr.ReadUInt32();
                    uint full_time = gr.ReadUInt32();
                    uint unk = gr.ReadUInt32();
                    uint count = gr.ReadUInt32();

                    for (uint i = 0; i < count; ++i)
                    {
                        float s_x = gr.ReadSingle();
                        float s_y = gr.ReadSingle();
                        float s_z = gr.ReadSingle();
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

            if ((updateflags & UpdateFlags.UPDATEFLAG_FULLGUID) != 0)
            {
                ulong f_guid = gr.ReadPackedGuid();
            }

            if ((updateflags & UpdateFlags.UPDATEFLAG_TRANSPORT) != 0)
            {
                uint t_time = gr.ReadUInt32();
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
                WoWObject wowobj = new WoWObject(WoWObject.GetValuesCountByObjectType(objectTypeId), objectTypeId);
                wowobj.UpdateMask = mask;
                wowobj.Initialize(values);
                wowobj.SetPosition(x, y, z, o);
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
            byte[] output = new byte[uncompressedLength];
            int len = (int)(gr.BaseStream.Length - gr.BaseStream.Position);
            byte[] temp = gr.ReadBytes(len);
            gr.Close();
            Stream s = new InflaterInputStream(new MemoryStream(temp));
            int offset = 0;
            while (true)
            {
                int size = s.Read(output, offset, uncompressedLength);
                if (size == uncompressedLength) break;
                offset += size;
                uncompressedLength -= size;
            }
            gr = new GenericReader(new MemoryStream(output));

            //gr = new GenericReader(new InflaterInputStream(gr.BaseStream));
        }

        public void PrintObjects(ListBox listBox)
        {
            foreach(KeyValuePair<ulong, WoWObject> pair in m_objects)
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
            KeyValuePair<ulong, WoWObject> pair = m_objects.ElementAt(index);
            return pair.Value;
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
