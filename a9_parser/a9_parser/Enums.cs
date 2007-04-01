using System;

namespace a9_parser
{
    public enum UpdateTypes
    {
        UPDATETYPE_VALUES = 0,
        UPDATETYPE_MOVEMENT = 1,
        UPDATETYPE_CREATE_OBJECT = 2,
        UPDATETYPE_CREATE_OBJECT2 = 3,
        UPDATETYPE_OUT_OF_RANGE_OBJECTS = 4,
        UPDATETYPE_NEAR_OBJECTS = 5,
    }

    [Flags]
    public enum UpdateFlags
    {
        UPDATEFLAG_SELFTARGET = 0x01,
        UPDATEFLAG_TRANSPORT = 0x02,
        UPDATEFLAG_FULLGUID = 0x04,
        UPDATEFLAG_HIGHGUID = 0x08,
        UPDATEFLAG_ALL = 0x10,
        UPDATEFLAG_LIVING = 0x20,
        UPDATEFLAG_HASPOSITION = 0x40
    }

    public enum ObjectTypes
    {
        TYPEID_OBJECT = 0,
        TYPEID_ITEM = 1,
        TYPEID_CONTAINER = 2,
        TYPEID_UNIT = 3,
        TYPEID_PLAYER = 4,
        TYPEID_GAMEOBJECT = 5,
        TYPEID_DYNAMICOBJECT = 6,
        TYPEID_CORPSE = 7,
        TYPEID_AIGROUP = 8,
        TYPEID_AREATRIGGER = 9,
    }
}
