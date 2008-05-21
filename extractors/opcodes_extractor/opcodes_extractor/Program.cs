using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using WoWReader;

namespace opcodes_extractor
{
    class Program
    {
        static string wow_exe = "WoW.exe";

        static FileStream fs = null;
        static GenericReader gr = null;
        static TextReader tr = null;

        static string stream_string = String.Empty;

        static void Main(string[] args)
        {
            if (!File.Exists(wow_exe))
            {
                Console.WriteLine("File {0} not found!", wow_exe);
                return;
            }

            fs = new FileStream(wow_exe, FileMode.Open);
            gr = new GenericReader(fs);
            tr = new StreamReader(fs, Encoding.ASCII);

            stream_string = tr.ReadToEnd();

            gr.BaseStream.Position = FindStartPos();

            string name = String.Empty;

            List<string> names = new List<string>();

            do
            {
                name = gr.ReadStringNull();
                names.Add(name);
                FindNextPos();
            } while (!name.Equals("MSG_NULL_ACTION"));

            tr.Close();
            gr.Close();
            fs.Close();

            names.Reverse();

            StreamWriter sw1 = new StreamWriter("Opcodes.h");
            StreamWriter sw2 = new StreamWriter("Opcodes.cpp");

            sw1.WriteLine("enum Opcodes");
            sw1.WriteLine("{");

            sw2.WriteLine("const char* g_worldOpcodeNames[NUM_MSG_TYPES] =");
            sw2.WriteLine("{");

            foreach (string str in names)
            {
                string temp = str;

                int spaces = 47 - temp.Length;

                for (int i = 0; i < spaces; i++)
                    temp += " ";

                sw1.WriteLine("    {0} = 0x{1},", temp, names.IndexOf(str).ToString("X3"));

                if (str != "NUM_MSG_TYPES")
                    sw2.WriteLine("    \"{0}\",", str);
            }

            names.Clear();

            sw1.WriteLine("};");
            sw2.WriteLine("};");

            sw1.Flush();
            sw1.Close();
            sw2.Flush();
            sw2.Close();
        }

        static int FindStartPos()
        {
            return stream_string.IndexOf("NUM_MSG_TYPES");
        }

        static int FindNextPos()
        {
            while (gr.PeekChar() == 0x00)
            {
                gr.BaseStream.Position++;
            }

            return (int)gr.BaseStream.Position;
        }
    }
}
