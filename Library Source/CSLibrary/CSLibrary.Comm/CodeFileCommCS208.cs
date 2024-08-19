using System;

using CSLibrary.Utils;

namespace CSLibrary
{
    public partial class HighLevelInterface
    {
        internal static readonly uint SAMSUNG6410IO_CTL_RFID_ONOFF = CTL_CODE(FILE_DEVICE_UNKNOWN, 2101, METHOD_BUFFERED, FILE_ANY_ACCESS);

        #region ====================== CS208 Power Control Function ======================

        private static uint CTL_CODE(uint DeviceType, uint Function, uint Method, uint Access)
        {
            return ((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method);
        }

        static void CS208_RFID_PowerOn()
        {
            CS208_RFID_PowerOnOff(true);
        }

        static void CS208_RFID_PowerOff()
        {
            CS208_RFID_PowerOnOff(false);
        }

        static bool CS208_RFID_PowerOnOff(bool OnOff)
        {
            byte[] onoff = new byte[4];
            bool result = false;

            if (OnOff)
            {
                onoff[0] = 0xff;
                onoff[1] = 0xff;
                onoff[2] = 0xff;
                onoff[3] = 0xff;
            }
            else
            {
                onoff[0] = 0x00;
                onoff[1] = 0x00;
                onoff[2] = 0x00;
                onoff[3] = 0x00;
            }

            IntPtr m_GpioFileHandle = NativeIOControl.CreateFile(
                    "CNE1:",
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
                    SAMSUNG6410IO_CTL_RFID_ONOFF,
                    //new byte[] { (byte)OnOff },
                    onoff,
                    4,
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

        #endregion
    }
}