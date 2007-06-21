﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Data.SQLite;

namespace proxy_db_parser
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo di = new DirectoryInfo(".");
            FileInfo[] fi = di.GetFiles("*.sqlite", SearchOption.AllDirectories);

            foreach (FileInfo f in fi)
            {
                Console.WriteLine("Parsing {0}...", f.Name);

                string ofn = f.FullName + ".dump";

                BinaryWriter bw = new BinaryWriter(new FileStream(ofn, FileMode.Create));

                SQLiteConnection connection = new SQLiteConnection("Data Source=" + f.Name);
                SQLiteCommand command = new SQLiteCommand(connection);
                connection.Open();
                command.CommandText = "SELECT opcode,data FROM packets;";
                command.Prepare();
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    uint opcode = (uint)reader.GetInt32(0);
                    byte[] data = (byte[])reader.GetValue(1);

                    uint size = sizeof(uint) + (uint)data.Length;

                    bw.Write(size);
                    bw.Write(opcode);
                    bw.Write(data);
                }

                reader.Close();
                connection.Close();

                bw.Flush();
                bw.Close();
            }
        }
    }
}
