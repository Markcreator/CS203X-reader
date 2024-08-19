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
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace CSLibrary.Utils
{
    internal class NativeSetupAPI
    {
        private const string DLL_PATH = @"setupapi.dll";
        // Water
        internal static readonly Guid GUID_CLASS_HARVEMAC = new Guid(0x48c602d4, unchecked((short)0xc77e), 0x45b9, new Byte[] { 0x81, 0x33, 0x20, 0xc9, 0x68, 0x3b, 0xd1, 0xa6 });

        // test ,for jungo driver
        //internal static readonly Guid GUID_CLASS_HARVEMAC = new Guid("C671678C-82C1-43F3-D700-0049433E9A4B");

        // test fort water 64 bit driver
        //internal static readonly Guid GUID_CLASS_HARVEMAC = new Guid("0284d7d6-1b8e-4fbb-bd44-cc3bf19916d0");

        // Org
        //internal static readonly Guid GUID_CLASS_HARVEMAC = new Guid(0x36FC9E60, unchecked((short)0xC465), 0x11CF, new Byte[] { 0x80, 0x56, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00 });

        #region enum
        [FlagsAttribute]
        internal enum DiGetClassFlags : uint {
            DIGCF_DEFAULT       = 0x00000001,  // only valid with DIGCF_DEVICEINTERFACE
            DIGCF_PRESENT       = 0x00000002,
            DIGCF_ALLCLASSES    = 0x00000004,
            DIGCF_PROFILE       = 0x00000008,
            DIGCF_DEVICEINTERFACE   = 0x00000010,
        }
        #endregion

        #region struct
        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public uint Flags;
            public IntPtr Reserved;
        }
#if nouse
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public UInt32 cbSize;
            public byte[] DevicePath;
        }
#endif
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public Int32 cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }
        #endregion

        [DllImport(DLL_PATH, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetupDiGetClassDevs(
                                              [In, Out]     ref Guid ClassGuid,
                                              [In]          IntPtr Enumerator,
                                              [In]          IntPtr hwndParent,
                                              [In]          DiGetClassFlags Flags
                                             );
        
        [DllImport(DLL_PATH, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern Boolean SetupDiEnumDeviceInterfaces(
                [In]        IntPtr DeviceInfoSet,
                [In, Out]   IntPtr/*ref SP_DEVINFO_DATA*/ DeviceInfoData,
                [In]        ref Guid InterfaceClassGuid,
                [In]        UInt32 MemberIndex,
                [In, Out]   ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData
                );

        [DllImport(DLL_PATH, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern Boolean SetupDiGetDeviceInterfaceDetail(
           [In]         IntPtr hDevInfo,
           [In, Out]    ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
           [In, Out]    ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
           [In]         UInt32 deviceInterfaceDetailDataSize,
           [Out]        out UInt32 requiredSize,
           [In]    IntPtr/*ref SP_DEVINFO_DATA*/ deviceInfoData
        );

        [DllImport(DLL_PATH, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern Boolean SetupDiGetDeviceInterfaceDetail(
           [In]         IntPtr hDevInfo,
           [In, Out]    ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
           [In, Out]    byte [] deviceInterfaceDetailData,
           [In]         UInt32 deviceInterfaceDetailDataSize,
           [Out]        out UInt32 requiredSize,
           [In]    IntPtr/*ref SP_DEVINFO_DATA*/ deviceInfoData
        );

        [DllImport(DLL_PATH, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern Boolean SetupDiGetDeviceInterfaceDetail(
           [In]         IntPtr hDevInfo,
           [In, Out]    ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
           [In, Out]    IntPtr deviceInterfaceDetailData,
           [In]         UInt32 deviceInterfaceDetailDataSize,
           [Out]        out UInt32 requiredSize,
           [In]    IntPtr deviceInfoData
        );

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SetupDiGetDeviceInterfaceDetail(
           IntPtr hDevInfo,
           ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
           ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
           UInt32 deviceInterfaceDetailDataSize,
           out UInt32 requiredSize,
           ref SP_DEVINFO_DATA deviceInfoData
        );


        [DllImport(DLL_PATH, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);
    }
}
