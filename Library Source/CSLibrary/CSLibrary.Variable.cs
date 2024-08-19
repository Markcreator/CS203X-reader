using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;

using CSLibrary;
using CSLibrary.Constants;
using CSLibrary.Structures;

namespace CSLibrary
{
    public partial class HighLevelInterface
    {
        #region Fixed Variable
        private const uint MAXPOWERLVL = 300;
        private const uint MINPOWERLVL = 0;
        private const uint MAXPOWERLVL_JP = 275;
        private const uint MAXPOWERLVL_TW = 270;
        private const uint MAXPOWERLVL_ETSI = 275;

        private const uint INVALID_POWER_VALUE = uint.MaxValue;
        private const uint INVALID_PROFILE_VALUE = uint.MaxValue;
        private const int DATA_FIELD_INDEX = 20;
        private const int RSSI_FIELD_INDEX = 12;
        private const int ANT_FIELD_INDEX = 14;
        private const int MS_FIELD_INDEX = 8;
        private const int RFID_PACKET_COMMON_SIZE = 8;

        private const ushort KILL_PWD_START_OFFSET = 0;
        private const ushort KILL_PWD_WORD_LENGTH = 2;

        private const ushort ACC_PWD_START_OFFSET = 2;
        private const ushort ACC_PWD_WORD_LENGTH = 2;

        private const ushort EPC_START_OFFSET = 2;
        private const ushort EPC_WORD_LENGTH = 6;

        private const ushort PC_START_OFFSET = 1;
        private const ushort PC_WORD_LENGTH = 1;

        private const ushort USER_WORD_LENGTH = 1;
        private const uint MAXFRECHANNEL = 50;

#if CS203
        private const uint TCP_DATA_PORT = 1515;
        private const uint TCP_CMD_PORT = 1516;
#endif
        #endregion

        #region Private Member
        private IReaderBase ReaderBase = new IReaderBase();

        private object syncRoot = new Object();
        private STATE m_state = STATE.NOT_INITIALIZED;
        private Result m_result = Result.OK;
        private uint m_totalRadioAttach = 0;
        private int m_radioIndex = 0;

        private Thread g_hWndThread = null;

        private bool bShutdownRequired = false;
        private int bStop = 0;

        //Engineering Test Mode
        private int m_stop_cw = 1;

        // Helper for marshalling execution to GUI thread
        private System.Windows.Forms.Control mGuiMarshaller;
#if DEBUG
        CSLibrary.Utility.Counter TimerCnt = new CSLibrary.Utility.Counter();
#endif
        #region Save Parmeters
        private List<RegionCode>    m_save_country_list = new List<RegionCode>();
        private RegionCode          m_save_region_code = RegionCode.UNKNOWN;
        private ResponseMode        m_save_resp_mode = ResponseMode.COMPACT;
        private RadioOperationMode  m_save_oper_mode = RadioOperationMode.NONCONTINUOUS;
        private SingulationAlgorithm m_save_singulation = SingulationAlgorithm.DYNAMICQ;
        private LBT     m_save_enable_lbt = LBT.OFF;
        private uint    m_save_freq_channel = 2;
        private uint    m_save_country_code = 0;
        private uint    m_save_link_profile = 2;
        private uint    m_save_power_level = 300;
        private double  m_save_selected_freq = 0;
        private uint    m_save_inventory_cycle = 0;
        private uint    m_save_inventory_duration = 3900;
        private bool    m_save_fixed_channel = false;
        private bool    m_save_blocking_mode = false;
        private bool    m_save_extern_lo = false;
        /// <summary>
        /// current MacErrorCode
        /// </summary>
        public uint     MacErrorCode = 0;
#if CS203
        private string m_save_tcp_ip = String.Empty;
        private uint m_save_tcp_port = TCP_DATA_PORT;
        private uint m_save_tcp_timeout = 30000;
        private IntPtr dllModule = IntPtr.Zero;
#endif
#if CS468
        readonly uint[] LogicToPhyPortMapping = new uint[16]
            {
                0,4,8,12,
                1,5,9,13,
                2,6,10,14,
                3,7,11,15
            };
#endif
        #endregion
#if TEMPLOG
        private const string TmpLogFile = "TempLogging.txt";
#endif

        #endregion

