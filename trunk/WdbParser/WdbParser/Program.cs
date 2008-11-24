using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Linq;
using System.Globalization;

namespace WdbParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                if (!Directory.Exists("wdb"))
                {
                    Console.WriteLine("Please specify folder");
                    return;
                }
                WdbParser wdbParser = new WdbParser("wdb");
            }
            else
            {
                if (!Directory.Exists(args[0]))
                {
                    Console.WriteLine("Please specify folder");
                    return;
                }
                WdbParser wdbParser = new WdbParser(args[0]);
            }
        }
    }

    class WdbParser
    {
        XmlDocument m_definitions2 = new XmlDocument();
        string quote = "\"";

        public WdbParser(string folderName)
        {
            LoadDefinitions();

            XmlNodeList xmlNodes = m_definitions2.GetElementsByTagName("wdbId");
            foreach (XmlElement el in xmlNodes)
            {
                string fileName = folderName + "\\" + el.Attributes["name"].Value + ".wdb";
                if (!File.Exists(fileName))
                {
                    Console.WriteLine("File {0} not found!", fileName);
                    continue;
                }
                ParseWdbFile(fileName, el);
            }

            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        void LoadDefinitions()
        {
            Console.WriteLine("Loading XML configuration file...");

            string defFileName = "definitions.xml";

            if (!File.Exists(defFileName))
            {
                Console.WriteLine("Configuration file {0} not found!", Directory.GetCurrentDirectory() + "\\" + defFileName);
                return;
            }

            m_definitions2.Load("definitions.xml");

            Console.WriteLine("Done.");
        }

        void ParseWdbFile(string fileName, XmlElement el)
        {
            GenericReader reader = new GenericReader(fileName);
            StreamWriter writer = new StreamWriter(fileName + ".sql");
            writer.AutoFlush = true;
            byte[] sig = reader.ReadBytes(4);
            uint build = reader.ReadUInt32();
            byte[] locale = reader.ReadBytes(4);
            uint unk1 = reader.ReadUInt32();
            uint unk2 = reader.ReadUInt32();

            Console.WriteLine("Signature: {0}, build {1}, locale {2}", Encoding.ASCII.GetString(sig.Reverse().ToArray()), build, Encoding.ASCII.GetString(locale.Reverse().ToArray()));
            Console.WriteLine("unk1 {0}, unk2 {1}", unk1, unk2);

            /*while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                uint entry = reader.ReadUInt32();
                int len2 = reader.ReadInt32();
                byte[] data = reader.ReadBytes(len2);
            }*/

            string InsertQuery = String.Format("INSERT INTO {0} VALUES (", el.Attributes["name"].Value);

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                uint i = 0;
                XmlNodeList xmlNodes = el.GetElementsByTagName("wdbElement");
                foreach (XmlElement elem in xmlNodes)
                {
                    var valtype = elem.Attributes["type"].Value;
                    var minver = Convert.ToUInt32(elem.Attributes["minver"].Value);
                    var maxver = Convert.ToUInt32(elem.Attributes["maxver"].Value);
                    string valname = null;
                    if (elem.Attributes["name"] != null)
                        valname = elem.Attributes["name"].Value;

                    if (build >= minver && build <= maxver)
                    {
                        switch (valtype.ToUpper())
                        {
                            case "INTEGER":
                                {
                                    var valval = reader.ReadInt32();
                                    if (valname != null)
                                        InsertQuery += valval;
                                    break;
                                }
                            case "UINTEGER":
                                {
                                    var valval = reader.ReadUInt32();
                                    if (valname != null)
                                        InsertQuery += valval;
                                    break;
                                }
                            case "SINGLE":
                                {
                                    var valval = reader.ReadSingle().ToString("F", CultureInfo.InvariantCulture);
                                    if (valname != null)
                                        InsertQuery += valval;
                                    break;
                                }
                            case "VARCHAR":
                                {
                                    var valval = Regex.Replace(reader.ReadStringNull(), @"'", @"\'");
                                    valval = Regex.Replace(valval, "\"", "\\\"");
                                    if (valname != null)
                                        InsertQuery += (quote + valval + quote);
                                    break;
                                }
                            case "SMALLINT":
                                {
                                    var valval = reader.ReadInt16();
                                    if (valname != null)
                                        InsertQuery += valval;
                                    break;
                                }
                            case "TINYINT":
                                {
                                    var valval = reader.ReadSByte();
                                    if (valname != null)
                                        InsertQuery += valval;
                                    break;
                                }
                            case "BYTETOINT":
                                {
                                    var valval = reader.ReadInt32();
                                    if (valname != null)
                                        InsertQuery += valval;
                                    break;
                                }
                            case "STRUCT":
                                {
                                    var valval = reader.ReadInt32();
                                    if (valname != null)
                                        InsertQuery += valval;
                                    InsertQuery += ",";

                                    var maxcount = Convert.ToUInt32(elem.Attributes["maxcount"].Value);

                                    XmlNodeList structNodes = elem.GetElementsByTagName("structElement");

                                    if (valval > 0)
                                    {
                                        for (int k = 0; k < valval; k++)
                                        {
                                            foreach (XmlElement structElem in structNodes)
                                            {
                                                // something wrong here...
                                                bool last = (valval == maxcount);
                                                ReadAndDumpByType(ref reader, structElem.Attributes["type"].Value.ToUpper(), valname, ref InsertQuery, last);
                                            }
                                        }
                                    }

                                    if(maxcount > 0)
                                    {
                                        int lim = (int)maxcount - valval;
                                        for (int p = 0; p < lim; p++)
                                            foreach (XmlElement structElem in structNodes)
                                                InsertQuery += "0,";
                                        // remove last ","
                                        InsertQuery = InsertQuery.Remove(InsertQuery.Length - 1);
                                    }

                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("Unknown type {0}", valtype.ToUpper());
                                    break;
                                }
                        }

                        if (i != xmlNodes.Count - 1)
                        {
                            if (valname != null)
                                InsertQuery += ",";
                        }
                        else
                            InsertQuery += ");";
                    }

                    if (reader.BaseStream.Length - reader.BaseStream.Position == 4)
                    {
                        Console.WriteLine("end {0}", reader.ReadUInt32());
                        break;
                    }

                    if (i == xmlNodes.Count - 1)
                    {
                        //int cols = InsertQuery.Split(',').Length;

                        //if (cols != 0x85)
                        //    Console.WriteLine(InsertQuery);

                        writer.WriteLine(InsertQuery);
                        InsertQuery = String.Format("INSERT INTO {0} VALUES (", el.Attributes["name"].Value);
                    }

                    i++;
                }
            }

            if (reader.BaseStream.Position != reader.BaseStream.Length)
                Console.WriteLine("wtf?");

            reader.Close();
            writer.Flush();
            writer.Close();
        }

        void ReadAndDumpByType(ref GenericReader reader, string valtype, string valname, ref string InsertQuery, bool last)
        {
            switch (valtype.ToUpper())
            {
                case "INTEGER":
                    {
                        var valval = reader.ReadInt32();
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                case "UINTEGER":
                    {
                        var valval = reader.ReadUInt32();
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                case "SINGLE":
                    {
                        var valval = reader.ReadSingle().ToString("F", CultureInfo.InvariantCulture);
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                case "VARCHAR":
                    {
                        var valval = Regex.Replace(reader.ReadStringNull(), @"'", @"\'");
                        valval = Regex.Replace(valval, "\"", "\\\"");
                        if (valname != null)
                            InsertQuery += (quote + valval + quote);
                        break;
                    }
                case "SMALLINT":
                    {
                        var valval = reader.ReadInt16();
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                case "TINYINT":
                    {
                        var valval = reader.ReadSByte();
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                case "BYTETOINT":
                    {
                        var valval = reader.ReadInt32();
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Unknown type {0}", valtype.ToUpper());
                        break;
                    }
            }

            if (!last)
                InsertQuery += ",";
            else
                Console.WriteLine("test!");
        }
    }
}
