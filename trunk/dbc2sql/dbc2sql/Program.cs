using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using WoWReader;

namespace dbc2sql
{
    class Program
    {
        static IWowClientDBReader m_reader;
        static XmlDocument m_definitions;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify file name!");
                return;
            }

            string fileName = args[0];

            if (!File.Exists(fileName))
            {
                Console.WriteLine("File {0} doesn't exist", fileName);
                return;
            }

            LoadDefinitions();

            if (Path.GetExtension(fileName).ToLowerInvariant() == ".dbc")
                m_reader = new DBCReader(fileName);
            else
                m_reader = new DB2Reader(fileName);

            Console.WriteLine("Records: {0}, Fields: {1}, Row Size {2}, String Table Size: {3}", m_reader.RecordsCount, m_reader.FieldsCount, m_reader.RecordSize, m_reader.StringTableSize);

            XmlElement definition = m_definitions["DBFilesClient"][fileName.Split('.')[0]];

            if (definition == null)
            {
                Console.WriteLine("Definition for file {0} not found! File name is case sensitive!", fileName);
                return;
            }

            XmlNodeList fields = definition.GetElementsByTagName("field");
            XmlNodeList indexes = definition.GetElementsByTagName("index");

            StreamWriter sqlWriter = new StreamWriter(fileName + ".sql");

            WriteSqlStructure(sqlWriter, fileName, fields, indexes);

            for (int i = 0; i < m_reader.RecordsCount; ++i)
            {
                GenericReader reader = m_reader.GetRowAsGenericReader(i);

                string result = "INSERT INTO `dbc_" + Path.GetFileNameWithoutExtension(fileName) + "` VALUES (";

                int flds = 0;

                foreach (XmlElement field in fields)
                {
                    switch (field.Attributes["type"].Value)
                    {
                        case "long":
                            {
                                var value1 = reader.ReadInt64();
                                result += value1;
                            }
                            break;
                        case "ulong":
                            {
                                var value2 = reader.ReadUInt64();
                                result += value2;
                            }
                            break;
                        case "int":
                            {
                                var value3 = reader.ReadInt32();
                                result += value3;
                            }
                            break;
                        case "uint":
                            {
                                var value4 = reader.ReadUInt32();
                                result += value4;
                            }
                            break;
                        case "short":
                            {
                                var value5 = reader.ReadInt16();
                                result += value5;
                            }
                            break;
                        case "ushort":
                            {
                                var value6 = reader.ReadUInt16();
                                result += value6;
                            }
                            break;
                        case "sbyte":
                            {
                                var value7 = reader.ReadSByte();
                                result += value7;
                            }
                            break;
                        case "byte":
                            {
                                var value8 = reader.ReadByte();
                                result += value8;
                            }
                            break;
                        case "float":
                            {
                                var value9 = Regex.Replace(reader.ReadSingle().ToString(), @",", @".");
                                result += value9;
                            }
                            break;
                        case "double":
                            {
                                var value10 = Regex.Replace(reader.ReadDouble().ToString(), @",", @".");
                                result += value10;
                            }
                            break;
                        case "string":
                            {
                                var value11 = StripBadCharacters(m_reader.GetString(reader.ReadInt32()));
                                result += ("\"" + value11 + "\"");
                            }
                            break;
                        default:
                            {
                                Console.WriteLine("Incorrect field type '{0}'.", field["type"].Value);
                            }
                            break;
                    }

                    if (flds != fields.Count - 1)
                        result += ", ";

                    flds++;
                }

                result += ");";
                sqlWriter.WriteLine(result);

                if (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    Console.WriteLine("Data under read!!!, diff {0}", reader.BaseStream.Length - reader.BaseStream.Position);
                }

                reader.Close();
            }

