using System;
using System.IO;

namespace WdbParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                if (!Directory.Exists("wdb"))
                {
                    Console.WriteLine("Please specify folder");
                    return;
                }
                WdbParser wdbParser = new WdbParser("wdb");
            }
            else
            {
                if (!Directory.Exists(args[0]))
                {
                    Console.WriteLine("Please specify folder");
                    return;
                }
                WdbParser wdbParser = new WdbParser(args[0]);
            }
        }
    }
}
