using System;
using System.Collections.Generic;
using System.Globalization;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace WoWPacketViewer
{
    public class Utility
    {
        public static byte[] HexStringToBinary(string data)
        {
            var bytes = new List<byte>();
            for (var i = 0; i < data.Length; i += 2)
            {
                bytes.Add(Byte.Parse(data.Substring(i, 2), NumberStyles.HexNumber));
            }
            return bytes.ToArray();
        }

        public static string ByteArrayToHexString(byte[] data)
        {
            var str = String.Empty;
            for (var i = 0; i < data.Length; ++i)
                str += data[i].ToString("X2", CultureInfo.InvariantCulture);
            return str;
        }

        public static string PrintHex(byte[] data, int start, int size)
        {
            var result = String.Empty;
            var counter = start;

            while (counter != size)
            {
                for (var i = 0; i < 0x10; ++i)
                {
                    result += String.Format("{0:X2} ", data[counter]);
                    counter++;

                    if (counter == size)
                    {
                        result += "\r\n";
                        break;
                    }
                }
                result += "\r\n";
            }
            return result;
        }

        public static byte[] Decompress(byte[] data)
        {
            var uncompressedLength = BitConverter.ToUInt32(data, 0);
            var output = new byte[uncompressedLength];

            var item = new Inflater();
            item.SetInput(data, 4, data.Length - 4);
            item.Inflate(output, 0, output.Length);

            return output;
        }
    }
}
