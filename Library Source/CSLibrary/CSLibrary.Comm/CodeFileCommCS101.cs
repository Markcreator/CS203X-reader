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

using CSLibrary.Utils;

namespace CSLibrary
{
    public partial class HighLevelInterface
    {
//        const uint GPIO_BASE_IOCODE = 8164;
//        const uint RFID_CTL_WRITE = (GPIO_BASE_IOCODE + 12);
//        const uint RFID_CTL_READ = (GPIO_BASE_IOCODE + 13);

        private static readonly uint GPIO_BASE_IOCODE = 8164;
        private static readonly uint GPIO00_CTL_READ = (GPIO_BASE_IOCODE);
        private static readonly uint GPIO01_CTL_READ = (GPIO00_CTL_READ + 1);
        private static readonly uint GPIO02_CTL_READ = (GPIO01_CTL_READ + 1);
        
        private static readonly uint GPIO00_CTL_WRITE = (GPIO02_CTL_READ + 1);
        private static readonly uint GPIO01_CTL_WRITE = (GPIO00_CTL_WRITE + 1);
        private static readonly uint GPIO02_CTL_WRITE = (GPIO01_CTL_WRITE + 1);

        private static readonly uint GPIO00_CTL_SET_IO = (GPIO02_CTL_WRITE + 1);
        private static readonly uint GPIO01_CTL_SET_IO = (GPIO00_CTL_SET_IO + 1);
        private static readonly uint GPIO02_CTL_SET_IO = (GPIO01_CTL_SET_IO + 1);

        private static readonly uint GPIO00_CTL_GET_IO = (GPIO02_CTL_SET_IO + 1);
        private static readonly uint GPIO01_CTL_GET_IO = (GPIO00_CTL_GET_IO + 1);
        private static readonly uint GPIO02_CTL_GET_IO = (GPIO01_CTL_GET_IO + 1);

        private static readonly uint GPIO03_CTL_READ = (RFID_CTL_READ + 1);
        private static readonly uint GPIO03_CTL_WRITE = (GPIO03_CTL_READ + 1);
        private static readonly uint GPIO03_CTL_GET_IO = (GPIO03_CTL_WRITE + 1);
        private static readonly uint GPIO03_CTL_SET_IO = (GPIO03_CTL_GET_IO + 1);

        private static readonly uint RFID_CTL_WRITE = (GPIO02_CTL_GET_IO + 1);
        private static readonly uint RFID_CTL_READ = (RFID_CTL_WRITE + 1);
        
        #region ====================== CS101 Power Control Function ======================

        // 0 = ON, 1 = OFF
        bool RFID_PowerOnOff(uint OnOff)
        {
            bool result = false;

            IntPtr m_GpioFileHandle = NativeIOControl.CreateFile(
                    "GIO1:",
                    NativeIOControl.EFileAccess.GenericRead | NativeIOControl.EFileAccess.GenericWrite,
                    NativeIOControl.EFileShare.Read | NativeIOControl.EFileShare.Write,
                    IntPtr.Zero,
                    NativeIOControl.ECreationDisposition.OpenExisting,
                    NativeIOControl.EFileAttributes.Normal | NativeIOControl.EFileAttributes.Overlapped,
                    IntPtr.Zero);

            if (m_GpioFileHandle != NativeIOControl.INVALID_HANDLE_VALUE)
            {
                int retSize = 0;
                result = NativeIOControl.DeviceIoControl(
                    m_GpioFileHandle,
                    RFID_CTL_WRITE,
                    new byte[] { (byte)OnOff },
                    1,
                    null,
                    0,
                    ref retSize,
                    IntPtr.Zero);
                //Debug.WriteLine(String.Format("RF Power turn on {0}", result));
            }

            if (m_GpioFileHandle != IntPtr.Zero)
            {
                NativeIOControl.CloseHandle(m_GpioFileHandle);
                m_GpioFileHandle = IntPtr.Zero;
            }

            return result;
        }

        public static bool GpioRead(int iGpio, ref byte piState)
        {
            bool bRet = false;
            int lpBytesReturned = 0;
            byte[] outBuf = new byte[1];

            IntPtr FileHandle = NativeIOControl.CreateFile("GIO1:",
                    NativeIOControl.EFileAccess.GenericRead | NativeIOControl.EFileAccess.GenericWrite,
                    NativeIOControl.EFileShare.Read | NativeIOControl.EFileShare.Write,
                    IntPtr.Zero,
                    NativeIOControl.ECreationDisposition.OpenExisting,
                    NativeIOControl.EFileAttributes.Normal | NativeIOControl.EFileAttributes.Overlapped,
                    IntPtr.Zero);
                
                //Object name.
                //            GENERIC_READ | GENERIC_WRITE,               //Desired access.
                //            FILE_SHARE_READ | FILE_SHARE_WRITE,         //Share Mode.
                //            IntPtr.Zero,                                //Security Attr
                //            OPEN_EXISTING,                              //Creation Disposition.
                //            FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, //Flag and Attributes.
                //            IntPtr.Zero);

            if (FileHandle == NativeIOControl.INVALID_HANDLE_VALUE)
                return false;

            switch (iGpio)
            {
                case 0:
                    //bRet = NativeIOControl.DeviceIoControl(FileHandle, GPIO00_CTL_READ, null, 0, outBuf, 1, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 1:
                    //bRet = NativeIOControl.DeviceIoControl(FileHandle, GPIO01_CTL_READ, null, 0, outBuf, 1, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 2:
                    //bRet = NativeIOControl.DeviceIoControl(FileHandle, GPIO02_CTL_READ, null, 0, outBuf, 1, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 3:
                    bRet = NativeIOControl.DeviceIoControl(FileHandle, RFID_CTL_READ, null, 0, outBuf, 1, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 4:
                    //bRet = NativeIOControl.DeviceIoControl(FileHandle, GPIO03_CTL_READ, null, 0, outBuf, 1, ref lpBytesReturned, IntPtr.Zero);
                    break;
                default:
                    bRet = false;
                    break;
            };

            if (FileHandle != IntPtr.Zero)
            {
                NativeIOControl.CloseHandle(FileHandle);
                FileHandle = IntPtr.Zero;
            }
            piState = outBuf[0];

            return bRet;
        }


        #endregion
    }
}