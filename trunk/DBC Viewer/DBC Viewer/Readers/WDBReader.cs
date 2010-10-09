using System;
using System.IO;
using System.Text;

namespace dbc2sql
{
    class DB2Reader : IWowClientDBReader
    {
        private const int HeaderSize = 48;
        private const uint WIDBFmtSig = 0x57494442;          // WIDB

        public int RecordsCount { get; private set; }
        public int FieldsCount { get; private set; }
        public int RecordSize { get; private set; }
        public int StringTableSize { get; private set; }

        public StringTable StringTable { get; private set; }

        private byte[][] m_rows;

        public byte[] GetRowAsByteArray(int row)
        {
            return m_rows[row];
        }

        public BinaryReader this[int row]
        {
            get { return new BinaryReader(new MemoryStream(m_rows[row]), Encoding.UTF8); }
        }

        public DB2Reader(string fileName)
        {
            using (var reader = BinaryReaderExtensions.FromFile(fileName))
            {
                if (reader.BaseStream.Length < HeaderSize)
                {
                    Console.WriteLine("File {0} is corrupted!", fileName);
                    return;
                }

                var signature = reader.ReadUInt32();

                if (signature != WIDBFmtSig)
                {
                    Console.WriteLine("File {0} isn't valid DBC file!", fileName);
                    return;
                }

                uint build = reader.ReadUInt32();
                uint locale = reader.ReadUInt32();
                var unk1 = reader.ReadInt32();
                var unk2 = reader.ReadInt32();
                var version = reader.ReadInt32();

                m_rows = new byte[RecordsCount][];

                for (int i = 0; i < RecordsCount; i++)
                    m_rows[i] = reader.ReadBytes(RecordSize);

                int stringTableStart = (int)reader.BaseStream.Position;

                StringTable = new StringTable();

                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    int index = (int)reader.BaseStream.Position - stringTableStart;
                    StringTable[index] = reader.ReadStringNull();
                }
            }
        }
    }
}
