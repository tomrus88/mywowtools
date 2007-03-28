using System;
using System.IO;

namespace a9_parser
{
    /// <summary>
    ///  Represents a coordinates of WoW object without orientation.
    /// </summary>
    struct Coords3
    {
        public float X, Y, Z;

        /// <summary>
        ///  Converts the numeric values of this instance to its equivalent string representations, separator is space.
        /// </summary>
        public string GetCoords()
        {
            string coords = String.Empty;

            coords += X.ToString().Replace(",", ".");
            coords += " ";
            coords += Y.ToString().Replace(",", ".");
            coords += " ";
            coords += Z.ToString().Replace(",", ".");

            return coords;
        }
    }

    /// <summary>
    ///  Represents a coordinates of WoW object with specified orientation.
    /// </summary>
    struct Coords4
    {
        public float X, Y, Z, O;

        /// <summary>
        ///  Converts the numeric values of this instance to its equivalent string representations, separator is space.
        /// </summary>
        public string GetCoords()
        {
            string coords = String.Empty;

            coords += X.ToString().Replace(",", ".");
            coords += " ";
            coords += Y.ToString().Replace(",", ".");
            coords += " ";
            coords += Z.ToString().Replace(",", ".");
            coords += " ";
            coords += O.ToString().Replace(",", ".");

            return coords;
        }
    }

    /// <summary>
    ///  Reads WoW specific data types as binary values in a specific encoding.
    /// </summary>
    class GenericReader : BinaryReader
    {
        /// <summary>
        ///  Not yet.
        /// </summary>
        public GenericReader(Stream input)
            : base(input)
        {
        }

        /// <summary>
        ///  Not yet.
        /// </summary>
        public GenericReader(string fname)
            : base(new FileStream(fname, FileMode.Open, FileAccess.Read))
        {
        }

        /// <summary>
        ///  Reads the packed guid from the current stream and advances the current position of the stream by packed guid size.
        /// </summary>
        public ulong ReadPackedGuid()
        {
            ulong res = 0;
            byte mask = ReadByte();

            if (mask == 0)
                return res;

            int i = 0;

            while (i < 9)
            {
                if ((mask & 1 << i) != 0)
                    res += (ulong)ReadByte() << (i * 8);
                i++;
            }
            return res;
        }

        /// <summary>
        ///  Reads the string with known length from the current stream and advances the current position of the stream by string length.
        /// </summary>
        public string ReadStringNumber() // read string with known length
        {
            string text = String.Empty;
            int num = ReadInt32(); // string length

            for (int i = 0; i < num; i++)
            {
                text += (char)ReadByte();
            }
            return text;
        }

        /// <summary>
        ///  Reads the NULL terminated string from the current stream and advances the current position of the stream by string length + 1.
        /// </summary>
        public string ReadStringNull() // read NULL terminated string
        {
            byte num;
            string text = String.Empty;

            while ((num = ReadByte()) != 0)
            {
                text += (char)num;
            }
            return text;
        }

        /// <summary>
        ///  Reads the object coordinates from the current stream and advances the current position of the stream by 12 bytes.
        /// </summary>
        public Coords3 ReadCoords3()
        {
            Coords3 v;

            v.X = ReadSingle();
            v.Y = ReadSingle();
            v.Z = ReadSingle();

            return v;
        }

        /// <summary>
        ///  Reads the object coordinates and orientation from the current stream and advances the current position of the stream by 16 bytes.
        /// </summary>
        public Coords4 ReadCoords4()
        {
            Coords4 v;

            v.X = ReadSingle();
            v.Y = ReadSingle();
            v.Z = ReadSingle();
            v.O = ReadSingle();

            return v;
        }
    }
}