            sqlWriter.Flush();
            sqlWriter.Close();
        }

        static void LoadDefinitions()
        {
            Console.WriteLine("Loading XML configuration file...");
            m_definitions = new XmlDocument();
            m_definitions.Load("dbclayout.xml");
            Console.WriteLine("Done!");
        }

        static void WriteSqlStructure(StreamWriter sqlWriter, string fileName, XmlNodeList fields, XmlNodeList indexes)
        {
            sqlWriter.WriteLine("DROP TABLE IF EXISTS `dbc_{0}`;", Path.GetFileNameWithoutExtension(fileName));
            sqlWriter.WriteLine("CREATE TABLE `dbc_{0}` (", Path.GetFileNameWithoutExtension(fileName));

            foreach (XmlElement field in fields)
            {
                sqlWriter.Write("\t" + String.Format("`{0}`", field.Attributes["name"].Value));

                switch (field.Attributes["type"].Value)
                {
                    case "long":
                        sqlWriter.Write(" BIGINT NOT NULL DEFAULT '0'");
                        break;
                    case "ulong":
                        sqlWriter.Write(" BIGINT UNSIGNED NOT NULL DEFAULT '0'");
                        break;
                    case "int":
                        sqlWriter.Write(" INT NOT NULL DEFAULT '0'");
                        break;
                    case "uint":
                        sqlWriter.Write(" INT UNSIGNED NOT NULL DEFAULT '0'");
                        break;
                    case "short":
                        sqlWriter.Write(" SMALLINT NOT NULL DEFAULT '0'");
                        break;
                    case "ushort":
                        sqlWriter.Write(" SMALLINT UNSIGNED NOT NULL DEFAULT '0'");
                        break;
                    case "sbyte":
                        sqlWriter.Write(" TINYINT NOT NULL DEFAULT '0'");
                        break;
                    case "byte":
                        sqlWriter.Write(" TINYINT UNSIGNED NOT NULL DEFAULT '0'");
                        break;
                    case "float":
                        sqlWriter.Write(" FLOAT NOT NULL DEFAULT '0'");
                        break;
                    case "double":
                        sqlWriter.Write(" DOUBLE NOT NULL DEFAULT '0'");
                        break;
                    case "string":
                        sqlWriter.Write(" TEXT NOT NULL");
                        break;
                    default:
                        Console.WriteLine("wtf?");
                        break;
                }

                sqlWriter.WriteLine(",");
            }

            foreach (XmlElement index in indexes)
            {
                sqlWriter.WriteLine("\tPRIMARY KEY (`{0}`)", index["primary"].InnerText);
            }

            sqlWriter.WriteLine(") ENGINE=MyISAM DEFAULT CHARSET=utf8 COMMENT='Export of {0}';", Path.GetFileName(fileName));
            sqlWriter.WriteLine();
        }

        static string StripBadCharacters(string input)
        {
            input = Regex.Replace(input, @"'", @"\'");
            input = Regex.Replace(input, @"\""", @"\""");
            return input;
        }
    }

    class DBCReader : IWowClientDBReader
    {
        private const uint HeaderSize = 20;
        private const uint DBCFmtSig = 0x43424457;          // WDBC

        private GenericReader m_reader;

        private Dictionary<int, string> m_stringTable = new Dictionary<int, string>();

        public int RecordsCount { get; private set; }
        public int FieldsCount { get; private set; }
        public int RecordSize { get; private set; }
        public int StringTableSize { get; private set; }

        private byte[][] m_rows;

        public byte[] GetRowAsByteArray(int row)
        {
            return m_rows[row];
        }

        public GenericReader GetRowAsGenericReader(int row)
        {
            return new GenericReader(new MemoryStream(m_rows[row]), Encoding.UTF8);
        }

        public string GetString(int offset)
        {
            return m_stringTable[offset];
        }

        public DBCReader(string fileName)
        {
            m_reader = new GenericReader(fileName, Encoding.UTF8);

            if (m_reader.BaseStream.Length < HeaderSize)
            {
                Console.WriteLine("File {0} is corrupted!", fileName);
                return;
            }

            if (m_reader.ReadUInt32() != DBCFmtSig)
            {
                Console.WriteLine("File {0} isn't valid DBC file!", fileName);
                return;
            }

            RecordsCount = m_reader.ReadInt32();
            FieldsCount = m_reader.ReadInt32();
            RecordSize = m_reader.ReadInt32();
            StringTableSize = m_reader.ReadInt32();

            m_rows = new byte[RecordsCount][];

            for (int i = 0; i < RecordsCount; i++)
                m_rows[i] = m_reader.ReadBytes(RecordSize);

            int stringTableStart = (int)m_reader.BaseStream.Position;

            while (m_reader.BaseStream.Position != m_reader.BaseStream.Length)
            {
                int index = (int)m_reader.BaseStream.Position - stringTableStart;
                m_stringTable[index] = m_reader.ReadStringNull();
            }

            m_reader.Close();
            m_reader = null;
        }

        ~DBCReader()
        {
            m_stringTable.Clear();

            if (m_reader != null)
                m_reader.Close();
        }
    }

    class DB2Reader : IWowClientDBReader
    {
        private const int HeaderSize = 48;
        private const uint DB2FmtSig = 0x32424457;          // WDB2
        private const uint ADBFmtSig = 0x32484357;          // WCH2

        private GenericReader m_reader;

        private Dictionary<int, string> m_stringTable = new Dictionary<int, string>();

        public int RecordsCount { get; private set; }
        public int FieldsCount { get; private set; }
        public int RecordSize { get; private set; }
        public int StringTableSize { get; private set; }

        private byte[][] m_rows;

        public byte[] GetRowAsByteArray(int row)
        {
            return m_rows[row];
        }

        public GenericReader GetRowAsGenericReader(int row)
        {
            return new GenericReader(new MemoryStream(m_rows[row]), Encoding.UTF8);
        }

        public string GetString(int offset)
        {
            return m_stringTable[offset];
        }

        public DB2Reader(string fileName)
        {
            m_reader = new GenericReader(fileName, Encoding.UTF8);

            if (m_reader.BaseStream.Length < HeaderSize)
            {
                Console.WriteLine("File {0} is corrupted!", fileName);
                return;
            }

            var signature = m_reader.ReadUInt32();

            if (signature != DB2FmtSig && signature != ADBFmtSig)
            {
                Console.WriteLine("File {0} isn't valid DBC file!", fileName);
                return;
            }

            RecordsCount = m_reader.ReadInt32();
            FieldsCount = m_reader.ReadInt32(); // not fields count in WCH2
            RecordSize = m_reader.ReadInt32();
            StringTableSize = m_reader.ReadInt32();

            uint tableHash = m_reader.ReadUInt32(); // new field in WDB2
            uint build = m_reader.ReadUInt32(); // new field in WDB2

            int unk1 = m_reader.ReadInt32(); // new field in WDB2 (Unix time in WCH2)
            int unk2 = m_reader.ReadInt32(); // new field in WDB2
            int unk3 = m_reader.ReadInt32(); // new field in WDB2 (index table?)
            int locale = m_reader.ReadInt32(); // new field in WDB2
            int unk5 = m_reader.ReadInt32(); // new field in WDB2

            if (unk3 != 0)
            {
                m_reader.ReadBytes(unk3 * 4 - HeaderSize);     // an index for rows
                m_reader.ReadBytes(unk3 * 2 - HeaderSize * 2); // a memory allocation bank
            }

            m_rows = new byte[RecordsCount][];

            for (int i = 0; i < RecordsCount; i++)
                m_rows[i] = m_reader.ReadBytes(RecordSize);

            int stringTableStart = (int)m_reader.BaseStream.Position;

            while (m_reader.BaseStream.Position != m_reader.BaseStream.Length)
            {
                int index = (int)m_reader.BaseStream.Position - stringTableStart;
                m_stringTable[index] = m_reader.ReadStringNull();
            }

            m_reader.Close();
            m_reader = null;
        }

        ~DB2Reader()
        {
            m_stringTable.Clear();

            if (m_reader != null)
                m_reader.Close();
        }
    }

    interface IWowClientDBReader
    {
        int RecordsCount { get; }
        int FieldsCount { get; }
        int RecordSize { get; }
        int StringTableSize { get; }
        byte[] GetRowAsByteArray(int row);
        GenericReader GetRowAsGenericReader(int row);
        string GetString(int offset);
    }
}
