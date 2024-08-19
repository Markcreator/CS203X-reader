using System;
using System.Collections.Specialized;
using INT8U = System.Byte;
using INT16U = System.UInt16;
using INT32U = System.UInt32;
using INT16S = System.Int16;
using INT32S = System.Int32;

using CSLibrary.RTLS.Constants;

namespace CSLibrary.RTLS.Structures
{
    class Frame
    {
        private BitVector32 headerBytes;
        protected INT8U[] id = new INT8U[6];
        protected MID msgID;
        protected INT8U length;
        protected INT8U[] timeStampSec = new INT8U[4];
        protected INT8U[] timeStampMisec = new INT8U[4];
        protected Byte[] data = new byte[0];
        protected INT8U checkSum;

        private static readonly BitVector32.Section HEADER_DELIMITER;
        private static readonly BitVector32.Section HEADER_TMS;
        private static readonly BitVector32.Section HEADER_TS;
        private static readonly BitVector32.Section HEADER_RN;
        private static readonly BitVector32.Section HEADER_DIR;

        const int STRING_INDEX_HEADER = 0;
        const int STRING_INDEX_ID = 2;
        const int STRING_INDEX_MID = 14;
        const int STRING_INDEX_LENGTH = 16;
        const int STRING_INDEX_TS = 18;
        const int STRING_INDEX_TSU = 26;
        int STRING_INDEX_DATA = 34;
        int STRING_INDEX_CHECKSUN = 0;

        const int STRING_SIZE_HEADER = 2;
        const int STRING_SIZE_ID = 12;
        const int STRING_SIZE_MID = 2;
        const int STRING_SIZE_LENGTH = 2;
        const int STRING_SIZE_TS = 8;
        const int STRING_SIZE_TSU = 8;
        int STRING_SIZE_DATA = 0;
        const int STRING_SIZE_CHECKSUM = 2;

        const int BYTE_INDEX_HEADER = 0;
        const int BYTE_INDEX_ID = BYTE_INDEX_HEADER + BYTE_SIZE_HEADER;
        const int BYTE_INDEX_MID = BYTE_INDEX_ID + BYTE_SIZE_ID;
        const int BYTE_INDEX_LENGTH = BYTE_INDEX_MID + BYTE_SIZE_MID;
        const int BYTE_INDEX_TS = BYTE_INDEX_LENGTH + BYTE_SIZE_LENGTH;
        const int BYTE_INDEX_TSU = BYTE_INDEX_TS + BYTE_SIZE_TS;
        int BYTE_INDEX_DATA = 0;
        int BYTE_INDEX_CHECKSUN = 0;

        const int BYTE_SIZE_HEADER = 1;
        const int BYTE_SIZE_ID = 6;
        const int BYTE_SIZE_MID = 1;
        const int BYTE_SIZE_LENGTH = 1;
        const int BYTE_SIZE_TS = 4;
        const int BYTE_SIZE_TSU = 4;
        int BYTE_SIZE_DATA = 0;
        const int BYTE_SIZE_CHECKSUM = 1;

        /*public string Header
        {
            get { return header.ToString("X"); }
            set { header = INT8U.Parse(value, System.Globalization.NumberStyles.HexNumber); }
        }*/

        public Byte[] ID
        {
            get { return id; }
            set { id = value; }
        }

        public MID MID
        {
            get { return msgID; }
            set { msgID = value; }
        }

        public INT8U Length
        {
            get { return length; }
            set { length = value; }
        }

        public Byte[] TimeStampSec
        {
            get { return timeStampSec; }
            set { timeStampSec = value; }
        }

        public Byte[] TimeStampMisec
        {
            get { return timeStampMisec; }
            set { timeStampMisec = value; }
        }
        public Byte[] Data
        {
            get { return data; }
            set { data = value; }
        }
        public INT8U CheckSum
        {
            get { return checkSum; }
            set { checkSum = value; }
        }

        /*internal void SetBit()
        {
            headerBytes = new BitVector32((int)header);
        }

        internal void GetBit()
        {
            header = (byte)headerBytes.Data;
        }*/

        public byte Delimiter
        {
            get { return (byte)headerBytes[HEADER_DELIMITER]; }
            set { headerBytes[HEADER_DELIMITER] = (int)value;}
        }

