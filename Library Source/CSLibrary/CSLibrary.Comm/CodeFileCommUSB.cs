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
using System.Threading;
using System.Runtime.InteropServices;

using CSLibrary.Utils;

namespace CSLibrary
{
    public partial class HighLevelInterface
    {
        const int FILE_DEVICE_UNKNOWN = 0x00000022;
        const int HARVEMAC_IOCTL_INDEX = 0x0000;
        const int METHOD_BUFFERED = 0;
        const int FILE_ANY_ACCESS = 0;
        const int IOCTL_HARVEMAC_WRITE = ((FILE_DEVICE_UNKNOWN) << 16) | ((FILE_ANY_ACCESS) << 14) | ((HARVEMAC_IOCTL_INDEX) << 2) | (METHOD_BUFFERED);
        const int IOCTL_HARVEMAC_READ = ((FILE_DEVICE_UNKNOWN) << 16) | ((FILE_ANY_ACCESS) << 14) | ((HARVEMAC_IOCTL_INDEX + 1) << 2) | (METHOD_BUFFERED);
        const int IOCTL_HARVEMAC_READ_CNT = ((FILE_DEVICE_UNKNOWN) << 16) | ((FILE_ANY_ACCESS) << 14) | ((HARVEMAC_IOCTL_INDEX + 2) << 2) | (METHOD_BUFFERED);
        const int IOCTL_HARVEMAC_CANCEL = ((FILE_DEVICE_UNKNOWN) << 16) | ((FILE_ANY_ACCESS) << 14) | ((HARVEMAC_IOCTL_INDEX + 3) << 2) | (METHOD_BUFFERED);
        const int IOCTL_HARVEMAC_SOFTRESET = ((FILE_DEVICE_UNKNOWN) << 16) | ((FILE_ANY_ACCESS) << 14) | ((HARVEMAC_IOCTL_INDEX + 4) << 2) | (METHOD_BUFFERED);
        const int IOCTL_HARVEMAC_ABORT = ((FILE_DEVICE_UNKNOWN) << 16) | ((FILE_ANY_ACCESS) << 14) | ((HARVEMAC_IOCTL_INDEX + 5) << 2) | (METHOD_BUFFERED);
        const int IOCTL_HARVEMAC_RESET_TO_BL = ((FILE_DEVICE_UNKNOWN) << 16) | ((FILE_ANY_ACCESS) << 14) | ((HARVEMAC_IOCTL_INDEX + 6) << 2) | (METHOD_BUFFERED);
        


        IntPtr usbHandle = IntPtr.Zero;

        private bool USB_Connect(string sn)
        {
            var dict = new Dictionary<string, string>();
            string deviceName;
            
            GetSN(ref dict);
            
            if (dict.Count == 0)
                return false;

            List<string> keys = new List<string>(dict.Keys);

            if (sn.Length == 0)
            {
                deviceName = dict[keys[0]];
            }
            else
            {
                deviceName = dict[sn];
            }

            usbHandle = NativeIOControl.CreateFile(
                deviceName,
                NativeIOControl.EFileAccess.GenericRead | NativeIOControl.EFileAccess.GenericWrite,
                NativeIOControl.EFileShare.Read | NativeIOControl.EFileShare.Write,
                IntPtr.Zero,
                NativeIOControl.ECreationDisposition.OpenExisting,
                NativeIOControl.EFileAttributes.Overlapped,
                IntPtr.Zero);
            //Console.WriteLine(usbHandle.ToString ());
            SystemDebug_WriteLine("Get USB Handle " + usbHandle.ToString());

            if (usbHandle == NativeIOControl.INVALID_HANDLE_VALUE)
            {
                SystemDebug_WriteLine("Get USB Handle Fail");
                return false;
            }

            SystemDebug_WriteLine("Get USB Handle Success");
            return true;
        }

