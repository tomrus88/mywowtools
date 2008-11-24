using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace WoWPacketViewer
{
    public class SqLitePacketViewer : PacketViewerBase
    {
        public override int LoadData(string file)
        {
            using (var connection = new SQLiteConnection("Data Source=" + file))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM packets;";
                command.Prepare();

                var rows = (long)command.ExecuteScalar();

                command.CommandText = "SELECT direction, opcode, data FROM packets ORDER BY id;";
                command.Prepare();

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    //worker.ReportProgress((int)((float)m_packets.Count / (float)rows * 100.0f));
                    try
                    {
                        var direction = (Direction)reader.GetByte(0);
                        var opcode = (OpCodes)reader.GetInt16(1);
                        var data = (byte[])reader.GetValue(2);

                        m_packets.Add(new Packet(direction, opcode, data));
                    }
                    catch { }
                }
            }
            return m_packets.Count;
        }
    }
}
