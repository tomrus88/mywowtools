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
            var sb = new StringBuilder();
            sb.AppendFormat("GUID: 0x{0:X16}", gr.ReadPackedGuid()).AppendLine();
            sb.AppendFormat("Type: {0}", (ManaType)gr.ReadByte()).AppendLine();
            sb.AppendFormat("Value: {0}", gr.ReadUInt32());

            if (gr.BaseStream.Position != gr.BaseStream.Length)
            {
                throw new Exception("Packet structure changed!");
            }

            return sb.ToString();
        }
    }
}
