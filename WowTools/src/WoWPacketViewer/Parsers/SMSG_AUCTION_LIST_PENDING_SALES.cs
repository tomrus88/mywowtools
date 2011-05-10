using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_AUCTION_LIST_PENDING_SALES)]
    internal class PowerUpdateParser : Parser
    {
        public PowerUpdateParser(Packet pkt)
            : base(pkt)
        {
        }

        public override void Parse()
        {
            var gr = Packet.CreateReader();

            var count = gr.ReadUInt32();
            for(var i = 0; i < count; ++i)
            {
                AppendFormatLine("Sale {0}: string {1}", i, gr.ReadCString());
                AppendFormatLine("Sale {0}: string {1}", i, gr.ReadCString());
                AppendFormatLine("Sale {0}: uint {1}", i, gr.ReadUInt32());
                AppendFormatLine("Sale {0}: uint {1}", i, gr.ReadUInt32());
                AppendFormatLine("Sale {0}: float {1}", i, gr.ReadSingle());
            }

            CheckPacket(gr);
        }
    }
}
