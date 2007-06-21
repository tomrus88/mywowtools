using System;
using System.Collections.Generic;
using System.Text;

using UpdateFields;

namespace WoWObjects
{
    // common class
    public class WoWObject
    {
        //public static ulong GUID = fields[0] & fields[1];

        public uint END;
        public Dictionary<uint, UpdateField> fields;
        ObjectTypes m_typeId;

        public ObjectTypes TypeId
        {
            get { return m_typeId; }
            set { m_typeId = value; }
        }
        //public static ObjectTypes GetTypeId() { return m_typeId; }
        //public static void SetTypeId(ObjectTypes typeid) { m_typeId = typeid; }

        public float GetFloatValue(uint index)
        {
            float result = 0;
            result = Convert.ToSingle(fields[index].Value);
            //result = BitConverter.ToSingle(fields[index].Value, 0);
            return result;
        }

        public uint GetUInt32Value(uint index)
        {
            uint result = 0;
            result = fields[index].Value;
            return result;
        }

        public ulong GetUInt64Value(uint index)
        {
            ulong result = 0;

            result += fields[index].Value;
            result += fields[index + 1].Value << 32;
            //result += fields[index + 1].Value >> 32;
            //result = Convert.ToUInt64(value1 + value2);
            return result;
        }

        public void SetFloatValue(uint index, float value)
        {
            //fields[index].Value = Convert.ToUInt32(value);
        }

        public void SetUInt32Value(uint index, uint value)
        {
            //fields[index].Value = value;
        }

        public void SetUInt64Value(uint index, ulong value)
        {
            uint low = (uint)(value >> 32);
            uint high = (uint)value;
            //UpdateField uf;
            //fields[index] = low;
            //fields[index + 1] = high;
        }

        public void SetField(uint field, UpdateField value)
        {
            fields[field] = value;
        }

        public WoWObject()
        {
            END = UpdateFields.UpdateFields.OBJECT_END;
            fields = new Dictionary<uint, UpdateField>((int)END);
            m_typeId = UpdateFields.ObjectTypes.TYPEID_OBJECT;
        }
    }

    // item
    public class Item : WoWObject
    {
        public Item()
        {
            END = UpdateFields.UpdateFields.ITEM_END;
            fields = new Dictionary<uint, UpdateField>((int)END);
            TypeId = UpdateFields.ObjectTypes.TYPEID_ITEM;
        }
    }

    // container
    public class Container : WoWObject
    {
        public Container()
        {
            END = UpdateFields.UpdateFields.CONTAINER_END;
            fields = new Dictionary<uint, UpdateField>((int)END);
            TypeId = UpdateFields.ObjectTypes.TYPEID_CONTAINER;
        }
    }

    // unit
    public class Unit : WoWObject
    {
        public Unit()
        {
            END = UpdateFields.UpdateFields.UNIT_END;
            fields = new Dictionary<uint, UpdateField>((int)END);
            TypeId = UpdateFields.ObjectTypes.TYPEID_UNIT;
        }
    }

    // player
    public class Player : Unit
    {
        public Player()
        {
            END = UpdateFields.UpdateFields.PLAYER_END;
            fields = new Dictionary<uint, UpdateField>((int)END);
            TypeId = UpdateFields.ObjectTypes.TYPEID_PLAYER;
        }
    }

    // gameobject
    public class GameObject : WoWObject
    {
        public GameObject()
        {
            END = UpdateFields.UpdateFields.GO_END;
            fields = new Dictionary<uint, UpdateField>((int)END);
            TypeId = UpdateFields.ObjectTypes.TYPEID_GAMEOBJECT;
        }
    }

    // dynamicobject
    public class DynamicObject : WoWObject
    {
        public DynamicObject()
        {
            END = UpdateFields.UpdateFields.DO_END;
            fields = new Dictionary<uint, UpdateField>((int)END);
            TypeId = UpdateFields.ObjectTypes.TYPEID_DYNAMICOBJECT;
        }
    }

    // corpse
    public class Corpse : WoWObject
    {
        public Corpse()
        {
            END = UpdateFields.UpdateFields.CORPSE_END;
            fields = new Dictionary<uint, UpdateField>((int)END);
            TypeId = UpdateFields.ObjectTypes.TYPEID_CORPSE;
        }
    }
}
