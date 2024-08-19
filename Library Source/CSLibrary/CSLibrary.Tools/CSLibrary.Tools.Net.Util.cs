//================================================================================================
// Author:	    Arunkumar Viswanathan
// Date:		Aug 25, 2007
// Version:     1.0
//================================================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Win32;
using System.Net.Sockets;

namespace CSLibrary.Net
{
    class IpHlpNetworkAdapterUtil
    {
        /// <summary>
        /// The GetAdaptersInfo function retrieves adapter information for the local computer.
        /// http://msdn2.microsoft.com/en-us/library/aa365917.aspx
        /// </summary>
        /// <param name="pAdapterInfo">A pointer to a buffer that receives a linked list of IP_ADAPTER_INFO structures.</param>
        /// <param name="pOutBufLen">A pointer to a ULONG variable that specifies the size of the buffer pointed to by the pAdapterInfo parameter. If this size is insufficient to hold the adapter information, GetAdaptersInfo fills in this variable with the required size, and returns an error code of ERROR_BUFFER_OVERFLOW.</param>
        /// <returns>
        /// If the function succeeds, the return value is ERROR_SUCCESS.
        /// If the function fails, the return value is one of the following error codes.
        ///     ERROR_BUFFER_OVERFLOW
        ///     ERROR_INVALID_DATA
        ///     ERROR_INVALID_PARAMETER
        ///     ERROR_NO_DATA
        ///     ERROR_NOT_SUPPORTED
        ///     Other
        /// </returns>
        [DllImport("Iphlpapi.dll", CharSet = CharSet.Auto)]
        private extern static uint GetAdaptersInfo( IntPtr pAdapterInfo,
                                                    ref int pOutBufLen);

        /// <summary>
        /// The GetAdaptersAddresses function retrieves the addresses associated with the adapters on the local computer.
        /// http://msdn2.microsoft.com/en-us/library/aa365915.aspx
        /// </summary>
        /// <param name="Family">Refer MSDN</param>
        /// <param name="flags">Refer MSDN</param>
        /// <param name="Reserved">Refer MSDN</param>
        /// <param name="PAdaptersAddresses">Refer MSDN</param>
        /// <param name="pOutBufLen">Refer MSDN</param>
        /// <returns>
        /// If the function succeeds, the return value is ERROR_SUCCESS.
        /// If the function fails, the return value is one of the following error codes.
        ///     ERROR_ADDRESS_NOT_ASSOCIATED
        ///     ERROR_BUFFER_OVERFLOW
        ///     ERROR_INVALID_PARAMETER
        ///     ERROR_NOT_ENOUGH_MEMORY
        ///     ERROR_NO_DATA
        ///     Other
        /// </returns>
        [DllImport("Iphlpapi.dll", CharSet = CharSet.Auto)]
        private static extern uint GetAdaptersAddresses( uint Family, 
                                                        uint flags,
                                                        IntPtr Reserved,
                                                        IntPtr PAdaptersAddresses,
                                                        ref uint pOutBufLen);

        /// <summary>
        /// A wrapper for GetAdaptersInfo. http://msdn2.microsoft.com/en-us/library/aa365917.aspx  
        /// </summary>
        /// <param name="adapterInfoCollection">that will contain the IP_ADAPTER_INFO class objects</param>
        public void GetAdaptersInfo( out List<IP_ADAPTER_INFO> adapterInfoCollection )
        {
            IP_ADAPTER_INFO ipAdInfo = new IP_ADAPTER_INFO();
            adapterInfoCollection = new List<IP_ADAPTER_INFO>();
            Int32 size = 1024; //Marshal.SizeOf(ipAdInfo); //typeof(IP_ADAPTER_INFO)
            IntPtr pAdapterInfoBuffer = Marshal.AllocHGlobal(size);

            UInt32 result = GetAdaptersInfo(pAdapterInfoBuffer, ref size);

            if (result == IpHlpConstants.ERROR_BUFFER_OVERFLOW)
            {
                Marshal.FreeHGlobal(pAdapterInfoBuffer);
                pAdapterInfoBuffer = Marshal.AllocHGlobal(size);
                result = GetAdaptersInfo(pAdapterInfoBuffer, ref size);
            }

            // If GetAdaptersInfo does not return a success code after the second call throw an exception.
            if (result != IpHlpConstants.ERROR_SUCCESS)
            {
                throw new Win32Exception((Int32)result, "GetAdaptersInfo did not return ERROR_SUCCESS.");
            }

            // Add results only for the success cases...
            if ((result == IpHlpConstants.ERROR_SUCCESS) && (pAdapterInfoBuffer != IntPtr.Zero))
            {
                IntPtr pTempAdapterInfoBuffer = pAdapterInfoBuffer;

                do
                {
                    IP_ADAPTER_INFO adapterInfoBuffer = new IP_ADAPTER_INFO();
                    adapterInfoBuffer = (IP_ADAPTER_INFO)Marshal.PtrToStructure((IntPtr)pTempAdapterInfoBuffer, typeof(IP_ADAPTER_INFO));
                    adapterInfoCollection.Add(adapterInfoBuffer);

                    pTempAdapterInfoBuffer = (IntPtr)adapterInfoBuffer.Next;
                }
                while (pTempAdapterInfoBuffer != IntPtr.Zero);
            }
          

            Marshal.FreeHGlobal(pAdapterInfoBuffer);
        }

