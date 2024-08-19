using System;
using System.Collections.Specialized;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    using Constants;
    class FirmwareUpgradeBlock
    {
        public ErrorCode errorCode = ErrorCode.NO_ERROR;
        public BitVector32 module;
        public uint blockIndex = 0;

        private static readonly BitVector32.Section fs;
        private static readonly BitVector32.Section fs7Bit;
        private static readonly BitVector32.Section fs8Bit;

        public FirmwareUpgradeBlock(ErrorCode errorCode, int module, uint blockIndex)
        {
            this.errorCode = errorCode;
            this.module = new BitVector32(module);
            if (this.module[fs7Bit] == 0x1)
            {
                this.blockIndex = blockIndex | 0x1 << 7;
            }
            if (this.module[fs8Bit] == 0x1)
            {
                this.blockIndex = blockIndex | 0x1 << 8;
            }
        }

        static FirmwareUpgradeBlock()
        {
            fs = BitVector32.CreateSection(0xff);
            fs7Bit = BitVector32.CreateSection(0x1, fs);
            fs8Bit = BitVector32.CreateSection(0x1, fs7Bit);
        }

        public static Byte[] Encode(
            byte[] anchorID,
            int blockCount,
            int blockIndex,
            byte[] blockData)
        {
            uint checkSum = 0;
            BitVector32 countBits = new BitVector32(blockCount);
            BitVector32 indexBits = new BitVector32(blockIndex + 1);

            Byte[] blocks = new byte[77];
            //copy first ID
            Array.Copy(anchorID, blocks, 6);
            blocks[6] = (byte)(0x2 | indexBits[fs7Bit] << 6 | indexBits[fs8Bit] << 4 | countBits[fs7Bit] << 7 | countBits[fs8Bit] << 5);
            blocks[7] = (byte)blockCount;
            blocks[8] = (byte)(blockIndex + 1);
            //check last block issue
            if ((blockIndex + 1) * 64 > blockData.Length)
            {
                Array.Copy(blockData, blockIndex * 64, blocks, 9, blockData.Length - blockIndex * 64);
                checkSum = AddUp(blockData, blockIndex * 64, blockData.Length - blockIndex * 64);
            }
            else
            {
                Array.Copy(blockData, blockIndex * 64, blocks, 9, 64);
                checkSum = AddUp(blockData, blockIndex * 64, 64);
            }
            byte[] bcs = new byte[] {
                (byte) (checkSum >> 24),
                (byte) (checkSum >> 16),
                (byte) (checkSum >> 8),
                (byte) (checkSum >> 0)};
            Array.Copy(bcs, 0, blocks, 73, 4);
            return blocks;
        }

        public static FirmwareUpgradeBlock Decode(byte[] raw)
        {
            if (raw == null || raw.Length != 3)
            {
                return null;
            }
            FirmwareUpgradeBlock arg = new FirmwareUpgradeBlock((ErrorCode)raw[0], raw[1], raw[2]);
            return arg;
        }

        static uint AddUp(byte[] bytes, int index, int count)
        {
            uint result = 0;
            if (bytes == null || bytes.Length == 0)
                return result;
            for (int i = index, c = 0; c < count; c++, i++)
            {
                result += bytes[i];
            }
            return result;
        }
    }
}
