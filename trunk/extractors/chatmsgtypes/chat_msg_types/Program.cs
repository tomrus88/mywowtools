using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using WoWReader;

namespace chat_msg_types
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
            gr = new GenericReader(fs, Encoding.ASCII);
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
                //if (name == "LOOT" || name == "BG_SYSTEM_ALLIANCE")
                //    gr.BaseStream.Position--;
            } while (!name.Equals("CHAT_MSG_ADDON"));

            tr.Close();
            gr.Close();
            fs.Close();

            names.Reverse();

            StreamWriter sw = new StreamWriter("ChatMsg.h");

            sw.WriteLine("enum ChatMsg");
            sw.WriteLine("{");

            int c = -1;
            foreach (string str in names)
            {
                string temp = str;
                int spaces = 45 - temp.Length;

                for (int i = 0; i < spaces; i++)
                    temp += " ";

                sw.WriteLine("    {0} = 0x{1},", temp, c.ToString("X2"));
                c++;
            }

            names.Clear();

            sw.WriteLine("};");

            sw.Flush();
            sw.Close();
        }

        static int FindStartPos()
        {
            int first = stream_string.IndexOf("CHAT_MSG_BATTLEGROUND_LEADER");
            return first;
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
