using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

using System.Data.SQLite;

namespace WoWPacketViewer
{
    public enum Direction
    {
        CLIENT = 0x00,  // from client
        SERVER = 0x01   // from server
    };

    public struct Packet
    {
        Direction m_direction;
        OpCodes m_opcode;
        byte[] m_data;

        public Direction Direction
        {
            get { return m_direction; }
            set { m_direction = value; }
        }

        public OpCodes Opcode
        {
            get { return m_opcode; }
            set { m_opcode = value; }
        }

        public byte[] Data
        {
            get { return m_data; }
            set { m_data = value; }
        }

        public Packet(Direction direction, OpCodes opcode, byte[] data)
        {
            m_direction = direction;
            m_opcode = opcode;
            m_data = data;
        }

        public string HexLike()
        {
            int length = m_data.Length;

            string dir = string.Empty;

            if (m_direction == Direction.CLIENT)
                dir = "C->S";
            else
                dir = "S->C";

            StringBuilder result = new StringBuilder();

            result.AppendLine(string.Format("Packet {0}, {1} ({2}), len {3}", dir, m_opcode, (ushort)m_opcode, length));

            int byteIndex = 0;

            int full_lines = length >> 4;
            int rest = length & 0x0F;

            int offset = 0;

            if ((full_lines == 0) && (rest == 0)) // empty packet (no data provided)
            {
                result.AppendLine("0000: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- | ................");
            }
            else
            {
                for (int i = 0; i < full_lines; ++i, byteIndex += 0x10)
                {
                    StringBuilder bytes = new StringBuilder();
                    StringBuilder chars = new StringBuilder();

                    for (int j = 0; j < 0x10; ++j)
                    {
                        int c = m_data[offset];
                        offset++;

                        bytes.Append(c.ToString("X2"));

                        bytes.Append(' ');

                        if (c >= 0x20 && c < 0x80)
                            chars.Append((char)c);
                        else
                            chars.Append('.');
                    }

                    result.AppendLine(byteIndex.ToString("X4") + ": " + bytes.ToString() + "| " + chars.ToString());
                }

                if (rest != 0)
                {
                    StringBuilder bytes = new StringBuilder();
                    StringBuilder chars = new StringBuilder(rest);

                    for (int j = 0; j < 0x10; ++j)
                    {
                        if (j < rest)
                        {
                            int c = m_data[offset];
                            offset++;

                            bytes.Append(c.ToString("X2"));

                            bytes.Append(' ');

                            if (c >= 0x20 && c < 0x80)
                                chars.Append((char)c);
                            else
                                chars.Append('.');
                        }
                        else
                        {
                            bytes.Append("-- ");
                            chars.Append('.');
                        }
                    }
                    result.AppendLine(byteIndex.ToString("X4") + ": " + bytes.ToString() + "| " + chars.ToString());
                }
            }

            result.AppendLine();

            return result.ToString();
        }
    };

    public partial class Form1 : Form
    {
        PacketViewer m_packetViewer;

        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Clear();
                textBox2.Clear();
                listView1.Items.Clear();

                toolStripStatusLabel1.Text = "Loading...";
                m_packetViewer = new PacketViewer();
                m_packetViewer.LoadData(openFileDialog1.FileName);
                toolStripStatusLabel1.Text = openFileDialog1.FileName;
                listView1.Items.AddRange(m_packetViewer.ListPackets());
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();

            ListView.SelectedIndexCollection sic = listView1.SelectedIndices;
            if (sic.Count == 0)
                return;
            m_packetViewer.ShowHex(textBox1, sic[0]);
            m_packetViewer.ShowParsed(textBox2, sic[0]);
        }
    }

    public class PacketViewer
    {
        List<Packet> m_packets = new List<Packet>();

        public int LoadData(string file)
        {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + file);
            connection.Open();

            SQLiteCommand command = new SQLiteCommand(connection);
            command.CommandText = "SELECT COUNT(*) FROM packets;";
            command.Prepare();

            int rows = 0;
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                rows = reader.GetInt32(0);
                reader.Close();
            }
            else
            {
                reader.Close();
                connection.Close();
                return 0;
            }

            command.CommandText = "SELECT direction, opcode, data FROM packets ORDER BY id;";
            command.Prepare();

            reader = command.ExecuteReader();

            while (reader.Read())
            {
                //worker.ReportProgress((int)((float)m_packets.Count / (float)rows * 100.0f));

                Direction direction = (Direction)reader.GetByte(0);
                OpCodes opcode = (OpCodes)reader.GetInt16(1);
                byte[] data = (byte[])reader.GetValue(2);

                m_packets.Add(new Packet(direction, opcode, data));
            }

            reader.Close();
            connection.Close();
            return m_packets.Count;
        }

        public ListViewItem[] ListPackets()
        {
            IEnumerable<Packet> pkts = from packet in m_packets where packet.Opcode == OpCodes.CMSG_AUTH_SESSION select packet;

            Packet pkt;
            try
            {
                pkt = pkts.First();
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
                return null;
            }

            uint build;
            try
            {
                build = BitConverter.ToUInt32(pkt.Data, 0);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                return null;
            }

            ListViewItem[] items = new ListViewItem[m_packets.Count];
            int i = 0;
            foreach (Packet p in m_packets)
            {
                ListViewItem item;
                if (p.Direction == Direction.CLIENT)
                    item = new ListViewItem(new string[] { build.ToString(), p.Opcode.ToString(), "", p.Data.Length.ToString() });
                else
                    item = new ListViewItem(new string[] { build.ToString(), "", p.Opcode.ToString(), p.Data.Length.ToString() });
                items[i] = item;
                i++;
            }

            return items;
        }

        public void ShowHex(TextBox tb, int index)
        {
            Packet pkt = m_packets[index];

            tb.Text = pkt.HexLike();
        }

        public void ShowParsed(TextBox tb, int index)
        {

        }
    }
}
