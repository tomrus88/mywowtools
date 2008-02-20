using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UpdateFields;

namespace WoWObjects
{
    public struct WoWObjectUpdate
    {
        public BitArray updatemask;
        public Dictionary<int, uint> updatedata;

        public WoWObjectUpdate(BitArray _mask, Dictionary<int, uint> _data)
        {
            updatemask = _mask;
            updatedata = _data;
        }
    };

    [Flags]
    enum MovementFlags
    {
        MOVEMENTFLAG_NONE = 0x00000000,
        MOVEMENTFLAG_FORWARD = 0x00000001,
        MOVEMENTFLAG_BACKWARD = 0x00000002,
        MOVEMENTFLAG_STRAFE_LEFT = 0x00000004,
        MOVEMENTFLAG_STRAFE_RIGHT = 0x00000008,
        MOVEMENTFLAG_LEFT = 0x00000010,
        MOVEMENTFLAG_RIGHT = 0x00000020,
        MOVEMENTFLAG_PITCH_UP = 0x00000040,
        MOVEMENTFLAG_PITCH_DOWN = 0x00000080,
        MOVEMENTFLAG_WALK = 0x00000100,
        MOVEMENTFLAG_ONTRANSPORT = 0x00000200,
        MOVEMENTFLAG_UNK1 = 0x00000400,
        MOVEMENTFLAG_FLY_UNK1 = 0x00000800,
        MOVEMENTFLAG_JUMPING = 0x00001000,
        MOVEMENTFLAG_UNK4 = 0x00002000,
        MOVEMENTFLAG_FALLING = 0x00004000,
        // 0x8000, 0x10000, 0x20000, 0x40000, 0x80000, 0x100000
        MOVEMENTFLAG_SWIMMING = 0x00200000,               // appears with fly flag also
        MOVEMENTFLAG_FLY_UP = 0x00400000,
        MOVEMENTFLAG_CAN_FLY = 0x00800000,
        MOVEMENTFLAG_FLYING = 0x01000000,
        MOVEMENTFLAG_UNK5 = 0x02000000,
        MOVEMENTFLAG_SPLINE = 0x04000000,               // probably wrong name
        MOVEMENTFLAG_SPLINE2 = 0x08000000,
        MOVEMENTFLAG_WATERWALKING = 0x10000000,
        MOVEMENTFLAG_SAFE_FALL = 0x20000000,               // active rogue safe fall spell (passive)
        MOVEMENTFLAG_UNK3 = 0x40000000
    };

    [Flags]
    enum SplineFlags
    {
        NONE = 0x00000000,
        POINT = 0x10000,
        TARGET = 0x20000,
        ORIENT = 0x40000
    };

    /// <summary>
    /// WoW Object Class
    /// </summary>
    public class WoWObject
    {
        uint m_valuesCount;
        uint[] m_uint32Values;
        ObjectTypes m_typeId;
        bool m_new;
        BitArray m_updatemask;
        List<WoWObjectUpdate> m_updates = new List<WoWObjectUpdate>();

        // position
        float x, y, z, o;

        public WoWObject(uint valuesCount, ObjectTypes typeId)
        {
            if (m_valuesCount != 0)
                m_valuesCount = valuesCount;
            else
                m_valuesCount = GetValuesCountByObjectType(typeId);

            m_uint32Values = new uint[m_valuesCount];
            m_typeId = typeId;
        }

        public void Initialize(Dictionary<int, uint> data)
        {
            for (int i = 0; i < m_updatemask.Count; ++i)
            {
                if (m_updatemask[i])
                {
                    m_uint32Values[i] = data[i];
                }
            }
        }

        public static uint GetValuesCountByObjectType(ObjectTypes typeId)
        {
            switch (typeId)
            {
                case ObjectTypes.TYPEID_ITEM:
                    return UpdateFieldsLoader.ITEM_END;
                case ObjectTypes.TYPEID_CONTAINER:
                    return UpdateFieldsLoader.CONTAINER_END;
                case ObjectTypes.TYPEID_UNIT:
                    return UpdateFieldsLoader.UNIT_END;
                case ObjectTypes.TYPEID_PLAYER:
                    return UpdateFieldsLoader.PLAYER_END;
                case ObjectTypes.TYPEID_GAMEOBJECT:
                    return UpdateFieldsLoader.GO_END;
                case ObjectTypes.TYPEID_DYNAMICOBJECT:
                    return UpdateFieldsLoader.DO_END;
                case ObjectTypes.TYPEID_CORPSE:
                    return UpdateFieldsLoader.CORPSE_END;
                default:
                    return 0;
            }
        }

        public WoWObject()
        {
            m_valuesCount = 0;
            m_uint32Values = null;
            m_typeId = ObjectTypes.TYPEID_OBJECT;
        }

