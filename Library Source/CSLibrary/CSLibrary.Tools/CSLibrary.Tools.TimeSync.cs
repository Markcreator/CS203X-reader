/*
 * NTPClient
 * Copyright (C)2001 Valer BOCAN <vbocan@dataman.ro>
 * Last modified: June 29, 2001
 * All Rights Reserved
 * 
 * This code is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY, without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * 
 * To fully understand the concepts used herein, I strongly
 * recommend that you read the RFC 2030.
 * 
 * NOTE: This example is intended to be compiled with Visual Studio .NET Beta 2
 */
#if WindowsCE
namespace CSLibrary.Tools
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading;
    /// <summary>
    /// Leap indicator field values
    /// </summary>
    public enum LeapIndicator
    {
        /// <summary>
        /// No warning
        /// </summary>
        NoWarning,		// 0 - No warning
        /// <summary>
        /// Last minute has 61 seconds
        /// </summary>
        LastMinute61,	// 1 - Last minute has 61 seconds
        /// <summary>
        /// Last minute has 59 seconds
        /// </summary>
        LastMinute59,	// 2 - Last minute has 59 seconds
        /// <summary>
        /// Alarm condition (clock not synchronized)
        /// </summary>
        Alarm			// 3 - Alarm condition (clock not synchronized)
    }

    /// <summary>
    /// Mode field values
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// Symmetric active
        /// </summary>
        SymmetricActive,	// 1 - Symmetric active
        /// <summary>
        /// Symmetric pasive
        /// </summary>
        SymmetricPassive,	// 2 - Symmetric pasive
        /// <summary>
        /// Client
        /// </summary>
        Client,				// 3 - Client
        /// <summary>
        /// Server
        /// </summary>
        Server,				// 4 - Server
        /// <summary>
        /// Broadcast
        /// </summary>
        Broadcast,			// 5 - Broadcast
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown				// 0, 6, 7 - Reserved
    }

    /// <summary>
    /// Stratum field values
    /// </summary>
    public enum Stratum
    {
        /// <summary>
        /// unspecified or unavailable
        /// </summary>
        Unspecified,			// 0 - unspecified or unavailable
        /// <summary>
        /// primary reference (e.g. radio-clock)
        /// </summary>
        PrimaryReference,		// 1 - primary reference (e.g. radio-clock)
        /// <summary>
        /// secondary reference (via NTP or SNTP)
        /// </summary>
        SecondaryReference,		// 2-15 - secondary reference (via NTP or SNTP)
        /// <summary>
        /// reserved
        /// </summary>
        Reserved				// 16-255 - reserved
    }

    /// <summary>
    /// NTPClient is a C# class designed to connect to time servers on the Internet.
    /// The implementation of the protocol is based on the RFC 2030.
    /// 
    /// Public class members:
    ///
    /// LeapIndicator - Warns of an impending leap second to be inserted/deleted in the last
    /// minute of the current day. (See the _LeapIndicator enum)
    /// 
    /// VersionNumber - Version number of the protocol (3 or 4).
    /// 
    /// Mode - Returns mode. (See the _Mode enum)
    /// 
    /// Stratum - Stratum of the clock. (See the _Stratum enum)
    /// 
    /// PollInterval - Maximum interval between successive messages.
    /// 
    /// Precision - Precision of the clock.
    /// 
    /// RootDelay - Round trip time to the primary reference source.
    /// 
    /// RootDispersion - Nominal error relative to the primary reference source.
    /// 
    /// ReferenceID - Reference identifier (either a 4 character string or an IP address).
    /// 
    /// ReferenceTimestamp - The time at which the clock was last set or corrected.
    /// 
    /// OriginateTimestamp - The time at which the request departed the client for the server.
    /// 
    /// ReceiveTimestamp - The time at which the request arrived at the server.
    /// 
    /// Transmit Timestamp - The time at which the reply departed the server for client.
    /// 
    /// RoundTripDelay - The time between the departure of request and arrival of reply.
    /// 
    /// LocalClockOffset - The offset of the local clock relative to the primary reference
    /// source.
    /// 
    /// Initialize - Sets up data structure and prepares for connection.
    /// 
    /// Connect - Connects to the time server and populates the data structure.
    ///	It can also set the system time.
    /// 
    /// IsResponseValid - Returns true if received data is valid and if comes from
    /// a NTP-compliant time server.
    /// 
    /// ToString - Returns a string representation of the object.
    /// 
    /// -----------------------------------------------------------------------------
    /// Structure of the standard NTP header (as described in RFC 2030)
    ///                       1                   2                   3
    ///   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    ///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///  |LI | VN  |Mode |    Stratum    |     Poll      |   Precision   |
    ///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///  |                          Root Delay                           |
    ///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///  |                       Root Dispersion                         |
    ///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///  |                     Reference Identifier                      |
    ///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///  |                                                               |
    ///  |                   Reference Timestamp (64)                    |
    ///  |                                                               |
    ///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///  |                                                               |
    ///  |                   Originate Timestamp (64)                    |
    ///  |                                                               |
    ///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///  |                                                               |
    ///  |                    Receive Timestamp (64)                     |
    ///  |                                                               |
    ///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///  |                                                               |
    ///  |                    Transmit Timestamp (64)                    |
    ///  |                                                               |
    ///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///  |                 Key Identifier (optional) (32)                |
    ///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///  |                                                               |
    ///  |                                                               |
    ///  |                 Message Digest (optional) (128)               |
    ///  |                                                               |
    ///  |                                                               |
    ///  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// 
    /// -----------------------------------------------------------------------------
    /// 
    /// NTP Timestamp Format (as described in RFC 2030)
    ///                         1                   2                   3
    ///     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |                           Seconds                             |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |                  Seconds Fraction (0-padded)                  |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// 
    /// </summary>

    public class TimeSync
    {
        // NTP Data Structure Length
        private const byte NTPDataLength = 48;
        // NTP Data Structure (as described in RFC 2030)
        byte[] NTPData = new byte[NTPDataLength];

        // Offset constants for timestamps in the data structure
        private const byte offReferenceID = 12;
        private const byte offReferenceTimestamp = 16;
        private const byte offOriginateTimestamp = 24;
        private const byte offReceiveTimestamp = 32;
        private const byte offTransmitTimestamp = 40;

        /// <summary>
        /// Leap Indicator
        /// </summary>
        public LeapIndicator LeapIndicator
        {
            get
            {
                // Isolate the two most significant bits
                byte val = (byte)(NTPData[0] >> 6);
                switch (val)
                {
                    case 0: return LeapIndicator.NoWarning;
                    case 1: return LeapIndicator.LastMinute61;
                    case 2: return LeapIndicator.LastMinute59;
                    case 3: goto default;
                    default:
                        return LeapIndicator.Alarm;
                }
            }
        }

        /// <summary>
        /// Version Number
        /// </summary>
        public byte VersionNumber
        {
            get
            {
                // Isolate bits 3 - 5
                byte val = (byte)((NTPData[0] & 0x38) >> 3);
                return val;
            }
        }

        /// <summary>
        /// Mode
        /// </summary>
        public Mode Mode
        {
            get
            {
                // Isolate bits 0 - 3
                byte val = (byte)(NTPData[0] & 0x7);
                switch (val)
                {
                    case 0: goto default;
                    case 6: goto default;
                    case 7: goto default;
                    default:
                        return Mode.Unknown;
                    case 1:
                        return Mode.SymmetricActive;
                    case 2:
                        return Mode.SymmetricPassive;
                    case 3:
                        return Mode.Client;
                    case 4:
                        return Mode.Server;
                    case 5:
                        return Mode.Broadcast;
                }
            }
        }

        /// <summary>
        /// Stratum
        /// </summary>
        public Stratum Stratum
        {
            get
            {
                byte val = (byte)NTPData[1];
                if (val == 0) return Stratum.Unspecified;
                else
                    if (val == 1) return Stratum.PrimaryReference;
                    else
                        if (val <= 15) return Stratum.SecondaryReference;
                        else
                            return Stratum.Reserved;
            }
        }

        /// <summary>
        /// Poll Interval
        /// </summary>
        public uint PollInterval
        {
            get
            {
                return (uint)Math.Round(Math.Pow(2, NTPData[2]));
            }
        }

        /// <summary>
        /// Precision (in milliseconds)
        /// </summary>
        public double Precision
        {
            get
            {
                return (1000 * Math.Pow(2, NTPData[3]));
            }
        }

        /// <summary>
        /// Root Delay (in milliseconds)
        /// </summary>
        public double RootDelay
        {
            get
            {
                int temp = 0;
                temp = 256 * (256 * (256 * NTPData[4] + NTPData[5]) + NTPData[6]) + NTPData[7];
                return 1000 * (((double)temp) / 0x10000);
            }
        }

        /// <summary>
        /// Root Dispersion (in milliseconds)
        /// </summary>
        public double RootDispersion
        {
            get
            {
                int temp = 0;
                temp = 256 * (256 * (256 * NTPData[8] + NTPData[9]) + NTPData[10]) + NTPData[11];
                return 1000 * (((double)temp) / 0x10000);
            }
        }

        /// <summary>
        /// Reference Identifier
        /// </summary>
        public string ReferenceID
        {
            get
            {
                string val = "";
                switch (Stratum)
                {
                    case Stratum.Unspecified:
                        goto case Stratum.PrimaryReference;
                    case Stratum.PrimaryReference:
                        val += (char)NTPData[offReferenceID + 0];
                        val += (char)NTPData[offReferenceID + 1];
                        val += (char)NTPData[offReferenceID + 2];
                        val += (char)NTPData[offReferenceID + 3];
                        break;
                    case Stratum.SecondaryReference:
                        switch (VersionNumber)
                        {
                            case 3:	// Version 3, Reference ID is an IPv4 address
                                string Address = NTPData[offReferenceID + 0].ToString() + "." +
                                                 NTPData[offReferenceID + 1].ToString() + "." +
                                                 NTPData[offReferenceID + 2].ToString() + "." +
                                                 NTPData[offReferenceID + 3].ToString();
                                try
                                {
#if false // remove compilter warning
									IPHostEntry Host = Dns.GetHostByAddress(Address);
#else
                                    IPHostEntry Host = Dns.GetHostEntry(Address);
#endif
                                    val = Host.HostName + " (" + Address + ")";
                                }
                                catch (Exception)
                                {
                                    val = "N/A";
                                }
                                break;
                            case 4: // Version 4, Reference ID is the timestamp of last update
                                DateTime time = ComputeDate(GetMilliSeconds(offReferenceID));
                                // Take care of the time zone
                                TimeSpan offspan = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                                val = (time + offspan).ToString();
                                break;
                            default:
                                val = "N/A";
                                break;
                        }
                        break;
                }

                return val;
            }
        }

        /// <summary>
        /// Reference Timestamp
        /// </summary>
        public DateTime ReferenceTimestamp
        {
            get
            {
                DateTime time = ComputeDate(GetMilliSeconds(offReferenceTimestamp));
                // Take care of the time zone
                TimeSpan offspan = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                return time + offspan;
            }
        }

        /// <summary>
        /// Originate Timestamp
        /// </summary>
        public DateTime OriginateTimestamp
        {
            get
            {
                return ComputeDate(GetMilliSeconds(offOriginateTimestamp));
            }
        }

        /// <summary>
        /// Receive Timestamp
        /// </summary>
        public DateTime ReceiveTimestamp
        {
            get
            {
                DateTime time = ComputeDate(GetMilliSeconds(offReceiveTimestamp));
                // Take care of the time zone
                TimeSpan offspan = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                return time + offspan;
            }
        }

        /// <summary>
        /// Transmit Timestamp
        /// </summary>
        public DateTime TransmitTimestamp
        {
            get
            {
                DateTime time = ComputeDate(GetMilliSeconds(offTransmitTimestamp));
                // Take care of the time zone
                TimeSpan offspan = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                return time + offspan;
            }
            set
            {
                SetDate(offTransmitTimestamp, value);
            }
        }

        /// <summary>
        /// Reception Timestamp
        /// </summary>
        public DateTime ReceptionTimestamp;

        /// <summary>
        /// Round trip delay (in milliseconds)
        /// </summary>
        public int RoundTripDelay
        {
            get
            {
                TimeSpan span = (ReceiveTimestamp - OriginateTimestamp) + (ReceptionTimestamp - TransmitTimestamp);
                return (int)span.TotalMilliseconds;
            }
        }

        /// <summary>
        /// Local clock offset (in milliseconds)
        /// </summary>
        public int LocalClockOffset
        {
            get
            {
                TimeSpan span = (ReceiveTimestamp - OriginateTimestamp) - (ReceptionTimestamp - TransmitTimestamp);
                return (int)(span.TotalMilliseconds / 2);
            }
        }

        // Compute date, given the number of milliseconds since January 1, 1900
        private DateTime ComputeDate(ulong milliseconds)
        {
            TimeSpan span = TimeSpan.FromMilliseconds((double)milliseconds);
            DateTime time = new DateTime(1900, 1, 1);
            time += span;
            return time;
        }

        // Compute the number of milliseconds, given the offset of a 8-byte array
        private ulong GetMilliSeconds(byte offset)
        {
            ulong intpart = 0, fractpart = 0;

            for (int i = 0; i <= 3; i++)
            {
                intpart = 256 * intpart + NTPData[offset + i];
            }
            for (int i = 4; i <= 7; i++)
            {
                fractpart = 256 * fractpart + NTPData[offset + i];
            }
            ulong milliseconds = intpart * 1000 + (fractpart * 1000) / 0x100000000L;
            return milliseconds;
        }

        // Compute the 8-byte array, given the date
        private void SetDate(byte offset, DateTime date)
        {
            ulong intpart = 0, fractpart = 0;
            DateTime StartOfCentury = new DateTime(1900, 1, 1, 0, 0, 0);	// January 1, 1900 12:00 AM

            ulong milliseconds = (ulong)(date - StartOfCentury).TotalMilliseconds;
            intpart = milliseconds / 1000;
            fractpart = ((milliseconds % 1000) * 0x100000000L) / 1000;

            ulong temp = intpart;
            for (int i = 3; i >= 0; i--)
            {
                NTPData[offset + i] = (byte)(temp % 256);
                temp = temp / 256;
            }

            temp = fractpart;
            for (int i = 7; i >= 4; i--)
            {
                NTPData[offset + i] = (byte)(temp % 256);
                temp = temp / 256;
            }
        }

        // Initialize the NTPClient data
        private void Initialize()
        {
            // Set version number to 4 and Mode to 3 (client)
            NTPData[0] = 0x1B;
            // Initialize all other fields with 0
            for (int i = 1; i < 48; i++)
            {
                NTPData[i] = 0;
            }
            // Initialize the transmit timestamp
            TransmitTimestamp = DateTime.Now;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public TimeSync() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host"></param>
        public TimeSync(string host)
        {
            timeServer = host;
        }

        /// <summary>
        /// Connect to the time server and update system time
        /// </summary>
        /// <param name="UpdateSystemTime"></param>
        public void Connect(bool UpdateSystemTime)
        {
            try
            {
                // Resolve server address
#if false // remove compiler warning
				IPHostEntry hostadd = Dns.Resolve(TimeServer);
#else
                IPHostEntry hostadd = Dns.GetHostEntry(TimeServer);
#endif
                IPEndPoint EPhost = new IPEndPoint(hostadd.AddressList[0], 123);

                //Connect the time server
                UdpClient TimeSocket = new UdpClient();
                TimeSocket.Connect(EPhost);

                // Initialize data structure
                Initialize();
                TimeSocket.Send(NTPData, NTPData.Length);
                NTPData = TimeSocket.Receive(ref EPhost);
                if (!IsResponseValid())
                {
                    throw new Exception("Invalid response from " + TimeServer);
                }
                ReceptionTimestamp = DateTime.Now;
            }
            catch (SocketException e)
            {
                throw new Exception(e.Message);
            }

            // Update system time
            if (UpdateSystemTime)
            {
                SetTime();
            }
        }

        /// <summary>
        /// SyncState
        /// </summary>
        public enum SyncState
        {
            /// <summary>
            /// Idle
            /// </summary>
            Idle,
            /// <summary>
            /// DNSResolve
            /// </summary>
            DNSResolve,
            /// <summary>
            /// Connect
            /// </summary>
            Connect,
            /// <summary>
            /// Request
            /// </summary>
            Request,
            /// <summary>
            /// WaitResponse
            /// </summary>
            WaitResponse,
        }
        /// <summary>
        /// Sync Completed Notify
        /// </summary>
        /// <param name="success"></param>
        /// <param name="errorMessage"></param>
        public delegate void SyncCompletedNotify(bool success, String errorMessage);
        /// <summary>
        /// Sync State Change Notify
        /// </summary>
        /// <param name="state"></param>
        public delegate void SyncStateChangedNotify(SyncState state);
        /// <summary>
        /// Sync Completed Notify
        /// </summary>
        public event SyncCompletedNotify OnSyncCompletedNotify;
        /// <summary>
        /// Sync State Change Notify
        /// </summary>
        public event SyncStateChangedNotify OnSyncStateChangedNotify;

        private SyncState _ClientState = SyncState.Idle;
        /// <summary>
        /// 
        /// </summary>
        public SyncState ClientState
        {
            protected set
            {
                _ClientState = value;
                if (OnSyncStateChangedNotify != null)
                    OnSyncStateChangedNotify(_ClientState);
            }
            get
            {
                return _ClientState;
            }
        }
        private IPHostEntry remoteHost = null;
        private IPEndPoint remoteEP = null;
        private IAsyncResult asyncRes = null;
        private Socket udpSock = null;
        private String asyncErrMsg = String.Empty;
        private bool dnsResolvErr = false;
        private int udpBytesSent = 0;
        private bool udpSentErr = false;
        private int udpBytesRcvd = 0;
        private bool udpRecvErr = false;


        #region AsyncRes Poll Timer
        private System.Windows.Forms.Timer asyncResPollTmr = null;

        private void StartAsyncResPollTimer()
        {
            if (asyncResPollTmr == null)
            {
                asyncResPollTmr = new System.Windows.Forms.Timer();
                asyncResPollTmr.Interval = 500; // check every 0.5 seconds
                asyncResPollTmr.Tick += new EventHandler(OnAsyncResPollTmrTick);
            }
            asyncResPollTmr.Enabled = true;
        }

        private void StopAsyncResPollTimer()
        {
            if (asyncResPollTmr != null)
                asyncResPollTmr.Enabled = false;
        }

        private void OnAsyncResPollTmrTick(object sender, EventArgs e)
        {
            if (asyncRes.IsCompleted)
            {
                StopAsyncResPollTimer();
                NetworkAsyncInvokeCompleted();
            }
        }
        #endregion

        // Note: this call should still be from the ThreadPool
        private void NetworkAsyncCb(IAsyncResult ar)
        {
            switch (ClientState)
            {
                case SyncState.DNSResolve:
                    try
                    {
                        remoteHost = Dns.EndGetHostEntry(ar);
                    }
                    catch (SocketException se)
                    {
                        asyncErrMsg = "Socket error: code=" + se.ErrorCode.ToString();
                        dnsResolvErr = true;
                    }
                    catch (Exception e)
                    {
                        asyncErrMsg = "DNS Error(" + e.GetType().Name + "): " + e.Message;
                        dnsResolvErr = true;
                    }
                    break;
                case SyncState.Request:
                    try
                    {
                        udpBytesSent += udpSock.EndSendTo(ar);
                    }
                    catch (SocketException se)
                    {
                        asyncErrMsg = "Socket error: code=" + se.ErrorCode.ToString();
                        udpSentErr = true;
                    }
                    catch (Exception e)
                    {
                        asyncErrMsg = "Send Error(" + e.GetType().Name + "): " + e.Message;
                        udpSentErr = true;
                    }
                    break;
                case SyncState.WaitResponse:
                    try
                    {
                        EndPoint EP = remoteEP;
                        udpBytesRcvd += udpSock.EndReceiveFrom(ar, ref EP);
                    }
                    catch (SocketException se)
                    {
                        asyncErrMsg = "Socket error: code=" + se.ErrorCode.ToString();
                        udpRecvErr = true;
                    }
                    catch (Exception e)
                    {
                        asyncErrMsg = "Receive Error(" + e.GetType().Name + "): " + e.Message;
                        udpRecvErr = true;
                    }
                    break;
            }
        }

        private Socket InstantiateSocket()
        {
            Socket sock = null;
            try
            {
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
#if false
                LingerOption LingerOpts = new LingerOption(true, 0);
                sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, LingerOpts);
#endif
            }
            catch (Exception e)
            {
                String errMsg = e.GetType().Name + ": " + e.Message;
                if (sock != null)
                    sock.Close();
                sock = null;
            }
            return sock;
        }

        private void CloseSocket()
        {
            if (udpSock != null && udpSock.Connected)
            {
                udpSock.Close();
            }
            udpSock = null;
        }

        private void NetworkAsyncInvokeCompleted()
        {
            switch (ClientState)
            {
                case SyncState.DNSResolve:
                    // create Socket, connect, send SNTP packet
                    try
                    {
                        // DNS look up fail or something
                        if (remoteHost == null || remoteHost.AddressList.Length <= 0)
                        {
                            throw new ApplicationException("Unable to resolve NTP server name.\n" +
                                (dnsResolvErr ? (asyncErrMsg + "\n") : String.Empty) +
                                 "Please check server name");
                        }
                        udpSock = InstantiateSocket();
                        if (udpSock == null)
                        {
                            throw new ApplicationException("Failed to create network socket");
                        }
                        // assuming that 'Connect()' will finish instantly no matter what
                        ClientState = SyncState.Connect;
                        Initialize();
                        // Connect all but only send to one of them
                        for (int i = 0; i < remoteHost.AddressList.Length; i++)
                        {
                            remoteEP = new IPEndPoint(remoteHost.AddressList[i], 123);
                            udpSock.Connect(remoteEP);
                        }
                        ClientState = SyncState.Request;
                        udpSentErr = false;
                        udpBytesSent = 0;
                        asyncRes = udpSock.BeginSendTo(NTPData, 0, NTPDataLength, SocketFlags.None,
                                remoteEP, NetworkAsyncCb, null);
                        StartAsyncResPollTimer();
                    }
                    catch (ApplicationException ae)
                    {
                        if (OnSyncCompletedNotify != null)
                            OnSyncCompletedNotify(false, ae.Message);
                        CloseSocket();
                        ClientState = SyncState.Idle;
                    }
                    catch (SocketException se)
                    {
                        if (OnSyncCompletedNotify != null)
                            OnSyncCompletedNotify(false, "Unexpected socket error: " + se.ErrorCode.ToString());
                        CloseSocket();
                        ClientState = SyncState.Idle;
                    }
                    catch (Exception e)
                    {
                        if (OnSyncCompletedNotify != null)
                            OnSyncCompletedNotify(false, "Unexpected network error(" + e.GetType().Name + "): " + e.Message);
                        CloseSocket();
                        ClientState = SyncState.Idle;
                    }
                    break;
                case SyncState.Request:
                    try
                    {
                        if (udpSock == null || udpSock.Connected == false) // in case user cancel
                        {
                            throw new ApplicationException("User Canceled");
                        }
                        else if (udpSentErr)
                        {
                            throw new ApplicationException("Error sending request to NTP server" + "\n" + asyncErrMsg);
                        }
                        else if (udpBytesSent < NTPDataLength)
                        {
                            asyncRes = udpSock.BeginSendTo(NTPData, udpBytesSent,
                                NTPDataLength - udpBytesSent, SocketFlags.None,
                                 (EndPoint)remoteEP, NetworkAsyncCb, null);
                            StartAsyncResPollTimer();
                        }
                        else
                        {
                            ClientState = SyncState.WaitResponse;
                            udpRecvErr = false;
                            udpBytesRcvd = 0;
                            EndPoint EP = remoteEP;
                            asyncRes = udpSock.BeginReceiveFrom(NTPData, 0, NTPDataLength, SocketFlags.None,
                               ref EP, NetworkAsyncCb, null);
                            StartAsyncResPollTimer();
                        }
                    }
                    catch (ApplicationException ae)
                    {
                        if (OnSyncCompletedNotify != null)
                            OnSyncCompletedNotify(false, ae.Message);
                        CloseSocket();
                        ClientState = SyncState.Idle;
                    }
                    catch (SocketException se)
                    {
                        if (OnSyncCompletedNotify != null)
                            OnSyncCompletedNotify(false, "Unexpected socket error: " + se.ErrorCode.ToString());
                        CloseSocket();
                        ClientState = SyncState.Idle;
                    }
                    catch (Exception e)
                    {
                        if (OnSyncCompletedNotify != null)
                            OnSyncCompletedNotify(false, "Unexpected network error(" + e.GetType().Name + "): " + e.Message);
                        CloseSocket();
                        ClientState = SyncState.Idle;
                    }
                    break;
                case SyncState.WaitResponse:
                    try
                    {
                        if (udpSock == null || udpSock.Connected == false) // in case user cancel
                        {
                            if (OnSyncCompletedNotify != null)
                                OnSyncCompletedNotify(true, "User Canceled");
                        }
                        else if (udpRecvErr)
                        {
                            if (OnSyncCompletedNotify != null)
                                OnSyncCompletedNotify(false, "Error receiving response from NTP server" + "\n" + asyncErrMsg);
                        }
                        else if (udpBytesRcvd < NTPDataLength)
                        {
                            EndPoint EP = remoteEP;
                            asyncRes = udpSock.BeginReceiveFrom(NTPData, udpBytesRcvd, NTPDataLength - udpBytesRcvd,
                                SocketFlags.None, ref EP, NetworkAsyncCb, null);
                        }
                        else
                        {
                            // Process result
                            ProcessNTPResult();
                            if (OnSyncCompletedNotify != null)
                                OnSyncCompletedNotify(true, String.Empty);
                            CloseSocket();
                            ClientState = SyncState.Idle;
                        }
                    }
                    catch (ApplicationException ae)
                    {
                        if (OnSyncCompletedNotify != null)
                            OnSyncCompletedNotify(false, ae.Message);
                        CloseSocket();
                        ClientState = SyncState.Idle;
                    }
                    catch (SocketException se)
                    {
                        if (OnSyncCompletedNotify != null)
                            OnSyncCompletedNotify(false, "Unexpected socket error: " + se.ErrorCode.ToString());
                        CloseSocket();
                        ClientState = SyncState.Idle;
                    }
                    catch (Exception e)
                    {
                        if (OnSyncCompletedNotify != null)
                            OnSyncCompletedNotify(false, "Unexpected network error(" + e.GetType().Name + "): " + e.Message);
                        CloseSocket();
                        ClientState = SyncState.Idle;
                    }
                    break;
            }
        }

        private void ProcessNTPResult()
        {
            if (IsResponseValid() == false)
            {
                throw new ApplicationException("Invalid response from " + TimeServer);
            }
            ReceptionTimestamp = DateTime.Now;
            // Update system time
            SetTime();
        }
        /// <summary>
        /// Asynchronous Time Sync Start
        /// </summary>
        /// <returns></returns>
        public bool SyncStart()
        {
            bool Succ = false;
            switch (ClientState)
            {
                case SyncState.Idle:
                    // Remote Address
                    ClientState = SyncState.DNSResolve;
                    try
                    {
                        asyncRes = Dns.BeginGetHostEntry(TimeServer, NetworkAsyncCb, null);
                        StartAsyncResPollTimer();
                        Succ = true;
                    }
                    catch
                    {
                        Succ = false;
                    }
                    break;
                default:
                    Succ = false; // busy
                    break;
            }

            return Succ;
        }
        /// <summary>
        /// Asynchronous Time Sync Stop
        /// </summary>
        /// <returns></returns>
        public bool SyncStop()
        {
            bool Succ = false;

            switch (ClientState)
            {
                case SyncState.DNSResolve:
                case SyncState.Connect:
                    Succ = false; // not supported
                    break;
                case SyncState.Request:
                case SyncState.WaitResponse:
                    StopAsyncResPollTimer();
                    if (asyncRes.IsCompleted == false)
                    {
                        CloseSocket();
                        asyncRes.AsyncWaitHandle.WaitOne();
                    }
                    else // no asynchronous requests outstanding
                    {
                        CloseSocket();
                    }
                    ClientState = SyncState.Idle;
                    Succ = true;
                    break;
            }
            return Succ;
        }

        /// <summary>
        /// Check if the response from server is valid
        /// </summary>
        /// <returns></returns>
        public bool IsResponseValid()
        {
            if (NTPData.Length < NTPDataLength || Mode != Mode.Server)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Converts the object to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str;

            str = "Leap Indicator: ";
            switch (LeapIndicator)
            {
                case LeapIndicator.NoWarning:
                    str += "No warning";
                    break;
                case LeapIndicator.LastMinute61:
                    str += "Last minute has 61 seconds";
                    break;
                case LeapIndicator.LastMinute59:
                    str += "Last minute has 59 seconds";
                    break;
                case LeapIndicator.Alarm:
                    str += "Alarm Condition (clock not synchronized)";
                    break;
            }
            str += "\r\nVersion number: " + VersionNumber.ToString() + "\r\n";
            str += "Mode: ";
            switch (Mode)
            {
                case Mode.Unknown:
                    str += "Unknown";
                    break;
                case Mode.SymmetricActive:
                    str += "Symmetric Active";
                    break;
                case Mode.SymmetricPassive:
                    str += "Symmetric Pasive";
                    break;
                case Mode.Client:
                    str += "Client";
                    break;
                case Mode.Server:
                    str += "Server";
                    break;
                case Mode.Broadcast:
                    str += "Broadcast";
                    break;
            }
            str += "\r\nStratum: ";
            switch (Stratum)
            {
                case Stratum.Unspecified:
                case Stratum.Reserved:
                    str += "Unspecified";
                    break;
                case Stratum.PrimaryReference:
                    str += "Primary Reference";
                    break;
                case Stratum.SecondaryReference:
                    str += "Secondary Reference";
                    break;
            }
            str += "\r\nLocal time: " + TransmitTimestamp.ToString();
            str += "\r\nPrecision: " + Precision.ToString() + " ms";
            str += "\r\nPoll Interval: " + PollInterval.ToString() + " s";
            str += "\r\nReference ID: " + ReferenceID.ToString();
            str += "\r\nRoot Dispersion: " + RootDispersion.ToString() + " ms";
            str += "\r\nRound Trip Delay: " + RoundTripDelay.ToString() + " ms";
            str += "\r\nLocal Clock Offset: " + LocalClockOffset.ToString() + " ms";
            str += "\r\n";

            return str;
        }

        // SYSTEMTIME structure used by SetSystemTime
        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct SYSTEMTIME
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

        [DllImport("coredll.dll")]
        static extern bool SetLocalTime(ref SYSTEMTIME time);


        // Set system time according to transmit timestamp
        private void SetTime()
        {
            SYSTEMTIME st;

            DateTime trts = TransmitTimestamp;
            st.year = (short)trts.Year;
            st.month = (short)trts.Month;
            st.dayOfWeek = (short)trts.DayOfWeek;
            st.day = (short)trts.Day;
            st.hour = (short)trts.Hour;
            st.minute = (short)trts.Minute;
            st.second = (short)trts.Second;
            st.milliseconds = (short)trts.Millisecond;

            SetLocalTime(ref st);
        }

        private string timeServer;
        /// <summary>
        /// The URL of the time server we're connecting to
        /// </summary>
        public String TimeServer
        {
            get
            {
                return timeServer;
            }
            set { timeServer = value; }
        }

    }
}
#endif