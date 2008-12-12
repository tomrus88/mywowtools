using System;

namespace WoWPacketViewer
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ParserAttribute : Attribute
    {
        public ParserAttribute(OpCodes code)
        {
            Code = code;
        }

        public OpCodes Code { get; private set; }
    }
}
