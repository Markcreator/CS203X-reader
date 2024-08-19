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
using System.Threading;

namespace CSLibrary
{
    sealed class Win32
    {
        /// <summary>
        /// Unsafe memory Compare
        /// </summary>
        /// <param name="src1"></param>
        /// <param name="src2"></param>
        /// <param name="size"></param>
#if WIN32
        [DllImport("msvcrt.dll")]
        public static extern int memcmp(byte[] src1, byte[] src2, int size);
#else
#if WindowsCE
        [DllImport("coredll.dll")]
        public static extern int memcmp(byte[] src1, byte[] src2, int size);
#else
        public static int memcmp(byte[] b1, byte[] b2, int length)
        {
            int result = 0;

            if (b1.Length < length || b2.Length < length)
            {
                if (b1.Length > b2.Length)
                    return 1;
                else
                    return -1;
            }

            for (int i = 0; i < length; i++)
            {
                if (b1[i] != b2[i])
                {
                    result = (int)(b1[i] - b2[i]);
                    break;
                }
            }

            return result;
        }
#endif
#endif


        public static void memcpy(UInt16[] dest, byte[] src, uint srcOffset, uint byteSize)
        {
            uint len = byteSize / 2;

            if ((byteSize % 2) != 0 || src.Length < byteSize || dest.Length < len)
                throw new ArgumentException();

            for (uint cnt = 0; cnt < len; cnt++)
            {
                dest[cnt] = (UInt16)(src[srcOffset + cnt * 2] << 8 | src[srcOffset + cnt * 2 + 1]);
            }
        }
        
        /*        public static void memcpy(byte[] dest, UInt16[] src, uint byteSize)
                {
                    uint len = byteSize / 2;

                    if ((byteSize % 2) != 0 || src.Length < len || dest.Length < byteSize)
                        throw new ArgumentException();

                    for (uint cnt = 0; cnt < len; cnt++)
                    {
                        dest[cnt * 2] = (byte)(src[cnt] >> 8);
                        dest[cnt * 2 + 1] = (byte)(src[cnt]);
                    }
                }
        */
        public static int memcmp(UInt16[] b1, UInt16[] b2, int length)
        {
            int result = 0;

            if (b1.Length < length || b2.Length < length)
            {
                if (b1.Length > b2.Length)
                    return 1;
                else
                    return -1;
            }

            for (int i = 0; i < length; i++)
            {
                if (b1[i] != b2[i])
                {
                    result = (int)(b1[i] - b2[i]);
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Unsafe GetExitCodeThread
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="size"></param>
#if WindowsCE
        [DllImport("coredll.dll")]
#else
        [DllImport("kernel32.dll")]
#endif
        public static extern bool GetExitCodeThread(UInt32 hThread, out uint lpExitCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
#if WindowsCE
        [DllImport("coredll.dll")]
#else
        [DllImport("kernel32.dll")]
#endif
        public static extern bool _lrotl(UInt32 value, UInt32 shift);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
#if WindowsCE
        [DllImport("coredll.dll")]
#else
        [DllImport("kernel32.dll")]
#endif
        public static extern bool _lrotr(UInt32 value, UInt32 shift);
    }
}



















#if nouse
        /// <summary>
        /// Unsafe memory copy
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="size"></param>
#if WIN32
        [DllImport("msvcrt.dll")]
        public static extern void memcpy(byte[] dest, byte[] src, int size);
#else
#if WindowsCE
        [DllImport("coredll.dll")]
        public static extern void memcpy(byte[] dest, byte[] src, int size);
#else
        /// <summary>
        ///  memory copy
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="byteSize"></param>
        public static void memcpy(byte[] dest, byte[] src, uint byteSize)
        {
            if (src.Length < byteSize || dest.Length < byteSize)
                throw new ArgumentException();

            for (uint cnt = 0; cnt < byteSize; cnt++)
            {
                dest[cnt] = src[cnt];
            }
        }
#endif
#endif

        /// <summary>
        /// Unsafe memory copy
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="size"></param>
#if WIN32
        [DllImport("msvcrt.dll")]
        public static extern void memcpy(UInt16[] dest, UInt16[] src, int size);
#else
#if WindowsCE
        [DllImport("coredll.dll")]
        public static extern void memcpy(UInt16 [] dest, UInt16 [] src, int size);
#else
#endif
#endif









    sealed class Win32
    {

        public static void memcpy(UInt16[] dest, byte[] src, uint byteSize)
        {
            uint len = byteSize / 2;

            if ((byteSize % 2) != 0 || src.Length < byteSize || dest.Length < len)
                throw new ArgumentException();

            for (uint cnt = 0; cnt < len; cnt++)
            {
                dest[cnt] = (UInt16)(src[cnt * 2] << 8 | src[cnt * 2 + 1]);
            }
        }

        public static void memcpy(UInt16[] dest, byte[] src, uint srcOffset, uint byteSize)
        {
            uint len = byteSize / 2;

            if ((byteSize % 2) != 0 || src.Length < byteSize || dest.Length < len)
                throw new ArgumentException();

            for (uint cnt = 0; cnt < len; cnt++)
            {
                dest[cnt] = (UInt16)(src[srcOffset + cnt * 2] << 8 | src[srcOffset + cnt * 2 + 1]);
            }
        }

        








#if nouse         
        /// <summary>
        ///  memory copy
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="size"></param>
#if WindowsCE
        [DllImport("coredll.dll")]
#else
        [DllImport("msvcrt.dll")]
#endif
        public extern void memcpy(byte* dest, byte* src, int size);
        /// <summary>
        ///  memory copy
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="size"></param>
#if WindowsCE
        [DllImport("coredll.dll")]
#else
        [DllImport("msvcrt.dll")]
#endif
        public extern void memcpy(UInt16* dest, UInt16* src, int size);
        /// <summary>
        ///  memory copy
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="size"></param>
#if WindowsCE
        [DllImport("coredll.dll")]
#else
        [DllImport("msvcrt.dll")]
#endif
        public extern void memcpy(byte* dest, byte[] src, int size);
        /// <summary>
        ///  memory Compare
        /// </summary>
        /// <param name="src1"></param>
        /// <param name="src2"></param>
        /// <param name="size"></param>
#if WindowsCE
        [DllImport("coredll.dll")]
#else
        [DllImport("msvcrt.dll")]
#endif
        public extern int memcmp(byte[] src1, byte[] src2, int size);

        /// <summary>
        ///  GetExitCodeThread
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="size"></param>
#if WindowsCE
        [DllImport("coredll.dll")]
#else
        [DllImport("kernel32.dll")]
#endif
        public extern bool GetExitCodeThread(UInt32 hThread, out uint lpExitCode);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
#if WindowsCE
        [DllImport("coredll.dll")]
#else
        [DllImport("kernel32.dll")]
#endif
        public extern bool _lrotl(UInt32 value, UInt32 shift);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
#if WindowsCE
        [DllImport("coredll.dll")]
#else
        [DllImport("kernel32.dll")]
#endif
        public extern bool _lrotr(UInt32 value, UInt32 shift);

#endif
    }
}
#endif
