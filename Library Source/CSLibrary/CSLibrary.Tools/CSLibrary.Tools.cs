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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using Microsoft.Win32;
#if CS101 && WindowsCE
using CSLibrary.Net;
#endif
namespace CSLibrary.Tools
{
    #region UID
    /// <summary>
    /// Autogen a guid
    /// </summary>
    public class GUID
    {
        /// <summary>
        /// 12 bit guid
        /// </summary>
        /// <returns></returns>
        public static byte[] Gen12BitUID()
        {
            return CSLibrary.Text.Hex.ToBytes(Guid.NewGuid().ToString().Replace("-", "").Substring(0, 3));
        }
        /// <summary>
        /// 24 bit guid
        /// </summary>
        /// <returns></returns>
        public static byte[] Gen24BitUID()
        {
            return CSLibrary.Text.Hex.ToBytes(Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6));
        }
        /// <summary>
        /// 48 bit guid
        /// </summary>
        /// <returns></returns>
        public static byte[] Gen48BitUID()
        {
            return CSLibrary.Text.Hex.ToBytes(Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12));
        }
        /// <summary>
        /// 96 bit guid
        /// </summary>
        /// <returns></returns>
        public static byte[] Gen96BitUID()
        {
            return CSLibrary.Text.Hex.ToBytes(Guid.NewGuid().ToString().Replace("-", "").Substring(0, 24));
        }
    }
    #endregion

    #region Sound
#if WIN32
    /// <summary>
    /// PC Sound
    /// </summary>
    public class Sound
    {
        [Flags]
        public enum SoundFlags : int
        {
            SND_SYNC = 0x0000,            // play synchronously (default)

            SND_ASYNC = 0x0001,        // play asynchronously

            SND_NODEFAULT = 0x0002,        // silence (!default) if sound not found

            SND_MEMORY = 0x0004,        // pszSound points to a memory file

            SND_LOOP = 0x0008,            // loop the sound until next sndPlaySound

            SND_NOSTOP = 0x0010,        // don't stop any currently playing sound

            SND_NOWAIT = 0x00002000,        // don't wait if the driver is busy

            SND_ALIAS = 0x00010000,        // name is a registry alias

            SND_ALIAS_ID = 0x00110000,        // alias is a predefined id

            SND_FILENAME = 0x00020000,        // name is file name

        }
        public enum MessageBeepFlags
        {
            MB_OK = 0x00000000,
            MB_OKCANCEL = 0x00000001,
            MB_ABORTRETRYIGNORE = 0x00000002,
            MB_YESNOCANCEL = 0x00000003,
            MB_YESNO = 0x00000004,
            MB_RETRYCANCEL = 0x00000005,
            MB_ICONHAND = 0x00000010,
            MB_ICONQUESTION = 0x00000020,
            MB_ICONEXCLAMATION = 0x00000030,
            MB_ICONASTERISK = 0x00000040,
        }
        [DllImport("winmm.dll", SetLastError = true)]
        public static extern bool PlaySound(IntPtr ptrToSound,
           System.UIntPtr hmod, SoundFlags fdwSound);

        /// <summary>
        /// Sound Beep
        /// </summary>
        /// <param name="frequency">Frequency</param>
        /// <param name="duration">Duration</param>
        /// <returns>BOOL</returns>
        [DllImport("Kernel32.dll")]
        public static extern bool Beep(UInt32 frequency, UInt32 duration);
        [DllImport("user32.dll")]
        public static extern bool MessageBeep(MessageBeepFlags uType);
    }
#endif
    #endregion

