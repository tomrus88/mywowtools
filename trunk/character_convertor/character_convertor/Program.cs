using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using MySql.Data.MySqlClient;

namespace character_convertor
{
    enum ObjectType
    {
        TYPE_OBJECT = 1,
        TYPE_ITEM = 2,
        TYPE_CONTAINER = 6,
        TYPE_UNIT = 8,
        TYPE_PLAYER = 16,
        TYPE_GAMEOBJECT = 32,
        TYPE_DYNAMICOBJECT = 64,
        TYPE_CORPSE = 128,
        TYPE_AIGROUP = 256,
        TYPE_AREATRIGGER = 512
    };

    enum ObjectTypeId
    {
        TYPEID_OBJECT = 0,
        TYPEID_ITEM = 1,
        TYPEID_CONTAINER = 2,
        TYPEID_UNIT = 3,
        TYPEID_PLAYER = 4,
        TYPEID_GAMEOBJECT = 5,
        TYPEID_DYNAMICOBJECT = 6,
        TYPEID_CORPSE = 7,
        TYPEID_AIGROUP = 8,
        TYPEID_AREATRIGGER = 9
    };

    class Program
    {
        static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(TopLevelErrorHandler);

            Console.WriteLine("Character convertor for MaNGoS");
            Console.WriteLine("Client varsion 2.0.12->2.1.1");

            Console.Write("Enter DB name: ");
            string dbName = Console.ReadLine();
            Console.Write("Enter DB user name: ");
            string dbUser = Console.ReadLine();
            Console.Write("Enter DB password: ");
            string dbPass = Console.ReadLine();

            string dbServer = "localhost";
            string dbPort = "3306";

            string cs = "Server=" + dbServer + ";Port=" + dbPort + ";Database=" + dbName + ";Uid=" + dbUser + ";Pwd=" + dbPass;
            string query = "SELECT `data` FROM `character`";

            MySqlConnection connection = new MySqlConnection(cs);

            try
            {
                connection.Open();
            }
            catch (Exception exc)
            {
                //Console.WriteLine(exc.ToString());
                Console.WriteLine(exc.Message);
                Console.ReadKey();
                return;
            }

            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader;
            try
            {
                reader = command.ExecuteReader();
            }
            catch (Exception exc)
            {
                //Console.WriteLine(exc.ToString());
                Console.WriteLine(exc.Message);
                Console.ReadKey();
                return;
            }

            while (reader.Read())
            {
                string data = reader.GetString(0);

                Object src = new Object((ushort)UpdateFieldsOld.PLAYER_END);
                Object dst = new Object((ushort)UpdateFieldsNew.PLAYER_END);

                // load values
                src.LoadValues(data);
                // update fields
                UpdatePlayerFields(src, dst);
                // save
                dst.Save(connection);
            }

            // close connection to database
            connection.Close();
        }

        private static void TopLevelErrorHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            //Console.WriteLine("Error Occured : " + e.Message);
            Console.WriteLine("Error Occured : " + e.ToString());
        }

        static void UpdatePlayerFields(Object srcobj, Object dstobj)
        {
            // TODO: implement it!
            // sample line:
            dstobj.SetUInt64Value((ushort)UpdateFieldsNew.UNIT_FIELD_CHARM, srcobj.GetUInt64Value((ushort)UpdateFieldsOld.UNIT_FIELD_CHARM));
        }
    }

    class Object
    {
        ushort m_valuesCount;
        uint[] m_uint32Values;

        public Object(ushort valuecount)
        {
            m_valuesCount = valuecount;

            m_uint32Values = new uint[m_valuesCount];
        }

        ~Object()
        {

        }

        public void LoadValues(string data)
        {
            string[] values = data.Split(' ');

            for (ushort i = 0; i < m_valuesCount; i++)
                m_uint32Values[i] = Convert.ToUInt32(values[i]);
        }

        public void Save(MySqlConnection conn)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("UPDATE `character` SET `data`='");

            for (ushort i = 0; i < m_valuesCount; i++)
            {
                sb.Append(GetUInt32Value(i));
                sb.Append(" ");
            }
            sb.Append("' WHERE `guid`='" + GetGUIDLow() + "'");

            MySqlCommand cmd = new MySqlCommand(sb.ToString(), conn);
            int affected = cmd.ExecuteNonQuery();

            if (affected == 0)
                Console.WriteLine("Query failed for player {0}", GetGUIDLow());
        }

        public uint GetGUIDLow()
        {
            return m_uint32Values[(int)UpdateFieldsNew.OBJECT_FIELD_GUID];
        }

        public uint GetGUIDHigh()
        {
            return m_uint32Values[(int)UpdateFieldsNew.OBJECT_FIELD_GUID + 1];
        }

        public uint GetUInt32Value(ushort index)
        {
            return m_uint32Values[index];
        }

        public ulong GetUInt64Value(ushort index)
        {
            ulong result = 0;

            result += m_uint32Values[index];
            result += ((ulong)m_uint32Values[index + 1] << 32);

            return result;
        }

        public float GetFloatValue(ushort index)
        {
            byte[] temp = BitConverter.GetBytes(m_uint32Values[index]);
            return BitConverter.ToSingle(temp, 0);
        }

        public void SetUInt32Value(ushort index, uint value)
        {
            m_uint32Values[index] = value;
        }

        public void SetUInt64Value(ushort index, ulong value)
        {
            uint low = (uint)value;
            uint high = (uint)(value >> 32);

            m_uint32Values[index] = low;
            m_uint32Values[index + 1] = high;
        }

        public void SetFloatValue(ushort index, float value)
        {
            byte[] temp = BitConverter.GetBytes(value);
            m_uint32Values[index] = BitConverter.ToUInt32(temp, 0);
        }
    }
}
