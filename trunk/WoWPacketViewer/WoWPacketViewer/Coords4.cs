using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace WoWPacketViewer
{
    /// <summary> Represents a coordinates
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Coords4
    {
        public float X, Y, Z, O;

        /// <summary> Converts the numeric values of this instance to 
        /// its equivalent string representations, separator is space.
        /// </summary>
        [Obsolete("Please use ToString() instead")]
        public string GetCoordsAsString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0} {1} {2} {3}", X, Y, Z, O);
        }
    }
}