#if nouse

    #region High Performance Timer
    /// <summary>
    /// High Performance Counter
    /// </summary>
    public class Counter
    {
#if WindowsCE
        [DllImport("coredll.dll", SetLastError = true)]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
#else
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceCounter(ref long lPerformanceCounter);
#endif
#if WindowsCE
        [DllImport("coredll.dll", SetLastError = true)]
#else
        [DllImport("Kernel32.dll", SetLastError = true)]
#endif
        private static extern bool QueryPerformanceFrequency(out long frequency);

        private long startTime, stopTime;
        private long freq;

        /// <summary>
        /// Constructor
        /// </summary>
        public Counter()
        {
            startTime = 0;
            stopTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported

                throw new Exception("High Resolution Not Support.");
            }
        }

        /// <summary>
        /// Start Timer
        /// </summary>
        public void Start()
        {
            // lets do the waiting threads there work

            Thread.Sleep(0);
#if WindowsCE
            QueryPerformanceCounter(out startTime);
#else
            QueryPerformanceCounter(ref startTime);
#endif
        }

        /// <summary>
        /// Stop Timer
        /// </summary>
        public void Stop()
        {
#if WindowsCE
            QueryPerformanceCounter(out stopTime);
#else
            QueryPerformanceCounter(ref stopTime);
#endif
        }

        /// <summary>
        /// Returns the duration of the timer (in seconds)
        /// </summary>
        public double Duration
        {
            get
            {
                return (double)(stopTime - startTime) / (double)freq;
            }
        }
    }
    #endregion
#endif

#if NOUSE
    #region Logging
    /// <summary>
    /// Log file
    /// </summary>
    public class Logging
    {
        private DateTime CurrentDateTime = DateTime.MinValue;

        private int timeout = 0;
        private string message;
        private string path;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Timeout">In Minutes</param>
        /// <param name="msg">Message you want to print out</param>
        /// <param name="Path">File Path to Save make sure you have enough space left</param>
        public Logging(int Timeout, string msg, string Path)
        {
            timeout = Timeout;
            message = msg;
            path = Path;
            CurrentDateTime = DateTime.Now;
        }
        /// <summary>
        /// 
        /// </summary>
        public void StartLogging()
        {
            try
            {
                TimeSpan Diff = DateTime.Now.Subtract(CurrentDateTime);
                if (Diff.Minutes >= timeout)
                {
                    SaveLogTo(path, message);
                    CurrentDateTime = DateTime.Now;
                }
            }
            catch (System.Exception e)
            {
                throw new Exception.ReaderException(e.Message);
            }
        }

        private void SaveLogTo(String Path, String Message)
        {
            try
            {
                using (FileStream FileIO = new FileStream(Path, FileMode.Append))
                using (StreamWriter sw = new StreamWriter(FileIO))
                {
                    sw.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") + Message + "\r\n");
                    sw.Flush();
                    FileIO.Flush();
                }
            }
            catch (System.Exception e)
            {
                throw new Exception.ReaderException(e.Message);
            }
        }
    }

    #endregion
#endif
#if CS101 && WindowsCE
    #region Get Mac Address
    /// <summary>
    /// Wifi class
    /// </summary>
    public class Wifi
    {
        /// <summary>
        /// Get MacAdress of Wifi adaptor
        /// </summary>
        /// <returns></returns>
        public static string GetMacAddress()
        {
            List<IP_ADAPTER_ADDRESSES_XP2K3> adaptersAddressesCollection;
            IpHlpNetworkAdapterUtil adapterUtils = new IpHlpNetworkAdapterUtil();

            adapterUtils.GetAdaptersAddresses(AddressFamily.Unspecified, GAA_FLAGS.GAA_FLAG_DEFAULT, out adaptersAddressesCollection);

            Debug.Assert(adaptersAddressesCollection.Count > 0, "IP_ADAPTER_ADDRESSES_XP2K3 collection is empty.");
            Debug.WriteLine("\nNumber of network adapters found: " + adaptersAddressesCollection.Count);

            foreach (IP_ADAPTER_ADDRESSES_XP2K3 adapterAddressesBuffer in adaptersAddressesCollection)
            {
                string adapterName = Marshal.PtrToStringUni(adapterAddressesBuffer.AdapterName); //PtrToStringAnsi
                string FriendlyName = Marshal.PtrToStringUni(adapterAddressesBuffer.FriendlyName);//PtrToStringAuto
                string description = Marshal.PtrToStringUni(adapterAddressesBuffer.Description); //PtrToStringAuto
                string dnssuffix = Marshal.PtrToStringUni(adapterAddressesBuffer.DnsSuffix);   //PtrToStringAuto
                IF_TYPE iftype = (IF_TYPE)adapterAddressesBuffer.IfType;
                OPERSTATUS operationstatus = (OPERSTATUS)adapterAddressesBuffer.OperStatus;

                string tmpString = string.Empty;

                for (int i = 0; i < 6; i++)
                {
                    tmpString += string.Format("{0:X2}", adapterAddressesBuffer.PhysicalAddress[i]);

                    if (i < 5)
                    {
                        tmpString += ":";
                    }
                }
                return tmpString;
            }
            return "";
        }
    }
    #endregion
