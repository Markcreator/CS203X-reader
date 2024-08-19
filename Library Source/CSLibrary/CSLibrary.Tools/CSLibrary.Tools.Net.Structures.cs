//================================================================================================
// Author:	    Arunkumar Viswanathan
// Date:		Aug 25, 2007
// Version:     1.0
//================================================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CSLibrary.Net
{
    // TODO: Refactor sockaddr and sockaddripv6 to their correct formats if necessary
    // http://msdn2.microsoft.com/en-us/library/ms740496.aspx

    /// <summary>
    /// Structure used to store IPV4 addresses.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct SOCKADDR
    {
        public Int32 sa_family;       /* address family */
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] sa_data;         /* up to 4 bytes of direct address */
    };

    /// <summary>
    /// Structure used to store IPV6 addresses.
    /// I made it 16 bytes to get rid of any zone ids if present after the address.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct SOCKADDRIPV6
    {
        public Int64 sa_family;       /* address family */
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] sa_data;         /* up to 16 bytes of direct address */
    };

    /// <summary>
    /// SOCKET_ADDRESS - http://msdn2.microsoft.com/en-us/library/ms740507.aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct SOCKET_ADDRESS
    {
        public IntPtr lpSockAddr;
        public Int32 iSockaddrLength;
    }

    /// <summary>
    /// IP_ADAPTER_UNICAST_ADDRESS - http://msdn2.microsoft.com/en-us/library/aa366066.aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct IP_ADAPTER_UNICAST_ADDRESS
    {
        public UInt32 Length;
        public UInt32 Flags;

        public IntPtr Next;
        public SOCKET_ADDRESS Address;
        public IP_PREFIX_ORIGIN PrefixOrigin;
        public IP_SUFFIX_ORIGIN SuffixOrigin;
        public IP_DAD_STATE DadState;
        public UInt32 ValidLifetime;
        public UInt32 PreferredLifetime;
        public UInt32 LeaseLifetime;
        public Byte OnLinkPrefixLength;
    }

    /// <summary>
    /// IP_ADAPTER_ANYCAST_ADDRESS - http://msdn2.microsoft.com/en-us/library/aa366059.aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct IP_ADAPTER_ANYCAST_ADDRESS
    {
        //UInt64 Alignment; // reserved
        public UInt32 Length;
        public UInt32 Flags;

        public IntPtr Next;
        public SOCKET_ADDRESS Address;
    }

    /// <summary>
    /// IP_ADAPTER_MULTICAST_ADDRESS - http://msdn2.microsoft.com/en-us/library/aa366063.aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct IP_ADAPTER_MULTICAST_ADDRESS
    {
        //UInt64 Alignment; // reserved
        public UInt32 Length;
        public UInt32 Flags;

        public IntPtr Next;
        public SOCKET_ADDRESS Address;
    }

    /// <summary>
    /// IP_ADAPTER_DNS_SERVER_ADDRESS - http://msdn2.microsoft.com/en-us/library/aa366060.aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct IP_ADAPTER_DNS_SERVER_ADDRESS
    {
        //UInt64 Alignment; // reserved
        public UInt32 Length;
        public UInt32 Flags;

        public IntPtr Next;
        public SOCKET_ADDRESS Address;
    }

    /// <summary>
    /// IP_ADDRESS_STRING - http://msdn2.microsoft.com/en-us/library/aa366067.aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)] //CharSet.Ansi
    class IP_ADDRESS_STRING
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string address;
    };

    /// <summary>
    /// IP_MASK_STRING - a clone of IP_ADDRESS_STRING used for retrieving subnet masks.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    class IP_MASK_STRING
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string address;
    };

    /// <summary>
    /// IP_ADDR_STRING - http://msdn2.microsoft.com/en-us/library/aa366068.aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    class IP_ADDR_STRING
    {
        public int Next;      /* struct _IP_ADDR_STRING* */
        public IP_ADDRESS_STRING IpAddress;
        public IP_MASK_STRING IpMask;
        public uint Context;
    }

    /// <summary>
    /// IP_ADAPTER_INFO - http://msdn2.microsoft.com/en-us/library/aa366062.aspx
    /// I have added _LEGACY to indicate that it is being deprecated by the IP_ADAPTER_ADDRESSES structure starting from Windows XP 
    /// </summary>
    /// struct _IP_ADAPTER_INFO
    [StructLayout(LayoutKind.Sequential)] //, CharSet = CharSet.Auto
    class IP_ADAPTER_INFO
    {
        public int  Next;
        public uint ComboIndex;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (IpHlpConstants.MAX_ADAPTER_NAME_LENGTH + 4))] //$$ 256
        public String AdapterName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = IpHlpConstants.MAX_ADAPTER_DESCRIPTION_LENGTH + 4)] //128+4
        public String Description;
        public int AddressLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IpHlpConstants.MAX_ADAPTER_ADDRESS_LENGTH)] //8
        public byte[] Address; //[]
        public int Index;
        public int Type;
        public int DhcpEnabled;
        public uint CurrentIpAddress; // RESERVED
        public IP_ADDR_STRING IpAddressList;
        public IP_ADDR_STRING GatewayList;
        public IP_ADDR_STRING DhcpServer;
        [MarshalAs(UnmanagedType.Bool)]
        public bool HaveWins;
        public IP_ADDR_STRING PrimaryWinsServer;
        public IP_ADDR_STRING SecondaryWinsServer;
        public uint LeaseObtained;  // time_t
        public uint LeaseExpires;   // time_t
    }

    /// <summary>
    /// IP_ADAPTER_ADDRESSES - http://msdn2.microsoft.com/en-us/library/aa366058.aspx
    /// I have added _XP2K3 to indicate that this structure has additional entities starting Windows Vista
    /// This structure is valid for Windows XP and Windows* 2003 and is backward compatible with Vista. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    class IP_ADAPTER_ADDRESSES_XP2K3
    {
        public uint Length;
        public uint IfIndex;
        public IntPtr Next;

        public IntPtr AdapterName;
        public IntPtr FirstUnicastAddress;
        public IntPtr FirstAnycastAddress;
        public IntPtr FirstMulticastAddress;
        public IntPtr FirstDnsServerAddress;

        public IntPtr DnsSuffix;
        public IntPtr Description;

        public IntPtr FriendlyName;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public Byte[] PhysicalAddress;

        public uint PhysicalAddressLength;
        public uint flags;
        public uint Mtu;
        public uint IfType;

        public uint OperStatus;

        public uint Ipv6IfIndex;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public uint[] ZoneIndices;

        public IntPtr FirstPrefix;            
    }
   
}