        public bool IsIncludeTimeStampMiSec
        {
            get { return headerBytes[HEADER_TMS] != 0x0; }
            set { headerBytes[HEADER_TMS] = value ? 0x1 : 0x0; }
        }

        public bool IsIncludeTimeStampSec
        {
            get { return headerBytes[HEADER_TS] != 0x0; }
            set { headerBytes[HEADER_TS] = value ? 0x1 : 0x0; }
        }

        public bool IsResponse
        {
            get { return headerBytes[HEADER_RN] == 0x0; }
            set { headerBytes[HEADER_RN] = value ? 0x0 : 0x1; }
        }

        public bool IsSend
        {
            get { return headerBytes[HEADER_DIR] == 0x0; }
            set { headerBytes[HEADER_DIR] = value ? 0x0 : 0x1; }
        }

        static Frame()
        {
            HEADER_DIR = BitVector32.CreateSection(0x1);
            HEADER_RN = BitVector32.CreateSection(0x1, HEADER_DIR);
            HEADER_TS = BitVector32.CreateSection(0x1, HEADER_RN);
            HEADER_TMS = BitVector32.CreateSection(0x1, HEADER_TS);
            HEADER_DELIMITER = BitVector32.CreateSection(0xF, HEADER_TMS);
        }
        public Frame()
        {

        }

        public static Frame Decode(string cmd)
        {
            return new Frame(ToBytes(cmd));
        }

