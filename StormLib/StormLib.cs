using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace StormLib
{
    // Flags for SFileOpenArchive
    [Flags]
    public enum OpenArchiveFlags : uint
    {
        NO_LISTFILE         = 0x0010, // Don't load the internal listfile
        NO_ATTRIBUTES       = 0x0020, // Don't open the attributes
        MFORCE_MPQ_V1       = 0x0040, // Always open the archive as MPQ v 1.00, ignore the "wFormatVersion" variable in the header
        MCHECK_SECTOR_CRC   = 0x0080, // On files with MPQ_FILE_SECTOR_CRC, the CRC will be checked when reading file
        READ_ONLY           = 0x0100, // Open the archive for read-only access
        ENCRYPTED           = 0x0200, // Opens an encrypted MPQ archive (Example: Starcraft II installation)
    };

    // Values for SFileExtractFile
    public enum OpenFileFlags : uint
    {
        FROM_MPQ        = 0x00000000,  // Open the file from the MPQ archive
        PATCHED_FILE    = 0x00000001,  // Open the file from the MPQ archive
        BY_INDEX        = 0x00000002,  // The 'szFileName' parameter is actually the file index
        ANY_LOCALE      = 0xFFFFFFFE,  // Reserved for StormLib internal use
        LOCAL_FILE      = 0xFFFFFFFF,  // Open the file from the MPQ archive
    };

    public class StormLib
    {
        [DllImport("StormLib.dll")]
        public static extern bool SFileOpenArchive([MarshalAs(UnmanagedType.LPStr)] string szMpqName,
            uint dwPriority, [MarshalAs(UnmanagedType.U4)] OpenArchiveFlags dwFlags, IntPtr phMpq);

        [DllImport("StormLib.dll")]
        public static extern bool SFileCloseArchive(IntPtr hMpq);

        [DllImport("StormLib.dll")]
        public static extern bool SFileExtractFile(IntPtr hMpq,
            [MarshalAs(UnmanagedType.LPStr)] string szToExtract,
            [MarshalAs(UnmanagedType.LPStr)] string szExtracted, [MarshalAs(UnmanagedType.U4)] OpenFileFlags dwSearchScope);

        [DllImport("StormLib.dll")]
        public static extern bool SFileHasFile(IntPtr hMpq, [MarshalAs(UnmanagedType.LPStr)] string szFileName);

        [DllImport("StormLib.dll")]
        public static extern bool SFileOpenPatchArchive(IntPtr hMpq, [MarshalAs(UnmanagedType.LPStr)] string szMpqName,
            [MarshalAs(UnmanagedType.LPStr)] string szPatchPathPrefix, uint dwFlags);
    }

    public class MpqArchiveSet : IDisposable
    {
        private List<MpqArchive> archives = new List<MpqArchive>();
        private string GameDir = ".\\";

        public void SetGameDir(string dir)
        {
            GameDir = dir;
        }

        public static string GetGameDirFromReg()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Blizzard Entertainment\\World of Warcraft");
            if (key == null)
                return null;
            Object val = key.GetValue("InstallPath");
            if (val == null)
                return null;
            return val.ToString();
        }

        public bool AddArchive(string file)
        {
            MpqArchive a = new MpqArchive(GameDir + file, 0, OpenArchiveFlags.READ_ONLY);
            if (a.IsOpen)
            {
                archives.Add(a);
                Console.WriteLine("Add archive: {0}", file);
                return true;
            }
            return false;
        }

        public int AddArchives(string[] files)
        {
            int n = 0;
            foreach (string s in files)
                if (AddArchive(s))
                    n++;
            return n;
        }

        public bool HasFile(string name)
        {
            foreach (MpqArchive a in archives)
                if (a.HasFile(name))
                    return true;
            return false;
        }

        public bool ExtractFile(string from, string to, OpenFileFlags dwSearchScope)
        {
            foreach (MpqArchive a in archives)
                if (a.HasFile(from))
                    return a.ExtractFile(from, to, dwSearchScope);
            return false;
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            foreach (MpqArchive a in archives)
                a.Close();
            archives.Clear();
        }
    }

    public class MpqLocale
    {
        public static readonly string[] Locales = new string[] {
            "enUS", "koKR", "frFR", "deDE", "zhTW", "esES", "esMX", "ruRU", "enGB", "enTW" };

        public static string GetPrefix(string file)
        {
            foreach (var loc in Locales)
                if (file.Contains(loc))
                    return loc;

            return "base";
        }
    }

    public class MpqArchive : IDisposable
    {
        private IntPtr handle = IntPtr.Zero;

        public MpqArchive(string file, uint Prio, OpenArchiveFlags Flags)
        {
            bool r = Open(file, Prio, Flags);
        }

        public bool IsOpen { get { return handle != IntPtr.Zero; } }

        private unsafe bool Open(string file, uint Prio, OpenArchiveFlags Flags)
        {
            IntPtr h;
            IntPtr hp = (IntPtr)(&h);
            bool r = StormLib.SFileOpenArchive(file, Prio, Flags, hp);
            if (r)
            {
                handle = h;
                OpenPatch(file);
            }
            return r;
        }

        private void OpenPatch(string file)
        {
            var patches = Directory.GetFiles(MpqArchiveSet.GetGameDirFromReg(), "Data\\wow-update-*.mpq");

            var prefix = MpqLocale.GetPrefix(file);

            if (prefix != "base")
            {
                var patches2 = Directory.GetFiles(MpqArchiveSet.GetGameDirFromReg(), String.Format("Data\\{0}\\wow-update-*.mpq", prefix));
                patches = new string[0].Concat(patches).Concat(patches2).ToArray();
            }

            foreach (var patch in patches)
            {
                bool r = StormLib.SFileOpenPatchArchive(handle, patch, prefix, 0);
            }
        }

        public void Dispose()
        {
            Close();
        }

        public bool Close()
        {
            bool r = StormLib.SFileCloseArchive(handle);
            if (r)
                handle = IntPtr.Zero;
            return r;
        }

        public bool HasFile(string name)
        {
            return StormLib.SFileHasFile(handle, name);
        }

        public bool ExtractFile(string from, string to, OpenFileFlags dwSearchScope)
        {
            return StormLib.SFileExtractFile(handle, from, to, dwSearchScope);
        }
    }
}
