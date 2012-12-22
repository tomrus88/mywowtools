using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Blizzard.BatchPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            //using (Patch p = new Patch(args[1]))
            //{
            //    p.PrintHeaders();
            //    p.Apply(args[0], args[2], true);
            //}

            string[] oldfiles = Directory.GetFiles("old", "*.stl");

            string[] ptchs = Directory.GetFiles("ptch", "*.stl");

            foreach (string of in oldfiles)
            {
                if (ptchs.Any(s => Path.GetFileName(s) == Path.GetFileName(of)))
                {
                    Console.WriteLine("has patch for {0}", of);

                    string patch = Path.Combine("ptch", Path.GetFileName(of));

                    if (File.ReadAllBytes(patch).Length == 0) // removed
                        continue;

                    using (Patch p = new Patch(patch))
                    {
                        //p.PrintHeaders();
                        p.Apply(of, Path.Combine("new", Path.GetFileName(of)), true);
                    }
                }
                else
                {
                    File.Copy(of, Path.Combine("new", Path.GetFileName(of)));
                }
            }

            foreach (string pf in ptchs)
            {
                if (File.ReadAllBytes(pf).Length == 0) // removed
                    continue;

                
                string newfile = Path.Combine("new", Path.GetFileName(pf));

                try
                {
                    using (Patch p = new Patch(pf))
                    {
                        //p.PrintHeaders();
                        if (p.PatchType != "COPY")
                            continue;

                        Console.WriteLine("new file {0}", newfile);

                        p.Apply("oldfile", newfile, true);
                    }
                }
                catch (InvalidDataException exc)
                {
                    File.Copy(pf, newfile);
                }
            }
        }
    }
}