#endif
#if NOUSE
    /// <summary>
    /// Summary description for Settings.
    /// </summary>
    public class ReaderSettings
    {
        private static NameValueCollection m_settings;
        private static string m_settingsPath;

        static ReaderSettings()
        {
            m_settingsPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            m_settingsPath += @"\ReaderSettings.xml";

            if (!File.Exists(m_settingsPath))
                throw new FileNotFoundException(m_settingsPath + " could not be found.");

            System.Xml.XmlDocument xdoc = new XmlDocument();
            xdoc.Load(m_settingsPath);
            XmlElement root = xdoc.DocumentElement;
            System.Xml.XmlNodeList nodeList = root.ChildNodes.Item(0).ChildNodes;

            // Add settings to the NameValueCollection.
            m_settings = new NameValueCollection();
            m_settings.Add("ServerIP", nodeList.Item(0).Attributes["value"].Value);
            m_settings.Add("UserName", nodeList.Item(1).Attributes["value"].Value);
            m_settings.Add("Password", nodeList.Item(2).Attributes["value"].Value);
            m_settings.Add("PhoneNumber", nodeList.Item(3).Attributes["value"].Value);
            m_settings.Add("TimeOut", nodeList.Item(4).Attributes["value"].Value);
            m_settings.Add("LastTransmit", nodeList.Item(5).Attributes["value"].Value);
            m_settings.Add("DatabasePath", nodeList.Item(6).Attributes["value"].Value);
        }

        public static void Update()
        {
            XmlTextWriter tw = new XmlTextWriter(m_settingsPath, System.Text.UTF8Encoding.UTF8);
            tw.WriteStartDocument();
            tw.WriteStartElement("configuration");
            tw.WriteStartElement("appSettings");

            for (int i = 0; i < m_settings.Count; ++i)
            {
                tw.WriteStartElement("add");
                tw.WriteStartAttribute("key", string.Empty);
                tw.WriteRaw(m_settings.GetKey(i));
                tw.WriteEndAttribute();

                tw.WriteStartAttribute("value", string.Empty);
                tw.WriteRaw(m_settings.Get(i));
                tw.WriteEndAttribute();
                tw.WriteEndElement();
            }

            tw.WriteEndElement();
            tw.WriteEndElement();

            tw.Close();
        }

        public static string ServerIP
        {
            get { return m_settings.Get("ServerIP"); }
            set { m_settings.Set("ServerIP", value); }
        }

        public static string UserName
        {
            get { return m_settings.Get("UserName"); }
            set { m_settings.Set("UserName", value); }
        }

        public static string Password
        {
            get { return m_settings.Get("Password"); }
            set { m_settings.Set("Password", value); }
        }

        public static string PhoneNumber
        {
            get { return m_settings.Get("PhoneNumber"); }
            set { m_settings.Set("PhoneNumber", value); }
        }

        public static string TimeOut
        {
            get { return m_settings.Get("TimeOut"); }
            set { m_settings.Set("TimeOut", value); }
        }

        public static string LastTransmit
        {
            get { return m_settings.Get("LastTransmit"); }
            set { m_settings.Set("LastTransmit", value); }
        }

        public static string DatabasePath
        {
            get { return m_settings.Get("DatabasePath"); }
            set { m_settings.Set("DatabasePath", value); }
        }
    }

    public class Time
    {
        [DllImport("coredll.dll", SetLastError = true)]
        static extern bool SetSystemTime(ref SYSTEMTIME time);

        public struct SYSTEMTIME
        {
            public short year;
            public short month;
            public short dayOfWeek;
            public short day;
            public short hour;
            public short minute;
            public short second;
            public short milliseconds;
        }
        public static bool SetSysTime(ref SYSTEMTIME time)
        {
            return SetSystemTime(ref time);
        }
    }
