using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

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

        public byte ReadUInt8(string format, params object[] args)
        {
            var newArgs = new List<object>();
            newArgs.AddRange(args);
            var ret = Reader.ReadByte();
            newArgs.Add(ret);
            var str = String.Format(format, newArgs.ToArray());
            AppendLine(str);
            return ret;
        }

        // for enums
        public T ReadUInt8<T>(string format, params object[] args)
        {
            var newArgs = new List<object>();
            newArgs.AddRange(args);
            var ret = Reader.ReadByte();
            newArgs.Add(Enum.ToObject(typeof(T), ret));
            var str = String.Format(format, newArgs.ToArray());
            AppendLine(str);
            return (T)Enum.ToObject(typeof(T), ret);
        }

        public uint ReadUInt32(string format, params object[] args)
        {
            var newArgs = new List<object>();
            newArgs.AddRange(args);
            var ret = Reader.ReadUInt32();
            newArgs.Add(ret);
            var str = String.Format(format, newArgs.ToArray());
            AppendLine(str);
            return ret;
        }

        public ulong ReadUInt64(string format, params object[] args)
        {
            var newArgs = new List<object>();
            newArgs.AddRange(args);
            var ret = Reader.ReadUInt32();
            newArgs.Add(ret);
            var str = String.Format(format, newArgs.ToArray());
            AppendLine(str);
            return ret;
        }

        public string ReadCString(string format, params object[] args)
        {
            var newArgs = new List<object>();
            newArgs.AddRange(args);
            var ret = Reader.ReadCString();
            newArgs.Add(ret);
            var str = String.Format(format, newArgs.ToArray());
            AppendLine(str);
            return ret;
        }

        public float ReadSingle(string format, params object[] args)
        {
            var newArgs = new List<object>();
            newArgs.AddRange(args);
            var ret = Reader.ReadSingle();
            newArgs.Add(ret);
            var str = String.Format(format, newArgs.ToArray());
            AppendLine(str);
            return ret;
        }

        public ulong ReadPackedGuid(string format, params object[] args)
        {
            var newArgs = new List<object>();
            newArgs.AddRange(args);
            var ret = Reader.ReadPackedGuid();
            newArgs.Add(ret);
            var str = String.Format(format, newArgs.ToArray());
            AppendLine(str);
            return ret;
        }

        public void For(int count, Action func)
        {
            for(var i = 0; i < count; ++i)
                func();
        }

        public void For(int count, Action<int> func)
        {
            for (var i = 0; i < count; ++i)
                func(i);
        }
    }
}
