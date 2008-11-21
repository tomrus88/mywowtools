using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

using WoWReader;

namespace dbc2sql
{
    class Program
    {
        static DBCReader m_reader;
        static XmlDocument m_definitions;

        static void Main(string[] args)
        {
            if(args.Length == 0)
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

            m_reader = new DBCReader(fileName);

            Console.WriteLine("Records: {0}, Fields: {1}, String Table Size: {2}", m_reader.RecordsCount, m_reader.FieldsCount, m_reader.StringTableSize);

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

                string result = "INSERT INTO dbc_" + Path.GetFileNameWithoutExtension(fileName) + " VALUES (";

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
            sqlWriter.WriteLine("DROP TABLE IF EXISTS dbc_{0};", Path.GetFileNameWithoutExtension(fileName));
            sqlWriter.WriteLine("CREATE TABLE dbc_{0} (", Path.GetFileNameWithoutExtension(fileName));

            foreach (XmlElement field in fields)
            {
                sqlWriter.Write("\t" + field.Attributes["name"].Value);

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
                sqlWriter.WriteLine("\tPRIMARY KEY ({0})", index["primary"].InnerText);
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

    class DBCReader
    {
        private const uint DBCFmtSig = 0x43424457;          // WDBC

        private GenericReader m_stringsReader;

        private int m_recordsCount;
        private int m_fieldsCount;

        private byte[][] m_rows;

        public int RecordsCount
        {
            get { return m_recordsCount; }
        }

        public int FieldsCount
        {
            get { return m_fieldsCount; }
        }

        public int StringTableSize
        {
            get { return (int)m_stringsReader.BaseStream.Length; }
        }

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
            m_stringsReader.BaseStream.Position = offset;
            return m_stringsReader.ReadStringNull();
        }

        public DBCReader(string fileName)
        {
            GenericReader m_reader = new GenericReader(fileName, Encoding.UTF8);

            if (m_reader.BaseStream.Length < 4 * 5)
            {
                Console.WriteLine("File {0} is corrupted!", fileName);
                return;
            }

            if (m_reader.ReadUInt32() != DBCFmtSig)
            {
                Console.WriteLine("File {0} isn't valid DBC file!", fileName);
                return;
            }

            m_recordsCount = m_reader.ReadInt32();
            m_fieldsCount = m_reader.ReadInt32();

            int recordSize = m_reader.ReadInt32();
            int stringTableSize = m_reader.ReadInt32();

            m_reader.BaseStream.Position = m_reader.BaseStream.Length - stringTableSize;

            byte[] stringTable = m_reader.ReadBytes(stringTableSize);
            m_stringsReader = new GenericReader(new MemoryStream(stringTable), Encoding.UTF8);

            m_reader.BaseStream.Position = 20;

            m_rows = new byte[m_recordsCount][];

            for (int i = 0; i < m_recordsCount; i++)
                m_rows[i] = m_reader.ReadBytes(recordSize);

            m_reader.Close();
        }
    }
}
