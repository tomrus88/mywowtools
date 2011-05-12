using WowTools.Core;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_AUCTION_LIST_PENDING_SALES)]
    internal class AuctionListPendngSales : Parser
    {
        public override void Parse()
        {
            var count = Reader.ReadUInt32();
            for(var i = 0; i < count; ++i)
            {
                AppendFormatLine("Sale {0}: string {1}", i, Reader.ReadCString());
                AppendFormatLine("Sale {0}: string {1}", i, Reader.ReadCString());
                AppendFormatLine("Sale {0}: uint {1}", i, Reader.ReadUInt32());
                AppendFormatLine("Sale {0}: uint {1}", i, Reader.ReadUInt32());
                AppendFormatLine("Sale {0}: float {1}", i, Reader.ReadSingle());
            }
        }
    }
}