        /// <summary>
        /// A wrapper for GetAdaptersAddresses. http://msdn2.microsoft.com/en-us/library/aa365915.aspx        
        /// </summary>
        /// <param name="addressFamily">System.Net.Sockets.AddressFamily</param>
        /// <param name="gaaFlags">IpHlpEnumerations.GAA_FLAGS</param>
        /// <param name="adaptersAddressesCollection"></param>
        public void GetAdaptersAddresses( System.Net.Sockets.AddressFamily addressFamily, GAA_FLAGS gaaFlags, out List<IP_ADAPTER_ADDRESSES_XP2K3> adaptersAddressesCollection )
        {
            if (!IpHlpNetworkAdapterUtil.TryGetAdaptersAddresses())
            {
                throw new NotSupportedException("GetAdaptersAddresses is supported only in Microsoft Windows XP and beyond.");
            }
                
            adaptersAddressesCollection = new List<IP_ADAPTER_ADDRESSES_XP2K3>();
            UInt32 size = (UInt32)Marshal.SizeOf(typeof(IP_ADAPTER_ADDRESSES_XP2K3));
            IntPtr pAdaptersAddressesBuffer = Marshal.AllocHGlobal((Int32)size);

            uint result = GetAdaptersAddresses((UInt32)addressFamily, (UInt32)gaaFlags, (IntPtr)0, pAdaptersAddressesBuffer, ref size);

            if (result == IpHlpConstants.ERROR_BUFFER_OVERFLOW)
            {
                Marshal.FreeHGlobal(pAdaptersAddressesBuffer);
                pAdaptersAddressesBuffer = Marshal.AllocHGlobal((Int32)size);
                result = GetAdaptersAddresses((UInt32)addressFamily, (UInt32)gaaFlags, (IntPtr)0, pAdaptersAddressesBuffer, ref size);
            }

            // If GetAdaptersAddresses does not return a success code after the second call throw an exception.
            if (result != IpHlpConstants.ERROR_SUCCESS)
            {
                throw new Win32Exception((Int32)result, "GetAdaptersAddresses did not return ERROR_SUCCESS.");
            }

            // Add results only for the success cases...
            if ((result == IpHlpConstants.ERROR_SUCCESS) && (pAdaptersAddressesBuffer != IntPtr.Zero))
            {
                IntPtr pTempAdaptersAddressesBuffer = pAdaptersAddressesBuffer;

                do
                {
                    IP_ADAPTER_ADDRESSES_XP2K3 adaptersAddressesBuffer = new IP_ADAPTER_ADDRESSES_XP2K3();
                    adaptersAddressesBuffer = (IP_ADAPTER_ADDRESSES_XP2K3)Marshal.PtrToStructure((IntPtr)pTempAdaptersAddressesBuffer, typeof(IP_ADAPTER_ADDRESSES_XP2K3));
                    adaptersAddressesCollection.Add(adaptersAddressesBuffer);

                    pTempAdaptersAddressesBuffer = (IntPtr)adaptersAddressesBuffer.Next;
                }
                while (pTempAdaptersAddressesBuffer != IntPtr.Zero);
            }
        }

        /// <summary>
        /// Check if GetAdaptersAddresses can be called. 
        /// The GetAdaptersAddresses function is available only in Windows XP and beyond.
        /// </summary>
        /// <returns>true or false</returns>
        public static bool TryGetAdaptersAddresses()
        {
            OperatingSystem os = Environment.OSVersion;
            Version ver = os.Version;
            bool isSupported = false;

            // UnComment if you wish to see this information
             Debug.WriteLine("Operating System Found: " + os.ToString());
             Debug.WriteLine("OS platform id:         " + os.Platform.ToString());
             Debug.WriteLine("OS major version:       " + ver.Major.ToString());
             Debug.WriteLine("OS minor version:       " + ver.Minor.ToString());            

            // The GetAdaptersAddresses function is available only in Windows XP and beyond and hence the version check!
            //if ((os.Platform == PlatformID.Win32NT) && ((ver.Major == 5 && ver.Minor >= 1) || (ver.Major >= 6)))
            {
                isSupported = true;
                // Trace.WriteLine("GetAdaptersAddresses is supported in this operating system.");
            //}else{
                // Trace.WriteLine("GetAdaptersAddresses is not supported in this operating system.");
            }

            return isSupported;
        }
    }
}
