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

        public string GetParsedString()
        {
        	return stringBuilder.ToString();
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

        private static readonly Dictionary<int, Type> Parsers = new Dictionary<int, Type>();
        private static void Init()
        {
            LoadAssembly(Assembly.GetCallingAssembly());
        	if (!Directory.Exists("parsers")) return;
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

        private static void LoadAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(Parser)))
                {
                    var attributes = (ParserAttribute[])type.GetCustomAttributes(typeof(ParserAttribute), true);
                    foreach (var attribute in attributes)
                    {
                        Parsers[(int)attribute.Code] = type;
                    }
                }
            }
        }

        private static readonly Parser UnknownParser = new UnknownPacketParser();

        public static Parser CreateParser(Packet packet)
        {
            Type type;
            if (Parsers.TryGetValue((int)packet.Code, out type))
            {
                return (Parser)Activator.CreateInstance(type, packet);
            }
            return UnknownParser;
        }

        protected Packet Packet { get; private set; }

        protected Parser(Packet packet)
        {
            Packet = packet;
        }

        public abstract string Parse();
    }
}