        public Byte[] Encode()
        {
            checkSum = (byte)((byte)headerBytes.Data +
                AddUp(id) +
                (byte)msgID +
                length +
                AddUp(timeStampSec) +
                AddUp(timeStampMisec) +
                AddUp(data));
            return GetBytes();
        }
        /*
        public Frame(string cmd)
        {
            //without A55A and "B66B"
            headerBytes = new BitVector32(int.Parse(cmd.Substring(STRING_INDEX_HEADER, STRING_SIZE_HEADER), System.Globalization.NumberStyles.HexNumber));
            id = cmd.Substring(STRING_INDEX_ID, STRING_SIZE_ID);
            msgID = (CSLibrary.RTLS.Constants.MID)int.Parse(cmd.Substring(STRING_INDEX_MID, STRING_SIZE_MID), System.Globalization.NumberStyles.HexNumber);
            length = byte.Parse(cmd.Substring(STRING_INDEX_LENGTH, STRING_SIZE_LENGTH), System.Globalization.NumberStyles.HexNumber);
            if (IsIncludeTimeStampMiSec && IsIncludeTimeStampSec)
            {
                timeStampSec = uint.Parse(cmd.Substring(STRING_INDEX_TS, STRING_SIZE_TS), System.Globalization.NumberStyles.HexNumber);
                timeStampMisec = uint.Parse(cmd.Substring(STRING_INDEX_TSU, STRING_SIZE_TSU), System.Globalization.NumberStyles.HexNumber);
                if (length > 0)
                {
                    STRING_SIZE_DATA = length * 2;
                    STRING_INDEX_DATA = STRING_INDEX_TSU + STRING_SIZE_TSU;
                    STRING_INDEX_CHECKSUN = STRING_INDEX_DATA + STRING_SIZE_DATA;
                }
                else
                {
                    STRING_INDEX_CHECKSUN = STRING_INDEX_TSU + STRING_SIZE_TSU;
                }
            }
            else if (IsIncludeTimeStampSec)
            {
                timeStampSec = uint.Parse(cmd.Substring(STRING_INDEX_TS, STRING_SIZE_TS), System.Globalization.NumberStyles.HexNumber);
                if (length > 0)
                {
                    STRING_SIZE_DATA = length * 2;
                    STRING_INDEX_DATA = STRING_INDEX_TS + STRING_SIZE_TS;
                    STRING_INDEX_CHECKSUN = STRING_INDEX_DATA + STRING_SIZE_DATA;
                }
                else
                {
                    STRING_INDEX_CHECKSUN = STRING_INDEX_TS + STRING_SIZE_TS;
                }
            }
            else if (IsIncludeTimeStampSec)
            {
                timeStampMisec = uint.Parse(cmd.Substring(STRING_INDEX_TS, STRING_SIZE_TS), System.Globalization.NumberStyles.HexNumber);
                if (length > 0)
                {
                    STRING_SIZE_DATA = length * 2;
                    STRING_INDEX_DATA = STRING_INDEX_TS + STRING_SIZE_TS;
                    STRING_INDEX_CHECKSUN = STRING_INDEX_DATA + STRING_SIZE_DATA;
                }
                else
                {
                    STRING_INDEX_CHECKSUN = STRING_INDEX_TS + STRING_SIZE_TS;
                }
            }
            else
            {
                if (length > 0)
                {
                    STRING_SIZE_DATA = length * 2;
                    STRING_INDEX_DATA = STRING_INDEX_LENGTH + STRING_SIZE_LENGTH;
                    STRING_INDEX_CHECKSUN = STRING_INDEX_DATA + STRING_SIZE_DATA;
                }
                else
                {
                    STRING_INDEX_CHECKSUN = STRING_INDEX_LENGTH + STRING_SIZE_LENGTH;
                }
            }
            if (length > 0)
            {
                data = cmd.Substring(STRING_INDEX_DATA, STRING_SIZE_DATA);
            }
            checkSum = byte.Parse(cmd.Substring(STRING_INDEX_CHECKSUN, STRING_SIZE_CHECKSUM));
        }*/
        public Frame(byte[] cmd)
        {
            if (cmd == null || cmd.Length == 0)
                return;
            //without A55A and "B66B"
            headerBytes = new BitVector32((int)cmd[BYTE_INDEX_HEADER]);
            Array.Copy(cmd, BYTE_INDEX_ID, id, 0, BYTE_SIZE_ID);
            //id = FromBytes(cmd, BYTE_INDEX_ID, BYTE_SIZE_ID); 
            msgID = (CSLibrary.RTLS.Constants.MID)cmd[BYTE_INDEX_MID];
            length = cmd[BYTE_INDEX_LENGTH];
            BYTE_SIZE_DATA = length - 1; // Include CheckSum
            if (IsIncludeTimeStampMiSec && IsIncludeTimeStampSec)
            {
                //timeStampSec = (uint)(cmd[BYTE_INDEX_TS] << 24 | cmd[BYTE_INDEX_TS + 1] << 16 | cmd[BYTE_INDEX_TS + 2] << 8 | cmd[BYTE_INDEX_TS + 3]);
                //timeStampMisec = (uint)(cmd[BYTE_INDEX_TS + 4] << 24 | cmd[BYTE_INDEX_TS + 5] << 16 | cmd[BYTE_INDEX_TS + 6] << 8 | cmd[BYTE_INDEX_TS + 7]);
                Array.Copy(cmd, BYTE_INDEX_TS, timeStampSec, 0, BYTE_SIZE_TS);
                Array.Copy(cmd, BYTE_INDEX_TSU, timeStampMisec, 0, BYTE_SIZE_TSU);
                BYTE_INDEX_DATA = BYTE_INDEX_TSU + BYTE_SIZE_TSU;
                BYTE_INDEX_CHECKSUN = BYTE_INDEX_DATA + BYTE_SIZE_DATA;
            }
            else if (IsIncludeTimeStampSec)
            {
                //timeStampSec = (uint)(cmd[BYTE_INDEX_TS] << 24 | cmd[BYTE_INDEX_TS + 1] << 16 | cmd[BYTE_INDEX_TS + 2] << 8 | cmd[BYTE_INDEX_TS + 3]);
                Array.Copy(cmd, BYTE_INDEX_TS, timeStampSec, 0, BYTE_SIZE_TS);
                BYTE_INDEX_DATA = BYTE_INDEX_TS + BYTE_SIZE_TS;
                BYTE_INDEX_CHECKSUN = BYTE_INDEX_DATA + BYTE_SIZE_DATA;
            }
            else if (IsIncludeTimeStampSec)
            {
                //timeStampMisec = (uint)(cmd[BYTE_INDEX_TS] << 24 | cmd[BYTE_INDEX_TS + 1] << 16 | cmd[BYTE_INDEX_TS + 2] << 8 | cmd[BYTE_INDEX_TS + 3]);
                Array.Copy(cmd, BYTE_INDEX_TS, timeStampMisec, 0, BYTE_SIZE_TS);
                BYTE_INDEX_DATA = BYTE_INDEX_TS + BYTE_SIZE_TS;
                BYTE_INDEX_CHECKSUN = BYTE_INDEX_DATA + BYTE_SIZE_DATA;
            }
            else
            {
                BYTE_INDEX_DATA = BYTE_INDEX_LENGTH + BYTE_SIZE_LENGTH;
                BYTE_INDEX_CHECKSUN = BYTE_INDEX_DATA + BYTE_SIZE_DATA;
            }
            if (length > 0)
            {
                data = new byte[BYTE_SIZE_DATA];
                Array.Copy(cmd, BYTE_INDEX_DATA, data, 0, BYTE_SIZE_DATA);
            }
            checkSum = cmd[BYTE_INDEX_CHECKSUN];

        }
        
