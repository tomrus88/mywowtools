using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace tcp_log_parser
{
    public partial class Form1 : Form
    {
        StreamReader sr;

        StreamWriter sw;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                sw = new StreamWriter("data.log");
                sw.AutoFlush = true;
                sr = new StreamReader("tcp.log");

                string line = String.Empty;
                string filter = String.Empty;
                bool active = false;
                while ((line = sr.ReadLine()) != null)
                {
                    if (cfg.Checked)
                    {
                        StreamReader cr = new StreamReader("filters.txt");
                        while ((filter = cr.ReadLine()) != null)
                        {
                            //MessageBox.Show(filter + "\n" + line);
                            if (line.StartsWith(filter))
                            {
                                active = false;
                                break;
                                //MessageBox.Show(filter + "\n" + line);
                            }
                            else if (line.StartsWith("Packet SMSG."))
                            {
                                active = true;
                                //break;
                            }
                        }
                        cr.Close();
                    }
                    else
                    {
                        if (line.StartsWith("Packet SMSG.(null) (742)"))
                        {
                            active = !wd.Checked;
                            //continue;
                        }
                        else if (line.StartsWith("Packet SMSG.MONSTER_MOVE (221)"))
                        {
                            active = !mm.Checked;
                            //continue;
                        }
                        else if (line.StartsWith("Packet SMSG.PONG (477)"))
                        {
                            active = !ping.Checked;
                            //continue;
                        }
                        else if (line.StartsWith("Packet SMSG.ITEM_QUERY_SINGLE_RESPONSE (88)"))
                        {
                            active = !q.Checked;
                            //continue;
                        }
                        else if (line.StartsWith("Packet SMSG.QUEST_QUERY_RESPONSE (93)"))
                        {
                            active = !q.Checked;
                            //continue;
                        }
                        else if (line.StartsWith("Packet SMSG.GAMEOBJECT_QUERY_RESPONSE (95)"))
                        {
                            active = !q.Checked;
                            //continue;
                        }
                        else if (line.StartsWith("Packet SMSG.ATTACKERSTATEUPDATE (330)"))
                        {
                            active = false;
                            //continue;
                        }
                        else if (line.StartsWith("Packet SMSG.COMPRESSED_UPDATE_OBJECT (502)") || line.StartsWith("Packet SMSG.UPDATE_OBJECT (169)"))
                        {
                            active = !up.Checked;
                            //continue;
                        }
                        else if (line.StartsWith("Packet SMSG."))
                        {
                            active = true;
                        }
                    }
                    if (active)
                    {
                        sw.WriteLine(line);
                    }
                }
                sw.Flush();
                sw.Close();
                sr.Close();
                MessageBox.Show("Done!");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
        }
    }
}