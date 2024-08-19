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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CSLibrary.Structures
{
    using Constants;

    /// <summary>
    /// Read memory structures, configure this before read
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TagReadParms
    {
        public MemoryBank bank;
        /// <summary>
        /// The access password for the tags.  A value of zero indicates no 
        /// access password. 
        /// </summary>
        public UInt32 accessPassword;
        /// <summary>
        /// Number of retrial will retry if read failure
        /// </summary>
        public UInt32 retryCount;
        /// <summary>
        /// The offset, in the memory bank, of the first 16-bit word to read.
        /// </summary>
        public UInt16 offset;
        /// <summary>
        /// The number of 16-bit words that will be read.
        /// </summary>
        public UInt16 count;
        /// <summary>
        /// An array to the 16-bit values to read from the tag's memory bank.
        /// </summary>
        public UInt16[] m_pData = new ushort[0];
        /// <summary>
        /// constructor
        /// </summary>
        public TagReadParms()
        {
            // NOP
        }
        /// <summary>
        /// An array to the 16-bit values to read from the tag's memory bank.
        /// </summary>
        public S_DATA pData
        {
            get { return new S_DATA(m_pData); }
            set { m_pData = value.ToUshorts(); }
        }
    }

    /// <summary>
    /// Read EPC structures, configure this before read current EPC
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TagReadEpcParms
    {
        /// <summary>
        /// The access password for the tags.  A value of zero indicates no 
        /// access password. 
        /// </summary>
        public UInt32 accessPassword;
        /// <summary>
        /// Number of retrial will retry if write failure
        /// </summary>
        public UInt32 retryCount;
        /// <summary>
        /// The offset, in the memory bank, of the first 16-bit word to read.
        /// </summary>
        public UInt16 offset;
        /// <summary>
        /// The number of 16-bit words that will be read. This field must be
        /// between 1 and 31, inclusive.                        
        /// </summary>          
        public UInt16 count;
        /// <summary>
        /// An EPC to the 16-bit values to write to the tag's memory bank.
        /// </summary>
        public UInt16[] m_epc = new ushort[31];
        /// <summary>
        /// 
        /// </summary>
        public TagReadEpcParms()
        {
            // NOP
        }
        /// <summary>
        /// An EPC to the 16-bit values to write to the tag's memory bank.
        /// </summary>
        public S_EPC epc
        {
            get { return new S_EPC(m_epc, count); }
        }
    }
    /// <summary>
    /// Read PC structures, configure this before read current PC
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TagReadPcParms
    {
        /// <summary>
        /// The access password for the tags.  A value of zero indicates no 
        /// access password. 
        /// </summary>
        public UInt32 accessPassword;
        /// <summary>
        /// Number of retrial will retry if read failure
        /// </summary>
        public UInt32 retryCount;
        /// <summary>
        /// A PC to the 16-bit values to read from the tag's memory bank.
        /// </summary>
        public ushort m_pc = 0;
        /// <summary>
        /// 
        /// </summary>
        public TagReadPcParms()
        {
            // NOP
        }
        /// <summary>
        /// A PC to the 16-bit values to read from the tag's memory bank.
        /// </summary>
        public S_PC pc
        {
            get { return new S_PC(m_pc); }
        }
    }
    /// <summary>
    /// Read TID structures, configure this before read current TID
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TagReadTidParms
    {
        /// <summary>
        /// The access password for the tags.  A value of zero indicates no 
        /// access password. 
        /// </summary>
        public UInt32 accessPassword;
        /// <summary>
        /// Number of retrial will retry if read failure
        /// </summary>
        public UInt32 retryCount;
        /// <summary>
        /// The offset, in the memory bank, of the first 16-bit word to read.
        /// </summary>
        public UInt16 offset = 0;
        /// <summary>
        /// The number of 16-bit words that will be read. This field must be
        /// between 1 and 31, inclusive.         
        /// </summary>
        public UInt16 count = 0;
        /// <summary>
        /// A pointer to the 16-bit values to read from the tag's memory bank.
        /// </summary>
        public UInt16[] pData = new UInt16[31];
        /// <summary>
        /// 
        /// </summary>
        public TagReadTidParms()
        {
            // NOP
        }
        /// <summary>
        /// An array to the 16-bit values to read from tag's memory bank.
        /// </summary>
        public S_TID tid
        {
            get
            {
                return new S_TID(pData, count);
            }
        }
    }
    /// <summary>
    /// Read password structures, configure this before read current password
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TagReadPwdParms
    {
        /// <summary>
        /// The access password for the tags.  A value of zero indicates no 
        /// access password. 
        /// </summary>
        public UInt32 accessPassword;
        /// <summary>
        /// Number of retrial will retry if read failure
        /// </summary>
        public UInt32 retryCount;
        /// <summary>
        /// A password to the 32-bit values to read from the tag's memory bank.
        /// </summary>
        public UInt32 m_password = 0;
        /// <summary>
        /// 
        /// </summary>
        public TagReadPwdParms()
        {
            // NOP
        }
        /// <summary>
        /// A password to the 32-bit values to read from the tag's memory bank.
        /// </summary>
        public S_PWD password
        {
            get { return new S_PWD(m_password); }
        }

    }
    /// <summary>
    /// Read User memory structures, configure this before read current User memory
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TagReadUserParms
    {
        /// <summary>
        /// The access password for the tags.  A value of zero indicates no 
        /// access password. 
        /// </summary>
        public UInt32 accessPassword;
        /// <summary>
        /// Number of retrial will retry if read failure
        /// </summary>
        public UInt32 retryCount;
        /// <summary>
        /// The offset, in the memory bank, of the first 16-bit word to read.
        /// </summary>
        public UInt16 offset;
        /// <summary>
        /// The number of 16-bit words that will be read.
        /// </summary>
        public UInt16 count;
        /// <summary>
        /// An array to the 16-bit values to read from the tag's memory bank.
        /// </summary>
        public UInt16[] m_pData = new ushort[0];
        /// <summary>
        /// constructor
        /// </summary>
        public TagReadUserParms()
        {
            // NOP
        }
        /// <summary>
        /// An array to the 16-bit values to read from the tag's memory bank.
        /// </summary>
        public S_DATA pData
        {
            get { return new S_DATA(m_pData); }
            set { m_pData = value.ToUshorts(); }
        }
    }
}