        private bool USB_Send(byte[] dataBuffer, int offset, int dataBufferSize)
        {
            int total = 0;
            TimeSpan TimeDiff;
            DateTime ProcStart = DateTime.Now;
            int failcnt = 0;

            while (true)
            {
                Thread.Sleep(10);

                if (NativeIOControl.DeviceIoControl(
                    usbHandle,
                    IOCTL_HARVEMAC_WRITE,
                    dataBuffer,
                    dataBufferSize,
                    null,
                    0,
                    ref total,
                    IntPtr.Zero
                    ))
                {
                    DEBUGT_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, "USB_Send : Success");
                    return true;
                }

                failcnt++;

                if (DateTime.Now.Subtract(ProcStart).TotalSeconds >= 1)
                {
                    DEBUGT_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, "USB_Send : Send Fail " + failcnt + " times");
                    return false;
                }


/*                TimeDiff = new TimeSpan(DateTime.Now.Ticks - ProcStart.Ticks);
                if (TimeDiff.TotalMilliseconds > 1000)
                {
                    DEBUGT_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, "USB_Send : Send Fail " + failcnt + " times");
                    return false;
                }*/

            }
        }


        byte[] USBDriverDataBuffer = new byte[9 * 1024];
        int USBDriverBufferStart = 0;
        int USBDriverBufferSize = 0;

        private bool USB_Recv(byte[] dataBuffer, int offset, int readSize)
        {
            if (USBDriverBufferSize < readSize)
            {
                byte[] USBDriverReadBuffer = new byte[8 * 1024];
                int ReadSize = readSize - USBDriverBufferSize;
                int dataAvailable = 0;
                int RecvLen = 0;
                TimeSpan TimeDiff;
                DateTime ProcStart = DateTime.Now;

                while (true)
                {
                    Thread.Sleep(1);

                    if (!NativeIOControl.DeviceIoControl(
                    usbHandle,
                    NativeIOControl.IOCTL_HARVEMAC_READ_CNT,
                    0,
                    0,
                    ref dataAvailable,
                    4,
                    ref RecvLen,
                    IntPtr.Zero
                    ))
                    {
                        DEBUGT_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, "USB_Recv : Check Data Available Fail, internal buffer size " + USBDriverBufferSize + ", USB Driver data available " + dataAvailable + ", Abort Command " + ReaderAbort);
                    }
                    else if (dataAvailable >= ReadSize)
                        break;

                    if (DateTime.Now.Subtract (ProcStart).TotalSeconds >= 1)
                    {
                        DEBUGT_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, "USB_Recv : Time Out Fail, internal buffer size " + USBDriverBufferSize + ", USB Driver data available " + dataAvailable + ", Abort Command " + ReaderAbort);
                        if (ReaderAbort)
                            USBDriverBufferSize = 0;
                        return false;
                    }
                    
                    
                    /*                    TimeDiff = new TimeSpan(DateTime.Now.Ticks - ProcStart.Ticks);
                                        if (TimeDiff.TotalMilliseconds > 1000)
                                        {
                                            DEBUGT_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, "USB_Recv : Time Out Fail, internal buffer size " + USBDriverBufferSize + ", USB Driver data available " + dataAvailable + ", Abort Command " + ReaderAbort);
                                            if (ReaderAbort)
                                                USBDriverBufferSize = 0;
                                            return false;
                                        }*/

                }

                //DEBUGT_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, "USB_Recv : Available Data " + dataAvailable);
                if (dataAvailable > 8000)
                    DEBUGT_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, "USB_Recv : Available Data " + dataAvailable);

                if (dataAvailable > USBDriverReadBuffer.Length)
                    dataAvailable = USBDriverReadBuffer.Length;

                if (!NativeIOControl.DeviceIoControl(
                    usbHandle,
                    NativeIOControl.IOCTL_HARVEMAC_READ,
                    null,
                    0,
                    USBDriverReadBuffer,
                    dataAvailable,
                    ref RecvLen,
                    IntPtr.Zero
                    ))
                {
                    DEBUGT_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, "USB_Recv : Receive data fail");
                    return false;
                }

                //DEBUGT_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, "USB_Recv : Read " + RecvLen + " bytes success");

                if (USBDriverBufferSize > 0)
                    Array.Copy(USBDriverDataBuffer, USBDriverBufferStart, USBDriverDataBuffer, 0, USBDriverBufferSize);

                Array.Copy(USBDriverReadBuffer, 0, USBDriverDataBuffer, USBDriverBufferSize, RecvLen);
                USBDriverBufferSize += RecvLen;
                USBDriverBufferStart = 0;
            }

            Array.Copy(USBDriverDataBuffer, USBDriverBufferStart, dataBuffer, offset, readSize);
            USBDriverBufferStart += readSize;
            USBDriverBufferSize -= readSize;

            return true;
        }

        static object DebugLock = new object();

