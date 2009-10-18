using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace WoWPacketViewer.Parsers
{
    enum CheckType
    {
        MEM_CHECK = 0,
        PAGE_CHECK = 1,
        MPQ_CHECK = 2,
        STR_CHECK = 3,
        UNK2_CHECK = 4,
        UNK_CHECK = 5,
        UNK3_CHECK = 6,
    }

    struct CheckInfo
    {
        public CheckType m_type;
        public int m_length;

        public CheckInfo(CheckType type, int len)
        {
            m_type = type;
            m_length = len; // for MEM_CHECK result
        }
    }

    [Parser(OpCodes.CMSG_WARDEN_DATA)]
    [Parser(OpCodes.SMSG_WARDEN_DATA)]
    internal class MSG_WARDEN_DATA : Parser
    {
        static List<CheckInfo> s_lastCheckInfo = new List<CheckInfo>();
        static Dictionary<byte, CheckType> s_checkTypes = new Dictionary<byte, CheckType>();
        static bool s_checkTypesInitialized;

        public MSG_WARDEN_DATA(Packet packet)
            : base(packet)
        {
            InitCheckTypes();
        }

        private void InitCheckTypes()
        {
            if (s_checkTypesInitialized)
                return;

            // MEM_CHECK
            s_checkTypes.Add(0x7C, CheckType.MEM_CHECK);
            s_checkTypes.Add(0x3D, CheckType.MEM_CHECK);
            s_checkTypes.Add(0xA5, CheckType.MEM_CHECK);
            s_checkTypes.Add(0xE4, CheckType.MEM_CHECK);
            s_checkTypes.Add(0x87, CheckType.MEM_CHECK);

            // PAGE_CHECK A/B
            s_checkTypes.Add(0x18, CheckType.PAGE_CHECK);
            s_checkTypes.Add(0x01, CheckType.PAGE_CHECK);
            s_checkTypes.Add(0x74, CheckType.PAGE_CHECK);
            s_checkTypes.Add(0x99, CheckType.PAGE_CHECK);
            s_checkTypes.Add(0xEC, CheckType.PAGE_CHECK);
            s_checkTypes.Add(0x78, CheckType.PAGE_CHECK);
            s_checkTypes.Add(0xDD, CheckType.PAGE_CHECK);
            s_checkTypes.Add(0xE3, CheckType.PAGE_CHECK);
            s_checkTypes.Add(0xE8, CheckType.PAGE_CHECK);

            // MPQ_CHECK
            s_checkTypes.Add(0xB3, CheckType.MPQ_CHECK);
            s_checkTypes.Add(0x6E, CheckType.MPQ_CHECK);
            s_checkTypes.Add(0xF6, CheckType.MPQ_CHECK);
            s_checkTypes.Add(0xA7, CheckType.MPQ_CHECK);
            s_checkTypes.Add(0xA6, CheckType.MPQ_CHECK);

            // STR_CHECK (LUA STRING CHECK)
            s_checkTypes.Add(0x34, CheckType.STR_CHECK);
            s_checkTypes.Add(0x55, CheckType.STR_CHECK);
            s_checkTypes.Add(0xCD, CheckType.STR_CHECK);
            s_checkTypes.Add(0x0C, CheckType.STR_CHECK);
            s_checkTypes.Add(0x4F, CheckType.STR_CHECK);

            // UNK2_CHECK (DRIVER CHECK)
            //s_checkTypes.Add(0xA6, CheckType.UNK2_CHECK);
            //s_checkTypes.Add(0x4F, CheckType.UNK2_CHECK);
            s_checkTypes.Add(0xD7, CheckType.UNK2_CHECK);
            s_checkTypes.Add(0xD6, CheckType.UNK2_CHECK);
            s_checkTypes.Add(0xFD, CheckType.UNK2_CHECK);

            // UNK_CHECK (may be tick count check)
            s_checkTypes.Add(0xD0, CheckType.UNK_CHECK);
            s_checkTypes.Add(0xB9, CheckType.UNK_CHECK);
            s_checkTypes.Add(0x21, CheckType.UNK_CHECK);
            s_checkTypes.Add(0xA0, CheckType.UNK_CHECK);
            s_checkTypes.Add(0xAB, CheckType.UNK_CHECK);

            // UNK_CHECK3 (module+proc)
            s_checkTypes.Add(0x54, CheckType.UNK3_CHECK);

            s_checkTypesInitialized = true;
        }

        public override string Parse()
        {
            var gr = Packet.CreateReader();

            switch (Packet.Code)
            {
                #region CMSG_WARDEN_DATA
                case OpCodes.CMSG_WARDEN_DATA:
                    {
                        var wardenOpcode = gr.ReadByte();
                        AppendFormatLine("C->S Warden Opcode: {0:X2}", wardenOpcode);

                        switch (wardenOpcode)
                        {
                            case 0x00:
                                {
                                    AppendFormatLine("Load module failed or module is missing...");
                                    AppendLine();
                                }
                                break;
                            case 0x01:
                                {
                                    AppendFormatLine("Module loaded successfully...");
                                    AppendLine();
                                }
                                break;
                            case 0x02:
                                {
                                    var bufLen = gr.ReadUInt16();
                                    var checkSum = gr.ReadUInt32();
                                    var result = gr.ReadBytes(bufLen);
                                    ValidateCheckSum(checkSum, result);
                                    AppendFormatLine("Cheat check result:");
                                    AppendFormatLine("Len: {0}", bufLen);
                                    AppendFormatLine("Checksum: 0x{0:X8}", checkSum);
                                    var reader = new BinaryReader(new MemoryStream(result), Encoding.ASCII);
                                    AppendFormatLine("====== CHEAT CHECKS RESULTS START ======");
                                    AppendLine();
                                    foreach (var check in s_lastCheckInfo)
                                    {
                                        switch (check.m_type)
                                        {
                                            case CheckType.MEM_CHECK:
                                                {
                                                    var res = reader.ReadByte();
                                                    AppendFormatLine("====== MEM_CHECK result START ======");
                                                    AppendFormatLine("MEM_CHECK result: 0x{0:X2}", res);
                                                    if (res == 0)
                                                    {
                                                        var bytes = reader.ReadBytes(check.m_length);
                                                        AppendFormatLine("MEM_CHECK bytes: 0x{0}", Utility.ByteArrayToHexString(bytes));
                                                    }
                                                    AppendFormatLine("====== MEM_CHECK result END ======");
                                                    AppendLine();
                                                }
                                                break;
                                            case CheckType.PAGE_CHECK:
                                                {
                                                    var res = reader.ReadByte();
                                                    AppendFormatLine("====== PAGE_CHECK result START ======");
                                                    AppendFormatLine("PAGE_CHECK result: 0x{0:X2}", res);
                                                    AppendFormatLine("====== PAGE_CHECK result END ======");
                                                    AppendLine();
                                                }
                                                break;
                                            case CheckType.MPQ_CHECK:
                                                {
                                                    var res = reader.ReadByte();
                                                    var sha1 = reader.ReadBytes(20);
                                                    AppendFormatLine("====== MPQ_CHECK result START ======");
                                                    AppendFormatLine("MPQ_CHECK result: 0x{0:X2}", res);
                                                    AppendFormatLine("SHA1: 0x{0}", Utility.ByteArrayToHexString(sha1));
                                                    AppendFormatLine("====== MPQ_CHECK result END ======");
                                                    AppendLine();
                                                }
                                                break;
                                            case CheckType.STR_CHECK:
                                                {
                                                    var res = reader.ReadBytes(2);
                                                    AppendFormatLine("====== STR_CHECK result START ======");
                                                    AppendFormatLine("STR_CHECK result: 0x{0}", Utility.ByteArrayToHexString(res));
                                                    AppendFormatLine("====== STR_CHECK result END ======");
                                                    AppendLine();
                                                }
                                                break;
                                            case CheckType.UNK2_CHECK:
                                                {
                                                    var res = reader.ReadByte();
                                                    AppendFormatLine("====== UNK2_CHECK result START ======");
                                                    AppendFormatLine("UNK2_CHECK result: 0x{0:X2}", res);
                                                    AppendFormatLine("====== UNK2_CHECK result END ======");
                                                    AppendLine();
                                                }
                                                break;
                                            case CheckType.UNK_CHECK:
                                                {
                                                    var res = reader.ReadByte();
                                                    var unk = reader.ReadInt32();
                                                    AppendFormatLine("====== UNK_CHECK result START ======");
                                                    AppendFormatLine("UNK_CHECK result: 0x{0:X2}", res);
                                                    AppendFormatLine("UNK_CHECK value: 0x{0:X8}", unk);
                                                    AppendFormatLine("====== UNK_CHECK result END ======");
                                                    AppendLine();
                                                }
                                                break;
                                            case CheckType.UNK3_CHECK:
                                                {
                                                    var res = reader.ReadByte();
                                                    AppendFormatLine("====== UNK3_CHECK result START ======");
                                                    AppendFormatLine("UNK3_CHECK result: 0x{0:X2}", res);
                                                    AppendFormatLine("====== UNK3_CHECK result END ======");
                                                    AppendLine();
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    AppendFormatLine("====== CHEAT CHECKS RESULTS END ======");

                                    s_lastCheckInfo.Clear();

                                    if (reader.BaseStream.Position != reader.BaseStream.Length)
                                        AppendFormatLine("Packet under read!");

                                    AppendLine();
                                }
                                break;
                            case 0x04:
                                {
                                    var hash = gr.ReadBytes(20); // hash
                                    AppendFormatLine("Hash: 0x{0}", Utility.ByteArrayToHexString(hash));
                                    AppendLine();
                                }
                                break;
                            default:
                                {
                                    AppendFormatLine("Unknown warden opcode {0}", wardenOpcode);
                                    AppendLine();
                                }
                                break;
                        }

                        CheckPacket(gr);

                        return GetParsedString();
                    }
                #endregion
                #region SMSG_WARDEN_DATA
                case OpCodes.SMSG_WARDEN_DATA:
                    {
                        var wardenOpcode = gr.ReadByte();
                        AppendFormatLine("S->C Warden Opcode: {0:X2}", wardenOpcode);

                        switch (wardenOpcode)
                        {
                            case 0x00:
                                {
                                    var md5 = gr.ReadBytes(16); // md5
                                    var rc4 = gr.ReadBytes(16); // rc4 key
                                    var len = gr.ReadInt32();   // len
                                    AppendFormatLine("MD5: 0x{0}", Utility.ByteArrayToHexString(md5));
                                    AppendFormatLine("RC4: 0x{0}", Utility.ByteArrayToHexString(rc4));
                                    AppendFormatLine("Len: {0}", len);
                                    AppendLine();
                                }
                                break;
                            case 0x01:
                                {
                                    var len = gr.ReadInt16();
                                    var chunk = gr.ReadBytes(len);
                                    AppendFormatLine("Received warden module chunk, len {0}", len);
                                    AppendLine();
                                }
                                break;
                            case 0x02:
                                {
                                    AppendFormatLine("====== CHEAT CHECKS START ======");
                                    AppendLine();

                                    List<string> strings = new List<string>();
                                    strings.Add("");
                                    byte len;
                                    while ((len = gr.ReadByte()) != 0)
                                    {
                                        var strBytes = gr.ReadBytes(len);
                                        var str = Encoding.ASCII.GetString(strBytes);
                                        strings.Add(str);
                                    }
                                    var rest = gr.BaseStream.Length - gr.BaseStream.Position;
                                    var checks = gr.ReadBytes((int)rest);
                                    var reader = new BinaryReader(new MemoryStream(checks), Encoding.ASCII);
                                    //var index = 0;
                                    while (reader.BaseStream.Position + 1 != reader.BaseStream.Length)
                                    {
                                        var Xor = checks[checks.Length - 1];

                                        var check = reader.ReadByte();
                                        var checkType = check ^ Xor;
                                        //index++;

                                        //if (IsUnkCheck(strings.Count, index, checks, strings))
                                        if(s_checkTypes[check] == CheckType.UNK_CHECK)
                                        {
                                            AppendFormatLine("====== UNK_CHECK START ======");
                                            AppendFormatLine("checkType {0:X2} ({1:X2})", checkType, check);
                                            AppendFormatLine("====== UNK_CHECK END ======");
                                            AppendLine();
                                            s_lastCheckInfo.Add(new CheckInfo(CheckType.UNK_CHECK, 0));
                                            continue;
                                        }
                                        //if (IsMemCheck(strings.Count, index, checks)) // memcheck
                                        if (s_checkTypes[check] == CheckType.MEM_CHECK)
                                        {
                                            var strIndex = reader.ReadByte(); // string index
                                            var offset = reader.ReadUInt32(); // offset
                                            var readLen = reader.ReadByte();  // bytes to read
                                            //index += 6;
                                            AppendFormatLine("====== MEM_CHECK START ======");
                                            AppendFormatLine("checkType {0:X2} ({1:X2})", checkType, check);
                                            AppendFormatLine("Module: {0}", (strings[strIndex] == "") ? "base" : strings[strIndex]);
                                            AppendFormatLine("Offset: 0x{0:X8}", offset);
                                            AppendFormatLine("Bytes to read: {0}", readLen);
                                            AppendFormatLine("====== MEM_CHECK END ======");
                                            AppendLine();
                                            s_lastCheckInfo.Add(new CheckInfo(CheckType.MEM_CHECK, readLen));
                                            continue;
                                        }
                                        //if (IsPageCheck(strings.Count, index, checks)) // pagecheck
                                        if (s_checkTypes[check] == CheckType.PAGE_CHECK)
                                        {
                                            var seed = reader.ReadUInt32();
                                            var sha1 = reader.ReadBytes(20);
                                            var addr = reader.ReadUInt32();
                                            var tord = reader.ReadByte(); // bytes to read
                                            //index += 29;
                                            AppendFormatLine("====== PAGE_CHECK START ======");
                                            AppendFormatLine("checkType {0:X2} ({1:X2})", checkType, check);
                                            AppendFormatLine("Seed: 0x{0:X8}", seed);
                                            AppendFormatLine("SHA1: 0x{0}", Utility.ByteArrayToHexString(sha1));
                                            AppendFormatLine("Address: 0x{0:X8}", addr);
                                            AppendFormatLine("Bytes to read: {0}", tord);
                                            AppendFormatLine("====== PAGE_CHECK END ======");
                                            AppendLine();
                                            s_lastCheckInfo.Add(new CheckInfo(CheckType.PAGE_CHECK, 0));
                                            continue;
                                        }
                                        if (s_checkTypes[check] == CheckType.UNK3_CHECK)
                                        {
                                            var seed = reader.ReadUInt32();
                                            var sha1 = reader.ReadBytes(20);
                                            var module = reader.ReadByte();
                                            var proc = reader.ReadByte();
                                            var addr = reader.ReadUInt32();
                                            var tord = reader.ReadByte(); // bytes to read
                                            //index += 29;
                                            AppendFormatLine("====== UNK3_CHECK START ======");
                                            AppendFormatLine("checkType {0:X2} ({1:X2})", checkType, check);
                                            AppendFormatLine("Seed: 0x{0:X8}", seed);
                                            AppendFormatLine("SHA1: 0x{0}", Utility.ByteArrayToHexString(sha1));
                                            AppendFormatLine("Module: {0}", strings[module]);
                                            AppendFormatLine("Proc: {0}", strings[proc]);
                                            AppendFormatLine("Address: 0x{0:X8}", addr);
                                            AppendFormatLine("Bytes to read: {0}", tord);
                                            AppendFormatLine("====== UNK3_CHECK END ======");
                                            AppendLine();
                                            s_lastCheckInfo.Add(new CheckInfo(CheckType.UNK3_CHECK, 0));
                                            continue;
                                        }
                                        //if (IsMpqCheck(strings.Count, index, checks, strings))
                                        if (s_checkTypes[check] == CheckType.MPQ_CHECK)
                                        {
                                            var fileNameIndex = reader.ReadByte();
                                            //index++;
                                            AppendFormatLine("====== MPQ_CHECK START ======");
                                            AppendFormatLine("checkType {0:X2} ({1:X2})", checkType, check);
                                            AppendFormatLine("File: {0}", strings[fileNameIndex]);
                                            AppendFormatLine("====== MPQ_CHECK END ======");
                                            AppendLine();
                                            s_lastCheckInfo.Add(new CheckInfo(CheckType.MPQ_CHECK, 0));
                                            continue;
                                        }
                                        //if (IsStrCheck(strings.Count, index, checks))
                                        if (s_checkTypes[check] == CheckType.STR_CHECK)
                                        {
                                            var stringIndex = reader.ReadByte();
                                            //index++;
                                            AppendFormatLine("====== STRING_CHECK START ======");
                                            AppendFormatLine("checkType {0:X2} ({1:X2})", checkType, check);
                                            AppendFormatLine("String: {0}", strings[stringIndex]);
                                            AppendFormatLine("====== STRING_CHECK END ======");
                                            AppendLine();
                                            s_lastCheckInfo.Add(new CheckInfo(CheckType.STR_CHECK, 0));
                                            continue;
                                        }
                                        //if (IsUnk2Check(strings.Count, index, checks))
                                        if (s_checkTypes[check] == CheckType.UNK2_CHECK)
                                        {
                                            var unk = reader.ReadBytes(24);
                                            var stringIndex = reader.ReadByte();
                                            //index += 25;
                                            AppendFormatLine("====== UNK2_CHECK START ======");
                                            AppendFormatLine("checkType {0:X2} ({1:X2})", checkType, check);
                                            AppendFormatLine("Unk bytes: 0x{0}", Utility.ByteArrayToHexString(unk));
                                            AppendFormatLine("Unk string: {0}", strings[stringIndex]);
                                            AppendFormatLine("====== UNK2_CHECK END ======");
                                            AppendLine();
                                            s_lastCheckInfo.Add(new CheckInfo(CheckType.UNK2_CHECK, 0));
                                            continue;
                                        }
                                    }
                                    AppendFormatLine("====== CHEAT CHECKS END ======");
                                    AppendLine();
                                }
                                break;
                            case 0x03:
                                {
                                    while (gr.BaseStream.Position != gr.BaseStream.Length)
                                    {
                                        var len = gr.ReadInt16();
                                        var checkSum = gr.ReadUInt32();
                                        var data = gr.ReadBytes(len);

                                        AppendFormatLine("Len: {0}", len);
                                        AppendFormatLine("Checksum: 0x{0:X8}", checkSum);
                                        AppendFormatLine("Data: 0x{0}", Utility.ByteArrayToHexString(data));
                                        AppendLine();
                                    }
                                }
                                break;
                            case 0x05:
                                {
                                    var seed = gr.ReadBytes(16); // seed
                                    AppendFormatLine("Seed: 0x{0}", Utility.ByteArrayToHexString(seed));
                                    AppendLine();
                                }
                                break;
                            default:
                                {
                                    AppendFormatLine("Unknown warden opcode {0}", wardenOpcode);
                                }
                                break;
                        }

                        CheckPacket(gr);

                        return GetParsedString();
                    }
                #endregion
                default:
                    return String.Empty;
            }
        }

        private void ValidateCheckSum(uint checkSum, byte[] data)
        {
            var hash = new SHA1CryptoServiceProvider().ComputeHash(data);
            var res = new uint[5];

            for (var i = 0; i < 5; ++i)
                res[i] = BitConverter.ToUInt32(hash, 4 * i);

            var newCheckSum = res[0] ^ res[1] ^ res[2] ^ res[3] ^ res[4];

            if (checkSum != newCheckSum)
                AppendFormatLine("CheckSum is not valid!");
            else
                AppendFormatLine("CheckSum is valid!");
        }

        /*private bool IsMemCheck(int strCount, int index, byte[] data)
        {
            if (index + 5 >= data.Length)
                return false;

            var temp = data[index];
            if (temp >= strCount)
                return false;

            temp = data[index + 4];
            if (temp != 0 && temp != 1)
                return false;

            temp = data[index + 5];

            if (temp == 0)      // bytes to read == 0
                return false;

            if (temp > 0x40)    // too many bytes to read
                return false;

            return true;
        }

        private bool IsPageCheck(int strCount, int index, byte[] data)
        {
            if (index + 28 >= data.Length)
                return false;

            var temp = data[index + 28];

            if (temp == 0)      // bytes to read == 0
                return false;

            if (temp > 0x80)    // too many bytes to read
                return false;

            temp = data[index + 27];
            if (temp != 0 && temp != 1)
                return false;

            temp = data[index + 26];
            if (temp > 0x40)
                return false;

            return true;
        }

        private bool IsMpqCheck(int strCount, int index, byte[] data, List<string> strings)
        {
            if (index >= data.Length)
                return false;

            var temp = data[index];

            if (temp == 0)
                return false;

            if (temp >= strCount)
                return false;

            if (!strings[temp].Contains("\\"))
                return false;

            //if (!IsMemCheck(strCount, index + 2, data) && !IsPageCheck(strCount, index + 2, data))
            //    return false;

            return true;
        }

        private bool IsStrCheck(int strCount, int index, byte[] data)
        {
            if (index >= data.Length)
                return false;

            var temp = data[index];

            if (temp == 0)
                return false;

            if (temp >= strCount)
                return false;

            return true;
        }

        private bool IsUnk2Check(int strCount, int index, byte[] data)
        {
            if (index + 24 >= data.Length)
                return false;

            var temp = data[index + 24];
            if (temp >= strCount)
                return false;

            return true;
        }

        private bool IsUnkCheck(int strCount, int index, byte[] data, List<string> strings)
        {
            if ((!IsMemCheck(strCount, index + 1, data) &&
                !IsPageCheck(strCount, index + 1, data) &&
                !IsMpqCheck(strCount, index + 1, data, strings) &&
                !IsUnk2Check(strCount, index + 1, data) &&
                !IsStrCheck(strCount, index + 1, data)) &&
                (IsMemCheck(strCount, index, data) ||
                IsPageCheck(strCount, index, data) ||
                IsUnk2Check(strCount, index, data) ||
                IsStrCheck(strCount, index, data) ||
                IsMpqCheck(strCount, index, data, strings)))
                return false;

            return true;
        }*/
    }
}
