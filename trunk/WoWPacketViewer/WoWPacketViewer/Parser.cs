using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WoWPacketViewer.Parsers;

namespace WoWPacketViewer {
	public abstract class Parser {
		static Parser() {
			Init();
		}

		private static readonly Dictionary<int, Type> _parsers = new Dictionary<int, Type>();
		private static void Init() {
			LoadAssembly(Assembly.GetCallingAssembly());
			if(Directory.Exists("parsers")) {
				foreach(var file in Directory.GetFiles("parsers", "*.dll", SearchOption.AllDirectories)) {
					try {
						var assembly = Assembly.LoadFile(Path.GetFullPath(file));
						LoadAssembly(assembly);
					} catch { }
				}
			}
		}

		private static void LoadAssembly(Assembly assembly) {
			foreach(var type in assembly.GetTypes()) {
				if(type.IsSubclassOf(typeof(Parser))) {
					var attributes = (ParserAttribute[])type.GetCustomAttributes(typeof(ParserAttribute), true);
					foreach(var attribute in attributes) {
						_parsers[(int)attribute.Code] = type;
					}
				}
			}
		}

		private static Parser _uncknownParser = new UnknownPacketParser();

		public static Parser CreateParser(Packet packet) {
			Type type;
			if(_parsers.TryGetValue((int)packet.Code, out type)) {
				return (Parser)Activator.CreateInstance(type, packet);
			}
			return _uncknownParser;
		}

		protected Packet Packet { get; private set; }

		protected Parser(Packet packet) {
			Packet = packet;
		}

		public abstract string Parse();
	}
}
