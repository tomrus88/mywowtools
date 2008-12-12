using System;
using System.Collections.Generic;
using System.Globalization;

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
    }
}
