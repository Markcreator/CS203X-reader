//================================================================================================
// Author:	    Arunkumar Viswanathan
// Date:		Aug 25, 2007
// Version:     1.0
//================================================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.Net
{
    /// <summary>
    /// IF_TYPE - http://msdn2.microsoft.com/en-us/library/aa366058(vs.80).aspx
    /// IpIfCons.h defines all these types in the platform SDK
    /// </summary>
    enum IF_TYPE
    {
        IF_TYPE_OTHER = 1,   // None of the below
        IF_TYPE_REGULAR_1822 = 2,
        IF_TYPE_HDH_1822 = 3,
        IF_TYPE_DDN_X25 = 4,
        IF_TYPE_RFC877_X25 = 5,
        IF_TYPE_ETHERNET_CSMACD = 6,
        IF_TYPE_IS088023_CSMACD = 7,
        IF_TYPE_ISO88024_TOKENBUS = 8,
        IF_TYPE_ISO88025_TOKENRING = 9,
        IF_TYPE_ISO88026_MAN = 10,
        IF_TYPE_STARLAN = 11,
        IF_TYPE_PROTEON_10MBIT = 12,
        IF_TYPE_PROTEON_80MBIT = 13,
        IF_TYPE_HYPERCHANNEL = 14,
        IF_TYPE_FDDI = 15,
        IF_TYPE_LAP_B = 16,
        IF_TYPE_SDLC = 17,
        IF_TYPE_DS1 = 18, // DS1-MIB
        IF_TYPE_E1 = 19, // Obsolete; see DS1-MIB
        IF_TYPE_BASIC_ISDN = 20,
        IF_TYPE_PRIMARY_ISDN = 21,
        IF_TYPE_PROP_POINT2POINT_SERIAL = 22, // proprietary serial
        IF_TYPE_PPP = 23,
        IF_TYPE_SOFTWARE_LOOPBACK = 24,
        IF_TYPE_EON = 25, // CLNP over IP
        IF_TYPE_ETHERNET_3MBIT = 26,
        IF_TYPE_NSIP = 27,  // XNS over IP
        IF_TYPE_SLIP = 28,  // Generic Slip
        IF_TYPE_ULTRA = 29,  // ULTRA Technologies
        IF_TYPE_DS3 = 30,  // DS3-MIB
        IF_TYPE_SIP = 31,  // SMDS, coffee
        IF_TYPE_FRAMERELAY = 32,  // DTE only
        IF_TYPE_RS232 = 33,
        IF_TYPE_PARA = 34,  // Parallel port
        IF_TYPE_ARCNET = 35,
        IF_TYPE_ARCNET_PLUS = 36,
        IF_TYPE_ATM = 37,  // ATM cells
        IF_TYPE_MIO_X25 = 38,
        IF_TYPE_SONET = 39, // SONET or SDH
        IF_TYPE_X25_PLE = 40,
        IF_TYPE_ISO88022_LLC = 41,
        IF_TYPE_LOCALTALK = 42,
        IF_TYPE_SMDS_DXI = 43,
        IF_TYPE_FRAMERELAY_SERVICE = 44,  // FRNETSERV-MIB
        IF_TYPE_V35 = 45,
        IF_TYPE_HSSI = 46,
        IF_TYPE_HIPPI = 47,
        IF_TYPE_MODEM = 48, // Generic Modem
        IF_TYPE_AAL5 = 49, // AAL5 over ATM
        IF_TYPE_SONET_PATH = 50,
        IF_TYPE_SONET_VT = 51,
        IF_TYPE_SMDS_ICIP = 52, // SMDS InterCarrier Interface
        IF_TYPE_PROP_VIRTUAL = 53, // Proprietary virtual/internal
        IF_TYPE_PROP_MULTIPLEXOR = 54, // Proprietary multiplexing
        IF_TYPE_IEEE80212 = 55, // 100BaseVG
        IF_TYPE_FIBRECHANNEL = 56,
        IF_TYPE_HIPPIINTERFACE = 57,
        IF_TYPE_FRAMERELAY_INTERCONNECT = 58,  // Obsolete, use 32 or 44
        IF_TYPE_AFLANE_8023 = 59,  // ATM Emulated LAN for 802.3
        IF_TYPE_AFLANE_8025 = 60,  // ATM Emulated LAN for 802.5
        IF_TYPE_CCTEMUL = 61,  // ATM Emulated circuit
        IF_TYPE_FASTETHER = 62,  // Fast Ethernet (100BaseT)
        IF_TYPE_ISDN = 63,  // ISDN and X.25
        IF_TYPE_V11 = 64,  // CCITT V.11/X.21
        IF_TYPE_V36 = 65,  // CCITT V.36
        IF_TYPE_G703_64K = 66,  // CCITT G703 at 64Kbps
        IF_TYPE_G703_2MB = 67,  // Obsolete; see DS1-MIB
        IF_TYPE_QLLC = 68,  // SNA QLLC
        IF_TYPE_FASTETHER_FX = 69,  // Fast Ethernet (100BaseFX)
        IF_TYPE_CHANNEL = 70,
        IF_TYPE_IEEE80211 = 71,  // Radio spread spectrum
        IF_TYPE_IBM370PARCHAN = 72,  // IBM System 360/370 OEMI Channel
        IF_TYPE_ESCON = 73,  // IBM Enterprise Systems Connection
        IF_TYPE_DLSW = 74,  // Data Link Switching
        IF_TYPE_ISDN_S = 75,  // ISDN S/T interface
        IF_TYPE_ISDN_U = 76,  // ISDN U interface
        IF_TYPE_LAP_D = 77,  // Link Access Protocol D
        IF_TYPE_IPSWITCH = 78,  // IP Switching Objects
        IF_TYPE_RSRB = 79,  // Remote Source Route Bridging
        IF_TYPE_ATM_LOGICAL = 80,  // ATM Logical Port
        IF_TYPE_DS0 = 81,  // Digital Signal Level 0
        IF_TYPE_DS0_BUNDLE = 82,  // Group of ds0s on the same ds1
        IF_TYPE_BSC = 83,  // Bisynchronous Protocol
        IF_TYPE_ASYNC = 84,  // Asynchronous Protocol
        IF_TYPE_CNR = 85,  // Combat Net Radio
        IF_TYPE_ISO88025R_DTR = 86,  // ISO 802.5r DTR
        IF_TYPE_EPLRS = 87,  // Ext Pos Loc Report Sys
        IF_TYPE_ARAP = 88,  // Appletalk Remote Access Protocol
        IF_TYPE_PROP_CNLS = 89,  // Proprietary Connectionless Proto
        IF_TYPE_HOSTPAD = 90,  // CCITT-ITU X.29 PAD Protocol
        IF_TYPE_TERMPAD = 91,  // CCITT-ITU X.3 PAD Facility
        IF_TYPE_FRAMERELAY_MPI = 92,  // Multiproto Interconnect over FR
        IF_TYPE_X213 = 93,  // CCITT-ITU X213
        IF_TYPE_ADSL = 94,  // Asymmetric Digital Subscrbr Loop
        IF_TYPE_RADSL = 95,  // Rate-Adapt Digital Subscrbr Loop
        IF_TYPE_SDSL = 96,  // Symmetric Digital Subscriber Loop
        IF_TYPE_VDSL = 97,  // Very H-Speed Digital Subscrb Loop
        IF_TYPE_ISO88025_CRFPRINT = 98,  // ISO 802.5 CRFP
        IF_TYPE_MYRINET = 99,  // Myricom Myrinet
        IF_TYPE_VOICE_EM = 100, // Voice recEive and transMit
        IF_TYPE_VOICE_FXO = 101, // Voice Foreign Exchange Office
        IF_TYPE_VOICE_FXS = 102, // Voice Foreign Exchange Station
        IF_TYPE_VOICE_ENCAP = 103, // Voice encapsulation
        IF_TYPE_VOICE_OVERIP = 104, // Voice over IP encapsulation
        IF_TYPE_ATM_DXI = 105, // ATM DXI
        IF_TYPE_ATM_FUNI = 106, // ATM FUNI
        IF_TYPE_ATM_IMA = 107, // ATM IMA
        IF_TYPE_PPPMULTILINKBUNDLE = 108, // PPP Multilink Bundle
        IF_TYPE_IPOVER_CDLC = 109, // IBM ipOverCdlc
        IF_TYPE_IPOVER_CLAW = 110, // IBM Common Link Access to Workstn
        IF_TYPE_STACKTOSTACK = 111, // IBM stackToStack
        IF_TYPE_VIRTUALIPADDRESS = 112, // IBM VIPA
        IF_TYPE_MPC = 113, // IBM multi-proto channel support
        IF_TYPE_IPOVER_ATM = 114, // IBM ipOverAtm
        IF_TYPE_ISO88025_FIBER = 115, // ISO 802.5j Fiber Token Ring
        IF_TYPE_TDLC = 116, // IBM twinaxial data link control
        IF_TYPE_GIGABITETHERNET = 117,
        IF_TYPE_HDLC = 118,
        IF_TYPE_LAP_F = 119,
        IF_TYPE_V37 = 120,
        IF_TYPE_X25_MLP = 121, // Multi-Link Protocol
        IF_TYPE_X25_HUNTGROUP = 122, // X.25 Hunt Group
        IF_TYPE_TRANSPHDLC = 123,
        IF_TYPE_INTERLEAVE = 124, // Interleave channel
        IF_TYPE_FAST = 125, // Fast channel
        IF_TYPE_IP = 126, // IP (for APPN HPR in IP networks)
        IF_TYPE_DOCSCABLE_MACLAYER = 127, // CATV Mac Layer
        IF_TYPE_DOCSCABLE_DOWNSTREAM = 128, // CATV Downstream interface
        IF_TYPE_DOCSCABLE_UPSTREAM = 129, // CATV Upstream interface
        IF_TYPE_A12MPPSWITCH = 130, // Avalon Parallel Processor
        IF_TYPE_TUNNEL = 131, // Encapsulation interface
        IF_TYPE_COFFEE = 132, // Coffee pot
        IF_TYPE_CES = 133, // Circuit Emulation Service
        IF_TYPE_ATM_SUBINTERFACE = 134, // ATM Sub Interface
        IF_TYPE_L2_VLAN = 135, // Layer 2 Virtual LAN using 802.1Q
        IF_TYPE_L3_IPVLAN = 136, // Layer 3 Virtual LAN using IP
        IF_TYPE_L3_IPXVLAN = 137, // Layer 3 Virtual LAN using IPX
        IF_TYPE_DIGITALPOWERLINE = 138, // IP over Power Lines
        IF_TYPE_MEDIAMAILOVERIP = 139, // Multimedia Mail over IP
        IF_TYPE_DTM = 140, // Dynamic syncronous Transfer Mode
        IF_TYPE_DCN = 141, // Data Communications Network
        IF_TYPE_IPFORWARD = 142, // IP Forwarding Interface
        IF_TYPE_MSDSL = 143, // Multi-rate Symmetric DSL
        IF_TYPE_IEEE1394 = 144, // IEEE1394 High Perf Serial Bus
        IF_TYPE_RECEIVE_ONLY = 145 // TV adapter type
    }

    /// <summary>
    /// The operational status for the interface as defined in RFC 2863.
    /// For more information, see http://www.ietf.org/rfc/rfc2863.txt.
    /// This member can be one of the values from the IF_OPER_STATUS enumeration type defined in the Iftypes.h header file. On Windows Vista and later, the header files were reorganized and this enumeration id defined in the Ifdef.h header file.
    /// </summary>
    enum OPERSTATUS
    {
        IfOperStatusUp = 1, // The interface is up and able to pass packets.
        IfOperStatusDown = 2, // The interface is down and not in a condition to pass packets.
        IfOperStatusTesting = 3, // The interface is in testing mode.
        IfOperStatusUnknown = 4, // The operational status of the interface is unkwown.
        IfOperStatusDormant = 5, // The interface is not actually in a condition to pass packets (it is not up), but is in a pending state, waiting for some external event.

        IfOperStatusNotPresent = 6, // A refinement on the IfOperStatusDown state which indicates that the relevant interface
        // is down specifically because some component (typically, a hardware component) is not present in the managed system. 

        IfOperStatusLowerLayerDown = 7  //A refinement on the IfOperStatusDown state. 
        //This new state indicates that this interface runs on top of one or more other interfaces and that this interface is down specifically because one or more of these lower-layer interfaces are down. 
    }

    /// <summary>
    /// The SCOPE_LEVEL enumeration is used with the IP_ADAPTER_ADDRESSES structure to identify scope levels for IPv6 addresses.
    /// http://msdn2.microsoft.com/en-us/library/aa366357(VS.80).aspx
    /// </summary>
    enum SCOPE_LEVEL
    {
        ScopeLevelInterface = 1,
        ScopeLevelLink = 2,
        ScopeLevelSubnet = 3,
        ScopeLevelAdmin = 4,
        ScopeLevelSite = 5,
        ScopeLevelOrganization = 8,
        ScopeLevelGlobal = 14
    }

    /// <summary>
    /// IP_PREFIX_ORIGIN - http://msdn2.microsoft.com/en-us/library/aa366281.aspx
    /// </summary>
    enum IP_PREFIX_ORIGIN
    {
        IpPrefixOriginOther = 0,
        IpPrefixOriginManual,
        IpPrefixOriginWellKnown,
        IpPrefixOriginDhcp,
        IpPrefixOriginRouterAdvertisement
    }

    // 
    /// <summary>
    /// IP_SUFFIX_ORIGIN - http://msdn2.microsoft.com/en-us/library/aa366283.aspx 
    /// </summary>
    enum IP_SUFFIX_ORIGIN
    {
        IpSuffixOriginOther = 0,
        IpSuffixOriginManual,
        IpSuffixOriginWellKnown,
        IpSuffixOriginDhcp,
        IpSuffixOriginLinkLayerAddress,
        IpSuffixOriginRandom
    }

    /// <summary>
    /// IP_DAD_STATE - http://msdn2.microsoft.com/en-us/library/aa366069.aspx 
    /// </summary>
    enum IP_DAD_STATE
    {
        IpDadStateInvalid = 0,
        IpDadStateTentative,
        IpDadStateDuplicate,
        IpDadStateDeprecated,
        IpDadStatePreferred
    }

    /// <summary>
    /// Flags used as argument to GetAdaptersAddresses().
    /// http://msdn2.microsoft.com/en-us/library/aa365915.aspx
    /// "SKIP" flags are added when the default is to include the information.
    /// "INCLUDE" flags are added when the default is to skip the information.
    /// All the values are obtained directly from IpTypes.h file in the platform SDK.
    /// </summary>
    [Flags]
    enum GAA_FLAGS
    {
        GAA_FLAG_DEFAULT = 0x0000,      // THIS IS MY OWN VALUE as C# does not have default params
        GAA_FLAG_SKIP_UNICAST = 0x0001,
        GAA_FLAG_SKIP_ANYCAST = 0x0002,
        GAA_FLAG_SKIP_MULTICAST = 0x0004,
        GAA_FLAG_SKIP_DNS_SERVER = 0x0008,
        GAA_FLAG_INCLUDE_PREFIX = 0x0010,
        GAA_FLAG_SKIP_FRIENDLY_NAME = 0x0020,
        GAA_FLAG_INCLUDE_WINS_INFO = 0x0040,
        GAA_FLAG_INCLUDE_GATEWAYS = 0x0080,
        GAA_FLAG_INCLUDE_ALL_INTERFACES = 0x0100,
        GAA_FLAG_INCLUDE_ALL_COMPARTMENTS = 0x0200,
        GAA_FLAG_INCLUDE_TUNNEL_BINDINGORDER = 0x0400
    }
}
