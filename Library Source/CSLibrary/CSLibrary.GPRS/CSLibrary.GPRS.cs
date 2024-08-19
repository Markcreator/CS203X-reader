#if CS101
using System;
//using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace CSLibrary.GPRS
{
    /// <summary>
    /// cs501 device
    /// </summary>
    public static class Device
    {
        internal const String cs501_dll = "cs501";

        /// <summary>
        /// cs501 status
        /// </summary>
        public enum Status : int
        {
            /// <summary>
            /// Unknown
            /// </summary>
            Unknown,
            /// <summary>
            /// Not Present
            /// </summary>
            NotPresent,
            /// <summary>
            /// Power Off
            /// </summary>
            PowerOff,
            /// <summary>
            /// In Use
            /// </summary>
            InUse,
            /// <summary>
            /// Ready
            /// </summary>
            Ready,
            /// <summary>
            /// Error
            /// </summary>
            Error,
            /// <summary>
            /// Bad Device
            /// </summary>
            BadDevice,
            /*/// <summary>
            /// Not Handled Status
            /// </summary>
            NotHandledStatus,*/
        }

        /// <summary>
        /// battery status
        /// </summary>
        public enum BatteryStatus : int
        {
            /// <summary>
            /// Unknown
            /// </summary>
            Unknown = -1,
            /// <summary>
            /// Battery
            /// </summary>
            Battery = 0,
            /// <summary>
            /// Charging
            /// </summary>
            Charging = 1,
            /// <summary>
            /// No Battery
            /// </summary>
            NoBattery = 2,
            /// <summary>
            /// Power Fault
            /// </summary>
            PowerFault = 3,
        }

        /// <summary>
        /// network registration report
        /// </summary>
        public enum NetworkRegistrationReport : int
        {
            /// <summary>
            /// Not Register
            /// </summary>
            NotRegister = 0,
            /// <summary>
            /// Home
            /// </summary>
            Home = 1,
            /// <summary>
            /// Searching
            /// </summary>
            Searching = 2,
            /// <summary>
            /// Registration Denied
            /// </summary>
            RegistrationDenied = 3,
            /// <summary>
            /// Unknown
            /// </summary>
            Unknown = 4,
            /// <summary>
            /// Roaming
            /// </summary>
            Roaming = 5,
        }

        /// <summary>
        /// Function Call Result
        /// </summary>
        public enum Result
        {
            /// <summary>
            /// No Error
            /// </summary>
            OK = 0,
            /// <summary>
            /// General failure 
            /// </summary>
            FAILURE,
            /// <summary>
            /// Data Mode
            /// </summary>
            DATA_MODE,
            /// <summary>
            /// Device not ready to do operation
            /// </summary>
            NOT_READY,
            /// <summary>
            /// Library has not been successfully initialized
            /// </summary>
            NOT_INITIALIZED,
            /// <summary>
            /// One of the parameters to the function is invalid
            /// </summary>
            INVALID_PARAMETER,
            /*/// <summary>
            /// Status Change
            /// </summary>
            StatusChange,*/
        }

        //internal static Device m_context = new Device();

        private static bool m_init = false;
        private static Status m_status = Status.Unknown;
        internal static bool m_gps_on = true;
        internal static bool m_gprs_on = false;

        /*/// <summary>
        /// Get cs501 instance
        /// </summary>
        /// <returns>cs501 instance</returns>
        static public Device GetInstance ()
        {
            return m_context;
        }*/

        /*private Device()
        { }*/

        [DllImport(cs501_dll, SetLastError = true)]
        internal static extern bool cs501_Init();

        /// <summary>
        /// initialize cs501 device
        /// </summary>
        /// <returns>success/fail</returns>
        public static bool Init()
        {
            m_init = cs501_Init();
            if (!m_init)
            {
                int err = Marshal.GetLastWin32Error();
                m_status = Status.NotPresent;
                switch (err)
                {
                    case 2: //ERROR_FILE_NOT_FOUND
                        m_status = Status.Unknown;
                        break;
                    case 5: //ERROR_ACCESS_DENIED
                        m_status = Status.InUse;
                        break;
                    case 15: //ERROR_INVALID_DRIVE
                        m_status = Status.PowerOff;
                        break;
                    case 1200: //ERROR_BAD_DEVICE
                        m_status = Status.BadDevice;
                        break;
                    /*default:
                        m_status = Status.NotHandledStatus;
                        break;*/
                }
            }
            else
            {
                m_status = Status.Ready;
            }
            m_gps_on = true;
            GPS.cs501_GPS_TurnOn(true);
            m_gprs_on = false;
            return m_init;
        }

        [DllImport(cs501_dll, SetLastError = true)]
        internal static extern bool cs501_Uninit();

        /// <summary>
        /// uninitialize cs501 device
        /// </summary>
        /// <returns>success/fail</returns>
        public static bool Uninit ()
        {
            if (!m_init)
                return false;
            m_init = false;
            m_status = Status.Unknown;
            return cs501_Uninit();
        }

        internal static Result FalseToResult()
        {
            int err = Marshal.GetLastWin32Error();
            if (err == 170)    // ERROR_BUSY
                return Device.Result.DATA_MODE;
            return Device.Result.OK;
        }

        [DllImport(Device.cs501_dll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool cs501_GetLastErrorW(StringBuilder buf);

        /// <summary>
        /// get device gprs ip
        /// </summary>
        /// <param name="buf">Error Message</param>
        /// <returns>Result</returns>
        public static Device.Result GetLastError(out string buf)
        {
            buf = null;
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            StringBuilder sb = new StringBuilder(100);
            if (!cs501_GetLastErrorW(sb))
                return Device.FalseToResult();
            buf = sb.ToString();
            return Device.Result.OK;
        }

        /// <summary>
        /// Get Last CS501 Error
        /// </summary>
        public static string LastError
        {
            get
            {
                string buf;
                return GetLastError(out buf) == Result.OK ? buf : null;
            }
        }

        [DllImport(cs501_dll, SetLastError = true)]
        internal static extern bool cs501_SoftwareShutdown();

        /// <summary>
        /// shutdown cs501 by software command
        /// </summary>
        /// <returns>Result</returns>
        public static Device.Result SoftwareShutdown()
        {
            if (!m_init)
                return Device.Result.NOT_INITIALIZED;
            m_init = false;
            m_status = Status.PowerOff;
            if (!cs501_SoftwareShutdown())
                return FalseToResult();
            return Device.Result.OK;
        }

        /// <summary>
        /// current device status
        /// </summary>
        public static Status DeviceStatus
        {
            get { return m_status; }
        }

        [DllImport(cs501_dll, SetLastError = true)]
        internal static extern ushort cs501_DllVersion();

        /// <summary>
        /// Get Dll Version
        /// </summary>
        /// <returns>Version Number</returns>
        public static ushort DllVersion()
        {
            return cs501_DllVersion();
        }

        [DllImport(cs501_dll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool cs501_GetVersionW(StringBuilder ver);

        /// <summary>
        /// get device version
        /// </summary>
        /// <param name="ver">version</param>
        /// <returns>Result</returns>
        public static Device.Result GetVersion(out string ver)
        {
            ver = null;
            if (!m_init)
                return Device.Result.NOT_INITIALIZED;
            StringBuilder sb = new StringBuilder(100);
            if (!cs501_GetVersionW(sb))
                return FalseToResult();
            ver = sb.ToString();
            return Device.Result.OK;
        }

        [DllImport(cs501_dll, SetLastError = true)]
        internal static extern bool cs501_BatteryStatus(out BatteryStatus status, out int level);

        /// <summary>
        /// get battery status
        /// </summary>
        /// <param name="status">battery mode</param>
        /// <param name="level">battery level</param>
        /// <returns>Result</returns>
        public static Device.Result GetBatteryStatus(out BatteryStatus status, out int level)
        {
            status = BatteryStatus.Unknown;
            level = 0;
            if (!m_init)
                return Device.Result.NOT_INITIALIZED;
            if (!cs501_BatteryStatus(out status, out level))
                return FalseToResult();
            return Device.Result.OK;
        }

        [DllImport(cs501_dll, SetLastError = true)]
        internal static extern bool cs501_SimCardReady();

        /// <summary>
        /// check sim card status
        /// </summary>
        /// <returns>ready/not ready</returns>
        public static Device.Result SimCardReady()
        {
            if (!m_init)
                return Device.Result.NOT_INITIALIZED;
            if (!cs501_SimCardReady())
                return FalseToResult();
            return Device.Result.OK;
        }

        [DllImport(cs501_dll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool cs501_PinStatusW(StringBuilder status);

        /// <summary>
        /// reports the PIN/PUK/PUK2 request status
        /// </summary>
        /// <param name="status">PIN/PUK/PUK2 request status</param>
        /// <returns>Result</returns>
        public static Device.Result GetPinStatus(out string status)
        {
            status = null;
            if (!m_init)
                return Device.Result.NOT_INITIALIZED;
            StringBuilder sb = new StringBuilder(100);
            if (!cs501_PinStatusW(sb))
                return FalseToResult();
            status = sb.ToString();
            return Device.Result.OK;
        }

        [DllImport(cs501_dll, SetLastError = true)]
        internal static extern bool cs501_NetworkRegistrationReport(out NetworkRegistrationReport status);

        /// <summary>
        /// get network registration report
        /// </summary>
        /// <param name="status">network registration report</param>
        /// <returns>Result</returns>
        public static Device.Result GetNetworkRegistrationReport(out NetworkRegistrationReport status)
        {
            status = NetworkRegistrationReport.Unknown;
            if (!m_init)
                return Device.Result.NOT_INITIALIZED;
            if (!cs501_NetworkRegistrationReport(out status))
                return FalseToResult();
            return Device.Result.OK;
        }

        [DllImport(cs501_dll, SetLastError = true)]
        internal static extern bool cs501_GetSignal(out int strength);

        /// <summary>
        /// get signal strength
        /// </summary>
        /// <param name="strength">signal strength</param>
        /// <returns>Result</returns>
        public static Device.Result GetSignal(out int strength)
        {
            strength = 0;
            if (!m_init)
                return Device.Result.NOT_INITIALIZED;
            if (!cs501_GetSignal(out strength))
                return FalseToResult();
            return Device.Result.OK;
        }

        [DllImport(cs501_dll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool cs501_ServingCellInformationW(StringBuilder info);

        /// <summary>
        /// get Serving Cell Information
        /// </summary>
        /// <param name="info">Serving Cell Information</param>
        /// <returns>Result</returns>
        /// <remarks>
        /// &lt;B-ARFCN&gt;,&lt;dBM&gt;,&lt;NetNameAsc&gt;,&lt;NetCode&gt;,&lt;BSIC&gt;,&lt;LAC&gt;,&lt;TA&gt;,&lt;GPRS&gt;
        /// where:
        /// &lt;B-ARFCN&gt; - BCCH ARFCN of the serving cell
        /// &lt;dBM&gt; - received signal strength in dBm
        /// &lt;NetNameAsc&gt; - operator name, quoted string type
        /// &lt;NetCode&gt; - country code and operator code, hexadecimal representation
        /// &lt;BSIC&gt; - Base Station Identification Code
        /// &lt;LAC&gt; - Localization Area Code
        /// &lt;TA&gt; - Time Advance: it’s available only if a GSM or GPRS is running
        /// &lt;GPRS&gt; - GPRS supported in the cell
        /// 0 - not supported
        /// 1 - supported
        /// </remarks>
        public static Device.Result GetServingCellInformation(out string info)
        {
            info = null;
            if (!m_init)
                return Device.Result.NOT_INITIALIZED;
            StringBuilder sb = new StringBuilder(100);
            if (!cs501_ServingCellInformationW(sb))
                return FalseToResult();
            info = sb.ToString();
            return Device.Result.OK;
        }
    }

    /// <summary>
    /// GSM Module
    /// </summary>
    public static class GSM
    {
        /// <summary>
        /// Band
        /// </summary>
        public enum Band : int
        {
            /// <summary>
            /// GSM 900MHz + DCS 1800MHz
            /// </summary>
            GSM_900MHz_DCS_1800MHz = 0,
            /// <summary>
            /// GSM 900MHz + PCS 1900MHz
            /// </summary>
            GSM_900MHz_PCS_1900MHz = 1,
            /// <summary>
            /// GSM 850MHz + DCS 1800MHz (available only on quadri-band modules)
            /// </summary>
            GSM_850MHz_DCS_1800MHz = 2,
            /// <summary>
            /// GSM 850MHz + PCS 1900MHz (available only on quadri-band modules)
            /// </summary>
            GSM_850MHz_PCS_1900MHz = 3,
        }

        /*private GSM()
        { }*/

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_GetCurrentBand(out Band band);

        /// <summary>
        /// Get current selected band
        /// </summary>
        /// <param name="band">Band</param>
        /// <returns>Result</returns>
        public static Device.Result GetCurrentBand(out Band band)
        {
            band = Band.GSM_900MHz_DCS_1800MHz;
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_GetCurrentBand(out band))
                return Device.FalseToResult();
            return Device.Result.OK;
        }

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_SelectBand(Band band);

        /// <summary>
        /// Selects the current band
        /// </summary>
        /// <param name="band"></param>
        /// <returns>Result</returns>
        public static Device.Result SetCurrentBand(Band band)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_SelectBand(band))
                return Device.FalseToResult();
            return Device.Result.OK;
        }

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_AutomaticBandSelection(bool on);

        /// <summary>
        /// Automatic Band Selection at power-on
        /// </summary>
        /// <param name="on">Enable/Disable</param>
        /// <returns>Result</returns>
        public static Device.Result AutomaticBandSelection(bool on)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_AutomaticBandSelection(on))
                return Device.FalseToResult();
            return Device.Result.OK;
        }
    }

    /// <summary>
    /// cs501 dialer
    /// </summary>
    public static class Dialer
    {
        /*private Dialer()
        { }*/

        [DllImport(Device.cs501_dll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool cs501_DialW(string str);

        /// <summary>
        /// Dial a voice call
        /// </summary>
        /// <param name="str">Dial string</param>
        /// <returns>Result</returns>
        public static Device.Result Dial(string str)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_DialW(str))
                return Device.FalseToResult();
            return Device.Result.OK;
        }

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_HangUp();

        /// <summary>
        /// Disconnect a voice call
        /// </summary>
        /// <returns>Result</returns>
        public static Device.Result HangUp()
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_HangUp())
                return Device.FalseToResult();
            return Device.Result.OK;
        }
    }

    /// <summary>
    /// cs501 GPS
    /// </summary>
    public static class GPS
    {
        /// <summary>
        /// GPS Position
        /// </summary>
        public class GPSPosition
        {
            /// <summary>
            /// UTC time
            /// </summary>
            public DateTime UTC = DateTime.MinValue;
            /// <summary>
            /// Latitude
            /// </summary>
            public GPSCoordinate latitude;
            /// <summary>
            /// Longitude
            /// </summary>
            public GPSCoordinate longitude;
            /// <summary>
            /// Horizontal Diluition of Precision
            /// </summary>
            public float hdop;
            /// <summary>
            ///  Altitude - mean-sea-level (geoid) in meters 
            /// </summary>
            public float altitude;
            /// <summary>
            /// Course over Ground
            /// </summary>
            public Coordinate cog;
            /// <summary>
            /// Speed over ground (Km/hr)
            /// </summary>
            public float spkm;
            /// <summary>
            /// Speed over ground (knots)
            /// </summary>
            public float spkn;
            /// <summary>
            /// Total number of satellites in use
            /// </summary>
            public int nsat;

        }
        /// <summary>
        /// GPS Coordinate
        /// </summary>
        public struct GPSCoordinate
        {
            /// <summary>
            /// Degree
            /// </summary>
            public int degrees;
            /// <summary>
            /// Minute
            /// </summary>
            public float minutes;
            /// <summary>
            /// Direction
            /// </summary>
            public Direction direction;
            /// <summary>
            /// Convert to String Format
            /// </summary>
            /// <returns></returns>
            public override string  ToString()
            {
                //ddmm.mmmm N/S or dddmm.mmmm E/W 
                return string.Format("{0:D2}{1:D2}.{2:D4},", degrees, minutes, (char)direction);
            }
        }
        /// <summary>
        /// Coordinate
        /// </summary>
        public struct Coordinate
        {
            /// <summary>
            /// Degree
            /// </summary>
            public int degrees;
            /// <summary>
            /// Minute
            /// </summary>
            public int minutes;
            /// <summary>
            /// Convert to String Format
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                //ddd.mm
                return string.Format("{0:D3}.{1:D2}", degrees, minutes);
            }
        }
        /// <summary>
        /// Direction
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// East
            /// </summary>
            East = 'E',
            /// <summary>
            /// West
            /// </summary>
            West = 'W',
            /// <summary>
            /// South
            /// </summary>
            South = 'S',
            /// <summary>
            /// North
            /// </summary>
            North = 'N',
            /// <summary>
            /// Unknown
            /// </summary>
            Unknown = 'U'
        }
        /// <summary>
        /// Fix
        /// </summary>
        public enum FIX
        {
            /// <summary>
            /// Invalid Fix 
            /// </summary>
            INVALID = '0',
            /// <summary>
            /// 2D fix 
            /// </summary>
            FIX2D = '2',
            /// <summary>
            /// 3D fix 
            /// </summary>
            FIX3D = '3',
            /// <summary>
            /// Unknown fix
            /// </summary>
            UNKNOWN = 'U'
        }

        /// <summary>
        /// GPS Reset Type
        /// </summary>
        public enum ResetType
        {
            /// <summary>
            /// 0 - Hardware reset: the GPS receiver is reset and restarts by using the
            /// values stored in the internal memory of the GPS receiver.
            /// </summary>
            Hardware = 0,
            /// <summary>
            /// 1 - Coldstart (No Almanac, No Ephemeris): this option clears all data that
            /// is currently stored in the internal memory of the GPS receiver including
            /// position, almanac, ephemeris, and time. The stored clock drift however,
            /// is retained. It is available in controlled mode only.
            /// </summary>
            Coldstart = 1,
            /// <summary>
            /// 2 - Warmstart (No ephemeris): this option clears all initialization data in the
            /// GPS receiver and subsequently reloads the data that is currently displayed
            /// in the Receiver Initialization Setup screen. The almanac is retained
            /// but the ephemeris is cleared. It is available in controlled mode only.
            /// </summary>
            Warmstart = 2,
            /// <summary>
            /// 3 - Hotstart (with stored Almanac and Ephemeris): the GPS receiver restarts
            /// by using the values stored in the internal memory of the GPS receiver;
            /// validated ephemeris and almanac. It is available in controlled mode only.
            /// </summary>
            Hotstart = 3,
        }

        //internal static bool m_gps_on = false;

        /*private GPS()
        { }*/

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_GPS_TurnOn(bool on);

        /// <summary>
        /// turn gps function on
        /// </summary>
        /// <param name="on">on/off</param>
        /// <returns>Result</returns>
        public static Device.Result TurnOn(bool on)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_GPS_TurnOn(on))
                return Device.FalseToResult();
            Device.m_gps_on = on;
            return Device.Result.OK;
        }

        /// <summary>
        /// gps status
        /// </summary>
        public static bool On
        {
            get { return Device.m_gps_on; }
            set
            {
                if (TurnOn(value) == Device.Result.OK)
                    Device.m_gps_on = value;
            }
        }

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_GPS_Reset(int type);

        /// <summary>
        /// Reset the GPS controller
        /// </summary>
        /// <param name="type">Reset Type</param>
        /// <returns>Result</returns>
        public static Device.Result Reset(ResetType type)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_GPS_Reset((int)type))
                return Device.FalseToResult();
            return Device.Result.OK;
        }

        [DllImport(Device.cs501_dll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool cs501_GPS_GetPositionW(StringBuilder pos);

        /// <summary>
        /// Get Acquired Position(Raw data)
        /// </summary>
        /// <param name="position">Acquired Position</param>
        /// <returns>Result</returns>
        /// <remarks>
        /// &lt;UTC&gt;,&lt;latitude&gt;,&lt;longitude&gt;,&lt;hdop&gt;,&lt;altitude&gt;,&lt;fix&gt;,&lt;cog&gt;,&lt;spkm&gt;,&lt;spkn&gt;,&lt;date&gt;,&lt;nsat&gt;
        /// where:
        /// &lt;UTC&gt; - UTC time (hhmmss.sss) referred to GGA sentence
        /// &lt;latitude&gt; - format is ddmm.mmmm N/S (referred to GGA sentence)
        /// where:
        /// dd - degrees
        /// 00..90
        /// mm.mmmm - minutes
        /// 00.0000..59.9999
        /// N/S: North / South
        /// &lt;longitude&gt; - format is dddmm.mmmm E/W (referred to GGA sentence)
        /// where:
        /// ddd - degrees
        /// 000..180
        /// mm.mmmm - minutes
        /// 00.0000..59.9999
        /// E/W: East / West
        /// &lt;hdop&gt; - x.x - Horizontal Diluition of Precision (referred to GGA sentence)
        /// &lt;altitude&gt; - xxxx.x Altitude - mean-sea-level (geoid) in meters (referred to GGA sentence)
        /// &lt;fix&gt; -
        /// 0 - Invalid Fix
        /// 2 - 2D fix
        /// 3 - 3D fix
        /// &lt;cog&gt; - ddd.mm - Course over Ground (degrees, True) (referred to VTG sentence)
        /// where:
        /// ddd - degrees
        /// 000..360
        /// mm - minutes
        /// 00..59
        /// &lt;spkm&gt; - xxxx.x Speed over ground (Km/hr) (referred to VTG sentence)
        /// &lt;spkn&gt; - xxxx.x- Speed over ground (knots) (referred to VTG sentence)
        /// &lt;date&gt; - ddmmyy Date of Fix (referred to RMC sentence)
        /// where:
        /// dd - day
        /// 01..31
        /// mm - month
        /// 01..12
        /// yy - year
        /// 00..99 - 2000 to 2099
        /// &lt;nsat&gt; - nn - Total number of satellites in use (referred to GGA sentence)
        /// 00..12
        /// </remarks>
        public static Device.Result GetPosition
            (
                ref String position
            )
        {
            position = null;
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            StringBuilder sb = new StringBuilder(100);
            if (!cs501_GPS_GetPositionW(sb))
                return Device.FalseToResult();
            position = sb.ToString();
            return Device.Result.OK;
        }
        #region ElevationMask

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_GPS_ElevationMask(int angle);

        /// <summary>
        /// A satellite with an elevation angle that is below the specified navigation mask angle is not used in the navigation solution
        /// </summary>
        /// <param name="angle">Navigation mask angle</param>
        /// <returns>Result</returns>
        /// <remarks>-20.0 - 90.0, default = 7.5 navigation degrees</remarks>
        public static Device.Result ElevationMask(double angle)
        {
            if (angle < -20 || angle > 90)
                throw new ArgumentOutOfRangeException();

            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_GPS_ElevationMask((int)Math.Round(angle * 10)))
                return Device.FalseToResult();
            return Device.Result.OK;
        }

        #endregion
        #region PowerMask

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_GPS_PowerMask(int dBHz);

        /// <summary>
        /// Limit on which satellites are used in navigation solutions: satellites with signals lower than the power mask are not used.
        /// </summary>
        /// <param name="dBHz">signal power lower bound</param>
        /// <returns>Result</returns>
        /// <remarks>20..50 - navigation mask in dBHz (default = 28)</remarks>
        public static Device.Result PowerMask(int dBHz)
        {
            if (dBHz < 20 || dBHz > 50)
                throw new ArgumentOutOfRangeException();

            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_GPS_PowerMask(dBHz))
                return Device.FalseToResult();
            return Device.Result.OK;
        }

        #endregion
        /// <summary>
        /// Get Acquired Position. If data content negative value, 
        /// it means can't get data from GPS
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Device.Result GetPosition
            (
                [In, Out] GPSPosition position
            )
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            StringBuilder sb = new StringBuilder(100);
            if (!cs501_GPS_GetPositionW(sb))
                return Device.FalseToResult();
            position = ParseGPSData(sb.ToString());
            return Device.Result.OK;
        }
        private static GPSPosition ParseGPSData(string data)
        {
            //<UTC>,<latitude>,<longitude>,<hdop>,<altitude>, 
            //<fix>,<cog>,<spkm>,<spkn>,<date>,<nsat> 
            GPSPosition position = new GPSPosition();
            try
            {
                //check
                if (IsEmplyString(data))
                    return null;

                string[] split = data.Split(new char[] { ',' });

                if (split.Length == 11)
                {
                    int hour = 0, min = 0, sec = 0;
                    int year = 0, month = 0, day = 0;
                    //hhmmss.sss
                    //ddmmyy
                    if (!IsEmplyString(split[0]))
                    {
                        hour = int.Parse(split[0].Substring(0, 2));
                        min = int.Parse(split[0].Substring(2, 2));
                        sec = int.Parse(split[0].Substring(4, 2));
                    }
                    if (!IsEmplyString(split[9]))
                    {
                        year = int.Parse(split[9].Substring(4, 2)) + 2000;
                        month = int.Parse(split[9].Substring(2, 2));
                        day = int.Parse(split[9].Substring(0, 2));
                    }
                    position.UTC = new DateTime(year, month, day, hour, min, sec, DateTimeKind.Utc);
                    //xxxx.x
                    position.altitude = !IsEmplyString(split[4]) ? float.Parse(split[4]) : -1;
                    //ddd.mm
                    string[] cog = split[6].Split(new char[] { '.' });
                    if (cog != null && cog.Length == 2)
                    {
                        position.cog.degrees = !IsEmplyString(cog[0]) ? int.Parse(cog[0]) : -1;
                        position.cog.minutes = !IsEmplyString(cog[1]) ? int.Parse(cog[1]) : -1;
                    }
                    else
                    {
                        position.cog.degrees = -1;
                        position.cog.minutes = -1;
                    }
                    //x.x
                    position.hdop = !IsEmplyString(split[3]) ? float.Parse(split[3]) : -1;
                    //ddmm.mmmmN/S
                    if (!IsEmplyString(split[1]))
                    {
                        position.latitude.degrees = int.Parse(split[1].Substring(0, 2));
                        position.latitude.minutes = float.Parse(split[1].Substring(2, 7));
                        position.latitude.direction = split[1].Substring(split[1].Length - 1, 1) == "N" ? Direction.North : Direction.South;
                    }
                    else
                    {
                        position.latitude.degrees = -1;
                        position.latitude.minutes = -1;
                        position.latitude.direction = Direction.Unknown;
                    }

                    //dddmm.mmmmE/W
                    if (!IsEmplyString(split[2]))
                    {
                        position.longitude.degrees = int.Parse(split[2].Substring(0, 3));
                        position.longitude.minutes = float.Parse(split[2].Substring(3, 7));
                        position.longitude.direction = split[2].Substring(split[2].Length - 1, 1) == "E" ? Direction.East : Direction.West;
                    }
                    else
                    {
                        position.longitude.degrees = -1;
                        position.longitude.minutes = -1;
                        position.longitude.direction = Direction.Unknown;
                    }
                    //nn
                    position.nsat = !IsEmplyString(split[10]) ? int.Parse(split[10]) : -1;
                    //xxxx.x 
                    position.spkm = !IsEmplyString(split[7]) ? float.Parse(split[7]) : -1;
                    //xxxx.x 
                    position.spkm = !IsEmplyString(split[8]) ? float.Parse(split[8]) : -1;
                }
            }
            catch { }

            return position;
        }

        private static bool IsEmplyString(string context)
        {
            return (context == null || context.Length == 0);
        }
        #region NMEA
        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_Unsolicited_NMEA_Mode(int mode, bool GGA, bool GLL, bool GSA, bool GSV, bool RMC, bool VTG);
        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_Unsolicited_NMEA_Read(byte[] buf, int bufLength, out uint lpNumberOfBytesRead);
        [Flags]
        internal enum NMEA
        {
            /// <summary>
            /// No NMEA Enable
            /// </summary>
            NONE,
            /// <summary>
            /// Global Positioning System Fix Data
            /// </summary>
            GGA = 1,
            /// <summary>
            /// Geographical Position - Latitude/Longitude
            /// </summary>
            GLL =2,
            /// <summary>
            /// GPS DOP and Active Satellites
            /// </summary>
            GSA = 4,
            /// <summary>
            /// GPS DOP and Active Satellites
            /// </summary>
            GSV = 8,
            /// <summary>
            /// recommended Minimum Specific GPS Data
            /// </summary>
            RMC = 16,
            /// <summary>
            /// Course Over Ground and Ground Speed
            /// </summary>
            VTG = 32
        }
        /// <summary>
        /// Set command permits to activate an Unsolicited streaming of GPS data
        /// </summary>
        /// <param name="nmea"></param>
        /// <returns></returns>
        internal static Device.Result SetNMEAMode(NMEA nmea)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_Unsolicited_NMEA_Mode(nmea == NMEA.NONE ? 0 : 2,
                (nmea & NMEA.GGA) == NMEA.GGA,
                (nmea & NMEA.GLL) == NMEA.GLL,
                (nmea & NMEA.GSA) == NMEA.GSA,
                (nmea & NMEA.GSV) == NMEA.GSV,
                (nmea & NMEA.RMC) == NMEA.RMC,
                (nmea & NMEA.VTG) == NMEA.VTG))
                return Device.FalseToResult();
            return Device.Result.OK;
        }
        /// <summary>
        /// Read Unsolicited streaming of GPS data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="received"></param>
        /// <returns></returns>
        internal static Device.Result ReadNMEA(byte[] data, out uint received)
        {
            received = 0;
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (data == null || data.Length == 0)
            {
                return Device.Result.INVALID_PARAMETER;
            }
            if (!cs501_Unsolicited_NMEA_Read(data, data.Length, out received))
                return Device.FalseToResult();
            return Device.Result.OK;
        }
