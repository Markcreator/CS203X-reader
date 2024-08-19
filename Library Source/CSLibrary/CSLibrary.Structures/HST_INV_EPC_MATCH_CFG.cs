/*
Copyright (c) 2023 Convergence Systems Limited

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Specialized;

namespace CSLibrary.Structures
{
    /// <summary>
    /// The host writes this register to enable matching of the EPC read during 
    /// the underlying Query and inventory operations. Only matching EPCs will be returned to the host.
    /// </summary>
    public class HST_INV_EPC_MATCH_CFG
    {
        #region Field
        private BitVector32 flags;
        private static readonly BitVector32.Section HST_INV_EPC_MATCH_CFG_ENABLE;
        private static readonly BitVector32.Section HST_INV_EPC_MATCH_CFG_TOGGLE;
        private static readonly BitVector32.Section HST_INV_EPC_MATCH_CFG_LENGTH;
        private static readonly BitVector32.Section HST_INV_EPC_MATCH_CFG_OFFSET;
        #endregion
        /// <summary>
        /// EPC matching enabled
        /// </summary>
        public Boolean enable
        {
            get { return flags[HST_INV_EPC_MATCH_CFG_ENABLE] != 0x0; }
            set { flags[HST_INV_EPC_MATCH_CFG_ENABLE] = value ? 0x1 : 0x0; }
        }
        /// <summary>
        /// Determines if the associated tag-protocol operation will be 
        /// applied to tags that match the mask or not.  A non-zero 
        /// value indicates that the tag-protocol operation should be 
        /// applied to tags that match the mask.  A value of zero 
        /// indicates that the tag-protocol operation should be applied 
        /// to tags that do not match the mask. 
        /// </summary>
        public Boolean match
        {
            get { return flags[HST_INV_EPC_MATCH_CFG_TOGGLE] != 0x0; }
            set { flags[HST_INV_EPC_MATCH_CFG_TOGGLE] = value ? 0x1 : 0x0; }
        }
        /// <summary>
        /// The offset in bits, from the start of the Electronic Product 
        /// Code (EPC), of the first bit that will be matched against the 
        /// mask.  If offset falls beyond the end of EPC, the tag is 
        /// considered non-matching. 
        /// </summary>
        public UInt32 offset
        {
            get { return (UInt32)flags[HST_INV_EPC_MATCH_CFG_OFFSET]; }
            set { flags[HST_INV_EPC_MATCH_CFG_OFFSET] = (Int32)value; }
        }
        /// <summary>
        /// The number of bits in the mask.  A length of zero will cause 
        /// all tags to match.  If (offset+count) falls beyond the end 
        /// of the EPC, the tag is considered non-matching.  Valid 
        /// values are 0 to 496, inclusive. 
        /// </summary>
        public UInt32 count
        {
            get { return (UInt32)flags[HST_INV_EPC_MATCH_CFG_LENGTH]; }
            set { flags[HST_INV_EPC_MATCH_CFG_LENGTH] = (Int32)value; }
        }
        /// <summary>
        /// Internal use
        /// </summary>
        internal uint value
        {
            get { return (uint)flags.Data; }
            set { flags = new BitVector32((int)value); }
        }
        /// <summary>
        /// Internal Constructor
        /// </summary>
        static HST_INV_EPC_MATCH_CFG()
        {
            HST_INV_EPC_MATCH_CFG_ENABLE = BitVector32.CreateSection(0x1);
            HST_INV_EPC_MATCH_CFG_TOGGLE = BitVector32.CreateSection(0x1, HST_INV_EPC_MATCH_CFG_ENABLE);
            HST_INV_EPC_MATCH_CFG_LENGTH = BitVector32.CreateSection(0x1FF, HST_INV_EPC_MATCH_CFG_TOGGLE);
            HST_INV_EPC_MATCH_CFG_OFFSET = BitVector32.CreateSection(0x1FF, HST_INV_EPC_MATCH_CFG_LENGTH);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public HST_INV_EPC_MATCH_CFG()
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data"></param>
        public HST_INV_EPC_MATCH_CFG(UInt32 data)
        {
            this.flags = new BitVector32((Int32)data);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enable">
        /// EPC matching enabled</param>
        /// <param name="match">
        /// Determines if the associated tag-protocol operation will be 
        /// applied to tags that match the mask or not.  A non-zero 
        /// value indicates that the tag-protocol operation should be 
        /// applied to tags that match the mask.  A value of zero 
        /// indicates that the tag-protocol operation should be applied 
        /// to tags that do not match the mask. 
        /// </param>
        /// <param name="offset">
        /// The offset in bits, from the start of the Electronic Product 
        /// Code (EPC), of the first bit that will be matched against the 
        /// mask.  If offset falls beyond the end of EPC, the tag is 
        /// considered non-matching. 
        /// </param>
        /// <param name="count">
        /// The number of bits in the mask.  A length of zero will cause 
        /// all tags to match.  If (offset+count) falls beyond the end 
        /// of the EPC, the tag is considered non-matching.  Valid 
        /// values are 0 to 496, inclusive. 
        /// </param>
        public HST_INV_EPC_MATCH_CFG(Boolean enable, Boolean match, UInt32 offset, UInt32 count)
        {
            this.enable = enable;
            this.match = match;
            this.offset = offset;
            this.count = count;
        }
    }

    /// <summary>
    /// The host writes mask filter to enable matching of the bank 1 read. 
    /// </summary>
    public class BNK1_MSK_FILTER_CFG
    {
        #region Field
        /// <summary>
        /// Bank 1 mask matching enabled
        /// </summary>
        public Boolean enable = false;
        /// <summary>
        /// Determines if the associated tag-protocol operation will be 
        /// applied to tags that match the mask or not.  A non-zero 
        /// value indicates that the tag-protocol operation should be 
        /// applied to tags that match the mask.  A value of zero 
        /// indicates that the tag-protocol operation should be applied 
        /// to tags that do not match the mask. 
        /// </summary>
        public Boolean match = true;
        /// <summary>
        /// The offset in bits, from the start of the Bank 1 (PC + EPC)
        /// , of the first bit that will be matched against the 
        /// mask.  If offset falls beyond the end of Bank 1, the tag is 
        /// considered non-matching. 
        /// </summary>
        public int offset = 0;
        /// <summary>
        /// The number of bits in the mask from offset 0.  A length of zero will cause 
        /// all tags to match.  If (offset+count) falls beyond the end 
        /// of the bank 1, the tag is considered non-matching.  Valid 
        /// values are 0 to 512, inclusive. 
        /// </summary>
        private int length;
        /// <summary>
        /// bank 1 mask, accept "0", "1", and other value is don't care
        /// </summary>
        public string mask;
        private byte [] valuemask = new byte[64];
        private byte [] dontcaremask = new byte[64];
        

        #endregion
        
        public Boolean PreProcessing ()
        {
            int cnt, pos;
            string mark;

            if (enable != true)
                return true;

            if (string.IsNullOrEmpty (mask))
            {
                length = 0;
                return true;
            }

            length = offset + mask.Length;
            
            if (length > 512)
                return false;

            for (cnt = 0; cnt < 64; cnt++)
            {
                valuemask[cnt] = 0;
                dontcaremask[cnt] = 0;
            }
            
            for (cnt = 0, pos = offset; cnt < mask.Length; cnt++, pos++)
            {
                mark = mask.Substring (cnt, 1);
                switch (mark)
                {
                    case "0":
                        dontcaremask[pos / 8] |= (byte)(1 << (7 - (pos % 8)));
                        length = pos;
                        break;
                    case "1":
                        valuemask[pos / 8] |= (byte)(1 << (7 - (pos % 8)));
                        dontcaremask[pos / 8] |= (byte)(1 << (7 - (pos % 8)));
                        length = pos;
                        break;
                    default:
                        break;
                }
            }

            for (cnt = 0; cnt < 64; cnt++)
            {
                dontcaremask[cnt] = (byte)~dontcaremask[cnt];
            }
            
            length++;
            return true;
        }

        public Boolean FilterCheck (byte [] bank1data, int datalen)
        {
            byte [] result = new byte [64];

            int cnt, clen = (length + 7) / 8;
            
            if (datalen < clen)
                return false;

            for (cnt = 0; cnt < clen; cnt++ )
            {
                result [cnt] = (byte)((byte)~(bank1data[cnt] ^ valuemask[cnt]) | (dontcaremask[cnt]));
                if (result[cnt] != 0xff)
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public BNK1_MSK_FILTER_CFG()
        {
        }
        /// <summary>
        /// Constructor
        /// default 
        /// enable = true
        /// match = true
        /// offset = 0
        /// </summary>
        public BNK1_MSK_FILTER_CFG(string mask)
        {
            if (mask.Length > 512)
                throw new SystemException("mask too long");

            this.enable = true;
            this.match = true;
            this.offset = 0;
            this.mask = mask;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enable">
        /// Bank 1 mask filter enabled</param>
        /// <param name="match">
        /// Determines if the associated tag-protocol operation will be 
        /// applied to tags that match the mask or not.  A non-zero 
        /// value indicates that the tag-protocol operation should be 
        /// applied to tags that match the mask.  A value of zero 
        /// indicates that the tag-protocol operation should be applied 
        /// to tags that do not match the mask. 
        /// </param>
        /// <param name="offset">
        /// The offset in bits, from the start of the bank 1 (PC + EPC)
        /// , of the first bit that will be matched against the 
        /// mask.  If offset falls beyond the end of bank 1, the tag is 
        /// considered non-matching. 
        /// </param>
        /// <param name="count">
        /// The number of bits in the mask.  A length of zero will cause 
        /// all tags to match.  If (offset+count) falls beyond the end 
        /// of the bank 1, the tag is considered non-matching.  Valid 
        /// values are 0 to 511, inclusive. 
        /// </param>
        public BNK1_MSK_FILTER_CFG(Boolean enable, Boolean match, int offset, string mask)
        {
            if (offset + mask.Length > 512)
                throw new SystemException("mask too long");

            this.enable = enable;
            this.match = match;
            this.offset = offset;
            this.mask = mask;
        }
    }
}