#if WindowsCE
        [DllImport("coredll.dll")]
        public static extern void NKDbgPrintf(string lpszFmt);
#else
        public static void NKDbgPrintf(string lpszFmt)
        { }
#endif

        public static void SystemDebug_WriteLine(string message)
        {
//            lock (DebugLock)
//                NKDbgPrintf("AppLog:" + DateTime.Now + "]" + message + Environment.NewLine);
        }


        private bool USB_Close()
        {
            SystemDebug_WriteLine("Close USB Handle " + usbHandle.ToString());

            if (NativeIOControl.CloseHandle(usbHandle) != true)
            {
                SystemDebug_WriteLine("Close USB Handle Fail");
                return false;
            }

            SystemDebug_WriteLine("Close USB Handle Success");
            usbHandle = IntPtr.Zero;
            
            if (m_oem_machine == CSLibrary.Constants.Machine.CS101 && m_DeviceName == "USB")
                RFID_PowerOnOff(1);

            return true;
        }

        /*****************************************************************************
         *  Function: OSL_FindToken
         *
         *  Parameters:
         *          str - a windows device name string
         *          tok - the token to find
         *
         *  Description:
         *       search str for first instance of tok.
         *
         *  Returns:       
         *       ptr to first instance of tok
         *      
         *****************************************************************************/
        // Example string:
        //"\\?\usb#vid_8086&pid_feed#serialnumff#{48c602d4-c77e-45b9-8133-20c9683bd1a6}"	
        private String OSL_FindToken(String str, Char tok)
        {
            String rp = String.Empty;
            if (String.IsNullOrEmpty(str))
                return null;
            return str.Substring(str.IndexOf(tok));
        }

        /*****************************************************************************
         *  Function: OSL_GetSerialNumber
         *
         *  Parameters:
         *         drvName: a windows device name string
         *         psnSize: size of the string
         *
         *  Description:
         *         Serach the device name for the serial number portion.
         *
         *  Returns:       
         *      ptr to the serial number component if preseent, else null.
         *      
         ****************************************************************************/
        private String OSL_GetSerialNumber(String drvName/*, UInt32 psnSize*/)
        {
            String p1, p2;//, pSn;
            //UInt32 snSize;

            p1 = OSL_FindToken(drvName, '#');	// gets us to usb#
            if (String.IsNullOrEmpty(p1))
                return null;
            //++p1;
            p1 = p1.Remove(0, 1);
            p1 = OSL_FindToken(p1, '#');	   // serial number starts on next byte
            if (String.IsNullOrEmpty(p1))
                return null;
            p1 = p1.Remove(0, 1);
            p2 = OSL_FindToken(p1, '#');		// end of the serial number
            if (String.IsNullOrEmpty(p2))
                return null;

            return p1.Substring(0, p1.Length - p2.Length);
        }

        public bool GetUsbDevicesList(ref List<string> DeviceList)
        {
            var dict = new Dictionary<string , string>();
            GetSN(ref dict);

            if (dict.Count == 0)
                return false;
           
            DeviceList = new List<string>(dict.Keys);
            return true;
        }

