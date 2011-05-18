using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_UPDATE_OBJECT)]
    [Parser(OpCodes.SMSG_COMPRESSED_UPDATE_OBJECT)]
    class UpdatePacketParser : Parser
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