#endif

#if WindowsCE
#if NOUSE
    /// <summary>
    /// Windows CE
    /// </summary>
    public class WCE
    {
        /// <summary>
        /// Disable SuspendMode in WindowsCE
        /// </summary>
        /// <param name="Enable"></param>
        public static void SuspendMode(bool Enable)
        {
            //System\\CurrentControlSet\\Control\\Power\\Timeouts
            using (Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Control\\Power\\Timeouts", true))
            {

            }
        }
        // HKEY_LOCAL_MACHINE\Drivers\USB\ClientDrivers\RFID_Transport\Prefix = RFT 
        // HKEY_LOCAL_MACHINE\Drivers\USB\ClientDrivers\RFID_Transport\DLL = usbDrvFileName
        // HKEY_LOCAL_MACHINE\Drivers\USB\LoadClients\32902_65261\Default\Default\RFID_Transport\DLL = usbDrvFileName
        /// <summary>
        /// Set UsbHarv for WindowsCE
        /// </summary>
        /// <param name="UsbName">UsbHarv driver name</param>
        /// <returns>Success if return true</returns>
        public static bool SetUsbHarvRegistry(string UsbName)
        {
            try
            {
                using (RegistryKey Key = Registry.LocalMachine.CreateSubKey("Drivers").CreateSubKey("USB").CreateSubKey("ClientDrivers").CreateSubKey("RFID_Transport"))
                {
                    if ((string)Key.GetValue("Prefix") != "RFT")
                    {
                        Key.SetValue("Prefix", "RFT");
                    }
                    if ((string)Key.GetValue("DLL") != UsbName)
                    {
                        Key.SetValue("DLL", UsbName);
                    }
                }
                using (RegistryKey Key = Registry.LocalMachine.CreateSubKey("Drivers").CreateSubKey("USB").CreateSubKey("LoadClients").CreateSubKey("32902_65261").CreateSubKey("Default").CreateSubKey("Default").CreateSubKey("RFID_Transport"))
                {
                    if ((string)Key.GetValue("DLL") != UsbName)
                    {
                        Key.SetValue("DLL", UsbName);
                    }
                }
            }
            catch(System.Exception ex)
            {
                CSLibrary.SysLogger.LogError(ex);
                return false;
            }
            return true;
        }
    }
#endif
    /// <summary>
    /// DateTimeEx class with high performance
    /// </summary>
    public class DateTimeEx
    {
        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        [DllImport("coredll.dll")]
        private static extern bool SetLocalTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("coredll.dll", EntryPoint = "GetLocalTime", SetLastError = true)]
        private static extern void GetLocalTime(out SYSTEMTIME st);
        /// <summary>
        /// New DateTime Class, using this instead of original DateTime class
        /// It directlt call GetLocalTime
        /// </summary>
        public static DateTime Now
        {
            get
            {
                SYSTEMTIME time;
                // Get local time
                GetLocalTime(out time);
                // Convert to DateTime
                DateTime now = new DateTime(time.wYear, time.wMonth,
                time.wDay, time.wHour, time.wMinute, time.wSecond, time.wMilliseconds);

                return now;
            }
            set
            {
                SYSTEMTIME time = new SYSTEMTIME();
                time.wYear = (ushort)value.Year;
                time.wMonth = (ushort)value.Month;
                time.wDay = (ushort)value.Day;
                time.wHour = (ushort)value.Hour;
                time.wMinute = (ushort)value.Minute;
                time.wSecond = (ushort)value.Second;
                if (!SetLocalTime(ref time))
                    throw new ApplicationException("Set Local Time Error");
            }
        }
    }
#endif
}