#if WIN32
        private bool GetSN(ref Dictionary<string, string> DeviceList)
        {
            bool result = false;
            bool done = false;

            switch (System.Environment.OSVersion.Platform)
            {
                case System.PlatformID.Win32NT:

#if false
                    // for test X64 
                    DeviceList.Add("DEVICE", "DEVICE");
                    result = true;
                    break;
#endif
#if WIN32
                    //TransStatus result = TransStatus.CPL_UNKNOWN;
                    Guid pGuid = NativeSetupAPI.GUID_CLASS_HARVEMAC;
                    UInt32 NumberDevices = 0;
                    IntPtr hOut = NativeIOControl.INVALID_HANDLE_VALUE;
                    IntPtr hardwareDeviceInfo;
                    NativeSetupAPI.SP_DEVICE_INTERFACE_DATA deviceInfoData = new NativeSetupAPI.SP_DEVICE_INTERFACE_DATA();

                    //
                    // Open a handle to the plug and play dev node.
                    // SetupDiGetClassDevs() returns a device information set that contains info on all
                    // installed devices of a specified class.
                    //
                    /*                    hardwareDeviceInfo = NativeSetupAPI.SetupDiGetClassDevs(
                                                                   ref pGuid,
                                                                   IntPtr.Zero, // Define no enumerator (global)
                                                                   IntPtr.Zero, // Define no
                                                                   (NativeSetupAPI.DiGetClassFlags.DIGCF_PRESENT | // Only Devices present
                                                                   NativeSetupAPI.DiGetClassFlags.DIGCF_DEVICEINTERFACE)); // Function class devices.
                    */
                    deviceInfoData.cbSize = 0;
                    deviceInfoData.Flags = 0;
                    deviceInfoData.InterfaceClassGuid = Guid.Empty;

                    hardwareDeviceInfo = NativeSetupAPI.SetupDiGetClassDevs(
                                               ref pGuid,
                                               IntPtr.Zero, // Define no enumerator (global)
                                               IntPtr.Zero, // Define no
                                               (NativeSetupAPI.DiGetClassFlags.DIGCF_PRESENT | // Only Devices present
                                               NativeSetupAPI.DiGetClassFlags.DIGCF_DEVICEINTERFACE)); // Function class devices.


                    if (hardwareDeviceInfo == NativeIOControl.INVALID_HANDLE_VALUE)
                        done = true;
                    else
                        done = false;

                    //
                    // Take a wild guess at the number of devices we have;
                    // Be prepared to realloc and retry if there are more than we guessed
                    //
                    deviceInfoData.cbSize = Marshal.SizeOf(typeof(NativeSetupAPI.SP_DEVICE_INTERFACE_DATA));

                    while (!done)
                    {
                        if (NativeSetupAPI.SetupDiEnumDeviceInterfaces(hardwareDeviceInfo,
                                                           IntPtr.Zero, // We don't care about specific PDOs
                                                           ref pGuid,
                                                           NumberDevices++,
                                                           ref deviceInfoData))
                        {

                            // build a DevInfo Data structure
                            NativeSetupAPI.SP_DEVINFO_DATA da = new NativeSetupAPI.SP_DEVINFO_DATA();
                            da.cbSize = (uint)Marshal.SizeOf(da);

                            // build a Device Interface Detail Data structure
                            NativeSetupAPI.SP_DEVICE_INTERFACE_DETAIL_DATA functionClassDeviceData = new NativeSetupAPI.SP_DEVICE_INTERFACE_DETAIL_DATA();

                            if (IntPtr.Size == 8) // for 64 bit operating systems
                                functionClassDeviceData.cbSize = 8;
                            else
                                functionClassDeviceData.cbSize = 4 + Marshal.SystemDefaultCharSize; // for 32 bit systems

                            // now we can get some more detailed information
                            uint nRequiredSize = 0;
                            uint nBytes = 256; // BUFFER_SIZE;

                            if (NativeSetupAPI.SetupDiGetDeviceInterfaceDetail(hardwareDeviceInfo, ref deviceInfoData, ref functionClassDeviceData, nBytes, out nRequiredSize, ref da))
                            {
                                string sn, deviceName;
                                //
                                // Make the callback to inform the library of the device
                                //
                                sn = OSL_GetSerialNumber(functionClassDeviceData.DevicePath/*, ref snSize*/);
                                deviceName = functionClassDeviceData.DevicePath.ToString();
                                //                                result = TransStatus.CPL_SUCCESS;
                                DeviceList.Add(sn, deviceName);
                                result = true;
                                //free(pSn);
                            }
                        }
                        else
                        {
                            done = true;
                        }
                    }

                    NativeSetupAPI.SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
#endif
                    break;

                case System.PlatformID.WinCE:
#if WindowsCE
                    UIntPtr kActive;
                    UIntPtr kDevice;
                    int status;
                    uint i = 0;

//                    Debug.WriteLine("rfidtx: Enum Start");
                    status = NativeRegistry.RegOpenKeyEx(
                                NativeRegistry.HKEY_LOCAL_MACHINE,
                                "Drivers\\Active",
                                0,
                                0,
                                out kActive
                                );
                    if (status != 0)
                        return false;
//                        return TransStatus.CPL_ERROR_INVALID;


                    do
                    {
                        Byte[] Name1 = new Byte[256];
                        uint Name1Len;
                        Byte[] Name2 = new Byte[256];
                        uint Name2Len;
                        long lt = 0;

                        Name1Len = 256;
                        status = NativeRegistry.RegEnumKeyEx(
                                        kActive,
                                        i++,
                                        Name1,
                                        ref Name1Len,
                                        IntPtr.Zero,
                                        IntPtr.Zero,
                                        IntPtr.Zero,
                                        out lt
                                        );

                        if (status != 0 && (status != 234/*ERROR_MORE_DATA*/))
                        {
//                            ++done;
                            done = true;
                        }
                        else
                        {
                            status = NativeRegistry.RegOpenKeyEx(
                                           kActive,
                                           Name1,
                                           0,
                                           0,
                                           out kDevice
                                           );

                            if (status == 0)
                            {
                                NativeRegistry.KeyType rtype = NativeRegistry.KeyType.REG_NONE;
                                int s1, s2;

                                Name1Len = 256;
                                Name2Len = 256;
                                s1 = NativeRegistry.RegQueryValueEx(
                                                kDevice,
                                                "Name",
                                                IntPtr.Zero,
                                                ref rtype,
                                                Name1,
                                                ref Name1Len
                                                );

                                s2 = NativeRegistry.RegQueryValueEx(
                                                kDevice,
                                                "SN",
                                                IntPtr.Zero,
                                                ref rtype,
                                                Name2,
                                                ref Name2Len
                                                );

                                NativeRegistry.RegCloseKey(kDevice);

                                try
                                {
                                    //if ((Name1[0] == (Byte)'R') && (Name1[2] == (Byte)'F') && (Name1[4] == (Byte)'T') && (Name1[8] == (Byte)':'))
                                    if ((Name1[0] == 0x52) && (Name1[2] == 0x46) && (Name1[4] == 0x54))
                                    {
//                                        Debug.WriteLine(String.Format("rfidtx: enum {0}: {1}  {2}", ++i, Encoding.Unicode.GetString(Name1, 0, (int)Name1Len), Encoding.ASCII.GetString(Name2, 0, (int)Name2Len)));
                                        sn = System.Text.Encoding.Default.GetString(Name2, 0, (int)Name2Len);
                                        deviceName = System.Text.Encoding.Unicode.GetString(Name1, 0, (int)Name1Len);

//                                        result = TransStatus.CPL_SUCCESS;
                                        result = true;
                                    }
                                }
                                catch
                                {
//                                    Debug.WriteLine(String.Format("Cannot find RFT in registry"));
                                }
                            }
                        }
                    }
                    while (done);
//                    while (done != 0) ;
                    NativeRegistry.RegCloseKey(kActive);
#endif
                    break;

                default:
                    return false;
            }

            return result;
        }
