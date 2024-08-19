#if CS101

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace CSLibrary.Device
{
    using HANDLE = System.IntPtr;

    #region public Variable
    /// <summary>
    /// The COLORREF value is used to specify an RGB color.
    /// </summary>
    public struct ColorRef
    {
        /// <summary>
        /// red   component   of   color 
        /// </summary>
        public Byte bRed;     
        /// <summary>
        /// green   component   of   color  
        /// </summary>
        public Byte bGreen;     
        /// <summary>
        /// blue   component   of   color  
        /// </summary>
        public Byte bBlue;
        /// <summary>
        /// Custom constructor
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        public ColorRef(byte red, byte green, byte blue)
        {
            this.bBlue = blue;
            this.bGreen = green;
            this.bRed = red;
        }
    }

    /// <summary>
    /// Buzzer Sound Volume
    /// </summary>
    public enum BuzzerVolume : uint
    {
        /// <summary>
        /// Low Volume
        /// </summary>
        LOW,
        /// <summary>
        /// Middle Volume
        /// </summary>
        MIDDLE,
        /// <summary>
        /// High Volume
        /// </summary>
        HIGH,
    }
    /// <summary>
    /// Ring Tone 
    /// Notes : Now only support 6 tones
    /// </summary>
    public enum RingTone : int
    {
        /// <summary>
        /// Melody 1
        /// </summary>
        T1 = 1,
        /// <summary>
        /// Melody 2
        /// </summary>
        T2,
        /// <summary>
        /// Melody 3
        /// </summary>
        T3,
        /// <summary>
        /// Melody 4
        /// </summary>
        T4,
        /// <summary>
        /// Melody 5
        /// </summary>
        T5,
        /// <summary>
        /// Melody 6
        /// </summary>
        T6,
        /// <summary>
        /// Unknown ring tone
        /// </summary>
        UNKNOWN = -1,
    };
    /// <summary>
    /// Wifi Status
    /// </summary>
    public enum WifiState
    {
        /// <summary>
        /// Unknown state
        /// </summary>
        WIFI_UNKNOWN,
        /// <summary>
        /// Wifi connected
        /// </summary>
        WIFI_CONNECTED,
        /// <summary>
        /// Wifi diconnected
        /// </summary>
        WIFI_DISCONNECTED,
        /// <summary>
        /// Wifi is power off
        /// </summary>
        WIFI_OFF,
    };
    /// <summary>
    /// CPU Speed of ARM core
    /// </summary>
    public enum Speed
    {
        /// <summary>
        /// Full speed
        /// </summary>
        FULL = 0,
        /// <summary>
        /// Power save mode
        /// </summary>
        LOW = 1
    }

    #endregion
    /// <summary>
    /// Device IO Class
    /// </summary>
    public class Device
    {

        #region Private Variable
        private const String B101DLL_Path = "GSLB101api.dll";

        private const int RadioPwrPin = 3;
        private const int ScnrPwrDownrPin = 2;
        private const int ScnrTriggerPin = 1;
        private const int ScnrWakePin = 0;

        //private static readonly object padlock = new object();
        private static HANDLE m_GpioFileHandle = HANDLE.Zero;
        #endregion

#if false
        #region Dispose
        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public static void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        private static void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here.
                // If disposing is false, 
                // only the following code is executed.
                if(!GpioUnini())
                    throw new ApplicationException("GpioUnini() failed!");
                handle = IntPtr.Zero;
            }
            disposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~Device()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        /*public static void Dispose()
        {
            if (!disposed)
            {
                // Unload native library
                GpioUnini();
                disposed = true;
            }
        }*/

        #endregion
#endif
        #region PInvoke
        //GSLB101API_API int fnGSLB101api(void);

        //GSLB101API_API void GetDeviceName(TCHAR *DeviceName);
        [DllImport(B101DLL_Path)]
        private static extern void GetDeviceName([In] IntPtr pDeviceName);
        /// <summary>
        /// Get Device Name
        /// </summary>
        public static string GetDeviceName()
        {
            string result = "Get device name failed.";
            IntPtr pDevicePtr = IntPtr.Zero;
            try
            {
                pDevicePtr = Marshal.AllocHGlobal(24 * sizeof(ushort));

                GetDeviceName(pDevicePtr);

                result = Marshal.PtrToStringUni(pDevicePtr);
            }
            catch
            {

            }

            if (pDevicePtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pDevicePtr);
            }
            return result;
        }

        /// <summary>
        /// Turn on Led
        /// </summary>
        /// <param name="colorRGB">Led Color</param>
        /// <returns>bool Result</returns>
        public static bool LedSetOn(ColorRef colorRGB)
        {
            return LedSetOnEx((uint)(colorRGB.bBlue << 16 | colorRGB.bGreen << 8 | colorRGB.bRed));
        }

        //LED
        //GSLB101API_API BOOL LedSetOn(COLORREF Color);
        /// <summary>
        /// Turn on Led
        /// </summary>
        /// <param name="Color">Led Color</param>
        /// <returns>bool Result</returns>
        [DllImport(B101DLL_Path, EntryPoint = "LedSetOn")]
        private static extern bool LedSetOnEx(uint Color);

        //GSLB101API_API void LedSetOff(); 
        /// <summary>
        /// Turn off Led
        /// </summary>
        [DllImport(B101DLL_Path, CharSet = CharSet.Auto)]
        public static extern void LedSetOff();



        //GSLB101API_API BOOL LedBlink(COLORREF colorRGB,WORD Period, WORD OnTime);
        /// <summary>
        /// Led Blinking
        /// </summary>
        /// <param name="colorRGB">Led Color</param>
        /// <param name="period">Period</param>
        /// <param name="onTime">On Time Period</param>
        /// <returns>bool Result</returns>
        public static bool LedSetBlink(ColorRef colorRGB, short period, short onTime)
        {
            return LedSetBlinkEx((uint)(colorRGB.bBlue << 16 | colorRGB.bGreen << 8 | colorRGB.bRed), period, onTime);
        }

        [DllImport(B101DLL_Path, EntryPoint = "LedBlink")]
        private static extern bool LedSetBlinkEx(uint colorRGB, short Period, short OnTime);
        //Buzzer Sound
        //GSLB101API_API void ToneOn(WORD freq,WORD Duration,BUZZER_SOUND SoundLevel);
        /// <summary>
        /// Turn on buzzer with custom frequency ,duration and volume
        /// </summary>
        /// <param name="freq">frequency range from 1kHz to 3kHz</param>
        /// <param name="Duration">duration in millisecond</param>
        /// <param name="SoundLevel">sound level</param>
        [DllImport(B101DLL_Path, EntryPoint = "ToneOn", CharSet = CharSet.Auto)]
        public static extern void BuzzerOn(short freq, short Duration, BuzzerVolume SoundLevel);

        //GSLB101API_API void ToneOff();
        /// <summary>
        /// Turn off buzzer
        /// </summary>
        [DllImport(B101DLL_Path, EntryPoint = "ToneOff", CharSet = CharSet.Auto)]
        public static extern void BuzzerOff();

        //GSLB101API_API void MelodyPlay(int ToneID,WORD Duration,BUZZER_SOUND SoundLevel);
        /// <summary>
        /// Play System default Melody
        /// </summary>
        /// <param name="ToneID">Melody ID</param>
        /// <param name="Duration">duration</param>
        /// <param name="SoundLevel">sound level</param>
        [DllImport(B101DLL_Path, CharSet = CharSet.Auto)]
        private static extern void MelodyPlay(RingTone ToneID, short Duration, BuzzerVolume SoundLevel);


        //GSLB101API_API void MelodyStop();
        /// <summary>
        /// Stop Melody
        /// </summary>
        [DllImport(B101DLL_Path, CharSet = CharSet.Auto)]
        public static extern void MelodyStop();

        
        static int lastRingTick = 0;
        static RingTone lastToneID = RingTone.UNKNOWN;

        /// <summary>
        /// Play Melody
        /// </summary>
        /// <param name="ToneID">see enum RingTone</param>
        /// <param name="SoundLevel">see enum BUZZER_SOUND</param>
        public static void MelodyPlay(RingTone ToneID, BuzzerVolume SoundLevel)
        {
            // ring
            int thisTick = Environment.TickCount;
            // minimum time between consecutive play
            if (ToneID != lastToneID)
            {
                MelodyStop();
                lastRingTick = 0; // so that it won't be limited by closeness from last Melody Play
            }
            if ((thisTick - lastRingTick) > 1000)
            {
                lastRingTick = thisTick;
                MelodyStop();
                MelodyPlay(ToneID, 1, SoundLevel);
                lastToneID = ToneID;
            }
        }
        /*
        class SoundClass
        {
            public RingTone tone = RingTone.UNKNOWN;
            public BUZZER_SOUND vol = BUZZER_SOUND.MIDDLE;
            public SoundClass(RingTone tone, BUZZER_SOUND vol)
            {
                this.tone = tone;
                this.vol = vol;
            }
        }
        //private delegate void MelodPlayDeleg(SoundClass sound);
        private static void MelodyPlayThread(object sound)
        {
            SoundClass snd = (SoundClass)sound;
            if (snd != null)
                MelodyPlay(snd.tone, 1, snd.vol);
        }*/



        
        //WiFi
        //GSLB101API_API void GetSerialNo(char *sn);
        [DllImport(B101DLL_Path, SetLastError = true)]
        private static extern void GetSerialNo
            (
                ref IntPtr sn
                //Byte * sn;
            );

        /// <summary>
        /// Get device serial number
        /// </summary>
        public static string GetSerialNumber()
        {
            //IntPtr snPtr = IntPtr.Zero;
            string err = "Get Serial Number failed.";
            string sn = String.Empty;
            //Byte[] snchar = new Byte[32];
            IntPtr snr = IntPtr.Zero;

            //fixed (Byte* snptr = snchar)
            {
                try
                {
                    //GetSerialNo(ref snr);

                    if (snr != IntPtr.Zero)
                    {
//                        for (int ofs = 0; ofs < snchar.Length && snptr[ofs] != 0; ofs++)
                        {

//                            sn += (char)snptr[ofs];
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    //CSLibrary.Diagnostics.CoreDebug.Logger.ErrorException("HighLevelInterface.GetSerialNumber()", ex);
                    return err;
                }
            }
            return sn;
        }
        /// <summary>
        /// PseudoSleepTimerReset
        /// </summary>
        [DllImport(B101DLL_Path, SetLastError = true)]
        public static extern void PseudoSleepTimerReset();
        /// <summary>
        /// PseudoSleepUserAttention
        /// </summary>
        [DllImport(B101DLL_Path, SetLastError = true)]
        public static extern void PseudoSleepUserAttention();

        //WiFi
        /// <summary>
        /// Get current Wifi state
        /// </summary>
        /// <returns></returns>
        [DllImport(B101DLL_Path, EntryPoint = "GetWiFiState", SetLastError = true)]
        public static extern WifiState WiFi_GetState();
        /// <summary>
        /// Wifi force to scan
        /// </summary>
        [DllImport(B101DLL_Path, EntryPoint = "ForceScan", SetLastError = true)]
        public static extern void WiFi_ForceScan();
        /// <summary>
        /// Turn on Wifi
        /// </summary>
        /// <returns></returns>
        [DllImport(B101DLL_Path, EntryPoint = "TurnOnWiFi", SetLastError = true)]
        public static extern bool WiFi_TurnOn();
        /// <summary>
        /// Turn off Wifi
        /// </summary>
        /// <returns></returns>
        [DllImport(B101DLL_Path, EntryPoint = "TurnOffWiFi", SetLastError = true)]
        public static extern bool WiFi_TurnOff();

        /// <summary>
        /// Set CPU Speed
        /// </summary>
        /// <param name="spd">Full speed or low speed</param>
        [DllImport(B101DLL_Path, SetLastError = true)]
        public static extern void SetCPUSpeed(Speed spd);
        /// <summary>
        /// Get current CPU Speed
        /// </summary>
        /// <returns>return enum <see cref="Speed"/> </returns>
        [DllImport(B101DLL_Path, SetLastError = true)]
        public static extern Speed GetCPUSpeed();

#if NO_USE_IN_NEW_CEOS


        //GSLB101API_API 	bool MSREncrypt(char* source,char* target);
        //Notify: if length of source = X, then length of target must >= ((X+12)/10+1)*10*2 
        /// <summary>
        /// MSREncrypt
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        [DllImport(B101DLL_Path, CharSet = CharSet.Auto)]
        public static extern bool MSREncrypt(string source, ref string target);
        
        //GSLB101API_API 	bool MSRDecrypt(char* source,char* target);
        //Notify: if length of source = X, then length of target should >= X/2-12 
        /// <summary>
        /// MSRDecrypt
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        [DllImport(B101DLL_Path, CharSet = CharSet.Auto)]
        public static extern bool MSRDecrypt(string source, ref string target);
#endif

// by mephist
#if !NOUSE
        [DllImport("coredll.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
        private static extern int DeviceIoControlCE(
            HANDLE hDevice,
            int dwIoControlCode,
            byte[] lpInBuffer,
            int nInBufferSize,
            byte[] lpOutBuffer,
            int nOutBufferSize,
            ref int lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("coredll", SetLastError = true)]
        private static extern HANDLE CreateFile(
            String lpFileName,
            UInt32 dwDesiredAccess,
            UInt32 dwShareMode,
            IntPtr lpSecurityAttributes,
            UInt32 dwCreationDisposition,
            UInt32 dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("coredll.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(HANDLE hObject);
#endif
        #endregion

        #region Public Function
// by mephist
#if !NO_USE_DO_IT_LOW_LEVEL
        /// <summary>
        /// Power on rfid device if device already on, it will power off and power on again
        /// with 1 sec Delay
        /// </summary>
        /// <returns></returns>
        public static bool RFID_TurnOn()
        {
            byte readValue = 0x0;
            bool ret = GpioRead(RadioPwrPin, ref readValue);
            if (!ret)
                return ret;
            if (readValue == 0x0)
            {
                ret = GpioWrite(RadioPwrPin, 1); //Power Off First
                if (!ret)
                    return ret;
                //ret = GpioRead(RadioPwrPin, ref readValue);
            }

            ret = GpioWrite(RadioPwrPin, 0); // set GPIO3 to Logic-L
            System.Threading.Thread.Sleep(2000);
            return ret;
        }
        /// <summary>
        /// Power off rfid device
        /// </summary>
        /// <returns></returns>
        public static bool RFID_TurnOff()
        {
            // Turn off Radio-related GPIO
            bool ret = GpioWrite(RadioPwrPin, 1); // set GPIO3 to Logic-H
            System.Threading.Thread.Sleep(20);
            return ret;
        }
        /// <summary>
        /// Power on Bacode scanner
        /// </summary>
        /// <returns></returns>
        public static bool BarcodePowerOn()
        {

            
            
            
            
            
            
//            GpioWrite(0, 1);   // nWake
//            GpioWrite(1, 0);  // nTr
//            System.Threading.Thread.Sleep(1000);

//            GpioWrite(0, 0);
//            System.Threading.Thread.Sleep (100);
//            GpioWrite(1, 1);
            
//            GpioWrite(2, 0);
//            GpioWrite(4, 0);
            
/*
 * //if (GpioWrite(ScnrTriggerPin, 0)) // set GPIO1 to Active-L
            if (GpioWrite(0, 0)) // set GPIO1 to Active-L
            {
                //Datalog.LogStr(Thread.CurrentThread.ManagedThreadId + " ++++ Laser On +++++");
                //Thread.Sleep(10); // prevent Off-On sequence too close apart, render useless
                // Use busy loop instead of Sleep to avoid thread-switch (Serial Port)
                for (int i = 0; i < 2000000; i++) ;
                return true;
            }
*/
            return false;
        }
        /// <summary>
        /// Power off barcode scanner
        /// </summary>
        /// <returns></returns>
        public static bool BarcodePowerOff()
        {
            if (GpioWrite(ScnrTriggerPin, 1)) // set GPIO1 to Active-L
            {
                //Datalog.LogStr(Thread.CurrentThread.ManagedThreadId + " ----- Laser Off -----");
                //Thread.Sleep(10); // prevent Off-On sequence too close apart, render useless
                // Use busy loop instead of Sleep to avoid thread-switch (Serial Port)
                for (int i = 0; i < 2000000; i++) ;
                return true;
            }
            return false;
        }
#endif
        #endregion

        #region Private Function
// by mephist
#if !NO_USE_DO_IT_LOW_LEVEL
   
        private static readonly int GPIO_BASE_IOCODE = 8164;
        private static readonly int GPIO00_CTL_READ = (GPIO_BASE_IOCODE);
        private static readonly int GPIO01_CTL_READ = (GPIO00_CTL_READ + 1);
        private static readonly int GPIO02_CTL_READ = (GPIO01_CTL_READ + 1);

        private static readonly int GPIO00_CTL_WRITE = (GPIO02_CTL_READ + 1);
        private static readonly int GPIO01_CTL_WRITE = (GPIO00_CTL_WRITE + 1);
        private static readonly int GPIO02_CTL_WRITE = (GPIO01_CTL_WRITE + 1);

        private static readonly int GPIO00_CTL_SET_IO = (GPIO02_CTL_WRITE + 1);
        private static readonly int GPIO01_CTL_SET_IO = (GPIO00_CTL_SET_IO + 1);
        private static readonly int GPIO02_CTL_SET_IO = (GPIO01_CTL_SET_IO + 1);

        private static readonly int GPIO00_CTL_GET_IO = (GPIO02_CTL_SET_IO + 1);
        private static readonly int GPIO01_CTL_GET_IO = (GPIO00_CTL_GET_IO + 1);
        private static readonly int GPIO02_CTL_GET_IO = (GPIO01_CTL_GET_IO + 1);

        private static readonly int GPIO03_CTL_READ = (RFID_CTL_READ + 1);
        private static readonly int GPIO03_CTL_WRITE = (GPIO03_CTL_READ + 1);
        private static readonly int GPIO03_CTL_GET_IO = (GPIO03_CTL_WRITE + 1);
        private static readonly int GPIO03_CTL_SET_IO = (GPIO03_CTL_GET_IO + 1);

        private static readonly int RFID_CTL_WRITE = (GPIO02_CTL_GET_IO + 1);
        private static readonly int RFID_CTL_READ = (RFID_CTL_WRITE + 1);

        private static readonly uint FILE_SHARE_READ = 0x00000001;
        private static readonly uint FILE_SHARE_WRITE = 0x00000002;

        private static readonly uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private static readonly uint FILE_FLAG_OVERLAPPED = 0x40000000;
        private static readonly uint GENERIC_READ = 0x80000000;
        private static readonly uint GENERIC_WRITE = 0x40000000;
        //private static readonly uint CREATE_NEW = 1;
        //private static readonly uint CREATE_ALWAYS = 2;
        private static readonly uint OPEN_EXISTING = 3;

        private static bool GpioWrite(int iGpio, byte iState)
        {
            // Write a state to Gpio 0--3    
            int bRet;
            int lpBytesReturned = 0;
            byte[] inBuf = new byte[] { iState };
            HANDLE FileHandle = CreateFile("GIO1:",                          //Object name.
                                        GENERIC_READ | GENERIC_WRITE,               //Desired access.
                                        FILE_SHARE_READ | FILE_SHARE_WRITE,         //Share Mode.
                                        IntPtr.Zero,                                       //Security Attr
                                        OPEN_EXISTING,                              //Creation Disposition.
                                        FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, //Flag and Attributes.
                                        IntPtr.Zero);
            if (FileHandle == HANDLE.Zero)
            {
                return false;
            }
            switch (iGpio)
            {
                case 0:
                    bRet = DeviceIoControlCE(FileHandle, GPIO00_CTL_WRITE, inBuf, 1, null, 0, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 1:
                    bRet = DeviceIoControlCE(FileHandle, GPIO01_CTL_WRITE, inBuf, 1, null, 0, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 2:
                    bRet = DeviceIoControlCE(FileHandle, GPIO02_CTL_WRITE, inBuf, 1, null, 0, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 3:
                    bRet = DeviceIoControlCE(FileHandle, RFID_CTL_WRITE, inBuf, 1, null, 0, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 4:
                    bRet = DeviceIoControlCE(FileHandle, GPIO03_CTL_WRITE, inBuf, 1, null, 0, ref lpBytesReturned, IntPtr.Zero);
                    break;
                default:
                    bRet = -1;
                    break;
            }
            if (FileHandle != HANDLE.Zero)
            {
                bRet = CloseHandle(FileHandle) ? 1 : 0;
                FileHandle = HANDLE.Zero;
            }
            return bRet == 1 ? true : false;
        }

        private static bool GpioRead(int iGpio, ref byte piState)
        {
            int bRet;
            int lpBytesReturned = 0;
            byte[] outBuf = new byte[1];
            HANDLE FileHandle = CreateFile("GIO1:",                          //Object name.
                            GENERIC_READ | GENERIC_WRITE,               //Desired access.
                            FILE_SHARE_READ | FILE_SHARE_WRITE,         //Share Mode.
                            IntPtr.Zero,                                       //Security Attr
                            OPEN_EXISTING,                              //Creation Disposition.
                            FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, //Flag and Attributes.
                            IntPtr.Zero);
            if (FileHandle == HANDLE.Zero)
            {
                return false;
            }
            switch (iGpio)
            {
                case 0:
                    bRet = DeviceIoControlCE(FileHandle, GPIO00_CTL_READ, null, 0, outBuf, 1, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 1:
                    bRet = DeviceIoControlCE(FileHandle, GPIO01_CTL_READ, null, 0, outBuf, 1, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 2:
                    bRet = DeviceIoControlCE(FileHandle, GPIO02_CTL_READ, null, 0, outBuf, 1, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 3:
                    bRet = DeviceIoControlCE(FileHandle, RFID_CTL_READ, null, 0, outBuf, 1, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 4:
                    bRet = DeviceIoControlCE(FileHandle, GPIO03_CTL_READ, null, 0, outBuf, 1, ref lpBytesReturned, IntPtr.Zero);
                    break;
                default:
                    bRet = -1;
                    break;
            };
            if (FileHandle != HANDLE.Zero)
            {
                bRet = CloseHandle(FileHandle) ? 1 : 0;
                FileHandle = HANDLE.Zero;
            }
            piState = outBuf[0];
            return bRet == 1 ? true : false;
        }

        private static bool GpioSetIo(int iGpio)
        {
            // Set the IO of Gpio 0-4    
            int bRet;
            Byte[] inBuf = new Byte[] { 0x1 };
            int lpBytesReturned = 0;

            HANDLE FileHandle = CreateFile("GIO1:",                          //Object name.
                GENERIC_READ | GENERIC_WRITE,               //Desired access.
                FILE_SHARE_READ | FILE_SHARE_WRITE,         //Share Mode.
                IntPtr.Zero,                                       //Security Attr
                OPEN_EXISTING,                              //Creation Disposition.
                FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, //Flag and Attributes.
                IntPtr.Zero);
            if (FileHandle == HANDLE.Zero)
            {
                return false;
            }

            switch (iGpio)
            {
                case 0:
                    bRet = DeviceIoControlCE(FileHandle, GPIO00_CTL_SET_IO, inBuf, 1, null, 0, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 1:
                    bRet = DeviceIoControlCE(FileHandle, GPIO01_CTL_SET_IO, inBuf, 1, null, 0, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 2:
                    bRet = DeviceIoControlCE(FileHandle, GPIO02_CTL_SET_IO, inBuf, 1, null, 0, ref lpBytesReturned, IntPtr.Zero);
                    break;
                case 4:
                    bRet = DeviceIoControlCE(FileHandle, GPIO03_CTL_SET_IO, inBuf, 1, null, 0, ref lpBytesReturned, IntPtr.Zero);
                    break;
                default:
                    bRet = -1;
                    break;
            }
            if (FileHandle != HANDLE.Zero)
            {
                bRet = CloseHandle(FileHandle) ? 1 : 0;
                FileHandle = HANDLE.Zero;
            }
            return bRet == 1 ? true : false;
        }

     private static bool GpioIni()
        { /// Open the filehandle
            int bRet;
            m_GpioFileHandle = CreateFile("GIO1:",                          //Object name.
                                    GENERIC_READ | GENERIC_WRITE,               //Desired access.
                                    FILE_SHARE_READ | FILE_SHARE_WRITE,         //Share Mode.
                                    IntPtr.Zero,                                       //Security Attr
                                    OPEN_EXISTING,                              //Creation Disposition.
                                    FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, //Flag and Attributes.
                                    IntPtr.Zero);
            if (m_GpioFileHandle == HANDLE.Zero)
            {
                bRet = -1; /// pFileHdl = NULL;
            }
            else
            {
                bRet = 0;  /// pFileHdl = m_GpioFileHandle;
            };
            return bRet == 0 ? true : false;
        }

        private static bool GpioUnini()
        { /// Close the filehandle
            bool bRet = true;
            if (m_GpioFileHandle != HANDLE.Zero)
            {

            }
            else
            {
                bRet = CloseHandle(m_GpioFileHandle);
                m_GpioFileHandle = HANDLE.Zero;
            };
            return bRet;
        }
#endif

        #endregion
    }
}
#endif