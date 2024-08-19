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

namespace CSLibrary.Utils
{
    class NativeIOControl
    {
        private const string DLL_PATH =
#if WindowsCE
        @"coredll.dll";
#else
        @"kernel32.dll";
#endif
        public static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

        private const uint FILE_DEVICE_UNKNOWN = 0x00000022;
        private const uint METHOD_BUFFERED = 0;
        private const uint FILE_ANY_ACCESS = 0;


        private const uint HARVEMAC_IOCTL_INDEX = 0x0000;


        internal static readonly uint IOCTL_HARVEMAC_WRITE = CTL_CODE(FILE_DEVICE_UNKNOWN,
                                                             HARVEMAC_IOCTL_INDEX,
                                                             METHOD_BUFFERED,
                                                             FILE_ANY_ACCESS);

        internal static readonly uint IOCTL_HARVEMAC_READ = CTL_CODE(FILE_DEVICE_UNKNOWN,
                                                             HARVEMAC_IOCTL_INDEX + 1,
                                                             METHOD_BUFFERED,
                                                             FILE_ANY_ACCESS);

        internal static readonly uint IOCTL_HARVEMAC_READ_CNT = CTL_CODE(FILE_DEVICE_UNKNOWN,
                                                             HARVEMAC_IOCTL_INDEX + 2,
                                                             METHOD_BUFFERED,
                                                             FILE_ANY_ACCESS);

        internal static readonly uint IOCTL_HARVEMAC_CANCEL = CTL_CODE(FILE_DEVICE_UNKNOWN,
                                                             HARVEMAC_IOCTL_INDEX + 3,
                                                             METHOD_BUFFERED,
                                                             FILE_ANY_ACCESS);

        internal static readonly uint IOCTL_HARVEMAC_SOFTRESET = CTL_CODE(FILE_DEVICE_UNKNOWN,
                                                             HARVEMAC_IOCTL_INDEX + 4,
                                                             METHOD_BUFFERED,
                                                             FILE_ANY_ACCESS);

        internal static readonly uint IOCTL_HARVEMAC_ABORT = CTL_CODE(FILE_DEVICE_UNKNOWN,
                                                             HARVEMAC_IOCTL_INDEX + 5,
                                                             METHOD_BUFFERED,
                                                             FILE_ANY_ACCESS);

        private static uint CTL_CODE(uint DeviceType, uint Function, uint Method, uint Access)
        {
            return ((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method);
        }

        #region enum
        [Flags]
        public enum EFileAccess : uint
        {
            /// <summary>
            /// 
            /// </summary>
            GenericRead = 0x80000000,
            /// <summary>
            /// 
            /// </summary>
            GenericWrite = 0x40000000,
            /// <summary>
            /// 
            /// </summary>
            GenericExecute = 0x20000000,
            /// <summary>
            /// 
            /// </summary>
            GenericAll = 0x10000000
        }

        [Flags]
        public enum EFileShare : uint
        {
            /// <summary>
            /// 
            /// </summary>
            None = 0x00000000,
            /// <summary>
            /// Enables subsequent open operations on an object to request read access. 
            /// Otherwise, other processes cannot open the object if they request read access. 
            /// If this flag is not specified, but the object has been opened for read access, the function fails.
            /// </summary>
            Read = 0x00000001,
            /// <summary>
            /// Enables subsequent open operations on an object to request write access. 
            /// Otherwise, other processes cannot open the object if they request write access. 
            /// If this flag is not specified, but the object has been opened for write access, the function fails.
            /// </summary>
            Write = 0x00000002,
            /// <summary>
            /// Enables subsequent open operations on an object to request delete access. 
            /// Otherwise, other processes cannot open the object if they request delete access.
            /// If this flag is not specified, but the object has been opened for delete access, the function fails.
            /// </summary>
            Delete = 0x00000004
        }

        public enum ECreationDisposition : uint
        {
            /// <summary>
            /// Creates a new file. The function fails if a specified file exists.
            /// </summary>
            New = 1,
            /// <summary>
            /// Creates a new file, always. 
            /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes, 
            /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
            /// </summary>
            CreateAlways = 2,
            /// <summary>
            /// Opens a file. The function fails if the file does not exist. 
            /// </summary>
            OpenExisting = 3,
            /// <summary>
            /// Opens a file, always. 
            /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
            /// </summary>
            OpenAlways = 4,
            /// <summary>
            /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
            /// The calling process must open the file with the GENERIC_WRITE access right. 
            /// </summary>
            TruncateExisting = 5
        }

        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }
        #endregion

        [DllImport(DLL_PATH, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateFile(
           Byte[] lpFileName,
           EFileAccess dwDesiredAccess,
           EFileShare dwShareMode,
           IntPtr lpSecurityAttributes,
           ECreationDisposition dwCreationDisposition,
           EFileAttributes dwFlagsAndAttributes,
           IntPtr hTemplateFile);
        [DllImport(DLL_PATH, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateFile(
           String lpFileName,
           EFileAccess dwDesiredAccess,
           EFileShare dwShareMode,
           IntPtr lpSecurityAttributes,
           ECreationDisposition dwCreationDisposition,
           EFileAttributes dwFlagsAndAttributes,
           IntPtr hTemplateFile);

        [DllImport(DLL_PATH, SetLastError = true)]
        internal static extern Boolean CloseHandle(IntPtr hObject);

        [DllImport(DLL_PATH)]
        internal static extern UInt32 GetLastError();

        [DllImport(DLL_PATH, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            Byte[] InBuffer,
            int nInBufferSize,
            Byte[] OutBuffer,
            int nOutBufferSize,
            ref int pBytesReturned,
            IntPtr lpOverlapped);
        [DllImport(DLL_PATH, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            uint InBuffer,
            uint nInBufferSize,
            ref int OutBuffer,
            int nOutBufferSize,
            ref int pBytesReturned,
            IntPtr lpOverlapped);
#if nouse
        public TransStatus MapWin32Err(uint err)
        {
            TransStatus s = TransStatus.CPL_SUCCESS;

#if UNITTEST
	        Debug.WriteLine(String.Format("MapWin32Err: {0} ({1:X})\n", err, err));
#endif

            switch (err)
            {
                case 995:
                case 1223:
                    s = TransStatus.CPL_WARN_CANCELLED;
                    break;
                case 0:
                    s = TransStatus.CPL_SUCCESS;
                    break;

                case 2:
                case 3:
                case 31:
                    s = TransStatus.CPL_ERROR_DEVICEGONE;
                    break;

                case 5:
                    s = TransStatus.CPL_ERROR_ACCESSDENIED;
                    break;

                case 1:
                case 6:
                case 11:
                case 12:
                case 13:
                    s = TransStatus.CPL_ERROR_INVALID;
                    break;

                default:
                    s = TransStatus.CPL_ERROR_DEVICEFAILURE;
                    break;
            }
            return s;
        }
#endif
    }
}