#elif WindowsCE        
        private bool GetSN(ref Dictionary<string, string> DeviceList)
        {
            bool result = false;
            bool done = false;

            UIntPtr kActive;
            UIntPtr kDevice;
            int status;
            uint i = 0;

    //                    Debug.WriteLine("rfidtx: Enum Start");
            status = NativeRegistry.RegOpenKeyEx(
                        NativeRegistry.HKEY_LOCAL_MACHINE,
                        "Drivers\\Active",
                        0,
                        0,
                        out kActive
                        );
            if (status != 0)
                return false;
    //                        return TransStatus.CPL_ERROR_INVALID;


            do
            {
                Byte[] Name1 = new Byte[256];
                uint Name1Len;
                Byte[] Name2 = new Byte[256];
                uint Name2Len;
                long lt = 0;

                Name1Len = 256;
                status = NativeRegistry.RegEnumKeyEx(
                                kActive,
                                i++,
                                Name1,
                                ref Name1Len,
                                IntPtr.Zero,
                                IntPtr.Zero,
                                IntPtr.Zero,
                                out lt
                                );

                if (status != 0 && (status != 234/*ERROR_MORE_DATA*/))
                {
    //                            ++done;
                    done = true;
                }
                else
                {
                    status = NativeRegistry.RegOpenKeyEx(
                                   kActive,
                                   Name1,
                                   0,
                                   0,
                                   out kDevice
                                   );

                    if (status == 0)
                    {
                        NativeRegistry.KeyType rtype = NativeRegistry.KeyType.REG_NONE;
                        int s1, s2;

                        Name1Len = 256;
                        Name2Len = 256;
                        s1 = NativeRegistry.RegQueryValueEx(
                                        kDevice,
                                        "Name",
                                        IntPtr.Zero,
                                        ref rtype,
                                        Name1,
                                        ref Name1Len
                                        );

                        s2 = NativeRegistry.RegQueryValueEx(
                                        kDevice,
                                        "SN",
                                        IntPtr.Zero,
                                        ref rtype,
                                        Name2,
                                        ref Name2Len
                                        );

                        NativeRegistry.RegCloseKey(kDevice);

                        try
                        {
                            //if ((Name1[0] == (Byte)'R') && (Name1[2] == (Byte)'F') && (Name1[4] == (Byte)'T') && (Name1[8] == (Byte)':'))
                            if ((Name1[0] == 0x52) && (Name1[2] == 0x46) && (Name1[4] == 0x54))
                            {
                                string sn, deviceName;
                                //                                        Debug.WriteLine(String.Format("rfidtx: enum {0}: {1}  {2}", ++i, Encoding.Unicode.GetString(Name1, 0, (int)Name1Len), Encoding.ASCII.GetString(Name2, 0, (int)Name2Len)));
                                sn = System.Text.Encoding.Default.GetString(Name2, 0, (int)Name2Len);
                                deviceName = System.Text.Encoding.Unicode.GetString(Name1, 0, (int)Name1Len);

                                DeviceList.Add(sn, deviceName);

    //                                        result = TransStatus.CPL_SUCCESS;
                                result = true;
                            }
                        }
                        catch
                        {
    //                                    Debug.WriteLine(String.Format("Cannot find RFT in registry"));
                        }
                    }
                }
            }
            while (!done);
            NativeRegistry.RegCloseKey(kActive);

            return result;
        }
