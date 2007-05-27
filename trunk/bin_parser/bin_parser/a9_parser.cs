using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace bin_parser
{
    public partial class Form1
    {
        /// <summary>
        /// Update mask bit array.
        /// </summary>
        public BitArr Mask;

        private void ParseUpdatePacket(GenericReader gr, GenericReader gr2, StringBuilder sb, StreamWriter swe)
        {
            //MessageBox.Show(gr.BaseStream.Position.ToString() + " " + gr.BaseStream.Length.ToString());

            string database_log = "data.txt";
            StreamWriter data2 = new StreamWriter(database_log, true);
            data2.AutoFlush = true;

            sb.AppendLine("Packet offset " + (gr.BaseStream.Position - 2).ToString("X2"));

            sb.AppendLine("Packet number: " + packet);

            uint count = gr2.ReadUInt32();
            sb.AppendLine("Object count: " + count);

            uint unk = gr2.ReadByte();
            sb.AppendLine("Unk: " + unk);

            for (uint i = 1; i < count+1; i++)
            {
                sb.AppendLine("Update block for object " + i + ":");
                sb.AppendLine("Block offset " + gr.BaseStream.Position.ToString("X2"));

                if (!ParseBlock(gr2, sb, swe, data2))
                    break;
            }

            if (gr2.BaseStream.Position == gr2.BaseStream.Length)
                sb.AppendLine("packet parse: OK...");
            else
                sb.AppendLine("packet parse: ERROR!");

            data2.Flush();
            data2.Close();
        }

        private bool ParseBlock(GenericReader gr, StringBuilder sb, StreamWriter swe, StreamWriter data)
        {
            UpdateTypes updatetype = (UpdateTypes)gr.ReadByte();
            sb.AppendLine("Updatetype: " + updatetype);

            if (updatetype < UpdateTypes.UPDATETYPE_VALUES || updatetype > UpdateTypes.UPDATETYPE_NEAR_OBJECTS)
            {
                long pos = gr.BaseStream.Position;
                swe.WriteLine("wrong updatetype at position " + pos.ToString("X2"));

                // we there only if we read packet wrong way
                swe.WriteLine("Updatetype " + updatetype + " is not supported");
                return false;
            }

            if (updatetype == UpdateTypes.UPDATETYPE_VALUES)
            {
                ulong guid = gr.ReadPackedGuid();
                sb.AppendLine("Object guid: " + guid.ToString("X16"));

                if (guid == 0)
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("wrong guid at position " + pos.ToString("X2"));

                    // we there only if we read packet wrong way
                    swe.WriteLine("Updatetype " + updatetype + " can't be with NULL guid");
                    return false;
                }

                ObjectTypes objecttype = ObjectTypes.TYPEID_OBJECT; // 0
                if(m_objects.ContainsKey(guid))
                    objecttype = m_objects[guid];
                else    // try old method...
                {
                    // object type auto detection:
                    // F00C0000, F02B0000 - gameobjects
                    // F00CXXXX, F02BXXXX - units (XXXX == 0000 for pets etc...)
                    // F02B00000000XXXX - corpses
                    // F02B00000000XXXX - dynamicobjects
                    if (guid.ToString("X16").Substring(0, 8) == "40000000")
                        objecttype = ObjectTypes.TYPEID_ITEM;
                    else if (guid.ToString("X16").Substring(0, 8) == "00000000")
                        objecttype = ObjectTypes.TYPEID_PLAYER;
                    else
                        objecttype = ObjectTypes.TYPEID_UNIT;

                    swe.WriteLine("problem with objecttypeid detection for UPDATETYPE_VALUES");
                }

                sb.AppendLine("objectTypeId " + objecttype);

                if (!ParseValuesUpdateBlock(gr, sb, swe, data, objecttype, updatetype))
                    return false;

                return true;
            }

            if (updatetype == UpdateTypes.UPDATETYPE_MOVEMENT)
            {
                ulong guid = gr.ReadPackedGuid();
                sb.AppendLine("Object guid: " + guid.ToString("X16"));

                if (!ParseMovementUpdateBlock(gr, sb, swe, data, ObjectTypes.TYPEID_UNIT))
                    return false;

                return true;
            }

            if (updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT || updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT2)
            {
                ulong guid = gr.ReadPackedGuid();
                sb.AppendLine("Object guid: " + guid.ToString("X16"));

                ObjectTypes objectTypeId = (ObjectTypes)gr.ReadByte();
                sb.AppendLine("objectTypeId " + objectTypeId);

                // check object existance and remove it if needed...
                if(m_objects.ContainsKey(guid))
                    m_objects.Remove(guid);

                // now we can add this object to Dictionary to get correct object type later...
                m_objects.Add(guid, objectTypeId);

                switch (objectTypeId)
                {
                    case ObjectTypes.TYPEID_OBJECT:
                        swe.WriteLine("Unhandled object type " + objectTypeId);
                        break;
                    case ObjectTypes.TYPEID_ITEM:
                    case ObjectTypes.TYPEID_CONTAINER:
                    case ObjectTypes.TYPEID_UNIT:
                    case ObjectTypes.TYPEID_PLAYER:
                    case ObjectTypes.TYPEID_GAMEOBJECT:
                    case ObjectTypes.TYPEID_DYNAMICOBJECT:
                    case ObjectTypes.TYPEID_CORPSE:
                        if (!ParseMovementUpdateBlock(gr, sb, swe, data, objectTypeId))
                            return false;
                        if (!ParseValuesUpdateBlock(gr, sb, swe, data, objectTypeId, updatetype))
                            return false;
                        break;
                    case ObjectTypes.TYPEID_AIGROUP:
                        swe.WriteLine("Unhandled object type " + objectTypeId);
                        break;
                    case ObjectTypes.TYPEID_AREATRIGGER:
                        swe.WriteLine("Unhandled object type " + objectTypeId);
                        break;
                    default:
                        swe.WriteLine("Unknown object type " + objectTypeId);
                        return false;
                }
                return true;
            }

            if (updatetype == UpdateTypes.UPDATETYPE_OUT_OF_RANGE_OBJECTS || updatetype == UpdateTypes.UPDATETYPE_NEAR_OBJECTS)
            {
                uint objects_count = gr.ReadUInt32();

                if (objects_count > 1000) // we read packet wrong way
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("Too many " + updatetype + " objects");
                    return false;
                }

                sb.AppendLine("guids_count " + objects_count);

                for(uint i = 0; i < objects_count; i++)
                    sb.AppendLine("Guid" + i + ": " + gr.ReadPackedGuid().ToString("X16"));
                return true;
            }

            return true;
        }

        private bool ParseValuesUpdateBlock(GenericReader gr, StringBuilder sb, StreamWriter swe, StreamWriter data, ObjectTypes objectTypeId, UpdateTypes updatetype)
        {
            sb.AppendLine("=== values_update_block_start ===");

            byte blocks_count = gr.ReadByte(); // count of update blocks (4 bytes for each update block)
            sb.AppendLine("Bit mask blocks count: " + blocks_count);

            uint[] updatemask = new uint[blocks_count]; // create array of update blocks
            for (int j = 0; j < blocks_count; j++)
                updatemask[j] = gr.ReadUInt32(); // populate array of update blocks with data

            Mask = (BitArr)updatemask; // convert array of update blocks to bitmask array

            int reallength = Mask.RealLength; // bitmask size (bits)

            // item/container values update block
            if (objectTypeId == ObjectTypes.TYPEID_ITEM || objectTypeId == ObjectTypes.TYPEID_CONTAINER)
            {
                if (reallength > 160) // 5*32, ??
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing ITEM values update block, count " + reallength);
                    return false;
                }

                for (uint index = 0; index < reallength; index++)
                {
                    if (index > UpdateFields.ITEM_END)
                        break;

                    if (Mask[index])
                    {
                        UpdateField uf = UpdateFields.item_uf[index];
                        ReadAndDumpField(uf, sb, gr, updatetype, data);
                    }
                }
            }

            // unit values update block
            if (objectTypeId == ObjectTypes.TYPEID_UNIT)
            {
                if (reallength > 256) // 32*8 (for units bitmask = 8)
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing UNIT values update block, count " + reallength);
                    return false;
                }

                for (uint index = 0; index < reallength; index++)
                {
                    if (index > UpdateFields.UNIT_END)
                        break;

                    if (Mask[index])
                    {
                        UpdateField uf = UpdateFields.unit_uf[index];
                        ReadAndDumpField(uf, sb, gr, updatetype, data);
                    }
                }
            }

            // player values update block
            if (objectTypeId == ObjectTypes.TYPEID_PLAYER)
            {
                if (reallength > 1440) // 32*45 (for player bitmask = 45)
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing PLAYER values update block, count " + reallength);
                    return false;
                }

                for (uint index = 0; index < reallength; index++)
                {
                    if (index > UpdateFields.PLAYER_END)
                        break;

                    if (Mask[index])
                    {
                        UpdateField uf = UpdateFields.unit_uf[index];
                        ReadAndDumpField(uf, sb, gr, updatetype, data);
                    }
                }
            }

            // gameobject values update block
            if (objectTypeId == ObjectTypes.TYPEID_GAMEOBJECT)
            {
                if (reallength > 32) // 1*32
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing GO values update block, count " + reallength);
                    return false;
                }

                for (uint index = 0; index < reallength; index++)
                {
                    if (index > UpdateFields.GO_END)
                        break;

                    if (Mask[index])
                    {
                        UpdateField uf = UpdateFields.go_uf[index];
                        ReadAndDumpField(uf, sb, gr, updatetype, data);
                    }
                }
            }

            // dynamicobject values update block
            if (objectTypeId == ObjectTypes.TYPEID_DYNAMICOBJECT)
            {
                if (reallength > 32) // 1*32
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing DO values update block, count " + reallength);
                    return false;
                }

                for (uint index = 0; index < reallength; index++)
                {
                    if (index > UpdateFields.DO_END)
                        break;

                    if (Mask[index])
                    {
                        UpdateField uf = UpdateFields.do_uf[index];
                        ReadAndDumpField(uf, sb, gr, updatetype, data);
                    }
                }
            }

            // corpse values update block
            if (objectTypeId == ObjectTypes.TYPEID_CORPSE)
            {
                if (reallength > 64) // 2*32
                {
                    long pos = gr.BaseStream.Position;
                    swe.WriteLine("error position " + pos.ToString("X2"));

                    swe.WriteLine("error while parsing CORPSE values update block, count " + reallength);
                    return false;
                }

                for (uint index = 0; index < reallength; index++)
                {
                    if (index > UpdateFields.CORPSE_END)
                        break;

                    if (Mask[index])
                    {
                        UpdateField uf = UpdateFields.corpse_uf[index];
                        ReadAndDumpField(uf, sb, gr, updatetype, data);
                    }
                }
            }

            sb.AppendLine("=== values_update_block_end ===");
            return true;
        }

        private void ReadAndDumpField(UpdateField uf, StringBuilder sb, GenericReader gr, UpdateTypes updatetype, StreamWriter data)
        {
            switch (uf.Type)
            {
                // TODO: add data writing
                /*case 3:
                    string val1 = gr.ReadSingle().ToString().Replace(",", ".");
                    if (updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT || updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT2)
                        data.WriteLine(uf.Name + " (" + uf.Identifier + "): " + val1);
                    sb.AppendLine(uf.Name + " (" + index + "): " + val1);
                    break;
                default:
                    uint val2 = gr.ReadUInt32();
                    if (updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT || updatetype == UpdateTypes.UPDATETYPE_CREATE_OBJECT2)
                        data.WriteLine(uf.Name + " (" + uf.Identifier + "): " + val2);
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + val2);
                    break;*/
                case 1: // uint32
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + gr.ReadUInt32().ToString("X8"));
                    break;
                case 2: // uint16+uint16
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + "first " + gr.ReadUInt16().ToString("X4") + ", second " + gr.ReadUInt16().ToString("X4"));
                    break;
                case 3: // float
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + gr.ReadSingle().ToString().Replace(",", "."));
                    break;
                case 4: // uint64 (can be only low part)
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + gr.ReadUInt32().ToString("X8"));
                    break;
                case 5: // bytes
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + gr.ReadUInt32().ToString("X8"));
                    break;
                default:
                    sb.AppendLine(uf.Name + " (" + uf.Identifier + "): " + "unknown type " + gr.ReadUInt32().ToString("X8"));
                    break;
            }
        }

        private bool ParseMovementUpdateBlock(GenericReader gr, StringBuilder sb, StreamWriter swe, StreamWriter data, ObjectTypes objectTypeId)
        {
            Coords4 coords;
            coords.X = 0;
            coords.Y = 0;
            coords.Z = 0;
            coords.O = 0;

            sb.AppendLine("=== movement_update_block_start ===");
            uint flags2 = 0;

            UpdateFlags flags = (UpdateFlags)gr.ReadByte();
            sb.AppendLine("flags " + flags.ToString("X"));

            if ((UpdateFlags.UPDATEFLAG_LIVING & flags) != 0) // 0x20
            {
                flags2 = gr.ReadUInt32();
                sb.AppendLine("flags2 " + flags2.ToString("X8"));

                uint time = gr.ReadUInt32();
                sb.AppendLine("time " + time.ToString("X8"));
            }

            if ((UpdateFlags.UPDATEFLAG_HASPOSITION & flags) != 0) // 0x40
            {
                // same data, but different for transport and st.
                if ((UpdateFlags.UPDATEFLAG_TRANSPORT & flags) != 0) // 0x02
                {
                    coords = gr.ReadCoords4();
                    sb.AppendLine("coords " + coords.GetCoordsAsString());
                }
                else // strange, we read the same data :)
                {
                    coords = gr.ReadCoords4();
                    sb.AppendLine("coords " + coords.GetCoordsAsString());
                }
            }

            if((flags & UpdateFlags.UPDATEFLAG_LIVING) != 0)   // 0x20
            {
                /*if (objectTypeId == ObjectTypes.TYPEID_UNIT || objectTypeId == ObjectTypes.TYPEID_GAMEOBJECT)
                {
                    data.WriteLine();
                    data.WriteLine(objectTypeId + ": " + coords.GetCoordsAsString());
                }*/

                if ((flags2 & 0x00000200) != 0) // transport
                {
                    ulong t_guid = gr.ReadUInt64();
                    sb.Append("t_guid " + t_guid.ToString("X16") + ", ");

                    Coords4 transport = gr.ReadCoords4();
                    sb.AppendLine("t_coords " + transport.GetCoordsAsString());

                    uint unk2 = gr.ReadUInt32(); // unk
                    sb.AppendLine("t_unk2 " + unk2.ToString("X8"));
                }

                if ((flags2 & 0x00200000) != 0)
                {
                    float unkf1 = gr.ReadSingle();
                    sb.AppendLine("flags2 & 0x200000: " + unkf1);
                }

                uint unk1 = gr.ReadUInt32();
                sb.AppendLine("unk1 " + unk1.ToString("X8"));

                if ((flags2 & 0x00002000) != 0)
                {
                    // looks like orientation/coords/speed
                    float unk3 = gr.ReadSingle();
                    sb.AppendLine("unk3 " + unk3);
                    float unk4 = gr.ReadSingle();
                    sb.AppendLine("unk4 " + unk4);
                    float unk5 = gr.ReadSingle();
                    sb.AppendLine("unk5 " + unk5);
                    float unk6 = gr.ReadSingle();
                    sb.AppendLine("unk6 " + unk6);
                }

                if ((flags2 & 0x04000000) != 0)
                {
                    float unkf2 = gr.ReadSingle();
                    sb.AppendLine("flags2 & 0x4000000: " + unkf2);
                }

                float ws = gr.ReadSingle();
                sb.AppendLine("Walk speed " + ws);
                float rs = gr.ReadSingle();
                sb.AppendLine("Run speed " + rs);
                float sbs = gr.ReadSingle();
                sb.AppendLine("Swimback speed " + sbs);
                float ss = gr.ReadSingle();
                sb.AppendLine("Swim speed " + ss);
                float wbs = gr.ReadSingle();
                sb.AppendLine("Walkback speed " + wbs);
                float fs = gr.ReadSingle();
                sb.AppendLine("Fly speed " + fs);
                float fbs = gr.ReadSingle();
                sb.AppendLine("Flyback speed " + fbs);
                float ts = gr.ReadSingle();
                sb.AppendLine("Turn speed " + ts); // pi = 3.14

                if ((flags2 & 0x8000000) != 0)
                {
                    uint flags3 = gr.ReadUInt32();
                    sb.AppendLine("flags3 " + flags3.ToString("X8"));

                    if ((flags3 & 0x10000) != 0)
                    {
                        Coords3 c = gr.ReadCoords3();
                        sb.AppendLine("flags2 & 0x10000: " + c.GetCoords());
                    }
                    if ((flags3 & 0x20000) != 0)
                    {
                        ulong g3 = gr.ReadUInt64();
                        sb.AppendLine("flags3_guid " + g3.ToString("X16")); // ????
                    }
                    if ((flags3 & 0x40000) != 0)
                    {
                        uint f3_3 = gr.ReadUInt32();
                        sb.AppendLine("flags3_unk_value3 " + f3_3.ToString("X8"));
                    }

                    uint t1 = gr.ReadUInt32();
                    sb.AppendLine("curr tick " + t1.ToString("X8"));

                    uint t2 = gr.ReadUInt32();
                    sb.AppendLine("last tick " + t2.ToString("X8"));

                    uint t3 = gr.ReadUInt32();
                    sb.AppendLine("tick count " + t3.ToString("X8"));

                    uint coords_count = gr.ReadUInt32();
                    sb.AppendLine("coords_count " + coords_count.ToString("X8"));

                    for (uint i = 0; i < coords_count; i++)
                    {
                        Coords3 v = gr.ReadCoords3();
                        sb.AppendLine("coord" + i + ": " + v.GetCoords());
                    }

                    Coords3 end = gr.ReadCoords3();
                    sb.AppendLine("end: " + end.GetCoords());
                }
            }

            if((flags & UpdateFlags.UPDATEFLAG_ALL) != 0)   // 0x10
            {
                uint temp = gr.ReadUInt32();
                sb.AppendLine("flags & 0x10: " + temp.ToString("X8"));
            }

            if ((UpdateFlags.UPDATEFLAG_HIGHGUID & flags) != 0) // 0x08
            {
                uint guid_high = gr.ReadUInt32();
                sb.AppendLine("flags & 0x08: " + guid_high.ToString("X8"));
            }

            if ((UpdateFlags.UPDATEFLAG_FULLGUID & flags) != 0) // 0x04
            {
                ulong guid2 = gr.ReadPackedGuid(); // looks like guid, but what guid?
                sb.AppendLine("flags & 0x04 guid: " + guid2.ToString("X16"));
            }

            if ((UpdateFlags.UPDATEFLAG_TRANSPORT & flags) != 0) // 0x02
            {
                uint time = gr.ReadUInt32();
                sb.AppendLine("flags & 0x02 t_time: " + time.ToString("X8"));
            }

            if ((UpdateFlags.UPDATEFLAG_SELFTARGET & flags) != 0) // 0x01
            {
                // nothing
            }

            sb.AppendLine("=== movement_update_block_end ===");
            return true;
        }
    }
}
