using System;

namespace Defines
{
    [Flags]
    enum Flags
    {
        flag01 = 0x00000001,
        flag02 = 0x00000002,
        flag03 = 0x00000004,
        flag04 = 0x00000008,
        flag05 = 0x00000010,
        flag06 = 0x00000020,
        flag07 = 0x00000040,
        flag08 = 0x00000080,
        flag09 = 0x00000100, // running
        flag10 = 0x00000200, // taxi
        flag11 = 0x00000400,
        flag12 = 0x00000800,
        flag13 = 0x00001000,
        flag14 = 0x00002000,
        flag15 = 0x00004000,
        flag16 = 0x00008000,
        flag17 = 0x00010000,
        flag18 = 0x00020000,
        flag19 = 0x00040000,
        flag20 = 0x00080000,
        flag21 = 0x00100000,
        flag22 = 0x00200000,
        flag23 = 0x00400000,
        flag24 = 0x00800000,
        flag25 = 0x01000000,
        flag26 = 0x02000000,
        flag27 = 0x04000000,
        flag28 = 0x08000000,
        flag29 = 0x10000000,
        flag30 = 0x20000000,
        flag31 = 0x40000000
    };

    enum TrainerType
    {
        type1 = 0,
        type2 = 1,
        type3 = 2,
        type4 = 3,
        type5 = 4
    };

    enum TrainerSpellState
    {
        TRAINER_SPELL_GREEN = 0,
        TRAINER_SPELL_RED = 1,
        TRAINER_SPELL_GRAY = 2
    };

    [Flags]
    enum HitInfo
    {
        HITINFO_NORMALSWING = 0x00000000,
        HITINFO_UNK1 = 0x00000001,
        HITINFO_NORMALSWING2 = 0x00000002,
        HITINFO_LEFTSWING = 0x00000004,
        HITINFO_UNK2 = 0x00000008,
        HITINFO_MISS = 0x00000010,
        HITINFO_ABSORB = 0x00000020,
        HITINFO_RESIST = 0x00000040,
        HITINFO_CRITICALHIT = 0x00000080,
        HITINFO_UNK3 = 0x00000100,
        HITINFO_UNK4 = 0x00000200,
        HITINFO_UNK5 = 0x00000400,
        HITINFO_UNK6 = 0x00000800,
        HITINFO_UNK7 = 0x00001000,
        HITINFO_UNK8 = 0x00002000,
        HITINFO_GLANCING = 0x00004000,
        HITINFO_CRUSHING = 0x00008000,
        HITINFO_NOACTION = 0x00010000,
        HITINFO_UNK9 = 0x00020000,
        HITINFO_UNK10 = 0x00040000,
        HITINFO_SWINGNOHITSOUND = 0x00080000,
        HITINFO_UNK11 = 0x00100000,
        HITINFO_UNK12 = 0x00200000,
        HITINFO_UNK13 = 0x00400000,
        HITINFO_UNK14 = 0x00800000,
        HITINFO_UNK15 = 0x01000000,
        HITINFO_UNK16 = 0x02000000,
        HITINFO_UNK17 = 0x04000000,
        HITINFO_UNK18 = 0x08000000,
        HITINFO_UNK19 = 0x10000000,
        HITINFO_UNK20 = 0x20000000,
        HITINFO_UNK21 = 0x40000000
    };

    enum ITEM_DAMAGE_TYPE
    {
        NORMAL_DAMAGE = 0,
        HOLY_DAMAGE = 1,
        FIRE_DAMAGE = 2,
        NATURE_DAMAGE = 3,
        FROST_DAMAGE = 4,
        SHADOW_DAMAGE = 5,
        ARCANE_DAMAGE = 6
    };

    enum VictimState
    {
        VICTIMSTATE_UNKNOWN1 = 0, // may be miss?
        VICTIMSTATE_NORMAL = 1,
        VICTIMSTATE_DODGE = 2,
        VICTIMSTATE_PARRY = 3,
        VICTIMSTATE_UNKNOWN2 = 4,
        VICTIMSTATE_BLOCKS = 5,
        VICTIMSTATE_EVADES = 6,
        VICTIMSTATE_IS_IMMUNE = 7,
        VICTIMSTATE_DEFLECTS = 8
    };

    enum Races
    {
        RACE_HUMAN = 1,
        RACE_ORC = 2,
        RACE_DWARF = 3,
        RACE_NIGHTELF = 4,
        RACE_UNDEAD_PLAYER = 5,
        RACE_TAUREN = 6,
        RACE_GNOME = 7,
        RACE_TROLL = 8,
        RACE_GOBLIN = 9,
        RACE_BLOODELF = 10,
        RACE_DRAENEI = 11,
        RACE_FEL_ORC = 12,
        RACE_NAGA = 13,
        RACE_BROKEN = 14,
        RACE_SKELETON = 15
    };

