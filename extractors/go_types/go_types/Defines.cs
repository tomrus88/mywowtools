using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace go_types
{
    // 0x4F0A70 for 0.4.0.8063
    [StructLayout(LayoutKind.Sequential)]
    struct GameObjectTypeInfo
    {
        public int Id;
        public uint NameOffset; //type name
        public uint DataCount;
        public uint DataListOffset;
        public uint UnkOffset;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct GameObjectDataNameInfo
    {
        public uint Id;
        public uint NameOffset; // data name
        public uint Unk1;
        public uint Unk2;
    };
}
