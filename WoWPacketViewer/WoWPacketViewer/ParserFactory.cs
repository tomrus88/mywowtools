using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WoWPacketViewer.Parsers;

namespace WoWPacketViewer
{
	public class ParserFactory
	{
		private static readonly Dictionary<int, Type> Parsers = new Dictionary<int, Type>();

		internal static void Init()
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
	}
}