        ~WoWObject()
        {

        }

        public void SetPosition(float X, float Y, float Z, float O)
        {
            x = X;
            y = Y;
            z = Z;
            o = O;
        }

        public bool IsNew
        {
            get { return m_new; }
            set { m_new = value; }
        }

        public ObjectTypes TypeId
        {
            get { return m_typeId; }
            set { m_typeId = value; }
        }

        public uint ValuesCount
        {
            get { return m_valuesCount; }
            set { m_valuesCount = value; }
        }

        public BitArray UpdateMask
        {
            get { return m_updatemask; }
            set { m_updatemask = value; }
        }

        public List<WoWObjectUpdate> Updates
        {
            get { return m_updates; }
            set { m_updates = value; }
        }

        public void AddUpdate(WoWObjectUpdate _update)
        {
            m_updates.Add(_update);
        }

        public void AddUpdate(BitArray _mask, Dictionary<int, uint> _data)
        {
            WoWObjectUpdate _update = new WoWObjectUpdate(_mask, _data);
            m_updates.Add(_update);
        }

        public uint[] UInt32Values
        {
            get { return m_uint32Values; }
            set { m_uint32Values = value; }
        }

        public ulong GetGUID()
        {
            return GetUInt64Value(0);   // OBJECT_FIELD_GUID
        }

        public uint GetGUIDLow()
        {
            return GetUInt32Value(0);   // OBJECT_FIELD_GUID (low)
        }

        public uint GetGUIDHigh()
        {
            return GetUInt32Value(1);   // OBJECT_FIELD_GUID (high)
        }

        public uint GetEntry()
        {
            return GetUInt32Value(3);   // OBJECT_FIELD_ENTRY
        }

        public uint GetUInt32Value(int index)
        {
            return m_uint32Values[index];
        }

        public ulong GetUInt64Value(int index)
        {
            uint low = m_uint32Values[index];
            uint high = m_uint32Values[index + 1];
            return (ulong)(low | (high << 32));
        }

        public float GetFloatValue(int index)
        {
            byte[] temp = BitConverter.GetBytes(m_uint32Values[index]);
            return BitConverter.ToSingle(temp, 0);
        }

        public void SetUInt32Value(int index, uint value)
        {
            m_uint32Values[index] = value;
        }

        public void SetUInt64Value(int index, ulong value)
        {
            m_uint32Values[index] = (uint)(value & 0xFFFFFFFF);
            m_uint32Values[index + 1] = (uint)(value >> 32);
        }

        public void SetFloatValue(int index, float value)
        {
            byte[] temp = BitConverter.GetBytes(value);
            m_uint32Values[index] = BitConverter.ToUInt32(temp, 0);
        }

        public void LoadValues(string data)
        {
            string[] values = data.Split(' ');

            for (ushort i = 0; i < m_valuesCount; i++)
                m_uint32Values[i] = Convert.ToUInt32(values[i]);
        }

        public void Save()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("INSERT INTO `objects` VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '", GetGUIDHigh(), GetGUIDLow(), x, y, z, o, (int)m_typeId);

            for (ushort i = 0; i < m_valuesCount; i++)
            {
                sb.AppendFormat("{0} ", GetUInt32Value(i));
            }

            sb.Append("');");

            StreamWriter sw = new StreamWriter("data.sql", true);
            sw.WriteLine(sb.ToString());
            sw.Flush();
            sw.Close();
        }
    }

    // item
    public class Item : WoWObject
    {
        public Item()
        {
            ValuesCount = UpdateFieldsLoader.ITEM_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_ITEM;
        }
    }

    // container
    public class Container : WoWObject
    {
        public Container()
        {
            ValuesCount = UpdateFieldsLoader.CONTAINER_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_CONTAINER;
        }
    }

    // unit
    public class Unit : WoWObject
    {
        public Unit()
        {
            ValuesCount = UpdateFieldsLoader.UNIT_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_UNIT;
        }
    }

    // player
    public class Player : Unit
    {
        public Player()
        {
            ValuesCount = UpdateFieldsLoader.PLAYER_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_PLAYER;
        }
    }

    // gameobject
    public class GameObject : WoWObject
    {
        public GameObject()
        {
            ValuesCount = UpdateFieldsLoader.GO_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_GAMEOBJECT;
        }
    }

    // dynamicobject
    public class DynamicObject : WoWObject
    {
        public DynamicObject()
        {
            ValuesCount = UpdateFieldsLoader.DO_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_DYNAMICOBJECT;
        }
    }

    // corpse
    public class Corpse : WoWObject
    {
        public Corpse()
        {
            ValuesCount = UpdateFieldsLoader.CORPSE_END;
            UInt32Values = new uint[ValuesCount];
            TypeId = ObjectTypes.TYPEID_CORPSE;
        }
    }
}
