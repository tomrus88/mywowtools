using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WoWPacketViewer
{
    [Flags]
    enum AuraFlags : byte
    {
        AURA_FLAG_NONE = 0x00,
        AURA_FLAG_INDEX_1 = 0x01,
        AURA_FLAG_INDEX_2 = 0x02,
        AURA_FLAG_INDEX_3 = 0x04,
        AURA_FLAG_NOT_OWNER = 0x08,
        AURA_FLAG_POSITIVE = 0x10,
        AURA_FLAG_DURATION = 0x20,
        AURA_FLAG_UNK1 = 0x40,
        AURA_FLAG_NEGATIVE = 0x80,
    };
}
