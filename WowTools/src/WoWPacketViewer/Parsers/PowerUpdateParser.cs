using System;
using System.Text;

namespace WoWPacketViewer.Parsers
{
    [Parser(OpCodes.SMSG_POWER_UPDATE)]
    internal class PowerUpdateParser : Parser
    {
        private enum ManaType
        {
            Mana,
            Rage,
            Focus,
            Energy,
        }

        public PowerUpdateParser(Packet pkt)
            : base(pkt)
        {
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            AppendFormatLine("GUID: 0x{0:X16}", gr.ReadPackedGuid());
            AppendFormatLine("Type: {0}", (ManaType)gr.ReadByte());
            AppendFormatLine("Value: {0}", gr.ReadUInt32());

            CheckPacket(gr);

            return GetParsedString();
        }
    }
}
