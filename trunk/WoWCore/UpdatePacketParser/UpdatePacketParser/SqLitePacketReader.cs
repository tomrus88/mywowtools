using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.IO;
using WoWReader;

namespace UpdatePacketParser {
	public class SqLitePacketReader : PacketReaderBase {
		private static DbProviderFactory factory = System.Data.SQLite.SQLiteFactory.Instance;
		// DbProviderFactories.GetFactory("System.Data.SQLite");
		private DbDataReader _reader;

		public SqLitePacketReader(string filename) {
			var connection = factory.CreateConnection();
			connection.ConnectionString = "Data Source=" + filename;
			connection.Open();

			//TODO: Добавить определение билда!
			var command = connection.CreateCommand();
			command.CommandText = "SELECT opcode, data FROM packets WHERE opcode=169 OR opcode=502 ORDER BY id;";
			command.Prepare();

			_reader = command.ExecuteReader();
		}

		public override Packet ReadPacket() {
			if(!_reader.Read()) {
				_reader.Close();
				return null;
			}

			var packet = new Packet();
			packet.Code = (int)_reader.GetInt32(0);
			packet.Data = (byte[])_reader.GetValue(1);
			packet.Size = packet.Data.Length;
			return packet;
		}
	}
}
