using System.Runtime.InteropServices;

namespace Blizzard
{
    struct PTCH
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] m_magic;
        public uint m_patchSize;
        public uint m_sizeBefore;
        public uint m_sizeAfter;
    }

    struct MD5_
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] m_magic;
        public uint m_md5BlockSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] m_md5Before;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] m_md5After;
    }

    struct XFRM
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] m_magic;
        public uint m_xfrmBlockSize;
    }

    struct BSDIFF40
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] m_magic;
        public ulong m_ctrlBlockSize;
        public ulong m_diffBlockSize;
        public ulong m_sizeAfter;
    }
}
