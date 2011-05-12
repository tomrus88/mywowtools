using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WowTools.Core
{
    public class Parser
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public void Append(string str)
        {
            stringBuilder.Append(str);
        }

        public void AppendLine()
        {
            stringBuilder.AppendLine();
        }

        public void AppendLine(string str)
        {
            stringBuilder.AppendLine(str);
        }

        public void AppendFormat(string format, params object[] args)
        {
            stringBuilder.AppendFormat(format, args);
        }

        public void AppendFormatLine(string format, params object[] args)
        {
            stringBuilder.AppendFormat(format, args).AppendLine();
        }

        public void CheckPacket()
        {
            if (Reader.BaseStream.Position != Reader.BaseStream.Length)
            {
                string msg = String.Format("{0}: Packet size changed, should be {1} instead of {2}", Packet.Code, Reader.BaseStream.Position, Reader.BaseStream.Length);
                MessageBox.Show(msg);
            }
        }

        protected Packet Packet { get; private set; }

        protected BinaryReader Reader { get; private set; }

        protected Parser() { }

        public void Initialize(Packet packet)
        {
            Packet = packet;

            if (packet != null)
            {
                Reader = Packet.CreateReader();
                Parse();
                CheckPacket();
            }
        }

        public virtual void Parse() { }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}
