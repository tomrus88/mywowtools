using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_TUTORIAL_FLAGS)]
    internal class TutorialFlags : Parser
    {
        public override void Parse()
        {
            for(var i = 0; i < 8; ++i)
                AppendFormatLine("Mask {0}: {1:X8}", i, Reader.ReadUInt32());
        }
    }
}