        static byte AddUp(byte[] bytes)
        {
            byte result = 0;
            if (bytes == null || bytes.Length == 0)
                return result;
            foreach (byte b in bytes)
            {
                result += b;
            }
            return result;
        }

        static string FromBytes(byte[] b, int index, int count)
        {
            string strB = "";
            for (int i = 0; i < count && index + i < b.Length; i++)
            {
                strB += b[index + i].ToString("X2");
            }
            return strB;
        }
        static Byte[] ToBytes(string b)
        {
            if (b == null || b.Length == 0)
                return null;
            byte[] bytes = new byte[b.Length / 2];
            for (int i = 0, _index = 0; i < b.Length; i += 2, _index++)
            {
                bytes[_index] = byte.Parse(b.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return bytes;
        }

        Byte[] GetBytes()
        {
            byte[] bytes = new byte[9 + (IsIncludeTimeStampMiSec ? BYTE_SIZE_TSU : 0) + (IsIncludeTimeStampSec ? BYTE_SIZE_TS : 0) + length + 1];
            bytes[BYTE_INDEX_HEADER] = (byte)(headerBytes.Data);
            Array.Copy(id, 0, bytes, BYTE_INDEX_ID, BYTE_SIZE_ID);
            bytes[BYTE_INDEX_MID] = (byte)msgID;
            bytes[BYTE_INDEX_LENGTH] = length;
            if (IsIncludeTimeStampMiSec && IsIncludeTimeStampSec)
            {
                Array.Copy(timeStampSec, 0, bytes, BYTE_INDEX_TS, BYTE_SIZE_TS);
                Array.Copy(timeStampMisec, 0, bytes, BYTE_INDEX_TSU, BYTE_SIZE_TSU);
                BYTE_INDEX_DATA = BYTE_INDEX_TSU + BYTE_SIZE_TSU;
                BYTE_INDEX_CHECKSUN = BYTE_INDEX_DATA + length;
            }
            else if (IsIncludeTimeStampMiSec)
            {
                Array.Copy(timeStampMisec, 0, bytes, BYTE_INDEX_TS, BYTE_SIZE_TS);
                BYTE_INDEX_DATA = BYTE_INDEX_TS + BYTE_SIZE_TS;
                BYTE_INDEX_CHECKSUN = BYTE_INDEX_DATA + length;
            }
            else if (IsIncludeTimeStampMiSec)
            {
                Array.Copy(timeStampSec, 0, bytes, BYTE_INDEX_TS, BYTE_SIZE_TS);
                BYTE_INDEX_DATA = BYTE_INDEX_TS + BYTE_SIZE_TS;
                BYTE_INDEX_CHECKSUN = BYTE_INDEX_DATA + length;
            }
            else
            {
                BYTE_INDEX_DATA = BYTE_INDEX_LENGTH + BYTE_SIZE_LENGTH;
                BYTE_INDEX_CHECKSUN = BYTE_INDEX_DATA + length;
            }
            if (data != null && data.Length > 0)
            {
                Array.Copy(data, 0, bytes, BYTE_INDEX_DATA, data.Length);
            }
            bytes[BYTE_INDEX_CHECKSUN] = checkSum;
            return bytes;
        }
    }
}