        #region extern variable
        private CSLibraryOperationParms m_rdr_opt_parms = new CSLibraryOperationParms();
        /// <summary>
        /// CSLibrary Operation parameters
        /// Notes : you must config this parameters before perform any operation
        /// </summary>
        public CSLibraryOperationParms Options
        {
            get { lock (syncRoot) { return m_rdr_opt_parms; } }
            set { lock (syncRoot) { m_rdr_opt_parms = value; } }
        }
        /// <summary>
        /// Write error log to file
        /// </summary>
        public bool WriteToLog
        {
            get { return CSLibrary.SysLogger.WriteToLog; }
            set { CSLibrary.SysLogger.WriteToLog = value; }
        }
        /// <summary>
        /// Get current power level
        /// </summary>
        public uint SelectedPowerLevel
        {
            get { lock (syncRoot) { return m_save_power_level; } }
        }
        /// <summary>
        /// Get Current Selected Frequency Channel
        /// </summary>
        public uint SelectedChannel
        {
            get { lock (syncRoot) { return m_save_freq_channel; } }
        }
        /// <summary>
        /// Get Region Profile
        /// </summary>
        public RegionCode SelectedRegionCode
        {
            get { lock (syncRoot) { return m_save_region_code; } }
        }
        /// <summary>
        /// Current selected frequency
        /// </summary>
        public uint SelectedLinkProfile
        {
            get { lock (syncRoot) { return m_save_link_profile; } }
        }
        /// <summary>
        /// Get current frequency 
        /// </summary>
        public double SelectedFrequencyBand
        {
            get { lock (syncRoot) { return m_save_selected_freq; } }
        }
        /// <summary>
        /// Get Current LBT status
        /// </summary>
        public LBT LBT_ON
        {
            get { lock (syncRoot) { return m_save_enable_lbt; } }
        }
        /// <summary>
        /// get last function return code
        /// </summary>
        public Result LastResultCode
        {
            get { lock (syncRoot) { return m_result; } }
        }
        /// <summary>
        /// Available Link Profile you can use
        /// </summary>
        public uint[] AvailableLinkProfile
        {
            get
            {
                switch (m_save_region_code)
                {
#if CS101
                    case RegionCode.CN:
                    case RegionCode.ETSI:
                    case RegionCode.JP:
                    case RegionCode.KR:
                        return new uint[] { 0, 2, 3, 5 };
                    case RegionCode.UNKNOWN:
                        return new uint[0];
                    default:
                        return new uint[] { 0, 1, 2, 3, 4, 5 };
#elif CS203
                    case RegionCode.CN:
                    case RegionCode.ETSI:
                    case RegionCode.JP:
                    case RegionCode.KR:
#if CS468
                        return new uint[] { 0, 2, 3, 5 };
#else
                        return new uint[] { 0, 2, 3 };
#endif
                    case RegionCode.UNKNOWN:
                        return new uint[0];
                    default:
#if CS468
                        return new uint[] { 0, 1, 2, 3, 4, 5 };
#else
                        return new uint[] { 0, 1, 2, 3, 4 };
#endif
#endif
                }
            }
        }
        /// <summary>
        /// Available Maximum Power you can set
        /// </summary>
        public uint AvailableMaxPower
        {
            get
            {
                lock (syncRoot)
                {
                    switch (m_save_region_code)
                    {
#if CS101
                        case RegionCode.ETSI:
                            return 275;
                        case RegionCode.TW:
                            return 270;
                        default:
                            return 300;
#elif CS203
                        case RegionCode.JP:
                            return 275;
                        default:
                            return 300;
#endif
                    }
                }
            }
        }
        /// <summary>
        /// Available region you can use
        /// </summary>
        public List<RegionCode> AvailableRegionCode
        {
            get { lock (syncRoot) { return m_save_country_list; } }
        }

        /// <summary>
        /// If true, it can only set to fixed channel.
        /// Otherwise, both fixed and hopping can be set.
        /// </summary>
        public bool IsFixedChannelOnly
        {
            get { return (m_save_country_code == 1 | m_save_country_code == 3); }
        }
        /// <summary>
        /// Get Fixed frequency channel
        /// </summary>
        public bool IsFixedChannel
        {
            get { lock (syncRoot) { return m_save_fixed_channel; } }
        }
        /// <summary>
        /// Current Operation State
        /// </summary>
        public STATE State
        {
            get { lock (syncRoot) { return m_state; } }
            private set { lock (syncRoot) { m_state = value; } }
        }
        /// <summary>
        /// Get the total attached radio, for windoes CE version, it at least return 1;
        /// </summary>
        public uint TotalNumberOfRadios
        {
            get { lock (syncRoot) { return m_totalRadioAttach; } }
            private set { lock (syncRoot) { m_totalRadioAttach = value; } }
        }
        /// <summary>
        /// Current Operation Radio Index
        /// </summary>
        public int RadioIndex
        {
            get { lock (syncRoot) { return m_radioIndex; } }
            set { lock (syncRoot) { m_radioIndex = value; } }
        }
        #endregion

    }
}
