using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using WoWPacketViewer.Parsers;

namespace WoWPacketViewer
{
    public abstract class Parser
    {
        private static StringBuilder _stringBuilder = new StringBuilder();

        public static void Append(string str)
        {
            _stringBuilder.Append(str);
        }

        public static void AppendLine()
        {
            _stringBuilder.AppendLine();
        }

        public static void AppendLine(string str)
        {
            _stringBuilder.AppendLine(str);
        }

        public static void AppendFormat(string format, params object[] args)
        {
            _stringBuilder.AppendFormat(format, args);
        }

        public static void AppendFormatLine(string format, params object[] args)
        {
            _stringBuilder.AppendFormat(format, args).AppendLine();
        }

        public static string GetParsedString()
        {
            string ret = _stringBuilder.ToString();
            _stringBuilder.Remove(0, _stringBuilder.Length);
            return ret;
        }

        public void CheckPacket(BinaryReader gr)
        {
            if (gr.BaseStream.Position != gr.BaseStream.Length)
            {
                string msg = String.Format("{0}: Packet size changed, should be {1} instead of {2}", Packet.Code, gr.BaseStream.Position, gr.BaseStream.Length);
                MessageBox.Show(msg);
            }
        }

        static Parser()
        {
            Init();
        }

        private static readonly Dictionary<int, Type> _parsers = new Dictionary<int, Type>();
        private static void Init()
        {
            LoadAssembly(Assembly.GetCallingAssembly());
            if (Directory.Exists("parsers"))
            {
                foreach (var file in Directory.GetFiles("parsers", "*.dll", SearchOption.AllDirectories))
                {
                    try
                    {
                        var assembly = Assembly.LoadFile(Path.GetFullPath(file));
                        LoadAssembly(assembly);
                    }
                    catch { }
                }
            }
        }

        private static void LoadAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(Parser)))
                {
                    var attributes = (ParserAttribute[])type.GetCustomAttributes(typeof(ParserAttribute), true);
                    foreach (var attribute in attributes)
                    {
                        _parsers[(int)attribute.Code] = type;
                    }
                }
            }
        }

        private static Parser _unknownParser = new UnknownPacketParser();

        public static Parser CreateParser(Packet packet)
        {
            Type type;
            if (_parsers.TryGetValue((int)packet.Code, out type))
            {
                return (Parser)Activator.CreateInstance(type, packet);
            }
            return _unknownParser;
        }

        protected Packet Packet { get; private set; }

        protected Parser(Packet packet)
        {
            Packet = packet;
        }

        public abstract string Parse();
    }
}
