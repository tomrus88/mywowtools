using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UpdateFields;
using WoWReader;

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

    public struct MovementInfo
    {
        public UpdateFlags m_updateFlags;

        // Living
        public MovementFlags m_movementFlags;
        public ushort m_unknown1;
        public uint m_timeStamp;

        // Position
        public Coords4 m_position;

        // Transport
        public TransportInfo m_transportInfo;

        // Swimming
        public float m_swimPitch;

        // Fall time
        public uint m_fallTime;

        // Jumping
        public float m_jump_Unk1;
        public float m_jump_sinAngle;
        public float m_jump_cosAngle;
        public float m_jump_xySpeed;

        // Spline
        public float m_unknown2;

        public float[] m_speeds;

        // Splines
        public SplineInfo m_splineInfo;

        // Other flags stuff
        public uint m_lowGuid;
        public uint m_highGuid;
        public ulong m_fullGuid;
        public uint m_transportTime;
        public uint m_vehicleId;
        public float m_facingAdjustement;
        public ulong m_0x200_guid;

        // 0x100
        public ulong m_0x100_guid;
        public Coords3 m_0x100_pos;
        public Coords4 m_0x100_pos2;
        public float m_0x100_unkf;

        public MovementInfo(int i)
        {
            m_updateFlags = UpdateFlags.UPDATEFLAG_NONE;
            m_movementFlags = MovementFlags.MOVEMENTFLAG_NONE;
            m_unknown1 = 0;
            m_timeStamp = 0;
            m_position = new Coords4();
            m_transportInfo = new TransportInfo(0);
            m_swimPitch = 0;
            m_fallTime = 0;
            m_jump_Unk1 = 0;
            m_jump_sinAngle = 0;
            m_jump_cosAngle = 0;
            m_jump_xySpeed = 0;
            m_unknown2 = 0;
            m_speeds = new float[9];
            m_splineInfo = new SplineInfo(0);
            m_lowGuid = 0;
            m_highGuid = 0;
            m_fullGuid = 0;
            m_transportTime = 0;
            m_vehicleId = 0;
            m_facingAdjustement = 0;
            m_0x100_guid = 0;
            m_0x100_pos = new Coords3();
            m_0x100_pos2 = new Coords4();
            m_0x100_unkf = 0;
            m_0x200_guid = 0;
        }
    };

    public struct SplineInfo
    {
        public SplineFlags m_splineFlags;
        public Coords3 m_splinePoint;
        public ulong m_splineGuid;
        public float m_splineRotation;
        public uint m_splineCurTime;
        public uint m_splineFullTime;
        public uint m_splineUnk1;
        public float m_uf1;
        public float m_uf2;
        public float m_uf3;
        public uint m_ui1;
        public uint m_splineCount;
        public List<Coords3> m_splines;
        public byte m_unk3;
        public Coords3 m_splineEndPoint;

        public SplineInfo(int i)
        {
            m_splineFlags = SplineFlags.NONE;
            m_splinePoint = new Coords3();
            m_splineGuid = 0;
            m_splineRotation = 0;
            m_splineCurTime = 0;
            m_splineFullTime = 0;
            m_splineUnk1 = 0;
            m_uf1 = 0;
            m_uf2 = 0;
            m_uf3 = 0;
            m_ui1 = 0;
            m_splineCount = 0;
            m_splines = new List<Coords3>();
            m_unk3 = 0;
            m_splineEndPoint = new Coords3();
        }
    };

    public struct TransportInfo
    {
        public ulong m_transportGuid;
        public Coords4 m_transportPos;
        public uint m_transportTime;
        public byte m_transportSeat;

        public TransportInfo(int i)
        {
            m_transportGuid = 0;
            m_transportPos = new Coords4();
            m_transportTime = 0;
            m_transportSeat = 0;
        }
    };

    [Flags]
    public enum MovementFlags
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
        MOVEMENTFLAG_UNK1 = 0x00000400, // levitating?
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
    public enum SplineFlags : uint
    {
        NONE = 0x00000000,
        UNK01 = 0x00000001,
        UNK02 = 0x00000002,
        UNK03 = 0x00000004,
        UNK04 = 0x00000008,
        UNK05 = 0x00000010,
        UNK06 = 0x00000020,
        UNK07 = 0x00000040,
        POINT = 0x00008000,
        TARGET = 0x00010000,
        ORIENT = 0x00020000,
        UNK11 = 0x00000400,
        UNK12 = 0x00000800,
        UNK13 = 0x00001000,
        UNK14 = 0x00002000,
        UNK15 = 0x00004000,
        UNK16 = 0x00008000,
        UNK17 = 0x00010000,
        UNK18 = 0x00020000,
        UNK19 = 0x00040000,
        UNK20 = 0x00080000,
        UNK21 = 0x00100000,
        UNK22 = 0x00200000,
        UNK23 = 0x00400000,
        UNK24 = 0x00800000,
        UNK25 = 0x01000000,
        UNK26 = 0x02000000,
        UNK27 = 0x04000000,
        UNK28 = 0x08000000,
        UNK29 = 0x10000000,
        UNK30 = 0x20000000,
        UNK31 = 0x40000000,
        UNK32 = 0x80000000,
    };

    /// <summary>
    /// WoW Object Class
    /// </summary>
    public class WoWObject
    {
        uint m_valuesCount;
        uint[] m_uint32Values;
        ObjectTypes m_typeId;
        BitArray m_updatemask;
        List<WoWObjectUpdate> m_updates = new List<WoWObjectUpdate>();

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
            for (var i = 0; i < m_updatemask.Count; ++i)
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

        public void SetPosition(MovementInfo mInfo)
        {
            MovementInfo = mInfo;
        }

        public bool IsNew { get; set; }

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

        public MovementInfo MovementInfo { get; private set; }

        public void AddUpdate(WoWObjectUpdate _update)
        {
            m_updates.Add(_update);
        }

        public void AddUpdate(BitArray _mask, Dictionary<int, uint> _data)
        {
            var _update = new WoWObjectUpdate(_mask, _data);
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

        public new ObjectTypeMask GetType()
        {
            return (ObjectTypeMask)GetUInt32Value(2);   // OBJECT_FIELD_TYPE
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
            var low = m_uint32Values[index];
            var high = m_uint32Values[index + 1];
            return (ulong)(low | (high << 32));
        }

        public float GetFloatValue(int index)
        {
            var temp = BitConverter.GetBytes(m_uint32Values[index]);
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
            var temp = BitConverter.GetBytes(value);
            m_uint32Values[index] = BitConverter.ToUInt32(temp, 0);
        }

        public void LoadValues(string data)
        {
            var values = data.Split(' ');

            for (ushort i = 0; i < m_valuesCount; i++)
                m_uint32Values[i] = Convert.ToUInt32(values[i]);
        }

        public void Save()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("INSERT INTO `objects` VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '", GetGUIDHigh(), GetGUIDLow(), MovementInfo.m_position.X, MovementInfo.m_position.Y, MovementInfo.m_position.Z, MovementInfo.m_position.O, (int)m_typeId);

            for (ushort i = 0; i < m_valuesCount; i++)
            {
                sb.AppendFormat("{0} ", GetUInt32Value(i));
            }

            sb.Append("');");

            var sw = new StreamWriter("data.sql", true);
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
