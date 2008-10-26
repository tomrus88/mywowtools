using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WoWPacketViewer
{
    [Flags]
    enum AuraFlags : byte
    {
        AURA_FLAG_00 = 0x00,
        AURA_FLAG_01 = 0x01,
        AURA_FLAG_02 = 0x02,
        AURA_FLAG_04 = 0x04,
        AURA_FLAG_08 = 0x08,
        AURA_FLAG_10 = 0x10,
        AURA_FLAG_20 = 0x20,
        AURA_FLAG_40 = 0x40,
        AURA_FLAG_80 = 0x80,
    };
}
