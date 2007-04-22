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
            // you must copy packet data to data.logfile
            // output file named data.dump
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

            string ifn = "data.log";
            string ofn = "data.dump";
            BinaryWriter bw = new BinaryWriter(new FileStream(ofn, FileMode.Create));
            StreamReader sr = new StreamReader(ifn);

            while(sr.Peek() >= 0)
            {
                string temp1 = sr.ReadLine();
                if(temp1.StartsWith("Packet SMSG.COMPRESSED_UPDATE_OBJECT (502)"))
                {
                    // read additional line
                    string[] temp = sr.ReadLine().Split('=');
                    uint size = Convert.ToUInt32(temp[1].Substring(0, temp[1].Length - 1));
                    bw.Write(size);
                }
                else if (temp1.StartsWith("Packet SMSG"))
                {
                    string[] temp = temp1.Split(' ');
                    uint size = Convert.ToUInt32(temp[4]);
                    bw.Write(size);
                }
                else
                {
                    string[] test3 = temp1.Split(' ');
                    for (int j = 1; j < test3.Length - 2; j++) // skip not needed stuff
                    {
                        if (j < 1 || j > 0x10)
                            continue;

                        if (test3[j].Contains("-") == false || !test3[j].Contains(":") == false) // skip bytes at end of packet
                        {
                            bw.Write(Convert.ToByte(test3[j], 16));
                        }
                    }
                }
            }
            bw.Flush();
            bw.Close();
            MessageBox.Show("Done!");
        }
    }
}