#endif

        private bool USB_Control(READERCMD Cmd)
        {
            int total = 0;
            byte [] flags = new byte [100];

            try
            {
                DEBUGT_Write(DEBUGLEVEL.USB_IO_CONTROL, "USB_Control : " + Cmd);

                switch (Cmd)
                {
                    case READERCMD.CANCEL:
                        if (!NativeIOControl.DeviceIoControl(
                        usbHandle,
                        IOCTL_HARVEMAC_CANCEL,
                        flags,
                        100, //sizeof(flags),
                        null,
                        0,
                        ref total,
                        IntPtr.Zero
                        ))
                        {
                            DEBUG_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, " Fail");
                            return false;
                        }
                        DEBUG_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, " Success");
                        break;

                    case READERCMD.ABORT:
                        if (!NativeIOControl.DeviceIoControl(
                        usbHandle,
                        IOCTL_HARVEMAC_ABORT,
                        null,
                        0,
                        null,
                        0,
                        ref total,
                        IntPtr.Zero
                        ))
                        {
                            DEBUG_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, " Fail");
                            return false;
                        }
                        DEBUG_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, " Success");
                        break;

                    case READERCMD.SOFTRESET:
                        ReaderStatus = 0;
                        if (!NativeIOControl.DeviceIoControl(
                        usbHandle,
                        IOCTL_HARVEMAC_SOFTRESET,
                        null,
                        0,
                        null,
                        0,
                        ref total,
                        IntPtr.Zero
                        ))
                        {
                            DEBUG_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, " Fail");
                            return false;
                        }
                        DEBUG_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, " Success");
                        break;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                DEBUG_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, ex.Message);
                Console.WriteLine(ex.Message);
            }
            return true;
        }
    }
}
