using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace WdbParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("Please specify folder");
                return;
            }
            WdbParser wdbParser = new WdbParser(args[0]);
        }
    }

    struct ConfigDocument
    {
        public string Name;
        public int skipBytes;
        public Dictionary<int, ConfigElement> Elements;
    };

    struct ConfigElement
    {
        public string Name;
        public string Key;
        public string Type;
        public string StructType;
        public int MinVer;
        public int MaxVer;
        public string CrLf;
        public int MaxVal;
    };

    class WdbParser
    {
        Dictionary<int, ConfigDocument> m_definitions = new Dictionary<int, ConfigDocument>();
        string quote = "\"";

        public WdbParser(string folderName)
        {
            LoadDefinitions();

            for (int i = 0; i < m_definitions.Count; i++)
            {
                ConfigDocument doc = m_definitions[i];
                string fileName = folderName + "\\" + doc.Name + ".wdb";
                if (!File.Exists(fileName))
                {
                    Console.WriteLine("File {0} not found!", fileName);
                    continue;
                }
                ParseWdbFile(fileName, i);
            }

            Console.ReadKey();
        }

        void LoadDefinitions()
        {
            Console.WriteLine("Loading XML configuration file...");

            XmlDocument definitions = new XmlDocument();
            definitions.Load("definitions.xml");

            int i = 0;
            foreach (XmlElement node in definitions.GetElementsByTagName("wdbId"))
            {
                ConfigDocument configDocument = new ConfigDocument();

                configDocument.Name = node.Attributes["name"].Value;
                configDocument.skipBytes = Convert.ToInt32(node["skipBytes"].Attributes["numBytes"].Value);
                configDocument.Elements = new Dictionary<int, ConfigElement>();

                int j = 0;
                foreach (XmlElement node2 in node.GetElementsByTagName("wdbElement"))
                {
                    ConfigElement configElement = new ConfigElement();

                    if (node2.Attributes["name"] != null)
                        configElement.Name = node2.Attributes["name"].Value;

                    if (node2.Attributes["key"] != null)
                        configElement.Key = node2.Attributes["key"].Value;

                    if (node2.Attributes["type"] != null)
                        configElement.Type = node2.Attributes["type"].Value;

                    if (node2.Attributes["structtype"] != null)
                        configElement.StructType = node2.Attributes["structtype"].Value;

                    if (node2.Attributes["minver"] != null)
                        configElement.MinVer = Convert.ToInt32(node2.Attributes["minver"].Value);

                    if (node2.Attributes["maxver"] != null)
                        configElement.MaxVer = Convert.ToInt32(node2.Attributes["maxver"].Value);

                    if (node2.Attributes["CrLf"] != null)
                        configElement.CrLf = node2.Attributes["CrLf"].Value;

                    if (node2.Attributes["maxval"] != null)
                    {
                        int tmp = Convert.ToInt32(node2.Attributes["maxval"].Value);
                        if (tmp > 0)
                            configElement.MaxVal = tmp;
                        else
                            configElement.MaxVal = 0;
                    }

                    configDocument.Elements[j] = configElement;
                    j++;
                }

                m_definitions[i] = configDocument;
                i++;
            }

            Console.WriteLine("Done.");
        }

        void ParseWdbFile(string fileName, int wdbIndex)
        {
            GenericReader reader = new GenericReader(fileName);
            StreamWriter writer = new StreamWriter(fileName + ".sql");
            writer.AutoFlush = true;
            byte[] sig = reader.ReadBytes(4);
            uint build = reader.ReadUInt32();
            byte[] locale = reader.ReadBytes(4);
            uint unk1 = reader.ReadUInt32();
            uint unk2 = reader.ReadUInt32();

            Console.WriteLine("Sig {0}, build {1}, locale {2}, unk1 {3}, unk2 {4}", Encoding.ASCII.GetString(sig), build, Encoding.ASCII.GetString(locale), unk1, unk2);

            /*while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                uint entry = reader.ReadUInt32();
                int len2 = reader.ReadInt32();
                byte[] data = reader.ReadBytes(len2);
            }*/

            //byte firstAdded = 0;

            string InsertQuery = String.Format("INSERT INTO {0} VALUES (", m_definitions[wdbIndex].Name);

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                for (int i = 0; i < m_definitions[wdbIndex].Elements.Count; i++)
                {
                    var valtype = m_definitions[wdbIndex].Elements[i].Type;
                    var structtype = m_definitions[wdbIndex].Elements[i].StructType;
                    var minver = m_definitions[wdbIndex].Elements[i].MinVer;
                    var maxver = m_definitions[wdbIndex].Elements[i].MaxVer;
                    //var valkey = m_definitions[wdbIndex].Elements[i].Key;
                    var valname = m_definitions[wdbIndex].Elements[i].Name;
                    //var valcrlf = m_definitions[wdbIndex].Elements[i].CrLf;
                    //var maxval = m_definitions[wdbIndex].Elements[i].MaxVal;

                    if (build >= minver && build <= maxver)
                    {
                        /*if (valname != "")
                        {
                            if (firstAdded > 0)
                                InsertQuery += ",";
                            else
                                firstAdded = 1;
                            InsertQuery += valname += "=";
                        }*/

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
                                    var valval = reader.ReadSingle().ToString().Replace(",", ".");
                                    if (valname != null)
                                        InsertQuery += valval;
                                    break;
                                }
                            case "VARCHAR":
                                {
                                    var valval = reader.ReadStringNull().Replace("'", "\'");
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
                            case "COUNT":
                                {
                                    var valval = reader.ReadInt32();
                                    if (valname != null)
                                        InsertQuery += valval;
                                    InsertQuery += ",";

                                    if (valval > 0)
                                    {
                                        var types = structtype.Split(' ');
                                        for (int k = 0; k < valval; k++)
                                        {
                                            for (int kk = 0; kk < types.Length; kk++)
                                            {
                                                bool last = (valval == 10);
                                                ReadAndDumpByType(ref reader, types[kk].ToUpper(), valname, ref InsertQuery, last);
                                            }
                                        }
                                    }

                                    for (int p = 0; p < (10 - valval); p++)
                                    {
                                        if (p != 10 - valval - 1)
                                            InsertQuery += "0,0,";
                                        else
                                            InsertQuery += "0,0";
                                    }

                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("Unknown type {0}", valtype.ToUpper());
                                    break;
                                }
                        }

                        if (i != m_definitions[wdbIndex].Elements.Count - 1)
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

                    if (i == m_definitions[wdbIndex].Elements.Count - 1)
                    {
                        //int cols = InsertQuery.Split(',').Length;

                        //if (cols != 0x85)
                        //    Console.WriteLine(InsertQuery);

                        writer.WriteLine(InsertQuery);
                        InsertQuery = String.Format("INSERT INTO {0} VALUES (", m_definitions[wdbIndex].Name);
                    }
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
                        var valval = reader.ReadSingle().ToString().Replace(",", ".");
                        if (valname != null)
                            InsertQuery += valval;
                        break;
                    }
                case "VARCHAR":
                    {
                        var valval = reader.ReadStringNull().Replace("'", "\'");
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
        }
    }
}