#if NOT
        public delegate void SatellitesChangedHandler(CSLibrary.GPS.SatelliteListEventArgs e);
        public static event SatellitesChangedHandler SatellitesChanged;

        public delegate void UtcDateTimeChangedHandler(CSLibrary.GPS.DateTimeEventArgs e);
        public static event UtcDateTimeChangedHandler UtcDateTimeChanged;

        public delegate void DateTimeChangedHandler(CSLibrary.GPS.DateTimeEventArgs e);
        public static event DateTimeChangedHandler DateTimeChanged;

        static CSLibrary.GPS.Nmea.NmeaInterpreter interpreter = null;
        static CSLibrary.GPS.IO.SerialDevice device = null;
        static AutoResetEvent startedThread = new AutoResetEvent(false);
        static AutoResetEvent stoppedThread = new AutoResetEvent(false);
        static Thread proc = null;
        static int stop = 0;
        static GPS()
        {
            interpreter = new CSLibrary.GPS.Nmea.NmeaInterpreter();
            interpreter.UtcDateTimeChanged += new EventHandler<CSLibrary.GPS.DateTimeEventArgs>(interpreter_UtcDateTimeChanged);
            interpreter.DateTimeChanged += new EventHandler<CSLibrary.GPS.DateTimeEventArgs>(interpreter_DateTimeChanged);
            interpreter.SatellitesChanged += new EventHandler<CSLibrary.GPS.SatelliteListEventArgs>(interpreter_SatellitesChanged);
            device = new CSLibrary.GPS.IO.SerialDevice("COM3:", 57600);
            device.Open(System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
        }

        static void interpreter_DateTimeChanged(object sender, CSLibrary.GPS.DateTimeEventArgs e)
        {
            if (DateTimeChanged != null)
            {
                DateTimeChanged(e);
            }
        }

        static void interpreter_SatellitesChanged(object sender, CSLibrary.GPS.SatelliteListEventArgs e)
        {
            if (SatellitesChanged != null)
            {
                SatellitesChanged(e);
            }
        }

        static void interpreter_UtcDateTimeChanged(object sender, CSLibrary.GPS.DateTimeEventArgs e)
        {
            if (UtcDateTimeChanged != null)
            {
                UtcDateTimeChanged(e);
            }
        }

        public static Device.Result StartNMEA()
        {
            /*if (!IsAlive())
            {
                Interlocked.Exchange(ref stop, 0);
                proc = new Thread(new ThreadStart(ProcStart));
                proc.IsBackground = true;
                proc.Start();
                startedThread.WaitOne();
            }*/
            interpreter.Start(device);
            return Device.Result.OK;
        }

        public static Device.Result StopNMEA()
        {
            /*if (IsAlive())
            {
                Interlocked.Exchange(ref stop, 1);
                stoppedThread.WaitOne();
            }*/
            interpreter.Stop();
            return Device.Result.OK;
        }
#endif
        /*static void ProcStart()
        {
            startedThread.Set();
            try
            {
                if (!cs501_Unsolicited_NMEA_Mode(2, true, true, true, true, true, true))
                {
                    throw new Exception();
                }
                while (Interlocked.Equals(stop, 0))
                {
                    CSLibrary.GPS.NmeaSentence sentence = reader.ReadTypedSentence();
                    if (sentence != null && SentenceReceived != null)
                    {
                        CSLibrary.GPS.NmeaSentenceEventArgs e = new CSLibrary.GPS.NmeaSentenceEventArgs(sentence);
                        SentenceReceived(e);
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
                if (!cs501_Unsolicited_NMEA_Mode(0, false, false, false, false, false, false))
                {
                    throw new Exception();
                }
            }
            catch { }
            stoppedThread.Set();
        }
#if WindowsCE
        [DllImport("coredll.dll")]
#else
        [DllImport("kernel32.dll")]
#endif
        private static extern bool GetExitCodeThread(UInt32 hThread, out uint lpExitCode);

        private static bool IsAlive()
        {
            if (proc != null)
            {
#if WindowsCE
                uint exCode = 0;
                if (!GetExitCodeThread((uint)proc.ManagedThreadId, out exCode))
                {
                    return false;
                }
                return (exCode == 0x00000103);

#else
                return g_hWndThread.IsAlive;
#endif
            }
            return false;
        }*/
        #endregion
    }

    /// <summary>
    /// cs501 DGPS
    /// </summary>
    public static class DGPS
    {
        #region Control

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_DGPS_Control(int mode, int timeout);

        /// <summary>
        /// DGPS control selection
        /// </summary>
        public enum Control
        {
            /// <summary>
            /// auto: use corrections when available (default)
            /// </summary>
            Auto = 0,
            /// <summary>
            /// exclusive: include in navigation solution only SVs with corrections
            /// </summary>
            Exclusive = 1,
            /// <summary>
            /// never use: ignore corrections
            /// </summary>
            NeverUse = 2,
        }

        /// <summary>
        /// DGPS Control. Enables users to control how the receiver uses differential GPS (DGPS) corrections.
        /// </summary>
        /// <param name="mode">DGPS control selection</param>
        /// <param name="timeout">DGPS time out</param>
        /// <returns>Result</returns>
        /// <remarks>
        /// DGPS control selection - auto: use corrections when available (default)
        /// DGPS time out - time out in seconds (default = 0)
        /// </remarks>
        public static Device.Result SetControl(Control mode, int timeout)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_DGPS_Control((int)mode, timeout))
                return Device.FalseToResult();
            return Device.Result.OK;
        }

        #endregion

        #region Source

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_DGPS_Source(int source);

        /// <summary>
        /// DGPS source selection
        /// </summary>
        public enum Source
        {
            /// <summary>
            /// none: DGPS corrections are not used, even if available (default)
            /// </summary>
            None = 0,
            /// <summary>
            /// SBAS: uses SBAS (Satellite Based Augmentation System) satellite (subject to SBAS satellite availability)
            /// </summary>
            SBAS = 1,
            /// <summary>
            /// external RTCM: data external RTCM input source (serial port B)
            /// </summary>
            ExternalRTCM = 2,
        }

        /// <summary>
        /// Differential GPS Corrections (DGPS) Source. It allows the user to select the source for Differential GPS (DGPS) corrections.
        /// </summary>
        /// <param name="source">DGPS source selection</param>
        /// <returns>Result</returns>
        /// <remarks>DGPS source selection - none: DGPS corrections are not used, even if available (default)</remarks>
        public static Device.Result SetSource(Source source)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_DGPS_Source((int)source))
                return Device.FalseToResult();
            return Device.Result.OK;
        }

        #endregion

        #region SetSBAS

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_DGPS_SetSBAS(int prn, int mode, int timeout);

        /// <summary>
        /// SBAS operating mode
        /// </summary>
        public enum SBASOperatingMode
        {
            /// <summary>
            /// testing mode: it accepts/uses SBAS corrections even if satellite is transmitting in a test mode
            /// </summary>
            TestMode = 0,
            /// <summary>
            /// integrity mode (default): it rejects SBAS corrections if the SBAS satellite is transmitting in a test mode
            /// </summary>
            IntegrityMode = 1,
        }

        /// <summary>
        /// DGPS timeout selection
        /// </summary>
        public enum DGPSTimeoutSelection
        {
            /// <summary>
            /// timeout specified by the SBAS satellite is used (default)
            /// </summary>
            Satellite = 0,
            /// <summary>
            /// user-specified timeout from DGPS Control <see cref="SetControl"/>
            /// </summary>
            DGPSControl = 1,
        }

        /// <summary>
        /// Allows the user to set the SBAS (Satellite Based Augmentation System) parameters
        /// </summary>
        /// <param name="prn">SBAS PRN</param>
        /// <param name="mode">SBAS operating mode</param>
        /// <param name="timeout">DGPS timeout selection</param>
        /// <returns>Result</returns>
        /// <remarks>
        /// SBAS PRN - 0 - automatic PRN (default), 120..138 - exclusive PRN
        /// SBAS operating mode - integrity mode (default)
        /// DGPS timeout selection - timeout specified by the SBAS satellite is used (default)
        /// </remarks>
        public static Device.Result SetSBAS(int prn, SBASOperatingMode mode, DGPSTimeoutSelection timeout)
        {
            if (prn == 0)
            { }
            else if (prn >= 120 && prn <= 138)
            { }
            else
                throw new ArgumentOutOfRangeException();

            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_DGPS_SetSBAS(prn, (int)mode, (int)timeout))
                return Device.FalseToResult();
            return Device.Result.OK;
        }

        #endregion
    }

    /// <summary>
    /// cs501 GPRS
    /// </summary>
    public static class GPRS
    {
        /// <summary>
        /// Ping Mode
        /// </summary>
        public enum PingMode
        {
            /// <summary>
            /// 0 - disable ICMP Ping support
            /// </summary>
            Disable = 0,
            /// <summary>
            /// 1 - enable firewalled ICMP Ping support: the module is sending a proper
            /// ECHO_REPLY only to a subset of IP Addresses pinging it
            /// </summary>
            Firewall = 1,
            /// <summary>
            /// 2 - enable free ICMP Ping support; the module is sending a proper
            /// ECHO_REPLY to every IP Address pinging it.
            /// </summary>
            Enable = 2,
        }

        //internal static bool m_gprs_on = false;
        /*private GPRS()
        { }*/


        [DllImport(Device.cs501_dll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool cs501_GPRS_SetAccessPointW(string apn);

        /// <summary>
        /// Set GPRS Access Point
        /// </summary>
        /// <param name="apn">APN</param>
        /// <returns>Result</returns>
        public static Device.Result SetAccessPoint(string apn)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_GPRS_SetAccessPointW(apn))
                return Device.FalseToResult();
            return Device.Result.OK;
        }

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_GPRS_TurnOn(bool on);

        /// <summary>
        /// turn grps function on
        /// </summary>
        /// <param name="on">on/off</param>
        /// <returns>Result</returns>
        public static Device.Result TurnOn(bool on)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_GPRS_TurnOn(on))
                return Device.FalseToResult();
            Device.m_gprs_on = on;
            return Device.Result.OK;
        }

        [DllImport(Device.cs501_dll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool cs501_GPRS_TurnOnExW(bool on, string userId, string pwd);

        /// <summary>
        /// turn grps function on
        /// </summary>
        /// <param name="on">on/off</param>
        /// <param name="userId">User ID</param>
        /// <param name="pwd">Password</param>
        /// <returns>Result</returns>
        public static Device.Result TurnOn(bool on, string userId, string pwd)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_GPRS_TurnOnExW(on, userId, pwd))
                return Device.FalseToResult();
            Device.m_gprs_on = on;
            return Device.Result.OK;
        }

        /// <summary>
        /// grps status
        /// </summary>
        public static bool On
        {
            get { return Device.m_gprs_on; }
            set
            {
                if (TurnOn(value) == Device.Result.OK)
                    Device.m_gprs_on = value;
            }
        }

        [DllImport(Device.cs501_dll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool cs501_GPRS_GetDeviceIpW(StringBuilder ip);

        /// <summary>
        /// get device gprs ip
        /// </summary>
        /// <param name="ip">ip address</param>
        /// <returns>Result</returns>
        public static Device.Result GetDeviceIp(out string ip)
        {
            ip = null;
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            StringBuilder sb = new StringBuilder(100);
            if (!cs501_GPRS_GetDeviceIpW(sb))
                return Device.FalseToResult();
            ip = sb.ToString();
            return Device.Result.OK;
        }

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_GPRS_SetPingMode(int mode);

        /// <summary>
        /// ICMP Ping Support
        /// </summary>
        /// <param name="mode">Ping Type</param>
        /// <returns>Result</returns>
        public static Device.Result SetPingMode(PingMode mode)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return Device.Result.NOT_READY;
            if (!cs501_GPRS_SetPingMode((int)mode))
                return Device.FalseToResult();
            return Device.Result.OK;
        }
    }
}

#endif
