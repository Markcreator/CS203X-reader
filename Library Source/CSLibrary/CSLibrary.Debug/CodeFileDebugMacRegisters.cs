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
using System.IO;
using CSLibrary.Constants;

namespace CSLibrary
{
    public partial class HighLevelInterface
    {
        public class DEBUGMACREGISTER
        {
            public UInt32[] _0000 = null;             // 0X0000~0X0002
            public UInt32[] _0100 = null;
            public UInt32[] _0200 = null;
            public UInt32[] _0300 = null;
            public UInt32[] _0400 = null;
            public UInt32[] _0500 = null;             // 0x0500 ~ 0x0501
            public UInt32[] _0600 = null;             // 0x0600 ~ 0x0603
            public UInt32[] _0700 = null;             // 0x0700
            public UInt32[] _0701 = null;             // 0x0701
            public UInt32[,] _0702_707 = null;     // 0x0702 ~ 0x0707
            public UInt32[] _0800 = null;             // 0x0800
            public UInt32[,] _0801_80c = null;    // 0x0800 ~ 0x080c
            public UInt32[] _0900 = null;             // 0X0900 ~ 0X0901
            public UInt32[] _0902 = null;             // 0X0902
            public UInt32[,] _0903_906 = null;     // 0X0903 ~ 0X0906
            public UInt32[] _0910_921 = null;        // 0X0910 ~ 0X0921
            public UInt32[] _0a00_a07 = null;            // 0X0a00 ~ 0x0a0f
            public UInt32[] _0a08 = null;            // 0X0a08
            public UInt32[,] _0a09_a18 = null;          // 0X0a09 ~ 0x0a18
            public UInt32[] _0b00 = null;          // 0x0b00 ~ 0x0b84
            public UInt32[] _0c01 = null;             // 0X0c01
            public UInt32[,] _0c02_c07 = null;    // 0X0c02 ~ 0x0c07
            public UInt32[] _0c08 = null;             // 0X0c08
            public UInt32[] _0d00 = null;
            public UInt32[] _0e00 = null;
            public UInt32[] _0f0f = null;

            public DEBUGMACREGISTER ()
            {
                _0000 = new UInt32[3];             // 0X0000~0X0002
                _0500 = new UInt32[2];             // 0x0500 ~ 0x0501
                _0600 = new UInt32[4];             // 0x0600 ~ 0x0603
                _0700 = new UInt32[1];             // 0x0700 
                _0701 = new UInt32[2];              // (Selector)
                _0702_707 = new UInt32[16, 6];     // 0x0702 ~ 0x0707
                _0800 = new UInt32[2];             // (Selector)
                _0801_80c = new UInt32[8, 12];     // 0x0800 ~ 0x080c
                _0900 = new UInt32[2];             // 0X0900 ~ 0X0901
                _0902 = new UInt32[2];              // Selector
                _0903_906 = new UInt32[4, 4];      // 0X0903 ~ 0X0906
                _0910_921 = new UInt32[12];        // 0X0910 ~ 0X0921
                _0a00_a07 = new UInt32[8];            // 0X0a00 ~ 0x0a07
                _0a08 = new UInt32[2];              // Selector
                _0a09_a18 = new UInt32[8, 16];      // 0X0a09 ~ 0X0a18
                _0b00 = new UInt32[0x85];          // 0x0b00 ~ 0x0b84
                _0c01 = new UInt32[2];             // 0X0c01 (Selector)
                _0c02_c07 = new UInt32[50, 6];     // 0X0c02 ~ 0x0c07
                _0c08 = new UInt32[1];             // 0X0c08
                _0f0f = new UInt32[1];
            }
        }


        public DEBUGMACREGISTER DebugMacReadRegisters()
        {
            DEBUGMACREGISTER debugRegs = new DEBUGMACREGISTER();
            UInt32 value = 0;

            for (int cnt = 0x0000; cnt < 3; cnt++)
            {
                MacReadRegister((MacRegister)cnt, ref value);
                debugRegs._0000[cnt] = value;
            }

            for (int cnt = 0x0000; cnt < 2; cnt++)
            {
                MacReadRegister((MacRegister)(cnt + 0x0500), ref value);
                debugRegs._0500[cnt] = value;
            }

            for (int cnt = 0x0000; cnt < 4; cnt++)
            {
                MacReadRegister((MacRegister)(cnt + 0x0600), ref value);
                debugRegs._0600[cnt] = value;
            }

            MacReadRegister((MacRegister)0x0700, ref value);
            debugRegs._0700[0] = value;

            for (uint cnt = 0x0000; cnt < 16; cnt++)
            {
                MacWriteRegister((MacRegister)0x0701, cnt);

                for (int cnt1 = 0x0000; cnt1 < 6; cnt1++)
                {
                    MacReadRegister((MacRegister)(cnt1 + 0x0702), ref value);
                    debugRegs._0702_707[cnt, cnt1] = value;
                }
            }

            for (uint cnt = 0x0000; cnt < 8; cnt++)
            {
                MacWriteRegister((MacRegister)0x0800, cnt);

                for (int cnt1 = 0x0000; cnt1 < 12; cnt1++)
                {
                    MacReadRegister((MacRegister)(cnt1 + 0x0801), ref value);
                    debugRegs._0801_80c[cnt, cnt1] = value;
                }
            }

            for (int cnt = 0x0000; cnt < 2; cnt++)
            {
                MacReadRegister((MacRegister)(cnt + 0x0900), ref value);
                debugRegs._0900[cnt] = value;
            }

            for (uint cnt = 0x0000; cnt < 4; cnt++)
            {
                MacWriteRegister((MacRegister)0x0902, cnt);

                for (int cnt1 = 0x0000; cnt1 < 4; cnt1++)
                {
                    MacReadRegister((MacRegister)(cnt1 + 0x0903), ref value);
                    debugRegs._0903_906[cnt, cnt1] = value;
                }
            }

            for (int cnt = 0x0000; cnt < 12; cnt++)
            {
                MacReadRegister((MacRegister)(cnt + 0x0910), ref value);
                debugRegs._0910_921[cnt] = value;
            }

            for (int cnt = 0x0000; cnt < 8; cnt++)
            {
                MacReadRegister((MacRegister)(cnt + 0x0a00), ref value);
                debugRegs._0a00_a07[cnt] = value;
            }

            for (uint cnt = 0x0000; cnt < 8; cnt++)
            {
                MacWriteRegister((MacRegister)0x0a08, cnt);

                for (int cnt1 = 0x0000; cnt1 < 16; cnt1++)
                {
                    MacReadRegister((MacRegister)(cnt1 + 0x0a09), ref value);
                    debugRegs._0a09_a18[cnt, cnt1] = value;
                }
            }

            for (int cnt = 0x0000; cnt < 0x85; cnt++)
            {
                MacReadRegister((MacRegister)(cnt + 0x0b00), ref value);
                debugRegs._0b00[cnt] = value;
            }

            for (uint cnt = 0x0000; cnt < 50; cnt++)
            {
                MacWriteRegister((MacRegister)0x0c01, cnt);

                for (int cnt1 = 0x0000; cnt1 < 6; cnt1++)
                {
                    MacReadRegister((MacRegister)(cnt1 + 0x0c02), ref value);
                    debugRegs._0c02_c07[cnt, cnt1] = value;
                }
            }

            MacReadRegister((MacRegister)0x0c08, ref value);
            debugRegs._0c08[0] = value;

            MacReadRegister((MacRegister)0x0f0f, ref value);
            debugRegs._0f0f[0] = value;

            return debugRegs;
        }
    }
}