    enum Class
    {
        CLASS_WARRIOR = 1,
        CLASS_PALADIN = 2,
        CLASS_HUNTER = 3,
        CLASS_ROGUE = 4,
        CLASS_PRIEST = 5,
        // CLASS_UNK1   = 6, unused
        CLASS_SHAMAN = 7,
        CLASS_MAGE = 8,
        CLASS_WARLOCK = 9,
        // CLASS_UNK2   = 10,unused
        CLASS_DRUID = 11
    };

    enum Gender
    {
        MALE = 0,
        FEMALE = 1,
        NOSEX = 2
    };

    enum Powers
    {
        POWER_MANA = 0,
        POWER_RAGE = 1,
        POWER_FOCUS = 2,
        POWER_ENERGY = 3,
        POWER_HAPPINESS = 4
    };

    [Flags]
    enum ProfessionFlags
    {
        PROF_FLAG_NONE = 0,
        PROF_FLAG_PASSIVE = 1,
        PROF_FLAG_SECONDARY = 2,
        PROF_FLAG_PRIMARY = 4,
    };

    [Flags]
    enum GroupUpdateFlags
    {
        GROUP_UPDATE_FLAG_NONE = 0x00000000,
        GROUP_UPDATE_FLAG_ONLINE = 0x00000001, // uint8
        GROUP_UPDATE_FLAG_CUR_HP = 0x00000002, // uint16
        GROUP_UPDATE_FLAG_MAX_HP = 0x00000004, // uint16
        GROUP_UPDATE_FLAG_POWER_TYPE = 0x00000008, // uint8
        GROUP_UPDATE_FLAG_CUR_POWER = 0x00000010, // uint16
        GROUP_UPDATE_FLAG_MAX_POWER = 0x00000020, // uint16
        GROUP_UPDATE_FLAG_LEVEL = 0x00000040, // uint16
        GROUP_UPDATE_FLAG_ZONE = 0x00000080, // uint16
        GROUP_UPDATE_FLAG_POSITION = 0x00000100, // uint16, uint16
        GROUP_UPDATE_FLAG_AURAS = 0x00000200, // uint64 mask, for each bit set uint16 spellid?
        GROUP_UPDATE_FLAG_UNK0 = 0x00000400, // unk uint64, it's 100% guid, probably target guid
        GROUP_UPDATE_FLAG_NAME = 0x00000800, // player name, NULL terminated string
        GROUP_UPDATE_FLAG_UNK1 = 0x00001000, // unk uint16, don't know what it is, probably target related
        GROUP_UPDATE_FLAG_UNK2 = 0x00002000, // unk cur value uint16 (may be target cur health)
        GROUP_UPDATE_FLAG_UNK3 = 0x00004000, // unk max value uint16 (may be target max health)
        GROUP_UPDATE_FLAG_UNK4 = 0x00008000, // unk uint8 (may be target power type)
        GROUP_UPDATE_FLAG_UNK5 = 0x00010000, // unk cur value uint16 (may be target cur power)
        GROUP_UPDATE_FLAG_UNK6 = 0x00020000, // unk max value uint16 (may be target max power)
        GROUP_UPDATE_FLAG_AURAS2 = 0x00040000, // uint64 mask, for each bit set uint16 spellid?, probably it's target auras...
        GROUP_UPDATE_FLAG_UNK7 = 0x00080000, // unused
        GROUP_UPDATE_FLAG_UNK8 = 0x00100000, // unused
        GROUP_UPDATE_FLAG_UNK9 = 0x00200000, // unused
        GROUP_UPDATE_FLAG_UNK10 = 0x00400000, // unused
        GROUP_UPDATE_FLAG_UNK11 = 0x00800000, // unused
        GROUP_UPDATE_FLAG_UNK12 = 0x01000000, // unused
        GROUP_UPDATE_FLAG_UNK13 = 0x02000000, // unused
        GROUP_UPDATE_FLAG_UNK14 = 0x04000000, // unused
        GROUP_UPDATE_FLAG_UNK15 = 0x08000000, // unused
        GROUP_UPDATE_FLAG_UNK16 = 0x10000000, // unused
        GROUP_UPDATE_FLAG_UNK17 = 0x20000000, // unused
        GROUP_UPDATE_FLAG_UNK18 = 0x40000000, // unused
    };

    [Flags]
    enum GroupMemberOnlineStatus
    {
        MEMBER_STATUS_OFFLINE = 0x00,
        MEMBER_STATUS_ONLINE = 0x01,
        MEMBER_STATUS_PVP = 0x02,
        MEMBER_STATUS_UNK0 = 0x04, // dead? (health=0)
        MEMBER_STATUS_UNK1 = 0x08, // ghost? (health=1)
        MEMBER_STATUS_UNK2 = 0x10, // never seen
        MEMBER_STATUS_UNK3 = 0x20, // never seen
        MEMBER_STATUS_UNK4 = 0x40, // appears with dead and ghost
        MEMBER_STATUS_UNK5 = 0x80, // never seen
    };
}
