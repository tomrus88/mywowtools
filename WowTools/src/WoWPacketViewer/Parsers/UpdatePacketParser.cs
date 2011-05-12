using System;
using System.Text;
using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_UPDATE_OBJECT)]
    [Parser(OpCodes.SMSG_COMPRESSED_UPDATE_OBJECT)]
    internal class UpdatePacketParser : Parser
    {
        public override void Parse()
        {
            if (Packet.Code == OpCodes.SMSG_COMPRESSED_UPDATE_OBJECT)
            {
                // unpack
            }
        }
    }
}
