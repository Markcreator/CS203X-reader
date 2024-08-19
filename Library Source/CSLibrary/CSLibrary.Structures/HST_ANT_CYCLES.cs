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
using CSLibrary.Constants;

using CSLibrary;
using CSLibrary.Structures;
using CSLibrary.Events;

namespace CSLibrary.Structures
{

#if testmephist    
    class HST_ANT_CYCLES
    {
        #region Field
        private BitVector32 flags;
        private static readonly BitVector32.Section HST_ANT_CYCLES_CYCLE_LOW;
        private static readonly BitVector32.Section HST_ANT_CYCLES_CYCLE_HIGH;
        private static readonly BitVector32.Section HST_ANT_CYCLES_MODE;
        private static readonly BitVector32.Section HST_ANT_CYCLES_SEQ_SIZE;
        private static readonly BitVector32.Section HST_ANT_CYCLES_FREQ_AGILE;
        #endregion

        public uint RawData
        {
            get { return (uint)flags.Data; }
        }

        public RadioOperationMode OperationMode
        {
            get 
            { 
                return (RadioOperationMode)(flags[HST_ANT_CYCLES_CYCLE_LOW] | flags[HST_ANT_CYCLES_CYCLE_HIGH] << 8); 
            }
            set
            {
                flags[HST_ANT_CYCLES_CYCLE_LOW] = (0xFF & (int)value);
                flags[HST_ANT_CYCLES_CYCLE_HIGH] = (0xFF & ((int)value >> 8));
            }
        }

        public AntennaSequenceMode SequenceMode
        {
            get 
            { 
                return (AntennaSequenceMode)flags[HST_ANT_CYCLES_MODE]; 
            }
            set 
            { 
                flags[HST_ANT_CYCLES_MODE] = (int)value; 
            }
        }

        public int SequenceSize
        {
            get 
            {
                return flags[HST_ANT_CYCLES_SEQ_SIZE]; 
            }
            set 
            { 
                flags[HST_ANT_CYCLES_SEQ_SIZE] = value; 
            }
        }

        public FreqAgileMode AgileEnable
        {
            get
            {
                return (FreqAgileMode)flags[HST_ANT_CYCLES_FREQ_AGILE];
            }
            set
            {
                flags[HST_ANT_CYCLES_FREQ_AGILE] = (int)value;
            }
        }

        static HST_ANT_CYCLES()
        {
            HST_ANT_CYCLES_CYCLE_LOW = BitVector32.CreateSection(0xFF);
            HST_ANT_CYCLES_CYCLE_HIGH = BitVector32.CreateSection(0xFF, HST_ANT_CYCLES_CYCLE_LOW);
            HST_ANT_CYCLES_MODE = BitVector32.CreateSection(0x3, HST_ANT_CYCLES_CYCLE_HIGH);
            HST_ANT_CYCLES_SEQ_SIZE = BitVector32.CreateSection(0x3F, HST_ANT_CYCLES_MODE);
            HST_ANT_CYCLES_FREQ_AGILE = BitVector32.CreateSection(0x1, HST_ANT_CYCLES_SEQ_SIZE);
            
        }

        public HST_ANT_CYCLES(UInt32 data)
        {
            this.flags = new BitVector32((Int32)data);
        }

        public HST_ANT_CYCLES(RadioOperationMode mode, AntennaSequenceMode sequenceMode, int sequenceSize)
                    : this(0)
        {
            OperationMode = mode;
            SequenceMode = sequenceMode;
            SequenceSize = sequenceSize;
        }

        public HST_ANT_CYCLES(RadioOperationMode mode, AntennaSequenceMode sequenceMode, int sequenceSize, FreqAgileMode agileEnable)
            : this(0)
        {
            OperationMode = mode;
            SequenceMode = sequenceMode;
            SequenceSize = sequenceSize;
            AgileEnable = agileEnable;
        }
    }

#endif
}
