using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace WoWPacketViewer
{
    /// <summary> Represents a coordinates
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Coords3
    {
        public float X;
        public float Y;
        public float Z;

        /// <summary>  Converts the numeric values of this instance to 
        /// its equivalent string representations, separator is space.
        /// </summary>
        [Obsolete("Please use ToString() instead")]
        public string GetCoords()
        {
            return ToString();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0} {1} {2}", X, Y, Z);
        }
    }
}
