using System;
using System.Windows.Forms;
using System.IO;

namespace log2bin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // you must copy packet data to in.txt file
            // output file named A9.dump
            // input packet must have following structure (first two lines required):
            // Packet SMSG.COMPRESSED_UPDATE_OBJECT (502), len: 33
            // (decompressed len=197)
            // 0000: a9 00 01 00 00 00 00 00 07 6c 30 40 2d 00 00 40 : .........l0@-..@
            // 0010: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 : ................
            // 0020: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 : ................
            // 0030: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 : ................
            // 0040: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 : ................
            // 0050: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 : ................
            // 0060: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 : ................
            // 0070: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 : ................
            // 0080: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 : ................
            // 0090: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 : ................
            // 00a0: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 : ................
            // 00b0: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 : ................
            // 00c0: 00 5e 00 00 00 -- -- -- -- -- -- -- -- -- -- -- : .^...

            string ifn = "in.txt";
            string ofn = "A9.dump";
            BinaryWriter bw = new BinaryWriter(new FileStream(ofn, FileMode.Create));
            StreamReader sr = new StreamReader(ifn);

            sr.BaseStream.Position = 0;
            sr.ReadLine(); // useless line
            string[] temp = sr.ReadLine().Split('=');
            uint size = Convert.ToUInt32(temp[1].Substring(0, temp[1].Length - 1));
            //bw.Write(size);

            string test = sr.ReadToEnd();
            sr.Close();
            string[] test2 = test.Split('\n');
            for (int k = 0; k < test2.Length; k++)
            {
                string[] test3 = test2[k].Split(' ');
                for (int j = 1; j < test3.Length-2; j++) // skip not needed stuff
                {
                    if (j < 1 || j > 0x10)
                        continue;

                    if (test3[j].Contains("-") == false || !test3[j].Contains(":") == false) // skip bytes at end of packet
                    {
                        bw.Write(Convert.ToByte(test3[j], 16));
                    }
                }
            }
            bw.Flush();
            bw.Close();
        }
    }
}
