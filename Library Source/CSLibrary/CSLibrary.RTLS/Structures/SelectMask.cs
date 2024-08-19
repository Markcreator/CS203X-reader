using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    /// <summary>
    /// The tag mask is used to specify a bit pattern that is used to match against one of 
    /// a tag's memory banks to determine if it is matching or non-matching.  A mask is 
    /// a combination of a memory bank and a sequence of bits that will be matched at 
    /// the specified offset within the chosen memory bank.  
    /// </summary>
    [Serializable]
    public class SelectMask
    {
        /// <summary>
        /// The offset, in bits, from the start of the memory bank, of the 
        /// first bit that will be matched against the mask.  If offset falls 
        /// beyond the end of the memory bank, the tag is considered 
        /// non-matching. 
        /// </summary>
        public UInt32 offset = 0;
        /// <summary>
        /// The number of bits in the mask.  A length of zero will cause all 
        /// tags to match.  If (offset+count) falls beyond the end of 
        /// the memory bank, the tag is considered non-matching.  Valid 
        /// values are 0 to 255, inclusive. 
        /// </summary>
        public UInt32 count = 0;
        /// <summary>
        /// A buffer that contains a left-justified bit array that represents 
        /// that bit pattern to match 
        /// </summary>
        protected Byte[] m_mask = new Byte[8];
        /// <summary>
        /// Constructor
        /// </summary>
        public SelectMask()
        {
            // NOP
        }
        /// <summary>
        /// Custom constructor
        /// </summary>
        /// <param name="offset">offset in bit</param>
        /// <param name="count">count in bit</param>
        /// <param name="mask">TagID mask</param>
        public SelectMask(UInt32 offset, UInt32 count, Byte[] mask)
        {
            this.count = count;
            this.mask = mask == null ? new Byte[8] : (byte[])mask.Clone();
            this.offset = offset;
        }
        /// <summary>
        /// A buffer that contains a left-justified bit array that represents 
        /// that bit pattern to match ¡V i.e., the most significant bit of the bit 
        /// array appears in the most-significant bit (i.e., bit 7) of the first 
        /// byte of the buffer (i.e., mask[0]).  All bits beyond count are 
        /// ignored.  For example, if the application wished to find tags 
        /// with the following 12 bits 1000.1100.1101, starting at offset 
        /// 16 in the memory bank, then the fields would be set as 
        /// follows: 
        /// offset  = 16 
        /// count   = 12 
        /// mask[0] = 0x8C (1000.1100) 
        /// mask[1] = 0xD? (1101.????) 
        /// </summary>
        public Byte[] mask
        {
            get { return m_mask; }
            set
            {
                if (value != null)
                {
                    if (value.Length > 0 && value.Length <= 8)
                    {
                        Array.Copy(value, m_mask, value.Length);
                    }
                    else if (value.Length > 8)
                    {
                        Array.Copy(value, m_mask, 8);
                    }
                }
            }
        }
    };
}
