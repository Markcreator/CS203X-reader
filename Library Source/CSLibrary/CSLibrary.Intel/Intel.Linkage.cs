/*******************************************************************************
 *  INTEL CONFIDENTIAL
 *  Copyright 2007 Intel Corporation All Rights Reserved.
 *
 *  The source code contained or described herein and all documents related to
 *  the source code ("Material") are owned by Intel Corporation or its suppliers
 *  or licensors. Title to the Material remains with Intel Corporation or its
 *  suppliers and licensors. The Material may contain trade secrets and
 *  proprietary and confidential information of Intel Corporation and its
 *  suppliers and licensors, and is protected by worldwide copyright and trade
 *  secret laws and treaty provisions. No part of the Material may be used,
 *  copied, reproduced, modified, published, uploaded, posted, transmitted,
 *  distributed, or disclosed in any way without Intel's prior express written
 *  permission. 
 *  
 *  No license under any patent, copyright, trade secret or other intellectual
 *  property right is granted to or conferred upon you by disclosure or delivery
 *  of the Materials, either expressly, by implication, inducement, estoppel or
 *  otherwise. Any license under such intellectual property rights must be
 *  express and approved by Intel in writing.
 *
 *  Unless otherwise agreed by Intel in writing, you may not remove or alter
 *  this notice or any other notice embedded in Materials by Intel or Intel's
 *  suppliers or licensors in any way.
 ******************************************************************************/



using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

using CSLibrary.Constants;
using CSLibrary.Structures;

using INT8U = System.Byte;
using INT16U = System.UInt16;
using INT32U = System.UInt32;
using INT64U = System.UInt64;
using UINT8 = System.Byte;
using UINT16 = System.UInt16;
using UINT32 = System.UInt32;
using UINT64 = System.UInt64;


namespace CSLibrary
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RfidHandle
    {
        internal IntPtr ptr;
        public static explicit operator IntPtr(RfidHandle handle) { return handle.ptr; }
        public static implicit operator RfidHandle(IntPtr p) { RfidHandle handle = new RfidHandle(); handle.ptr = p; return handle; }
    }
#if USE_LOADLIBRARY
    /// <summary>
    /// Native Class Library Linkage
    /// </summary>
    sealed class Linkage
    {
        private RfidHandle pHandle;
#if CS101
        /// <summary>
        /// Initializes the RFID Reader Library
        /// </summary>
        /// <param name="pVersion">pointer to structure that on return will contain the
        /// version of the library.  May be NULL if not required by application.</param>
        /// <param name="mode">library startup flags.  May be zero or a combination of the
        /// following:
        /// RFID_FLAG_LIBRARY_EMULATION - libary should be run in emulation mode</param>
        /// <returns></returns>
        public Result Startup
        (
            [In, Out] LibraryVersion pVersion,
            [In]      LibraryMode mode
        )
        {
            return Native.RFID_Startup(pVersion, mode);
        }
#elif CS203
        public IntPtr dllModule = IntPtr.Zero;
        /// <summary>
        /// Initializes the RFID Reader Library
        /// </summary>
        /// <param name="pVersion">pointer to structure that on return will contain the
        /// version of the library.  May be NULL if not required by application</param>
        /// <param name="mode">library startup flags.  May be zero or a combination of the
        /// following:
        /// RFID_FLAG_LIBRARY_EMULATION - libary should be run in emulation mode</param>
        /// <param name="ip">IP Address</param>
        /// <param name="port">Connect Port</param>
        /// <param name="timeout">Connection Timeout</param>
        /// <returns></returns>
        public Result Startup
        (
            [In, Out]   LibraryVersion pVersion,
            [In]        LibraryMode mode,
            [In]        String ip,
            [In]        UInt32 port,
            [In]        UInt32 timeout
        )
        {
            Result status = Result.OK;
            try
            {
                if (ip != null || ip.Length > 0)
                {
                    status = Native.RFID_Startup(dllModule, out pHandle, pVersion, mode, (ulong)System.Net.IPAddress.Parse(ip).Address, port, timeout);
                }
            }
            catch (ArgumentNullException)
            {
                status = Result.INVALID_PARAMETER;
            }
            catch (FormatException)
            {
                status = Result.INVALID_PARAMETER;
            }
            catch (Exception)
            {
                status = Result.SYSTEM_CATCH_EXCEPTION;
            }
            return status;
        }
#endif

        /// <summary>
        /// Shuts down RFID Reader Library, cleaning up all resources including closing
        /// all open radio handles and returning radios to idle.
        /// 
        /// NOTE: This function must not be called from the packet callback function.
        /// 
        /// </summary>
        /// <returns></returns>
        public Result Shutdown()
        {
#if CS203
            return Native.RFID_Shutdown(dllModule);
#elif CS101
            return Native.RFID_Shutdown();
#endif
        }


        /// <summary>
        /// Retrieves the list of radio modules attached to the system.
        /// </summary>
        /// <param name="pRadioEnum">pointer to a buffer into which attached radio information will
        /// be placed.  On input, pBuffer->totalLength must specify the length of
        /// the buffer.  If the buffer is not large enough, on return
        /// pBuffer->totalLength will contain the number of bytes required.</param>
        /// <param name="flags">Reserved for future use.  Set to zero.</param>
        /// <returns></returns>
        public Result RetrieveAttachedRadiosList
        (
            [In, Out] RadioEnumeration pRadioEnum,
            [In]      UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBuf = IntPtr.Zero;
            Int32 szTot = 128;            // Rnd value chosen for initial size

            try
            {
                lpBuf = Marshal.AllocHGlobal(szTot);

                while (true)
                {
                    Marshal.WriteInt32(lpBuf, 0, (int)pRadioEnum.length);
                    Marshal.WriteInt32(lpBuf, 4, szTot);
#if CS203
                    Result = Native.RFID_RetrieveAttachedRadiosList(dllModule, pHandle, lpBuf, flags);
#elif CS101
                    Result = Native.RFID_RetrieveAttachedRadiosList(lpBuf, flags);
#endif
                    if (Result.BUFFER_TOO_SMALL == Result)
                    {
                        szTot = szTot * 2;
                        lpBuf = Marshal.ReAllocHGlobal(lpBuf, new IntPtr(szTot));
                    }
                    else if (Result.OK != Result)
                    {
                        break;
                    }
                    else
                    {
                        // pRadioEnum.length      = set during obj construction
                        pRadioEnum.totalLength = (UInt32)Marshal.ReadInt32(lpBuf, 4);
                        pRadioEnum.countRadios = (UInt32)Marshal.ReadInt32(lpBuf, 8);

                        pRadioEnum.radioInfo = new RadioInformation[pRadioEnum.countRadios];

                        IntPtr infoPtrArray = Marshal.ReadIntPtr(new IntPtr(lpBuf.ToInt64() + 12));

                        int index = 0;

                        for (index = 0; index < pRadioEnum.countRadios; ++index)
                        {
                            IntPtr infoPtr =
                                Marshal.ReadIntPtr
                                    (
                                        new IntPtr
                                        (
                                            lpBuf.ToInt64() +
                                            (
                                                infoPtrArray.ToInt64() - lpBuf.ToInt64() +
                                                (
                                                    IntPtr.Size * index
                                                )
                                            )
                                        )
                                    );

                            pRadioEnum.radioInfo[index] = new RadioInformation();

                            pRadioEnum.radioInfo[index].length
                                = (UInt32)Marshal.ReadInt32(infoPtr, 0);
                            pRadioEnum.radioInfo[index].driverVersion.major
                                = (UInt32)Marshal.ReadInt32(infoPtr, 4);
                            pRadioEnum.radioInfo[index].driverVersion.minor
                                = (UInt32)Marshal.ReadInt32(infoPtr, 8);
                            pRadioEnum.radioInfo[index].driverVersion.patch
                                = (UInt32)Marshal.ReadInt32(infoPtr, 12);
                            pRadioEnum.radioInfo[index].cookie
                                = (UInt32)Marshal.ReadInt32(infoPtr, 16);
                            pRadioEnum.radioInfo[index].idLength
                                = (UInt32)Marshal.ReadInt32(infoPtr, 20) - 1;

                            IntPtr idPtr = Marshal.ReadIntPtr(new IntPtr(infoPtr.ToInt64() + 24));

                            // We are removing the last byte ( char ) from the id as the
                            // current usb and serial drivers actually return a null
                            // terminated ( ascii ) character string...

                            pRadioEnum.radioInfo[index].uniqueId =
                                new Byte[pRadioEnum.radioInfo[index].idLength];

                            Marshal.Copy
                            (
                                idPtr,
                                pRadioEnum.radioInfo[index].uniqueId,
                                0,
                                (int)pRadioEnum.radioInfo[index].idLength
                            );
                        }

                        break;
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBuf)
            {
                Marshal.FreeHGlobal(lpBuf);
            }

            return Result;
        }




        /// <summary>
        /// Requests explicit control of a radio.
        /// </summary>
        /// <param name="cookie">the unique cookie for the radio to open.  This cookie was returned
        /// in the RFID_RADIO_INFO structure that was returned from a call to
        /// RFID_RetrieveAttachedRadiosList.</param>
        /// <param name="pHandle">a pointer to a radio pHandle that upon successful return will
        /// contain the pHandle the application will subsequently use to refer to the
        /// radio.
        /// Must not be NULL.</param>
        /// <param name="mode">radio open flags.  May be zero or a combination of the following:
        /// RFID_FLAG_MAC_EMULATION - instruct MAC to run in radio-emulation mode
        /// RFID_FLAG_SOFT_RESET_MAC - reset the MAC before returing the pHandle</param>
        /// <returns></returns>
        public Result RadioOpen
        (
            [In]          UInt32 cookie,
            [In]          MacMode mode
        )
        {
#if CS203
            return Native.RFID_RadioOpen(dllModule, pHandle, cookie, mode);
#elif CS101
            return Native.RFID_RadioOpen(cookie, ref pHandle, mode);
#endif
        }



        /// <summary>
        /// Release control of a previously-opened radio.  On close, any currently-
        /// executing or outstanding requests are cancelled and the radio is returned
        /// to idle state.
        /// 
        /// NOTE: This function must not be called from the packet callback function.
        /// 
        /// </summary>
        /// <param name="pHandle">the pHandle to the radio to close.  This is the pHandle returned
        /// from a successful call to RFID_RadioOpen.  If the pHandle doesn't
        /// reference a currently-opened radio, function does nothing and returns
        /// success.</param>
        /// <returns></returns>
        public Result RadioClose
        (
        )
        {
#if CS203
            return Native.RFID_RadioClose(dllModule, pHandle);
#elif CS101
            return Native.RFID_RadioClose(pHandle);
#endif
        }



        /// <summary>
        /// Sets the low-level configuration parameter for the radio module.  Radio
        /// configuration parameters may not be set while a radio module is executing
        /// a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which low-level configuration parameter is to
        /// be set.  This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="param">the configuration parameter to set</param>
        /// <param name="val">the value to which the configuration parameter will be set</param>
        /// <returns></returns>
        public Result RadioSetConfigurationParameter
        (
            [In] UInt16 param,
            [In] UInt32 val
        )
        {
#if CS203
            return Native.RFID_RadioSetConfigurationParameter(dllModule, pHandle, param, val);
#elif CS101
            return Native.RFID_RadioSetConfigurationParameter(param, val);
#endif
        }



        /// <summary>
        /// Retrieves a low-level radio module configuration parameter.  Radio
        /// configuration parameters may not be retrieved while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which low-level configuration parameter is to
        /// be retrieved.  This is the pHandle from a successful call to
        /// RFID_RadioOpen.</param>
        /// <param name="param">parameter to retrieve</param>
        /// <param name="val">pointer to variable that will receive configuration parameter
        /// value.  Must not be NULL.</param>
        /// <returns></returns>
        public Result RadioGetConfigurationParameter
        (
            [In]          UInt16 param,
            [In, Out] ref UInt32 val
        )
        {
#if CS203
            return Native.RFID_RadioGetConfigurationParameter(dllModule, pHandle, param, ref val);
#elif CS101
            return Native.RFID_RadioGetConfigurationParameter(param, ref val);
#endif
        }




        /// <summary>
        /// Sets the radio's operation mode.  An RFID radio module operation mode
        /// will remain in effect until it is explicitly changed via
        /// RFID_RadioSetOperationMode.  The operation mode may not be set while a
        /// radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which operation mode will be set.  This is
        /// the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="mode">the operation mode for the radio</param>
        /// <returns></returns>
        public Result RadioSetOperationMode
        (
            [In] RadioOperationMode mode
        )
        {
#if CS203
            return Native.RFID_RadioSetOperationMode(dllModule, pHandle, mode);
#elif CS101
            return Native.RFID_RadioSetOperationMode(mode);
#endif
        }



        /// <summary>
        /// Retrieves the radio's operation mode.  The operation mode may not be
        /// retrieved while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which operation mode is requested.  This is
        /// the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="mode">pointer to a variable that will on return will contain the
        /// current operation mode for the radio.  Must not be NULL.</param>
        /// <returns></returns>
        public Result RadioGetOperationMode
        (
            [In, Out] ref RadioOperationMode mode
        )
        {
            try
            {
#if CS203
                return Native.RFID_RadioGetOperationMode(dllModule, pHandle, ref mode);
#elif CS101
                return Native.RFID_RadioGetOperationMode(ref mode);
#endif
            }
            catch
            {
                // Error auto-unmarshal encountered value that could
                // not be translated to a RadioOperationType

                mode = RadioOperationMode.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }




        /// <summary>
        /// Sets the radio module's power state (not to be confused with the antenna
        /// RF power).  The power state may not be set while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which power state is to be set.  This is the
        /// pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="state">the power state for the radio</param>
        /// <returns></returns>
        public Result RadioSetPowerState
        (
            [In] RadioPowerState state
        )
        {
#if CS203
            return Native.RFID_RadioSetPowerState(dllModule, pHandle, state);
#elif CS101
            return Native.RFID_RadioSetPowerState(state);
#endif
        }




        /// <summary>
        /// Retrieves the radio module's power state (not to be confused with the
        /// antenna RF power).  The power state may not be retrieved while a radio
        /// module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle for which power state is requested.  This is the pHandle
        /// from a successful call to RFID_RadioOpen.</param>
        /// <param name="state">a pointer to a variable that on return will contain the radio
        /// module's power state.  Must not be NULL.</param>
        /// <returns></returns>
        public Result RadioGetPowerState
        (
            [In, Out] ref RadioPowerState state
        )
        {
            try
            {
#if CS203
                return Native.RFID_RadioGetPowerState(dllModule, pHandle, ref state);
#elif CS101
                return Native.RFID_RadioGetPowerState(ref state);
#endif
            }
            catch
            {
                // Error auto-unmarshal encountered value that could
                // not be translated to a RadioPowerState

                state = RadioPowerState.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }




        /// <summary>
        /// Sets the current link profile for the radio module.  The curren link
        /// profile may not be set while a radio module is executing a tag-protocol
        /// operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which current link profile is to be set.  This
        /// is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="profile">the profile to make the current one</param>
        /// <returns></returns>
        public Result RadioSetCurrentLinkProfile
        (
            [In] UInt32 profile
        )
        {
#if CS203
            return Native.RFID_RadioSetCurrentLinkProfile(dllModule, pHandle, profile);
#elif CS101
            return Native.RFID_RadioSetCurrentLinkProfile(profile);
#endif
        }



        /// <summary>
        /// Retrieves the current link profile for the radio module.  The current link
        /// profile may not be retrieved while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which current link profile is to be retrieved.
        /// This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="profile">a pointer to an unsigned 32-bit integer that will receive
        /// the current profile.  Must not be NULL.</param>
        /// <returns></returns>
        public Result RadioGetCurrentLinkProfile
        (
            [In, Out] ref UInt32 profile
        )
        {
#if CS203
            return Native.RFID_RadioGetCurrentLinkProfile(dllModule, pHandle, ref profile);
#elif CS101
            return Native.RFID_RadioGetCurrentLinkProfile(ref profile);
#endif
        }



        /// <summary>
        /// Retrieves the information for the specified link profile for the radio
        /// module.  A link profile may not be retrieved while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which link profile information is to be
        /// retrieved.  This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="profile">the link profile for which information will be retrieved</param>
        /// <param name="profileInfo">a pointer to a structure that will be filled in with link
        /// profile information.  Must not be NULL.</param>
        /// <returns></returns>
        public Result RadioGetLinkProfile
        (
            [In]      UInt32 profile,
            [In, Out] RadioLinkProfile profileInfo
        )
        {
            Result Result = Result.OK;
            IntPtr lpBuf = IntPtr.Zero;

            try
            {
                lpBuf = Marshal.AllocHGlobal((int)profileInfo.length);  // sizeof(RFID_RADIO_LINK_PROFILE)

                Marshal.WriteInt32(lpBuf, 0, (int)profileInfo.length);
#if CS203
                Result = Native.RFID_RadioGetLinkProfile(dllModule, pHandle, profile, lpBuf);
#elif CS101
                Result = Native.RFID_RadioGetLinkProfile(profile, lpBuf);
#endif

                if (Result.OK == Result)
                {
                    // profile.length  =
                    //     supplied during profile object construction
                    profileInfo.enabled =
                        (UInt32)Marshal.ReadInt32(lpBuf, 4);
                    profileInfo.profileId =
                        (UInt64)Marshal.ReadInt64(lpBuf, 8);
                    profileInfo.profileVersion =
                        (UInt32)Marshal.ReadInt32(lpBuf, 16);

                    // Catch if value read in has no match in protocol type

                    try
                    {
                        profileInfo.profileProtocol =
                            (RadioProtocol)Marshal.ReadInt32(lpBuf, 20);
                    }
                    catch
                    {
                        profileInfo.profileProtocol = RadioProtocol.UNKNOWN;

                        Result = Result.DRIVER_MISMATCH;
                    }

                    // Grab all data we can even though we are returning
                    // a mis-match error...

                    profileInfo.denseReaderMode =
                        (UInt32)Marshal.ReadInt32(lpBuf, 24);
                    profileInfo.widebandRssiSamples =
                        (UInt32)Marshal.ReadInt32(lpBuf, 28);
                    profileInfo.narrowbandRssiSamples =
                        (UInt32)Marshal.ReadInt32(lpBuf, 32);

                    profileInfo.realtimeRssiEnabled =
                        (UInt32)Marshal.ReadInt32(lpBuf, 36);
                    profileInfo.realtimeWidebandRssiSamples =
                        (UInt32)Marshal.ReadInt32(lpBuf, 40);
                    profileInfo.realtimeNarrowbandRssiSamples =
                        (UInt32)Marshal.ReadInt32(lpBuf, 44);

                    if (RadioProtocol.ISO18K6C == profileInfo.profileProtocol)
                    {
                        RadioLinkProfileConfig profileConfig =
                            new RadioLinkProfileConfig();

                        try
                        {
                            Marshal.PtrToStructure
                            (
                                new IntPtr(lpBuf.ToInt64() + 48),
                                profileConfig
                            );
                        }
                        catch
                        {
                            // This can potentiall occur if any of the well defined types encounter
                            // a non-convertable int value when perform ptr to struct marshal. Just
                            // set any field(s) that can cause the error to UNKNOWN status.

                            profileConfig.modulationType = ModulationType.UNKNOWN;
                            profileConfig.data01Difference = DataDifference.UNKNOWN;
                            profileConfig.divideRatio = DivideRatio.UNKNOWN;
                            profileConfig.millerNumber = MillerNumber.UNKNOWN;

                            Result = Result.DRIVER_MISMATCH;
                        }

                        profileInfo.profileConfig = profileConfig;
                    }
                    else
                    {
                        // This will set the value of profile.profileProtocol field
                        // to be set to UNKNOWN - i.e. library / linkage mis-match

                        profileInfo.profileConfig = new ProfileConfig();
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBuf)
            {
                Marshal.FreeHGlobal(lpBuf);
            }

            return Result;
        }



        /// <summary>
        /// Writes a valut to a link-profile register for the specified link
        /// profile.  A link-profile regsiter may not be written while a radio module
        /// is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which link-profile register should be written</param>
        /// <param name="profile">the link profile for link-profile register should be written</param>
        /// <param name="address">address of the link-profile register</param>
        /// <param name="value">the value to be written to the register</param>
        /// <returns></returns>
        public Result RadioWriteLinkProfileRegister
        (
            [In] UInt32 profile,
            [In] UInt16 address,
            [In] UInt16 value
        )
        {
#if CS203
            return Native.RFID_RadioWriteLinkProfileRegister(dllModule, pHandle, profile, address, value);
#elif CS101
            return Native.RFID_RadioWriteLinkProfileRegister(profile, address, value);
#endif
        }



        /// <summary>
        /// Retrieves the contents of a link-profile register for the specified link
        /// profile.  A link-profile regsiter may not be read while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which link-profile register should be read</param>
        /// <param name="profile">the link profile for link-profile register should be read</param>
        /// <param name="address">address of the link-profile register</param>
        /// <param name="value">a pointer to a 16-bit unsigned integer that upon return will
        /// contain the register's value</param>
        /// <returns></returns>
        public Result RadioReadLinkProfileRegister
        (
            [In]          UInt32 profile,
            [In]          UInt16 address,
            [In, Out] ref UInt16 value
        )
        {
#if CS203
            return Native.RFID_RadioReadLinkProfileRegister(dllModule, pHandle, profile, address, ref value);
#elif CS101
            return Native.RFID_RadioReadLinkProfileRegister(profile, address, ref value);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHandle"></param>
        /// <returns></returns>
        public Result RadioTurnCarrierWaveOn
        (
        )
        {
#if CS203
            return Native.RFID_RadioTurnCarrierWaveOn(dllModule, pHandle);
#elif CS101
            return Native.RFID_RadioTurnCarrierWaveOn(pHandle);
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHandle"></param>
        /// <returns></returns>
        public Result RadioTurnCarrierWaveOff
        (
        )
        {
#if CS203
            return Native.RFID_RadioTurnCarrierWaveOff(dllModule, pHandle);
#elif CS101
            return Native.RFID_RadioTurnCarrierWaveOff(pHandle);
#endif
        }

        /// <summary>
        /// Retrieves the status of a radio module's antenna port.  The antenna port
        /// status may not be retrieved while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to the radio for which antenna status is requested.  This
        /// is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="port">the antenna port for which status is to be retrieved. 
        /// Antenna ports are numbered beginning with 0.</param>
        /// <param name="status">pointer to the structure which upon return will contain the
        /// antenna port's status.  Must not be NULL.</param>
        /// <returns></returns>
        public Result AntennaPortGetStatus
        (
            [In]      UInt32 port,
            [In, Out] AntennaPortStatus status
        )
        {
            try
            {
#if CS203
                return Native.RFID_AntennaPortGetStatus(dllModule, pHandle, port, status);
#elif CS101
                return Native.RFID_AntennaPortGetStatus(port, status);
#endif
            }
            catch
            {
                // Here if the port state value was unknown to us and
                // couldn't be converted automatically - set to unknown

                status.state = AntennaPortState.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }



        /// <summary>
        /// Sets the state of a radio module's antenna port.  The antenna port state
        /// may not be set while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to the radio for which antenna port state will be set.
        /// This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="port">the antenna port for which state will be set.  Antenna ports
        /// are numbered beginning with 0.</param>
        /// <param name="state">the state for the antenna port</param>
        /// <returns></returns>
        public Result AntennaPortSetState
        (
            [In] UInt32 port,
            [In] AntennaPortState state
        )
        {
#if CS203
            return Native.RFID_AntennaPortSetState(dllModule, pHandle, port, state);
#elif CS101
            return Native.RFID_AntennaPortSetState(port, state);
#endif
        }





        /// <summary>
        /// Sets the configuration for a radio module's antenna port.  The antenna
        /// port configuration may not be set while a radio module is executing a
        /// tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which antenna-port configuration will be set.
        /// This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="port">the antenna port for which configuration will be set.
        /// Antenna ports are numbered beginning with 0.</param>
        /// <param name="config">pointer to structure containing antenna port configuration.  Must
        /// not be NULL.  In version 1.0, the physicalRxPort and physicalTxPort
        /// fields must be the same.</param>
        /// <returns></returns>
        public Result AntennaPortSetConfiguration
        (
            [In] UInt32 port,
            [In] AntennaPortConfig config
        )
        {
#if CS203
            return Native.RFID_AntennaPortSetConfiguration(dllModule, pHandle, port, config);
#elif CS101
            return Native.RFID_AntennaPortSetConfiguration(port, config);
#endif
        }



        /// <summary>
        /// Retrieves the configuration for a radio module's antenna port.  The antenna
        /// port configuration may not be retrieved while a radio module is executing a
        /// tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which antenna port configuration will be
        /// retrieved.  This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="port">the antenna port for which configuration will be
        /// retrieved.  Antenna ports are numbered beginning with 0.</param>
        /// <param name="config">pointer to structure that on return will contain the antenna
        /// port configuration.  Must not be NULL.</param>
        /// <returns></returns>
        public Result AntennaPortGetConfiguration
        (
            [In]      UInt32 port,
            [In, Out] AntennaPortConfig config
        )
        {
#if CS203
            return Native.RFID_AntennaPortGetConfiguration(dllModule, pHandle, port, config);
#elif CS101
            return Native.RFID_AntennaPortGetConfiguration(port, config);
#endif
        }




        /// <summary>
        /// Configures the tag-selection criteria for the ISO 18000-6C select command.
        /// The supplied tag-selection criteria will be used for any tag-protocol
        /// operations (i.e., RFID_18K6CTagInventory, etc.) in which the application
        /// specifies that an ISO 18000-6C select command should be issued prior to
        /// executing the tag-protocol operation.  The tag-selection criteria will
        /// stay in effect until the next call to RFID_18K6CSetSelectCriteria.  The
        /// select criteria may not be set while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which tag-selection criteria will be
        /// configured.  This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="pCriteria">pointer to a structure that specifies the ISO 18000-6C
        /// tag-selection criteria.  This parameter must not be NULL.</param>
        /// <param name="flags">Reserved for future use.  Set to zero.</param>
        /// <returns></returns>
        public Result Set18K6CSelectCriteria
        (
            [In] SelectCriteria pCriteria,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            // Check immediately if the specified count > than actual count
            // of criteria since that will cause null ptr exception when we
            // perform our array marshaling loop...

            if (pCriteria.countCriteria > pCriteria.pCriteria.Length)
            {
                return Result.INVALID_PARAMETER;
            }

            try
            {
                // The select mask and action do not have embedded length fields so
                // we must hand calculate for use when copying data to native block

                Int32 selectMaskSize =
                    Marshal.SizeOf(typeof(SelectMask));

                Int32 selectCriterionSize =
                    selectMaskSize +
                    Marshal.SizeOf(typeof(SelectAction));

                // This is to hold the count value, the ptr to select criterion array
                // and the select criterion array members

                lpBufPtr = Marshal.AllocHGlobal
                (
                    4 +
                    IntPtr.Size +
                    (Int32)(selectCriterionSize * pCriteria.countCriteria)
                );

                Marshal.WriteInt32(lpBufPtr, 0, (Int32)pCriteria.countCriteria);

                // pCriteria.pCriteria - ptr to array of criteria

                Marshal.WriteIntPtr
                   (
                       new IntPtr(lpBufPtr.ToInt64() + 4),
                       new IntPtr(lpBufPtr.ToInt64() + 4 + IntPtr.Size)
                   );

                // Copy the individual select criteria array elements over
                // to native memory block...

                for (int index = 0; index < pCriteria.countCriteria; ++index)
                {
                    IntPtr lpCurrentCriteriaPtr =
                        new IntPtr
                            (
                                lpBufPtr.ToInt64() + 4 + IntPtr.Size + (selectCriterionSize * index)
                            );

                    Marshal.StructureToPtr
                        (
                            pCriteria.pCriteria[index].mask,
                            lpCurrentCriteriaPtr,
                            false
                        );

                    Marshal.StructureToPtr
                        (
                            pCriteria.pCriteria[index].action,
                            new IntPtr(lpCurrentCriteriaPtr.ToInt64() + selectMaskSize),
                            false
                        );
                }
#if CS203
                Result = Native.RFID_18K6CSetSelectCriteria(dllModule, pHandle, lpBufPtr, flags);
#elif CS101
                Result = Native.RFID_18K6CSetSelectCriteria(lpBufPtr, flags);
#endif
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }




        /// <summary>
        /// Retrieves the configured tag-selection criteria for the ISO 18000-6C select
        /// command.  The returned tag-selection criteria are used for any tag-protocol
        /// operations (i.e., RFID_18K6CTagInventory, etc.) in which the application
        /// specifies that an ISO 18000-6C select command should be issued prior to 
        /// executing the tag-protocol operation.  The select criteria may not be
        /// retrieved while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which tag-selection criteria will be
        /// retrieved.  This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="pCriteria">pointer to a structure that specifies the ISO 18000-6C
        /// tag-selection criteria.  On entry to the function, the countCriteria 
        /// field must contain the number of entries in the array pointed to by the
        /// pCriteria field.  On return from the function, the countCriteria field
        /// will contain the number of tag-selection criteria returned in the array
        /// pointed to by pCriteria.  If the array pointed to by pCriteria is not
        /// large enough to hold the configured tag-selection criteria, on return
        /// countCriteria will contain the number of entries required and the
        /// function will return RFID_ERROR_BUFFER_TOO_SMALL.  This parameter must
        /// not be NULL.  The pCriteria field may be NULL only if the countCriteria
        /// field is zero.</param>
        /// <returns></returns>
        public Result Get18K6CSelectCriteria
        (
            [In, Out] SelectCriteria pCriteria
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            // The select mask and action do not have embedded length fields so
            // we must hand calculate for use when retrieving native block data

            Int32 selectMaskSize =
                Marshal.SizeOf(typeof(SelectMask));

            Int32 selectCriterionSize =
                selectMaskSize +
                Marshal.SizeOf(typeof(SelectAction));

            // Start by specifying single rule...

            Int32 countCriteria = 1;

            try
            {
                // This is to hold the count value, the ptr to select criterion array
                // and the select criterion array members

                lpBufPtr = Marshal.AllocHGlobal
                (
                    4 +
                    IntPtr.Size +
                    (Int32)(selectCriterionSize * countCriteria)
                );

                Marshal.WriteInt32(lpBufPtr, 0, countCriteria);

                //!! pCriteria.pCriteria - ptr to array of criteria

                Marshal.WriteIntPtr
                (
                    new IntPtr(lpBufPtr.ToInt64() + 4),
                    new IntPtr(lpBufPtr.ToInt64() + 4 + IntPtr.Size)
                );
#if CS203
                Result = Native.RFID_18K6CGetSelectCriteria(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CGetSelectCriteria(lpBufPtr);
#endif

                if (Result.BUFFER_TOO_SMALL == Result)
                {
                    // On buffer too small, the returned count in native block
                    // will hold the required count: length of criterion array

                    countCriteria = Marshal.ReadInt32(lpBufPtr, 0);

                    lpBufPtr = Marshal.ReAllocHGlobal
                    (
                        lpBufPtr,
                        new IntPtr
                        (
                            4 + IntPtr.Size + (Int32)(selectCriterionSize * countCriteria)
                        )
                    );

                    // Try native call again - we should NEVER see buff too small error
                    // since set to size lib told us but we can see other errors...

#if CS203
                    Result = Native.RFID_18K6CGetSelectCriteria(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CGetSelectCriteria(lpBufPtr);
#endif              
                }

                if (Result.OK == Result)
                {
                    pCriteria.countCriteria = (UInt32)Marshal.ReadInt32(lpBufPtr, 0);

                    pCriteria.pCriteria = new SelectCriterion[countCriteria];

                    for (int index = 0; index < countCriteria; ++index)
                    {
                        // We need to calculate offset over the criteria count, ptr to the
                        // criteria array and into the proper array element...

                        IntPtr lpCurrentCriteriaPtr =
                            new IntPtr
                                (
                                    lpBufPtr.ToInt64() + 4 + IntPtr.Size + (selectCriterionSize * index)
                                );

                        pCriteria.pCriteria[index] = new SelectCriterion();

                        try
                        {
                            Marshal.PtrToStructure
                            (
                                lpCurrentCriteriaPtr,
                                pCriteria.pCriteria[index].mask
                            );
                        }
                        catch
                        {
                            // Non-convertable value or other error...

                            pCriteria.pCriteria[index].mask.bank = MemoryBank.UNKNOWN;

                            Result = Result.DRIVER_MISMATCH;
                        }

                        try
                        {
                            Marshal.PtrToStructure
                            (
                                new IntPtr(lpCurrentCriteriaPtr.ToInt64() + selectMaskSize),
                                pCriteria.pCriteria[index].action
                            );
                        }
                        catch
                        {
                            // Non-convertable value or other error...

                            pCriteria.pCriteria[index].action.target = Target.UNKNOWN;
                            pCriteria.pCriteria[index].action.action = CSLibrary.Constants.Action.UNKNOWN;

                            Result = Result.DRIVER_MISMATCH;
                        }
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }




        /// <summary>
        /// Configures the post-singulation match criteria to be used by the RFID
        /// radio module.  The supplied post-singulation match criteria will be used
        /// for any tag-protocol operations (i.e., RFID_18K6CTagInventory, etc.) in
        /// which the application specifies that a post-singulation match should be
        /// performed on the tags that are singulated by the tag-protocol operation.
        /// The post-singulation match criteria will stay in effect until the next call
        /// to RFID_18K6CSetPostMatchCriteria.  The post-singulation match criteria
        /// may not be set while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which post-singulation match criteria will be
        /// configured.  This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="pCriteria">a pointer to a structure that specifies the post-singulation
        /// match mask and disposition that are to be applied to the tag Electronic
        /// Product Code after it is singulated to determine if it is to have the
        /// tag-protocol operation applied to it.  This parameter must not be NULL.</param>
        /// <param name="flags">Reserved for future use.  Set to zero.</param>
        /// <returns></returns>
        public Result Set18K6CPostMatchCriteria
        (
            [In] SingulationCriteria pCriteria,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            // Check immediately if the specified count > than actual count
            // of criteria since that will cause null ptr exception when we
            // perform our array marshaling loop...

            if (pCriteria.countCriteria > pCriteria.pCriteria.Length)
            {
                return Result.INVALID_PARAMETER;
            }

            try
            {
                // The singlation mask does not have an embedded length field so
                // hand calculte for use when retrieving native block data

                Int32 singulationCriterionSize =
                    4 +
                    Marshal.SizeOf(typeof(SingulationMask));

                lpBufPtr = Marshal.AllocHGlobal
                (
                    4 +
                    IntPtr.Size +
                    (Int32)(singulationCriterionSize * pCriteria.countCriteria)
                );

                // pCriteria.countCriteria

                Marshal.WriteInt32(lpBufPtr, 0, (Int32)pCriteria.countCriteria);

                // pCriteria.pCriteria - ptr to array of criteria

                Marshal.WriteIntPtr
                   (
                       new IntPtr(lpBufPtr.ToInt64() + 4),
                       new IntPtr(lpBufPtr.ToInt64() + 4 + IntPtr.Size)
                   );

                for (int index = 0; index < pCriteria.countCriteria; ++index)
                {
                    // Calculate pointer so we jump over the count, pointer to array
                    // and then into the desired position in the embedded array...

                    IntPtr lpCurrentCriteriaPtr =
                        new IntPtr
                            (
                                lpBufPtr.ToInt64() + 4 + IntPtr.Size + (singulationCriterionSize * index)
                            );

                    Marshal.WriteInt32
                        (
                            lpCurrentCriteriaPtr, (Int32)pCriteria.pCriteria[index].match
                        );

                    Marshal.StructureToPtr
                        (
                            pCriteria.pCriteria[index].mask,
                            new IntPtr(lpCurrentCriteriaPtr.ToInt64() + 4),
                            false
                        );
                }
#if CS203
                Result = Native.RFID_18K6CSetPostMatchCriteria(dllModule, pHandle, lpBufPtr, flags);
#elif CS101
                Result = Native.RFID_18K6CSetPostMatchCriteria(lpBufPtr, flags);
#endif
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }


        /// <summary>
        /// Retrieves the configured post-singulation match criteria to be used by the
        /// RFID radio module.  The post-singulation match criteria is used for any
        /// tag-protocol operations (i.e., RFID_18K6CTagInventory, etc.) in which the
        /// application specifies that a post-singulation match should be performed on
        /// the tags that are singulated by the tag-protocol operation.  Post-
        /// singulation match criteria may not be retrieved while a radio module is
        /// executing a tag-protocol operation.  The post-singulation match criteria
        /// may not be retrieved while a radio module is executing a tag-protocol
        /// operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which post-singulation match criteria will be
        /// retrieved.  This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="pCriteria">a pointer to a structure that upon return will contain the
        /// post-singulation match criteria that are to be applied to the tag
        /// Electronic Product Code after it is singulated to determine if it is to
        /// have the tag-protocol operation applied to it.  On entry to the function,
        /// the countCriteria field must contain the number of entries in the array
        /// pointed to by the pCriteria field.  On return from the function, the
        /// countCriteria field will contain the number of post-singulation match
        /// criteria returned in the array pointed to by pCriteria.  If the array
        /// pointed to by pCriteria is not large enough to hold the configured tag-
        /// selection criteria, on return countCriteria will contain the number of
        /// entries required and the function will return
        /// RFID_ERROR_BUFFER_TOO_SMALL.  This parameter must not be NULL.  The
        /// pCriteria field may be NULL only if the countCriteria field is zero.</param>
        /// <returns></returns>
        public Result Get18K6CPostMatchCriteria
        (
            [In, Out] SingulationCriteria pCriteria
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            // This is to hold the count value, the ptr to post singulation
            // criterion count and associated array members

            Int32 singulationCriterionSize = 74;  // marshal sizeof pads bytes in the count !

            // Max out - we currently support 1 but may eventually support 8 so...

            Int32 countCriteria = 1;

            try
            {
                // Alloc our native buffer for diving down into rfid.dll

                lpBufPtr = Marshal.AllocHGlobal
                (
                    4 +
                    IntPtr.Size +
                    (Int32)(singulationCriterionSize * countCriteria)
                );

                Marshal.WriteInt32(lpBufPtr, 0, countCriteria);

                // pCriteria.pCriteria - ptr to the loc where the native
                // version of the criterion array data will be placed

                Marshal.WriteIntPtr
                (
                    new IntPtr(lpBufPtr.ToInt64() + 4),
                    new IntPtr(lpBufPtr.ToInt64() + 4 + IntPtr.Size)
                );
#if CS203
                Result = Native.RFID_18K6CGetPostMatchCriteria(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CGetPostMatchCriteria(lpBufPtr);
#endif


                if (Result.BUFFER_TOO_SMALL == Result)
                {
                    // On buffer too small, the returned count in native block
                    // will hold the required count: length of criterion array

                    countCriteria = Marshal.ReadInt32(lpBufPtr, 0);

                    // Resize our buffer to the required amount...

                    lpBufPtr = Marshal.ReAllocHGlobal
                    (
                        lpBufPtr,
                        new IntPtr
                        (
                            4 + IntPtr.Size + (Int32)(singulationCriterionSize * countCriteria)
                        )
                    );

                    // Make another native attempt at native call - we should never
                    // fail ( due to insufficient memory ) twice in a row...

#if CS203
                    Result = Native.RFID_18K6CGetPostMatchCriteria(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CGetPostMatchCriteria(lpBufPtr);
#endif

                }

                if (Result.OK == Result)
                {
                    pCriteria.countCriteria = (UInt32)Marshal.ReadInt32(lpBufPtr, 0);

                    pCriteria.pCriteria = new SingulationCriterion[countCriteria];

                    for (int index = 0; index < countCriteria; ++index)
                    {
                        // We need to calculate offset over the criteria count, ptr to the
                        // criteria array and into the proper array element...

                        IntPtr lpCurrentCriteriaPtr =
                            new IntPtr
                                (
                                    lpBufPtr.ToInt64() + 4 + IntPtr.Size + (singulationCriterionSize * index)
                                );

                        pCriteria.pCriteria[index] = new SingulationCriterion();

                        // UnMarshal the post singulation object at the current array index...

                        pCriteria.pCriteria[index].match = (UInt32)Marshal.ReadInt32
                            (
                                lpCurrentCriteriaPtr,
                                0
                            );

                        Marshal.PtrToStructure
                            (
                                new IntPtr(lpCurrentCriteriaPtr.ToInt64() + 4),
                                pCriteria.pCriteria[index].mask
                            );
                    }
                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }




        /// <summary>
        /// Specifies which tag group will have subsequent tag-protocol operations
        /// (e.g., inventory, tag read, etc.) applied to it.  The tag group may not be
        /// changed while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which the tag group will be configured.
        /// This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="group">a pointer to a structure that specifies the tag group that will
        /// have subsequent tag-protocol operations applied to it.  This parameter
        /// must not be NULL.</param>
        /// <returns></returns>
        public Result Set18K6CQueryTagGroup
        (
            [In] TagGroup group
        )
        {
#if CS203
            return Native.RFID_18K6CSetQueryTagGroup(dllModule, pHandle, group);
#elif CS101
            return Native.RFID_18K6CSetQueryTagGroup(group);
#endif
        }



        /// <summary>
        /// Retrieves the tag group that will have subsequent tag-protocol operations
        /// (e.g., inventory, tag read, etc.) applied to it.  The tag group may not be
        /// retrieved while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which the tag group will be retrieved.
        /// This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="pGroup">a pointer to a structure that upon return contains the configured
        /// tag group.  This parameter must not be NULL.</param>
        /// <returns></returns>
        public Result Get18K6CQueryTagGroup
        (
            [In, Out] TagGroup pGroup
        )
        {
            try
            {
#if CS203
                return Native.RFID_18K6CGetQueryTagGroup(dllModule, pHandle, pGroup);
#elif CS101
                return Native.RFID_18K6CGetQueryTagGroup(pGroup);
#endif
            }
            catch
            {
                // Auto unmarshal error - set all typed fields to unknown

                pGroup.selected = Selected.UNKNOWN;
                pGroup.session = Session.UNKNOWN;
                pGroup.target = SessionTarget.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }



        /// <summary>
        /// Allows the application to set the currently-active singulation algorithm
        /// (i.e., the one that is used when performing a tag-protocol operation
        /// (e.g., inventory, tag read, etc.)).  The currently-active singulation
        /// algorithm may not be changed while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which the currently-active singulation
        /// algorithm will be set.  This is the pHandle from a successful call to
        /// RFID_RadioOpen.</param>
        /// <param name="algorithm">the singulation algorithm to make currently active.</param>
        /// <returns></returns>
        public Result Set18K6CCurrentSingulationAlgorithm
        (
            [In] SingulationAlgorithm algorithm
        )
        {
#if CS203
            return Native.RFID_18K6CSetCurrentSingulationAlgorithm(dllModule, pHandle, algorithm);
#elif CS101
            return Native.RFID_18K6CSetCurrentSingulationAlgorithm(algorithm);
#endif
        }




        /// <summary>
        /// Allows the application to retrieve the currently-active singulation
        /// algorithm.  The currently-active singulation algorithm may not be changed
        /// while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which the currently-active singulation
        /// algorithm will be retrieved.  This is the pHandle from a successful call
        /// to RFID_RadioOpen.</param>
        /// <param name="algorithm">a pointer to a singulation-algorithm variable that upon
        /// return will contain the currently-active singulation algorithm.  This
        /// parameter must not be NULL.</param>
        /// <returns></returns>
        public Result Get18K6CCurrentSingulationAlgorithm
        (
            [In, Out] ref SingulationAlgorithm algorithm
        )
        {
            try
            {
#if CS203
                return Native.RFID_18K6CGetCurrentSingulationAlgorithm(dllModule, pHandle, ref algorithm);
#elif CS101
                return Native.RFID_18K6CGetCurrentSingulationAlgorithm(ref algorithm);
#endif
            }
            catch
            {
                algorithm = SingulationAlgorithm.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }




        /// <summary>
        /// Allows the application to configure the settings for a particular
        /// singulation algorithm.  A singulation algorithm may not be configured while
        /// a radio module is executing a tag-protocol operation.
        /// 
        /// NOTE:  Configuring a singulation algorithm does not automatically set it as
        /// the current singulation algorithm
        /// (see RFID_18K6CSetCurrentSingulationAlgorithm). 
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which the singulation-algorithm parameters
        /// will be set.  This is the pHandle from a successful call to
        /// RFID_RadioOpen.</param>
        /// <param name="algorithm">the singulation algorithm to be configured.  This parameter
        /// determines the type of structure to which pParms points.  For example,
        /// if this parameter is RFID_18K6C_SINGULATION_ALGORITHM_FIXEDQ, pParms must
        /// point to a RFID_18K6C_SINGULATION_FIXEDQ_PARMS structure.  If this
        /// parameter does not represent a valid singulation algorithm,
        /// RFID_ERROR_INVALID_PARAMETER is returned.</param>
        /// <param name="parms">a pointer to a structure that contains the singulation-algorithm
        /// parameters.  The type of structure this points to is determined by
        /// algorithm.  The structure length field must be filled in appropriately.
        /// This parameter must not be NULL.</param>
        /// <returns></returns>
        public Result Set18K6CSingulationAlgorithmParameters
        (
            [In] SingulationAlgorithm algorithm,
            [In] SingulationAlgorithmParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr pParms = IntPtr.Zero;

            try
            {
                pParms = Marshal.AllocHGlobal(Marshal.SizeOf(parms));

                Marshal.StructureToPtr(parms, pParms, false);

#if CS203
                Result = Native.RFID_18K6CSetSingulationAlgorithmParameters(dllModule, pHandle, algorithm, pParms);
#elif CS101
                Result = Native.RFID_18K6CSetSingulationAlgorithmParameters(algorithm, pParms);
#endif
            }
            catch
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != pParms)
            {
                Marshal.FreeHGlobal(pParms);
            }

            return Result;
        }



        /// <summary>
        /// Allows the application to retrieve the settings for a particular
        /// singulation algorithm.  Singulation-algorithm parameters may not be
        /// retrieved while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which the singulation-algorithm parameters
        /// will be retrieved.  This is the pHandle from a successful call to
        /// RFID_RadioOpen.</param>
        /// <param name="algorithm">The singulation algorithm for which parameters are to be
        /// retrieved.  This parameter determines the type of structure to which
        /// pParms points.  For example, if this parameter is
        /// RFID_18K6C_SINGULATION_ALGORITHM_FIXEDQ, pParms must point to a
        /// RFID_18K6C_SINGULATION_FIXEDQ_PARMS structure.  If this parameter does
        /// not represent a valid singulation algorithm,
        /// RFID_ERROR_INVALID_PARAMETER is returned.</param>
        /// <param name="parms">a pointer to a structure that upon return will contain the
        /// singulation-algorithm parameters.  The type of structure this points to
        /// is determined by algorithm.  The structure length field must be filled
        /// in appropriately.  This parameter must not be NULL.</param>
        /// <returns></returns>
        public Result Get18K6CSingulationAlgorithmParameters
        (
            [In]      SingulationAlgorithm algorithm,
            [In, Out] SingulationAlgorithmParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr pParms = IntPtr.Zero;

            if (null == parms)
            {
                return Result.INVALID_PARAMETER;
            }

            // Instead of the following, could simply make the native call
            // with given fields and catch the Resulting marshal exception
            // when trying to do ptr to structure

            Type algoType = parms.GetType();

            if
            (
                (SingulationAlgorithm.UNKNOWN == algorithm)
             || (SingulationAlgorithm.FIXEDQ == algorithm && parms.GetType() != typeof(FixedQParms))
             || (SingulationAlgorithm.DYNAMICQ == algorithm && parms.GetType() != typeof(DynamicQParms))
             || (SingulationAlgorithm.DYNAMICQ_ADJUST == algorithm && parms.GetType() != typeof(DynamicQAdjustParms))
             || (SingulationAlgorithm.DYNAMICQ_THRESH == algorithm && parms.GetType() != typeof(DynamicQThresholdParms))
            )
            {
                return Result.INVALID_PARAMETER;
            }

            try
            {
                pParms = Marshal.AllocHGlobal(Marshal.SizeOf(parms));

                Marshal.StructureToPtr(parms, pParms, false);
#if CS203
                Result = Native.RFID_18K6CGetSingulationAlgorithmParameters(dllModule, pHandle, algorithm, pParms);
#elif CS101
                Result = Native.RFID_18K6CGetSingulationAlgorithmParameters(algorithm, pParms);
#endif

                if (Result.OK == Result)
                {
                    Marshal.PtrToStructure(pParms, parms);
                }
            }
            catch
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != pParms)
            {
                Marshal.FreeHGlobal(pParms);
            }

            return Result;
        }


        /// <summary>
        /// Configures the parameters for the ISO 18000-6C query command.  The supplied
        /// query parameters will be used for any subsequent tag-protocol operations
        /// (i.e., RFID_18K6CTagInventory, etc.) and will stay in effect until the next
        /// call to RFID_18K6CSetQueryParameters.  The query parameters may not be set
        /// while a radio module is executing a tag-protocol operation.
        /// 
        /// NOTE: Failure to call RFID_18K6CSetQueryParameters prior to executing the
        /// first tag-protocol operation (i.e., RFID_18K6CTagInventory, etc.) will
        /// Result in the RFID radio module using default values for the ISO 18000-6C
        /// query parameters.
        /// 
        /// NOTE:  As of version 1.1 of the RFID Reader Library, this function has been
        /// deprecated and replaced by the combination of RFID_18K6CSetQueryTagGroup,
        /// RFID_18K6CSetCurrentSingulationAlgorithm, and
        /// RFID_18K6CSetSingulationAlgorithmParameters.  This function remains for
        /// backwards compatibility, however new code should not use it as it will be
        /// removed in a future version.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which query parameters will be configured.
        /// This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="parms">a pointer to a structure that defines the inventory round ISO
        /// 18000-6C query parameters.  This parameter must not be NULL.</param>
        /// <param name="flags">Reserved for future use.  Set to zero.</param>
        /// <returns></returns>
        public Result SetQueryParameters
        (
            [In] QueryParms parms,
            [In] Int32 flags
        )
        {
            // This function implementation uses the new set tag group, set
            // current algorithm and set algorithm parameters function as
            // the native function this is based on has been deprecated

            Result Result = Result.OK;

            Result = Set18K6CQueryTagGroup(parms.tagGroup);

            if (Result.OK != Result)
            {
                return Result;
            }

            SingulationAlgorithm algorithm = SingulationAlgorithm.UNKNOWN;

            Type algoType = parms.singulationParms.GetType();

            if (algoType == typeof(FixedQParms))
                algorithm = SingulationAlgorithm.FIXEDQ;
            else if (algoType == typeof(DynamicQParms))
                algorithm = SingulationAlgorithm.DYNAMICQ;
            else if (algoType == typeof(DynamicQAdjustParms))
                algorithm = SingulationAlgorithm.DYNAMICQ_ADJUST;
            else if (algoType == typeof(DynamicQThresholdParms))
                algorithm = SingulationAlgorithm.DYNAMICQ_THRESH;
            else
                return Result.INVALID_PARAMETER;

            Result = Set18K6CCurrentSingulationAlgorithm(algorithm);

            if (Result.OK != Result)
            {
                return Result;
            }

            Result = Set18K6CSingulationAlgorithmParameters
                (
                    algorithm,
                    parms.singulationParms
                );

            return Result;
        }


        /// <summary>
        /// Retrieves the parameters for the ISO 18000-6C query command.  These are
        /// the query parameters that used for tag-protocol operations (i.e.,
        /// RFID_18K6CTagInventory, etc.).  Query parameters may not be retrieved
        /// while a radio module is executing a tag-protocol operation.  The query
        /// parameters may not be retrieved while a radio module is executing a tag-
        /// protocol operation.
        /// 
        /// NOTE:  As of version 1.1 of the RFID Reader Library, this function has been
        /// deprecated and replaced by the combination of RFID_18K6CGetQueryTagGroup,
        /// RFID_18K6CGetCurrentSingulationAlgorithm, and
        /// RFID_18K6CGetSingulationAlgorithmParameters.  This function remains for
        /// backwards compatibility, however new code should not use it as it will be
        /// removed in a future version.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which query parameters will be retrieved.
        /// This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="parms">a pointer to a structure that on return contains the ISO
        /// 18000-6C query parameters for each inventory round.  This parameter must
        /// not be NULL.</param>
        /// <returns></returns>
        public Result GetQueryParameters
        (
            [In, Out] QueryParms parms
        )
        {
            Result Result = Result.OK;

            // This function implementation uses the new get tag group, get
            // current algorithm and get algorithm parameters function as
            // the native function this is based on has been deprecated

            // Check that the internal tag group obj exists and create if
            // necessary - alternative is to return invalid parameter...

            if (null == parms.tagGroup)
            {
                parms.tagGroup = new TagGroup();
            }

            Result = Get18K6CQueryTagGroup(parms.tagGroup);

            if (Result.OK != Result)
            {
                return Result;
            }

            SingulationAlgorithm algorithm = SingulationAlgorithm.UNKNOWN;

            Result = Get18K6CCurrentSingulationAlgorithm(ref algorithm);

            if (Result.OK != Result)
            {
                return Result;
            }

            switch (algorithm)
            {
                case SingulationAlgorithm.FIXEDQ:
                    {
                        parms.singulationParms = new FixedQParms();
                    }
                    break;
                case SingulationAlgorithm.DYNAMICQ:
                    {
                        parms.singulationParms = new DynamicQParms();
                    }
                    break;
                case SingulationAlgorithm.DYNAMICQ_ADJUST:
                    {
                        parms.singulationParms = new DynamicQAdjustParms();
                    }
                    break;
                case SingulationAlgorithm.DYNAMICQ_THRESH:
                    {
                        parms.singulationParms = new DynamicQThresholdParms();
                    }
                    break;
                default:
                    {
                        return Result.DRIVER_MISMATCH; // Firmware and library mismatch ?
                    }
            }

            Result = Get18K6CSingulationAlgorithmParameters
                (
                    algorithm,
                    parms.singulationParms
                );

            return Result;
        }

        /// <summary>
        /// Executes a tag inventory for the tags of interest.  If the
        /// RFID_FLAG_PERFORM_SELECT flag is specified, the tag population is
        /// partitioned (i.e., ISO 18000-6C select) prior to the inventory operation.
        /// If the RFID_FLAG_PERFORM_POST_MATCH flag is specified, the post-singulation
        /// match mask is applied to a singulated tag's EPC to determine if the tag
        /// will be returned to the application.  The operation-response packets
        /// will be returned to the application via the application-supplied callback
        /// function.  An application may prematurely stop an inventory operation by
        /// calling RFID_Radio{Cancel|Abort}Operation on another thread or by returning
        /// a non-zero value from the callback function.  A tag inventory may not be
        /// issued while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which inventory operation will be performed.
        /// This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="parms">pointer to a structure that specifies the ISO 18000-6C inventory
        /// operation parameters.  This parameter must not be NULL.</param>
        /// <param name="flags">inventory flags.  May be zero or a combination of the following:
        /// RFID_FLAG_PERFORM_SELECT - perform one or more selects before performing
        ///   the inventory.
        /// RFID_FLAG_PERFORM_POST_MATCH - perform post-singulation mask match on
        ///   singulated tags.</param>
        /// <returns></returns>
        public Result Tag18K6CInventory
        (
            [In] InventoryParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                // Manually marshal as .NET CF has issues automatically marshalling
                // when the struct has an internal delegate...

                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                Marshal.WriteInt32(lpBufPtr, 0, (Int32)parms.length);
                Marshal.WriteInt32(lpBufPtr, 4, (Int32)parms.common.tagStopCount);
                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + 4 + IntPtr.Size), Marshal.GetFunctionPointerForDelegate(parms.common.callback));
                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + 8 + IntPtr.Size), (IntPtr)parms.common.context);
                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + 12 + IntPtr.Size), (IntPtr)parms.common.callbackCode);
#if CS203
                Result = Native.RFID_18K6CTagInventory(dllModule, pHandle, lpBufPtr, flags);
#elif CS101
                Result = Native.RFID_18K6CTagInventory(lpBufPtr, flags);
#endif
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }



        /// <summary>
        /// Executes a tag read for the tags of interest.  If the
        /// RFID_FLAG_PERFORM_SELECT flag is specified, the tag population is
        /// partitioned (i.e., ISO 18000-6C select) prior to the tag-read operation.
        /// If the RFID_FLAG_PERFORM_POST_MATCH flag is specified, the post-singulation
        /// match mask is applied to a singulated tag's EPC to determine if the tag
        /// will be read from.  Reads may only be performed on 16-bit word boundaries
        /// and for multiples of 16-bit words.  If one or more of the memory words
        /// specified by the offset/count combination do not exist or are read-locked,
        /// the read from the tag will fail and this failure will be reported through
        /// the operation response packet.    The operation-response packets will
        /// be returned to the application via the application-supplied callback
        /// function.  Each tag-read record is grouped with its corresponding tag-
        /// inventory record.  An application may prematurely stop a read operation by
        /// calling RFID_Radio{Cancel|Abort}Operation on another thread or by returning
        /// a non-zero value from the callback function.  A tag read may not be
        /// issued while a radio module is executing a tag-protocol operation.
        /// 
        /// Note that read should not be confused with inventory.  A read allows for
        /// reading a sequence of one or more 16-bit words starting from an arbitrary
        /// 16-bit location in any of the tag's memory banks.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which the read operation will be performed.
        /// This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="parms">pointer to a structure that specifies the ISO 18000-6C tag-read
        /// operation parameters.  This parameter must not be NULL.</param>
        /// <param name="flags">read flags.  May be zero or a combination of the following:
        /// RFID_FLAG_PERFORM_SELECT - perform one or more selects before performing
        ///   the read.
        /// RFID_FLAG_PERFORM_POST_MATCH - perform post-singulation mask match on
        ///   singulated tags.</param>
        /// <returns></returns>
        public Result Tag18K6CRead
        (
            [In] ReadParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // Common parms struct

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.common.tagStopCount);
                lpBufOff += 4;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), Marshal.GetFunctionPointerForDelegate(parms.common.callback));
                lpBufOff += IntPtr.Size;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (IntPtr)parms.common.context);
                lpBufOff += IntPtr.Size;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (IntPtr)parms.common.callbackCode);
                lpBufOff += IntPtr.Size;

                // bank

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.bank);
                lpBufOff += 4;

                // offset

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.offset);
                lpBufOff += 2;

                // count

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.count);
                lpBufOff += 2;

                // access password

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;
#if CS203
                Result = Native.RFID_18K6CTagRead(dllModule, pHandle, lpBufPtr, flags);
#elif CS101
                Result = Native.RFID_18K6CTagRead(lpBufPtr, flags);
#endif

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }



        /// <summary>
        /// Executes a tag write for the tags of interest.  If the
        /// RFID_FLAG_PERFORM_SELECT flag is specified, the tag population is
        /// partitioned (i.e., ISO 18000-6C select) prior to the tag-write operation.
        /// If the RFID_FLAG_PERFORM_POST_MATCH flag is specified, the post-singulation
        /// match mask is applied to a singulated tag's EPC to determine if the tag
        /// will be written to.  Writes may only be performed on 16-bit word boundaries
        /// and for multiples of 16-bit words.  If one or more of the specified memory
        /// words do not exist or are write-locked, the write to the tag will fail and
        /// this failure will be reported through the operation-response packet.  The
        /// operation-response packets will be returned to the application via
        /// the application-supplied callback function.  Each tag-write record is
        /// grouped with its corresponding tag-inventory record.  An application may
        /// prematurely stop a write operation by calling
        /// RFID_Radio{Cancel|Abort}Operation on another thread or by returning a non-
        /// zero value from the callback function.  A tag write may not be
        /// issued while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which the write operation will be performed.
        /// This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="parms">pointer to a structure that specifies the ISO 18000-6C tag-write
        /// operation parameters.  This parameter must not be NULL.</param>
        /// <param name="flags">write flags.  May be zero or a combination of the following:
        /// RFID_FLAG_PERFORM_SELECT - perform one or more selects before performing
        ///   the write.
        /// RFID_FLAG_PERFORM_POST_MATCH - perform post-singulation mask match on
        ///   singulated tags.</param>
        /// <returns></returns>
        public Result Tag18K6CWrite
        (
            [In] WriteParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            IntPtr ptr_A = IntPtr.Zero;
            IntPtr ptr_B = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // Common parms struct

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.common.tagStopCount);
                lpBufOff += 4;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), Marshal.GetFunctionPointerForDelegate(parms.common.callback));
                lpBufOff += IntPtr.Size;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (IntPtr)parms.common.context);
                lpBufOff += IntPtr.Size;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (IntPtr)parms.common.callbackCode);
                lpBufOff += IntPtr.Size;

                // writeType

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.writeType);
                lpBufOff += 4;

                if (WriteType.SEQUENTIAL == parms.writeType)
                {
                    WriteSequentialParms seqParms = (WriteSequentialParms)parms.writeParms;

                    Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)seqParms.length);
                    lpBufOff += 4;

                    Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)seqParms.bank);
                    lpBufOff += 4;

                    Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)seqParms.count);
                    lpBufOff += 2;

                    Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)seqParms.offset);
                    lpBufOff += 2;

                    // Allocate native memory to hold the pData array elements

                    ptr_A = Marshal.AllocHGlobal((int)seqParms.pData.Length * 2);

                    // Marshal the ptr for native dll use...

                    Marshal.WriteIntPtr
                    (
                        new IntPtr(lpBufPtr.ToInt64() + lpBufOff),
                        ptr_A
                    );

                    lpBufOff += IntPtr.Size;

                    // The seq and rnd parms are represented natively as part of a
                    // union so we have to do extra skip here...

                    lpBufOff += IntPtr.Size;

                    // Write out pData elements to the native memory...

                    for (int index = 0; index < seqParms.pData.Length; ++index)
                    {
                        Marshal.WriteInt16
                        (
                            ptr_A,
                            index * 2,
                            (Int16)seqParms.pData[index]
                        );
                    }
                }
                else if (WriteType.RANDOM == parms.writeType)
                {
                    WriteRandomParms seqParms = (WriteRandomParms)parms.writeParms;

                    Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)seqParms.length);
                    lpBufOff += 4;

                    Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)seqParms.bank);
                    lpBufOff += 4;

                    Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)seqParms.count);
                    lpBufOff += 2;

                    Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)seqParms.reserved);
                    lpBufOff += 2;

                    // Allocate native memory for our offset array and marshal ptr

                    ptr_A = Marshal.AllocHGlobal((int)seqParms.pOffset.Length * 2);

                    Marshal.WriteIntPtr
                    (
                        new IntPtr(lpBufPtr.ToInt64() + lpBufOff),
                        ptr_A
                    );

                    lpBufOff += IntPtr.Size;

                    // Allocate native memory for our data array and marshal ptr

                    ptr_B = Marshal.AllocHGlobal((int)seqParms.pData.Length * 2);

                    Marshal.WriteIntPtr
                    (
                        new IntPtr(lpBufPtr.ToInt64() + lpBufOff),
                        ptr_B
                    );


                    // Finally copy over individual ( 16 bit ) words of offsets and
                    // data to the appropriate native memory block...

                    for (int index = 0; index < seqParms.pOffset.Length; ++index)
                    {
                        Marshal.WriteInt16
                        (
                            ptr_A,
                            index * 2,
                            (Int16)seqParms.pOffset[index]
                        );
                    }

                    for (int index = 0; index < seqParms.pData.Length; ++index)
                    {
                        Marshal.WriteInt16
                        (
                            ptr_B,
                            index * 2,
                            (Int16)seqParms.pData[index]
                        );
                    }
                }

                // verify 

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.verify);
                lpBufOff += 4;

                // verifyRetryCount

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.verifyRetryCount);
                lpBufOff += 4;

                // access password

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
#if CS203
                Result = Native.RFID_18K6CTagWrite(dllModule, pHandle, lpBufPtr, flags);
#elif CS101
                Result = Native.RFID_18K6CTagWrite(lpBufPtr, flags);
#endif

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

                if (IntPtr.Zero != ptr_A)
                {
                    Marshal.FreeHGlobal(ptr_A);

                    if (IntPtr.Zero != ptr_B)
                    {
                        Marshal.FreeHGlobal(ptr_B);
                    }
                }
            }

            return Result;
        }

#if Library1300

        public Result Tag18K6CBlockWrite
        (
            [In] Int32 pHandle,
            [In] BlockWriteParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            IntPtr ptr_A = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // length

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // Common parms struct

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.common.tagStopCount);
                lpBufOff += 4;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), Marshal.GetFunctionPointerForDelegate(parms.common.callback));
                lpBufOff += IntPtr.Size;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (IntPtr)parms.common.context);
                lpBufOff += IntPtr.Size;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (IntPtr)parms.common.callbackCode);
                lpBufOff += IntPtr.Size;

                // verify 

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.verify);
                lpBufOff += 4;

                // verifyRetryCount

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.verifyRetryCount);
                lpBufOff += 4;

                // access password

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;

                // bank

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.bank);
                lpBufOff += 4;

                // count

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.count);
                lpBufOff += 2;

                // pData

                ptr_A = Marshal.AllocHGlobal((int)parms.pData.Length);

                // Marshal the ptr for native dll use...

                Marshal.WriteIntPtr
                (
                    new IntPtr(lpBufPtr.ToInt64() + lpBufOff),
                    ptr_A
                );

                lpBufOff += IntPtr.Size;

                // The seq and rnd parms are represented natively as part of a
                // union so we have to do extra skip here...

                lpBufOff += IntPtr.Size;

                // Write out pData elements to the native memory...

                for (int index = 0; index < parms.pData.Length; ++index)
                {
                    Marshal.WriteInt16
                    (
                        ptr_A,
                        index * 2,
                        (Int16)parms.pData[index]
                    );
                }

                Result = Native.RFID_18K6CTagBlockWrite(lpBufPtr, flags);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

                if (IntPtr.Zero != ptr_A)
                {
                    Marshal.FreeHGlobal(ptr_A);

                }
            }

            return Result;
        }
        public Result Tag18K6CBlockErase
        (
            [In] Int32 pHandle,
            [In] BlockEraseParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            IntPtr ptr_A = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // length

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // Common parms struct

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.common.tagStopCount);
                lpBufOff += 4;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), Marshal.GetFunctionPointerForDelegate(parms.common.callback));
                lpBufOff += IntPtr.Size;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (IntPtr)parms.common.context);
                lpBufOff += IntPtr.Size;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (IntPtr)parms.common.callbackCode);
                lpBufOff += IntPtr.Size;

                // verify 

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.verify);
                lpBufOff += 4;

                // verifyRetryCount

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.verifyRetryCount);
                lpBufOff += 4;

                // access password

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;

                // bank

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.bank);
                lpBufOff += 4;

                // count

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.count);

                Result = Native.RFID_18K6CTagBlockWrite(lpBufPtr, flags);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

                if (IntPtr.Zero != ptr_A)
                {
                    Marshal.FreeHGlobal(ptr_A);

                }
            }

            return Result;
        }
#endif
        /// <summary>
        /// Executes a tag kill for the tags of interest.  If the
        /// RFID_FLAG_PERFORM_SELECT flag is specified, the tag population is
        /// partitioned (i.e., ISO 18000-6C select) prior to the tag-kill operation.
        /// If the RFID_FLAG_PERFORM_POST_MATCH flag is specified, the post-singulation
        /// match mask is applied to a singulated tag's EPC to determine if the tag
        /// will be killed.  The operation-response packets will be returned to the
        /// application via the application-supplied callback function.  Each tag-kill
        /// record is grouped with its corresponding tag-inventory record.  An
        /// application may prematurely stop a kill operation by calling
        /// RFID_Radio{Cancel|Abort}Operation on another thread or by returning a non-
        /// zerovalue from the callback function.  A tag kill may not be
        /// issued while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which the kill operation will be performed.
        /// This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="parms">pointer to a structure that specifies the ISO 18000-6C tag-kill
        /// operation parameters.  This parameter must not be NULL.</param>
        /// <param name="flags">kill flags.  May be zero or a combination of the following:
        /// RFID_FLAG_PERFORM_SELECT - perform one or more selects before performing
        ///   the kill.
        /// RFID_FLAG_PERFORM_POST_MATCH - perform post-singulation mask match on
        ///   singulated tags.</param>
        /// <returns></returns>
        public Result Tag18K6CKill
        (
            [In] KillParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // common parms struct

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.common.tagStopCount);
                lpBufOff += 4;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), Marshal.GetFunctionPointerForDelegate(parms.common.callback));
                lpBufOff += IntPtr.Size;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (IntPtr)parms.common.context);
                lpBufOff += IntPtr.Size;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (IntPtr)parms.common.callbackCode);
                lpBufOff += IntPtr.Size;

                // access password

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;

                // kill password

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.killPassword);
                lpBufOff += 4;
#if CS203
                Result = Native.RFID_18K6CTagKill(dllModule, pHandle, lpBufPtr, flags);
#elif CS101
                Result = Native.RFID_18K6CTagKill(lpBufPtr, flags);
#endif
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }


        /// <summary>
        /// Executes a tag lock for the tags of interest.  If the
        /// RFID_FLAG_PERFORM_SELECT flag is specified, the tag population is
        /// partitioned (i.e., ISO 18000-6C select) prior to the tag-lock operation.
        /// If the RFID_FLAG_PERFORM_POST_MATCH flag is specified, the post-singulation
        /// match mask is applied to a singulated tag's EPC to determine if the tag
        /// will be locked.  The operation-response packets will be returned to the
        /// application via the application-supplied callback function.  Each tag-lock
        /// record is grouped with its corresponding tag-inventory record.  An
        /// application may prematurely stop a lock operation by calling
        /// RFID_Radio{Cancel|Abort}Operation on another thread or by returning a non-
        /// zero value from the callback function.  A tag lock may not be
        /// issued while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which the lock operation will be performed.
        /// This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="parms">pointer to a structure that specifies the ISO 18000-6C tag-lock
        /// operation parameters.  This parameter must not be NULL.</param>
        /// <param name="flags">lock flags.  May be zero or a combination of the following:
        /// RFID_FLAG_PERFORM_SELECT - perform one or more selects before performing
        ///   the lock.
        /// RFID_FLAG_PERFORM_POST_MATCH - perform post-singulation mask match on
        ///   singulated tags.</param>
        /// <returns></returns>
        public Result Tag18K6CLock
        (
            [In] LockParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // common parms struct

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.common.tagStopCount);
                lpBufOff += 4;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), Marshal.GetFunctionPointerForDelegate(parms.common.callback));
                lpBufOff += IntPtr.Size;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (IntPtr)parms.common.context);
                lpBufOff += IntPtr.Size;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (IntPtr)parms.common.callbackCode);
                lpBufOff += IntPtr.Size;

                // permissions
                Marshal.StructureToPtr(parms.permissions, new IntPtr(lpBufPtr.ToInt64() + lpBufOff), false);
                lpBufOff += Marshal.SizeOf(typeof(TagPerm));

                // password

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
#if CS203
                Result = Native.RFID_18K6CTagLock(dllModule, pHandle, lpBufPtr, flags);
#elif CS101
                Result = Native.RFID_18K6CTagLock(lpBufPtr, flags);
#endif
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }


        /// <summary>
        /// Stops a currently-executing tag-protocol operation on a radio module.  The
        /// packet callback function will be executed until the command-end packet is
        /// received from the MAC or the packet callback returns a non-zero Result.
        /// 
        /// NOTE: This function must not be called from the packet callback function.
        /// </summary>
        /// <param name="pHandle">the pHandle to the radio for which the tag-protocol operation
        /// should be cancelled.  This is the pHandle from a successful call to
        /// RFID_RadioOpen.</param>
        /// <param name="flags">Reserved for future use.  Set to zero.</param>
        /// <returns></returns>
        public Result RadioCancelOperation
            (
                [In] UInt32 flags
            )
        {
#if CS203
            return Native.RFID_RadioCancelOperation(dllModule, pHandle, flags);
#elif CS101
            return Native.RFID_RadioCancelOperation(flags);
#endif
        }

        /// <summary>
        /// Stops a currently-executing tag-protocol operation on a radio module and
        /// discards all remaining command-reponse packets.
        /// 
        /// NOTE: This function must not be called from the packet callback function.
        /// 
        /// </summary>
        /// <param name="pHandle">the pHandle to the radio for which the tag-protocol operation
        /// should be aborted.  This is the pHandle from a successful call to
        /// RFID_RadioOpen.</param>
        /// <param name="flags">Reserved for future use.  Set to zero.</param>
        /// <returns></returns>
        public Result RadioAbortOperation
        (
            [In] UInt32 flags
        )
        {
#if CS203
            return Native.RFID_RadioAbortOperation(dllModule, pHandle, flags);
#elif CS101
            return Native.RFID_RadioAbortOperation(flags);
#endif
        }

        /// <summary>
        /// Sets the operation response data reporting mode for tag-protocol
        /// operations.  By default, the reporting mode is set to "normal".  The 
        /// reporting mode will remain in effect until a subsequent call to
        /// RFID_RadioSetResponseDataMode.  The mode may not be changed while the
        /// radio is executing a tag-protocol operation.  The data response mode may
        /// not be set while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">the pHandle to the radio for which the operation response data
        /// reporting mode is to be set.  This is the pHandle from a successful call
        /// to RFID_RadioOpen.</param>
        /// <param name="responseType">the type of data that will have its reporting mode set.  For
        /// version 1.0 of the library, the only valid value is
        /// RFID_RESPONSE_TYPE_DATA.</param>
        /// <param name="responseMode">the operation response data reporting mode</param>
        /// <returns></returns>
        public Result RadioSetResponseDataMode
        (
            [In] ResponseType responseType,
            [In] ResponseMode responseMode
        )
        {
#if CS203
            return Native.RFID_RadioSetResponseDataMode(dllModule, pHandle, responseType, responseMode);
#elif CS101
            return Native.RFID_RadioSetResponseDataMode(responseType, responseMode);
#endif
        }

        /// <summary>
        /// Retrieves the operation response data reporting mode for tag-protocol
        /// operations.  The data response mode may not be retrieved while a radio
        /// module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">the pHandle to the radio for which the operation response data
        /// reporting mode is to be retrieved.  This is the pHandle from a successful
        /// call to RFID_RadioOpen.</param>
        /// <param name="responseType">the type of data that will have its reporting mode
        /// retrieved.  For version 1.0 of the library, the only valid value is
        /// RFID_RESPONSE_TYPE_DATA.</param>
        /// <param name="responseMode">a pointer to a RFID_RESPONSE_MODE variable that upon return
        /// will contain the operation response data reporting mode.  Must not be
        /// NULL.</param>
        /// <returns></returns>
        public Result RadioGetResponseDataMode
        (
            [In]      ResponseType responseType,
            [In] ref  ResponseMode responseMode
        )
        {
            try
            {
#if CS203
                return Native.RFID_RadioGetResponseDataMode(dllModule, pHandle, responseType, ref responseMode);
#elif CS101
                return Native.RFID_RadioGetResponseDataMode(responseType, ref responseMode);
#endif
            }
            catch
            {
                // If encountered an error while auto-unmarshalling then
                // we have seen an int value that cannot be converted to
                // responseType or responseMode - mark both unknown.

                responseType = ResponseType.UNKNOWN;
                responseMode = ResponseMode.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }



        /// <summary>
        /// Writes the specified data to the radio module nonvolatile-memory 
        /// block(s).  After a successful update, the RFID radio module resets itself
        /// and the RFID Reader Library closes and invalidates the radio pHandle so that
        /// it may no longer be used by the application.
        /// 
        /// In the case of an unsuccessful update the RFID Reader Library does not
        /// invalidate the radio pHandle ?i.e., it is the application responsibility
        /// to close the pHandle.
        /// 
        /// Alternatively, an application can perform the update in “test?mode.
        /// An application uses the “test?mode, by checking the returned status, to
        /// verify that the update would succeed before performing the destructive
        /// update of the radio module nonvolatile memory.  When a “test?update has
        /// completed, either successfully or unsuccessfully, the MAC firmware returns
        /// to its normal idle state and the radio pHandle remains valid (indicating
        /// that the application is still responsible for closing it).
        /// 
        /// The radio module nonvolatile memory may not be updated while a radio
        /// module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">The pHandle, previously returned by a successful call to
        /// RFID_RadioOpen, of the RFID radio module for which the nonvolatile
        /// memory should be updated.</param>
        /// <param name="countBlocks">The number of nonvolatile memory blocks in the array pointed
        /// to by pBlocks.  This value must be greater than zero.</param>
        /// <param name="pBlocks">A pointer to an array of countBlocks nonvolatile memory block
        /// structures that are used to control the update of the radio module
        /// nonvolatile memory.  This pointer must not be NULL.</param>
        /// <param name="flags">Zero, or a combination of the following:
        /// RFID_FLAG_TEST_UPDATE - Indicates that the RFID Reader Library is to
        ///   perform a non-destructive nonvolatile memory update to verify that
        ///   the update would succeed.  The RFID Reader Library will perform all
        ///   of the update operations with the exception that the data will not be
        ///   committed to nonvolatile memory.</param>
        /// <returns></returns>
        public Result MacUpdateNonvolatileMemory
        (
            [In] UInt32 countBlocks,
            [In] NonVolatileMemoryBlock[] pBlocks,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr pBlocksPtr = IntPtr.Zero;
            IntPtr[] pDataPtrs = null;

            // Short-circuit check of parameter validity... not reason to
            // dive into native code if we know the parameters are wrong.

            if (countBlocks > pBlocks.Length)
            {
                return Result.INVALID_PARAMETER;
            }

            try
            {
                // Allocate memory to hold the pData arrays that
                // are held by the individual pBlock structures

                pDataPtrs = new IntPtr[countBlocks];

                for (int idx = 0; idx < countBlocks; ++idx)
                {
                    if (pBlocks[idx].length > pBlocks[idx].pData.Length)
                    {
                        return Result.INVALID_PARAMETER;
                    }

                    pDataPtrs[idx] = Marshal.AllocHGlobal((int)pBlocks[idx].length);

                    // Copy C# byte array data into native...

                    Marshal.Copy(pBlocks[idx].pData, 0, pDataPtrs[idx], (int)pBlocks[idx].length);
                }

                // Allocate memory to hold countBlocks count
                // of pBlocks structures

                pBlocksPtr = Marshal.AllocHGlobal((int)(countBlocks * (12 + IntPtr.Size)));

                Int32 pBlocksPtrOffset = 0;

                // Copy pBlocks structures over to native memory

                for (int idx = 0; idx < countBlocks; ++idx)
                {
                    Marshal.WriteInt32(pBlocksPtr, pBlocksPtrOffset, (Int32)pBlocks[idx].address);
                    pBlocksPtrOffset += 4;

                    Marshal.WriteInt32(pBlocksPtr, pBlocksPtrOffset, (Int32)pBlocks[idx].length);
                    pBlocksPtrOffset += 4;

                    Marshal.WriteIntPtr(new IntPtr(pBlocksPtr.ToInt64() + pBlocksPtrOffset), pDataPtrs[idx]);
                    pBlocksPtrOffset += IntPtr.Size;

                    Marshal.WriteInt32(pBlocksPtr, pBlocksPtrOffset, (Int32)pBlocks[idx].flags);
                    pBlocksPtrOffset += 4;
                }
#if CS203
                Result = Native.RFID_MacUpdateNonvolatileMemory(dllModule, pHandle, countBlocks, pBlocksPtr, flags);
#elif CS101
                Result = Native.RFID_MacUpdateNonvolatileMemory(countBlocks, pBlocksPtr, flags);
#endif
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (null != pDataPtrs)
            {
                for (int idx = 0; idx < countBlocks; ++idx)
                {
                    if (IntPtr.Zero != pDataPtrs[idx])
                    {
                        Marshal.FreeHGlobal(pDataPtrs[idx]);
                    }
                }
            }
            if (IntPtr.Zero != pBlocksPtr)
            {
                Marshal.FreeHGlobal(pBlocksPtr);
            }

            return Result;
        }

        /// <summary>
        /// Retrieves the radio module's MAC firmware version information.  The MAC
        /// version may not be retrieved while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which MAC firmware version information is
        /// requested.  This is the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="pVersion">pointer to structure that upon return will contain the radio
        /// module's MAC firmware version information.  Must not be NULL.</param>
        /// <returns></returns>
        public Result MacGetVersion
        (
            [In, Out] MacVersion pVersion
        )
        {
#if CS203
            return Native.RFID_MacGetVersion(dllModule, pHandle, pVersion);
#elif CS101
            return Native.RFID_MacGetVersion(pVersion);
#endif
        }

        /// <summary>
        /// Reads one or more 32-bit words from the MAC's OEM configuration data
        /// area.  The OEM data are may not be read while a radio module is executing
        /// a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which MAC OEM configuration data is to be
        /// read.  This is the pHandle returned from a successful call to
        /// RFID_RadioOpen.</param>
        /// <param name="address">the 32-bit address of the first 32-bit word to read from
        /// the MAC's OEM configuration data area.  Note that this is not a byte
        /// address - i.e., address 1 is actually byte 4, address 2 is actually byte
        /// 8, etc.  If the address is beyond the end of the OEM configuration data
        /// area, Results in an error.</param>
        /// <param name="count">the number of 32-bit words to read.   Must be greater
        /// than zero.  If count causes the read to extend beyond the end of the
        /// OEM configuration data area, Results in an error.</param>
        /// <param name="data"> pointer to the buffer into which the OEM configuration data will
        /// be placed.  The buffer must be at least (count * 4) bytes in length.
        /// Must not be NULL.  Note that the data returned will be in the MAC's
        /// native format (i.e., little endian).</param>
        /// <returns></returns>
        public Result MacReadOemData
        (
            [In] UInt32 address,
            [In] UInt32 count,
            [In] UInt32[] data
        )
        {
            Result Result = Result.OK;
            IntPtr dataPtr = IntPtr.Zero;

            // Short-circuit check of parameter validity... not reason to
            // dive into native code if we know the parameters are wrong.

            if (count > data.Length)
            {
                return Result.INVALID_PARAMETER;
            }

            try
            {
                dataPtr = Marshal.AllocHGlobal((int)(count * 4));
#if CS203
                Result = Native.RFID_MacReadOemData(dllModule, pHandle, address, count, dataPtr);
#elif CS101
               Result = Native.RFID_MacReadOemData(address, count, dataPtr);
#endif

                // Required due to non-castability of data to int[ ] ?

                for (int index = 0; index < count; ++index)
                {
                    data[index] = (UInt32)Marshal.ReadInt32(dataPtr, index * 4);
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != dataPtr)
            {
                Marshal.FreeHGlobal(dataPtr);
            }

            return Result;
        }


        /// <summary>
        /// Writes one or more 32-bit words to the MAC's OEM configuration data
        /// area.  The OEM data area may not be written while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which MAC OEM configuration data is to be
        /// written.  This is the pHandle returned from a successful call to
        /// RFID_RadioOpen.</param>
        /// <param name="address">the 32-bit address of the first 32-bit word to write in
        /// the MAC's OEM configuration data area.  Note that this is not a byte
        /// address - i.e., address 1 is actually byte 4, address 2 is actually byte
        /// 8, etc.  If the address is beyond the end of the OEM configuration data
        /// area, Results in an error.</param>
        /// <param name="count">the number of 32-bit words to write.   Must be greater
        /// than zero.  If count causes the write to extend beyond the end of the
        /// OEM configuration data area, Results in an error.</param>
        /// <param name="data">pointer to the buffer that contains the data to write to the OEM
        /// configuration area.  The buffer must be at least (count * 4) bytes in
        /// length.  Must not be NULL.  Note that the data must be in the MAC's
        /// native format (i.e., little endian).</param>
        /// <returns></returns>
        public Result MacWriteOemData
        (
            [In] UInt32 address,
            [In] UInt32 count,
            [In] UInt32[] data
        )
        {
            Result Result = Result.OK;
            IntPtr dataPtr = IntPtr.Zero;

            try
            {
                dataPtr = Marshal.AllocHGlobal((int)(count * 4));

                // Required due to non-castability of data to int[]

                for (int index = 0; index < count; ++index)
                {
                    Marshal.WriteInt32(dataPtr, index * 4, (int)data[index]);
                }
#if CS203
                Result = Native.RFID_MacWriteOemData(dllModule, pHandle, address, count, dataPtr);
#elif CS101
                Result = Native.RFID_MacWriteOemData(address, count, dataPtr);
#endif
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != dataPtr)
            {
                Marshal.FreeHGlobal(dataPtr);
            }

            return Result;
        }


        /// <summary>
        /// Instructs the radio module's MAC firmware to perform the specified reset.
        /// Any currently executing tag-protocol operations will be aborted, any
        /// unconsumed data will be discarded, and tag-protocol operation functions
        /// (i.e., RFID_18K6CTagInventory, etc.) will return immediately with an
        /// error of RFID_ERROR_OPERATION_CANCELLED.
        /// Upon reset, the connection to the radio module is lost and the pHandle
        /// to the radio is invalid.  To obtain control of the radio module after it
        /// has been reset, the application must re-enumerate the radio modules, via
        /// RFID_RetrieveAttachedRadiosList, and request control via RFID_RadioOpen.
        /// 
        /// NOTE: This function must not be called from the packet callback function.
        /// 
        /// </summary>
        /// <param name="pHandle">pHandle for the radio which will be reset.  This is the pHandle that
        /// was returned from a successful call to RFID_RadioOpen.  Upon return
        /// the pHandle is invalid and may only be used for RFID_RadioClose.</param>
        /// <param name="resetType">the type of reset to perform on the radio</param>
        /// <returns></returns>
        public Result MacReset
        (
            [In] MacResetType resetType
        )
        {
#if CS203
            return Native.RFID_MacReset(dllModule, pHandle, resetType);
#elif CS101
            return Native.RFID_MacReset(resetType);
#endif
        }


        /// <summary>
        /// Attempts to clear the error state for the radio module MAC firmware.  The
        /// MAC error may not be cleared while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle for the radio which will have MAC error state cleared.
        /// This is the pHandle thatwas returned from a successful call to
        /// RFID_RadioOpen.</param>
        /// <returns></returns>
        public Result MacClearError
        (
        )
        {
#if CS203
            return Native.RFID_MacClearError(dllModule, pHandle);
#elif CS101
            return Native.RFID_MacClearError(pHandle);
#endif
        }
        /// <summary>
        /// Allows for direct writing of registers on the radio (i.e., bypassing the
        /// MAC).  The radio registers may not be written while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which register is to be written.  This is
        /// the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="address">the address of the register to write.  An address that is beyond
        ///  the end of the radio module's register set Results in an invalid-parameter
        ///  return status.</param>
        /// <param name="value">the value to write to the register</param>
        /// <returns></returns>
        public Result MacBypassWriteRegister
        (
            [In] UInt16 address,
            [In] UInt16 value
        )
        {
#if CS203
            return Native.RFID_MacBypassWriteRegister(dllModule, pHandle, address, value);
#elif CS101
            return Native.RFID_MacBypassWriteRegister(address, value);
#endif
        }

        /// <summary>
        /// Allows for direct reading of registers on the radio (i.e., bypassing the
        /// MAC).  The radio regsiters mode may not be read while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which register is to be read.  This is
        /// the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="address">the address of the register to write  An address that is beyond
        ///  the end of the radio module's register set Results in an invalid-parameter
        ///  return status.</param>
        /// <param name="value">pointer to unsigned 16-bit integer that will receive register
        /// value.  This parameter must not be NULL.</param>
        /// <returns></returns>
        public Result MacBypassReadRegister
        (
            [In]     UInt16 address,
            [In] ref UInt16 value
        )
        {
#if CS203
            return Native.RFID_MacBypassReadRegister(dllModule, pHandle, address, ref value);
#elif CS101
            return Native.RFID_MacBypassReadRegister(address, ref value);
#endif
        }

        /// <summary>
        /// Sets the regulatory mode region for the MAC's operation.  The region of 
        /// operation may not be set while a radio module is executing a tag-protocol
        /// operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which region is to be set.  This is
        /// the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="region">the region to which the radio operation is to be set.</param>
        /// <param name="regionConfig">reserved for future use.  Must be NULL.</param>
        /// <returns></returns>
        public Result MacSetRegion
        (
            [In] MacRegion region,
            [In] IntPtr regionConfig
        )
        {
            // See notes - region config null currently required...
#if CS203
            return Native.RFID_MacSetRegion(dllModule, pHandle, region, IntPtr.Zero);
#elif CS101
            return Native.RFID_MacSetRegion(region, IntPtr.Zero);
#endif
        }


        /// <summary>
        /// Retrieves the regulatory mode region for the MAC's operation.  The region
        /// of operation may not be retrieved while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio upon which region is to be retrieved.  This is
        /// the pHandle from a successful call to RFID_RadioOpen.</param>
        /// <param name="region">pointer to variable that will receive region.  Must not be NULL.</param>
        /// <param name="regionConfig">reserved for future use.  Must be NULL.</param>
        /// <returns></returns>
        public Result MacGetRegion
        (
            [In] ref MacRegion region,
            [In]     IntPtr regionConfig
        )
        {
            // See notes - region config null currently required...
#if CS203
            return Native.RFID_MacGetRegion(dllModule, pHandle, ref region, IntPtr.Zero);
#elif CS101
            return Native.RFID_MacGetRegion(ref region, IntPtr.Zero);
#endif
        }

        /// <summary>
        /// Configures the specified radio module's GPIO pins.  For version 1.0 of the
        /// library, only GPIO pins 0-3 are valid.  The GPIO pin configuration may not
        /// be set while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which GPIO pins should be configured.  This is
        /// the pHandle that was returned from a successful call to RFID_RadioOpen.</param>
        /// <param name="mask">a 32-bit mask which specifies which GPIO pins are to be configured.
        /// Bit 0 (i.e., the lowest-order bit) represents GPIO pin 0, bit 1
        /// represents GPIO pin 1, etc.  The presence of a 1 bit in a mask-bit
        /// location indicates that the GPIO pin is to be configured.  The presence
        /// of a 0 bit in a mask-bit location indicates that the GPIO pin
        /// configuration is to remain unchanged.</param>
        /// <param name="configuration">A 32-bit value that indicates the configuration for the
        /// bits corresponding to the ones set in mask ?bit 0 (i.e., the lowest-
        /// order bit) represents GPIO pin 0's configuration, etc.  Bits which
        /// correspond to bits set to 0 in mask are ignored.  The presence of a 1 in
        /// a bit location indicates that the GPIO pin is to be configured as an
        /// output pin.  The presence of a 0 in a bit location indicates that the
        /// GPIO pin is to be configured as an input pin.</param>
        /// <returns></returns>
        public Result RadioSetGpioPinsConfiguration
        (
            [In] UInt32 mask,
            [In] UInt32 configuration
        )
        {
#if CS203
            return Native.RFID_RadioSetGpioPinsConfiguration(dllModule, pHandle, mask, configuration);
#elif CS101
            return Native.RFID_RadioSetGpioPinsConfiguration(mask, configuration);
#endif
        }

        /// <summary>
        /// Retrieves the configuration for the radio module's GPIO pins.  For version
        /// 1.0 of the library, only GPIO pins 0-3 are valid.  The GPIO pin 
        /// configuration may not be retrieved while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which GPIO pin configuration should be
        /// retrieved.  This is the pHandle that was returned from a successful call
        /// to RFID_RadioOpen.</param>
        /// <param name="configuration">A pointer to an unsigned 32-bit integer that upon return
        /// contains the configuration for the radio module GPIO pins ?bit 0
        /// (i.e., the lowest-order bit) represents GPIO pin 0, etc.  The presence
        /// of a 1 in a bit location indicates that the GPIO pin is configured as an
        /// output pin.  The presence of a 0 in a bit location indicates that the
        /// GPIO pin is configured as an input pin.</param>
        /// <returns></returns>
        public Result RadioGetGpioPinsConfiguration
        (
            [In] ref UInt32 configuration
        )
        {
#if CS203
            return Native.RFID_RadioGetGpioPinsConfiguration(dllModule, pHandle, ref configuration);
#elif CS101
            return Native.RFID_RadioGetGpioPinsConfiguration(ref configuration);
#endif
        }

        /// <summary>
        /// Reads the specified radio module's GPIO pins.  Attempting to read from an
        /// output GPIO pin Results in an error.  For version 1.0 of the library, only
        /// GPIO pins 0-3 are valid.  The GPIO pins may not be read while a radio 
        /// module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which GPIO pins should be read.  This is the
        ///   pHandle that was returned from a successful call to RFID_RadioOpen.</param>
        /// <param name="mask">a 32-bit mask which specifies which GPIO pins are to be read.  Bit
        ///   0 (i.e., the lowest-order bit) represents GPIO pin 0, bit 1 represents
        ///   GPIO pin 1, etc.  The presence of a 1 bit in a mask bit location
        ///   indicates that the GPIO pin is to be read.</param>
        /// <param name="value">a pointer to a 32-bit unsigned integer that upon return will
        ///   contain the bit values of the GPIO pins specified in the mask.  Bit 0 of
        ///   the *pValue corresponds to GPIO pin 0, bit 1 corresponds to GPIO
        ///   pin 1, etc.  If a GPIO pin's bit is not set in mask, then the bit value
        ///   in the corresponding bit in *pValue is undefined.</param>
        /// <returns></returns>
        public Result RadioReadGpioPins
        (
            [In]     UInt32 mask,
            [In] ref UInt32 value
        )
        {
#if CS203
            return Native.RFID_RadioReadGpioPins(dllModule, pHandle, mask, ref value);
#elif CS101
            return Native.RFID_RadioReadGpioPins(mask, ref value);
#endif
        }
        /// <summary>
        /// Writes the specified radio module's GPIO pins.  Attempting to write to an
        /// input GPIO pin Results in an error.  For version 1.0 of the library, only
        /// GPIO pins 0-3 are valid.  The GPIO pins may not be written while a radio
        /// module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pHandle">pHandle to radio for which GPIO pins should be written.  This is
        /// the pHandle that was returned from a successful call to RFID_RadioOpen.</param>
        /// <param name="mask">a 32-bit mask which specifies which GPIO pins are to be written.
        /// Bit 0 (i.e., the lowest-order bit) represents GPIO pin 0, bit 1
        /// represents GPIO pin 1, etc.  The presence of a 1 in a mask bit location
        /// indicates that the corresponding bit in value is to be written to the 
        /// GPIO pin.</param>
        /// <param name="value">
        /// a 32-bit unsigned integer that contains the bits to write to the
        /// GPIO pins specifed in mask.  Bit 0 of the value corresponds to the value
        /// to write to GPIO pin 0, bit 1 corresponds to the value to write to GPIO
        /// pin 1, etc.  If a GPIO pin's bit is not set in mask, then the bit value
        /// in the corresponding bit is ignored.</param>
        /// <returns></returns>
        public Result RadioWriteGpioPins
        (
            [In] UInt32 mask,
            [In] UInt32 value
        )
        {
#if CS203
            return Native.RFID_RadioWriteGpioPins(dllModule, pHandle, mask, value);
#elif CS101
            return Native.RFID_RadioWriteGpioPins(mask, value);
#endif
        }

        #region WaterYu @ 16-Sept-2009
        public Result Tag18K6CWriteEPC
        (
            [In] TagWriteEpcParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                if (parms == null || parms.count == 0)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal((int)parms.Length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.Length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.retryCount);
                lpBufOff += 4;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.offset);
                lpBufOff += 2;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.count);
                lpBufOff += 2;

                Marshal.Copy(parms.epc.ToShorts(), 0, new IntPtr(lpBufPtr.ToInt64() + lpBufOff), parms.epc.GetLength());

#if CS203
                Result = Native.RFID_18K6CTagWriteEPC(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagWriteEPC(lpBufPtr);
#endif

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

            }

            return Result;
        }

        public Result Tag18K6CWritePC
        (
            [In] TagWritePcParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagWritePcParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);
#if CS203
                Result = Native.RFID_18K6CTagWritePC(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagWritePC(lpBufPtr);
#endif

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CWriteAccPwd
        (
            [In] TagWritePwdParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagWritePwdParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);
#if CS203
                Result = Native.RFID_18K6CTagWriteAccPwd(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagWriteAccPwd(lpBufPtr);
#endif

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CWriteKillPwd
        (
            [In] TagWritePwdParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagWritePwdParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);
#if CS203
                Result = Native.RFID_18K6CTagWriteKillPwd(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagWriteKillPwd(lpBufPtr);
#endif

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }
        public Result Tag18K6CWriteUser
        (
            [In] TagWriteUserParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            IntPtr ptr_A = IntPtr.Zero;

            try
            {
                if (parms == null || parms.pData == null || parms.pData.Length != parms.count || parms.count == 0)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.Length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.Length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.retryCount);
                lpBufOff += 4;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.offset);
                lpBufOff += 2;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.count);
                lpBufOff += 2;


                ptr_A = Marshal.AllocHGlobal(parms.pData.Length * 2);

                Marshal.WriteIntPtr
                (
                    new IntPtr(lpBufPtr.ToInt64() + lpBufOff),
                    ptr_A
                );

                lpBufOff += IntPtr.Size;

                for (int index = 0; index < parms.count; ++index)
                {
                    Marshal.WriteInt16
                    (
                        ptr_A,
                        index * 2,
                        (Int16)parms.pData[index]
                    );
                }
#if CS203
                Result = Native.RFID_18K6CTagWriteUser(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagWriteUser(lpBufPtr);
#endif

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

                if (IntPtr.Zero != ptr_A)
                {
                    Marshal.FreeHGlobal(ptr_A);
                }
            }

            return Result;
        }

        public Result Tag18K6CReadEPC
        (
            [In] TagReadEpcParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null || parms.count == 0)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagReadEpcParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);
#if CS203
                Result = Native.RFID_18K6CTagReadEPC(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagReadEPC(lpBufPtr);
#endif

                if (Result == Result.OK)
                {
                    Marshal.PtrToStructure(lpBufPtr, parms);
                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CReadTID
        (
            [In] TagReadTidParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null || parms.count == 0)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagReadTidParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);
#if CS203
                Result = Native.RFID_18K6CTagReadTID(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagReadTID(lpBufPtr);
#endif

                if (Result == Result.OK)
                {
                    Marshal.PtrToStructure(lpBufPtr, parms);

                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CReadPC
        (
            [In] TagReadPcParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagReadPcParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);
#if CS203
                Result = Native.RFID_18K6CTagReadPC(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagReadPC(lpBufPtr);
#endif

                if (Result == Result.OK)
                {
                    Marshal.PtrToStructure(lpBufPtr, parms);
                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CReadAccPwd
        (
            [In] TagReadPwdParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagReadPwdParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);
#if CS203
                Result = Native.RFID_18K6CTagReadAccPwd(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagReadAccPwd(lpBufPtr);
#endif

                if (Result == Result.OK)
                {
                    Marshal.PtrToStructure(lpBufPtr, parms);
                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CReadKillPwd
        (
            [In] TagReadPwdParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagReadPwdParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);
#if CS203
                Result = Native.RFID_18K6CTagReadKillPwd(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagReadKillPwd(lpBufPtr);
#endif

                if (Result == Result.OK)
                {
                    Marshal.PtrToStructure(lpBufPtr, parms);
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }
        public Result Tag18K6CReadUser
        (
            [In] TagReadUserParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            IntPtr ptr_A = IntPtr.Zero;

            try
            {
                if (parms == null || parms.count == 0)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.retryCount);
                lpBufOff += 4;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.offset);
                lpBufOff += 2;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.count);
                lpBufOff += 2;

                ptr_A = Marshal.AllocHGlobal(parms.count * 2);

                Marshal.WriteIntPtr
                (
                    new IntPtr(lpBufPtr.ToInt64() + lpBufOff),
                    ptr_A
                );

                lpBufOff += IntPtr.Size;
#if CS203
                Result = Native.RFID_18K6CTagReadUser(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagReadUser(lpBufPtr);
#endif

                if (Result == Result.OK)
                {
                    short[] tmp = new short[parms.count];

                    Marshal.Copy(ptr_A, tmp, 0, parms.count);

                    parms.pData = new S_DATA(tmp);
                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

                if (IntPtr.Zero != ptr_A)
                {
                    Marshal.FreeHGlobal(ptr_A);
                }
            }

            return Result;
        }

        public Result Tag18K6CRawLock
        (
            [In] TagLockParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            //Int32 lpBufOff = 0;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagLockParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                // count
                /*
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.Length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.retryCount);
                lpBufOff += 4;

                // permissions
                Marshal.StructureToPtr(parms.permissions, new IntPtr(lpBufPtr.ToInt64() + lpBufOff), false);*/
#if CS203
                Result = Native.RFID_18K6CTagRawLock(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagRawLock(lpBufPtr);
#endif

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

            }

            return Result;
        }
        public Result Tag18K6CRawKill
        (
            [In] TagKillParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            //Int32 lpBufOff = 0;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagKillParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);
#if CS203
                Result = Native.RFID_18K6CTagRawKill(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagRawKill(lpBufPtr);
#endif

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

            }

            return Result;
        }

        public Result Tag18K6CBlockLock
            (
            [In] TagBlockPermalockParms parms
            )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero, ptr_A = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // offset

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)(parms.setPermalock ? 0x1 : 0));
                lpBufOff += 4;
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.retryCount);
                lpBufOff += 4;
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.flags);
                lpBufOff += 4;
                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.count);
                lpBufOff += 2;
                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.offset);
                lpBufOff += 2;

                //parms.mask = new ushort[parms.count];

                ptr_A = Marshal.AllocHGlobal(parms.count * 2);

                Marshal.WriteIntPtr
                (
                    new IntPtr(lpBufPtr.ToInt64() + lpBufOff),
                    ptr_A
                );

                lpBufOff += IntPtr.Size;

                if (parms.mask != null && parms.setPermalock)
                {
                    for (int index = 0; index < parms.count; ++index)
                    {
                        Marshal.WriteInt16
                        (
                            ptr_A,
                            index * 2,
                            (Int16)parms.mask[index]
                        );
                    }
                }

#if CS203
                Result = Native.RFID_18K6CTagBlockPermalock(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagBlockPermalock(lpBufPtr, flags);
#endif
                if (Result == Result.OK && !parms.setPermalock)
                {
                    byte[] tmpByte = new byte[parms.count * 2];
                    parms.mask = new ushort[parms.count];
                    Marshal.Copy(ptr_A, tmpByte, 0, parms.count * 2);
                    for (int i = 0; i < parms.count; i++)
                    {
                        parms.mask[i] = (ushort)(tmpByte[i + 1] << 8 | tmpByte[i]);
                    }
                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                if (ptr_A != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr_A);
                }

                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CSearchAny
        (
            [In] InternalTagInventoryParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.flags);
                lpBufOff += 4;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), Marshal.GetFunctionPointerForDelegate(parms.pCallback));
#if CS203
                Result = Native.RFID_18K6CTagSearchAny(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagSearchAny(lpBufPtr);
#endif

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CSearchOne
        (
           [In] InternalTagSearchOneParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, parms.avgRssi ? 1 : 0);
                lpBufOff += 4;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), Marshal.GetFunctionPointerForDelegate(parms.pCallback));
#if CS203
                Result = Native.RFID_18K6CTagSearchOne(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagSearchOne(lpBufPtr);
#endif

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CRanging
        (
           [In] InternalTagRangingParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.flags);
                lpBufOff += 4;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), Marshal.GetFunctionPointerForDelegate(parms.pCallback));
#if CS203
                Result = Native.RFID_18K6CTagRanging(dllModule, pHandle, lpBufPtr);
#elif CS101
                Result = Native.RFID_18K6CTagRanging(lpBufPtr);
#endif

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }
        #endregion

#if NOUSE
        public Result Initialize(IntPtr hWnd)
        {
            return Native.rfidmx_Initialize(hWnd);
        }

        public Result Uninitialize()
        {
            return Native.rfidmx_Uninitialize();
        }

        public Result PostMessage
        (
            [In] RFID_OPERATION operation,
            [In] cmnparm parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            int lpBufOff = 0;

            try
            {
                switch (operation)
                {
                    case RFID_OPERATION.TAG_BLOCK_ERASE:

                        break;
                    case RFID_OPERATION.TAG_BLOCK_WRITE:
                        break;
                    case RFID_OPERATION.TAG_INVENTORY:
                        if (parms.GetType() != typeof(CB_INV_PARMS))
                        {
                            return Result.INVALID_PARAMETER;
                        }
                        CB_INV_PARMS invparm = (CB_INV_PARMS)parms;

                        lpBufPtr = Marshal.AllocHGlobal((int)invparm.length);

                        Marshal.WriteInt32(lpBufPtr, 0, (Int32)invparm.length);
                        Marshal.WriteInt32(lpBufPtr, 4, (Int32)invparm.pHandle);
                        Marshal.WriteInt32(lpBufPtr, 8, (Int32)invparm.flags);
                        Marshal.WriteInt32(lpBufPtr, 12, (Int32)invparm.tagStopCount);
                        //Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + 16), Marshal.GetFunctionPointerForDelegate(invparm.callback));
                        Result = Native.rfidmx_PostMessage(operation, lpBufPtr);
                        break;
                    case RFID_OPERATION.TAG_KILL:
                    case RFID_OPERATION.TAG_LOCK:
                        break;
                    case RFID_OPERATION.TAG_READ:
                        if (parms.GetType() != typeof(CB_READ_PARMS))
                        {
                            return Result.INVALID_PARAMETER;
                        }
                        CB_READ_PARMS parm = (CB_READ_PARMS)parms;

                        lpBufPtr = Marshal.AllocHGlobal((int)parm.length);

                        WriteInt32(lpBufPtr, ref lpBufOff, parm.length);

                        WriteInt32(lpBufPtr, ref lpBufOff, parm.pHandle);
                        WriteInt32(lpBufPtr, ref lpBufOff, parm.flags);

                        // count
                        WriteInt32(lpBufPtr, ref lpBufOff, parm.parms.length);

                        // Common parms struct

                        WriteInt32(lpBufPtr, ref lpBufOff, parm.parms.common.tagStopCount);

                        WriteIntPtr(lpBufPtr, ref lpBufOff, IntPtr.Zero);

                        WriteIntPtr(lpBufPtr, ref lpBufOff, IntPtr.Zero);

                        WriteIntPtr(lpBufPtr, ref lpBufOff, IntPtr.Zero);

                        // bank

                        WriteInt32(lpBufPtr, ref lpBufOff, (int)parm.parms.bank);

                        // offset

                        WriteInt16(lpBufPtr, ref lpBufOff, parm.parms.offset);

                        // count

                        WriteInt16(lpBufPtr, ref lpBufOff, parm.parms.count);

                        // access password

                        WriteInt32(lpBufPtr, ref lpBufOff, parm.parms.accessPassword);


                        Result = Native.rfidmx_PostMessage(operation, lpBufPtr);
                        break;
                    case RFID_OPERATION.TAG_WRITE:
                        break;
                    case RFID_OPERATION.TAG_SEARCH:
                        if (parms.GetType() != typeof(CB_TAG_SEARCH_PARMS))
                        {
                            return Result.INVALID_PARAMETER;
                        }
                        CB_TAG_SEARCH_PARMS searchparm = (CB_TAG_SEARCH_PARMS)parms;

                        lpBufPtr = Marshal.AllocHGlobal((int)searchparm.length);

                        WriteInt32(lpBufPtr, ref lpBufOff, (Int32)searchparm.length);
                        WriteInt32(lpBufPtr, ref lpBufOff, (Int32)searchparm.pHandle);
                        WriteInt32(lpBufPtr, ref lpBufOff, searchparm.average ? 1 : 0);
                        WriteInt32(lpBufPtr, ref lpBufOff, searchparm.bUsePc ? 1 : 0);
                        WriteInt32(lpBufPtr, ref lpBufOff, (Int32)searchparm.retryCount);
                        WriteInt32(lpBufPtr, ref lpBufOff, (Int32)searchparm.masklen);
                        Marshal.Copy(searchparm.pc_epc_mask, 0, new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (Int32)searchparm.masklen);
                        Result = Native.rfidmx_PostMessage(operation, lpBufPtr);
                        break;
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        private void WriteInt32(IntPtr ptr, ref int offset, Int32 value)
        {
            Marshal.WriteInt32(ptr, offset, value);
            offset += 4;
        }
        private void WriteInt32(IntPtr ptr, ref int offset, UInt32 value)
        {
            Marshal.WriteInt32(ptr, offset, (Int32)value);
            offset += 4;
        }
        private void WriteInt16(IntPtr ptr, ref int offset, UInt16 value)
        {
            Marshal.WriteInt16(ptr, offset, (Int16)value);
            offset += 2;
        }
        private void WriteInt16(IntPtr ptr, ref int offset, Int16 value)
        {
            Marshal.WriteInt16(ptr, offset, value);
            offset += 2;
        }

        private void WriteIntPtr(IntPtr ptr, ref int offset, IntPtr value)
        {
            Marshal.WriteIntPtr(new IntPtr(ptr.ToInt64() + offset), value);
            offset += IntPtr.Size;
        }
#endif

    }  // Linkage class END
#else
        /// <summary>
    /// Native Class Library Linkage
    /// </summary>
    sealed class Linkage
    {
        private RfidHandle pHandle;

#if USB_BUILD
        /// <summary>
        /// Initializes the RFID Reader Library
        /// </summary>
        /// <param name="pVersion">pointer to structure that on return will contain the
        /// version of the library.  May be NULL if not required by application.</param>
        /// <param name="mode">library startup flags.  May be zero or a combination of the
        /// following:
        /// RFID_FLAG_LIBRARY_EMULATION - libary should be run in emulation mode</param>
        /// <returns></returns>
        public Result Startup
        (
            [In, Out] LibraryVersion pVersion,
            [In]      LibraryMode mode
        )
        {
            pHandle = new RfidHandle();
            return Native.RFID_Startup(out pHandle, pVersion, mode);
        }
#elif NET_BUILD
        //public IntPtr  = IntPtr.Zero;
            
        /// <summary>
        /// Initializes the RFID Reader Library
        /// </summary>
        /// <param name="pVersion">pointer to structure that on return will contain the
        /// version of the library.  May be NULL if not required by application.</param>
        /// <param name="mode">library startup flags.  May be zero or a combination of the
        /// following:
        /// RFID_FLAG_LIBRARY_EMULATION - libary should be run in emulation mode</param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Result Startup
        (
            [In, Out]   LibraryVersion pVersion,
            [In]        LibraryMode mode,
            [In]        String ip,
            [In]        UInt32 port,
            [In]        UInt32 timeout
        )
        {
            Result rc = Result.OK;

            if (ip == null || ip.Length == 0)
            {
                return Result.INVALID_PARAMETER;
            }
            pHandle = new RfidHandle();
            rc = Native.RFID_Startup(out pHandle, pVersion, mode, (ulong)System.Net.IPAddress.Parse(ip).Address, port, timeout);

            //GC.KeepAlive(this);

            return rc;
        }
#endif

        /// <summary>
        /// Shuts down RFID Reader Library, cleaning up all resources including closing
        /// all open radio handles and returning radios to idle.
        /// 
        /// NOTE: This function must not be called from the packet callback function.
        /// 
        /// </summary>
        /// <returns></returns>
        public Result Shutdown()
        {
            return Native.RFID_Shutdown(pHandle);
        }


        /// <summary>
        /// Retrieves the list of radio modules attached to the system.
        /// </summary>
        /// <param name="pRadioEnum">pointer to a buffer into which attached radio information will
        /// be placed.  On input, pBuffer->totalLength must specify the length of
        /// the buffer.  If the buffer is not large enough, on return
        /// pBuffer->totalLength will contain the number of bytes required.</param>
        /// <param name="flags">Reserved for future use.  Set to zero.</param>
        /// <returns></returns>
        public Result RetrieveAttachedRadiosList
        (
            [In, Out] RadioEnumeration pRadioEnum,
            [In]      UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBuf = IntPtr.Zero;
            Int32 szTot = 128;            // Rnd value chosen for initial size

            try
            {
                lpBuf = Marshal.AllocHGlobal(szTot);

                while (true)
                {
                    Marshal.WriteInt32(lpBuf, 0, (int)pRadioEnum.length);
                    Marshal.WriteInt32(lpBuf, 4, szTot);

                    Result = Native.RFID_RetrieveAttachedRadiosList(pHandle, lpBuf, flags);
                    
                    if (Result.BUFFER_TOO_SMALL == Result)
                    {
                        szTot = szTot * 2;
                        lpBuf = Marshal.ReAllocHGlobal(lpBuf, new IntPtr(szTot));
                    }
                    else if (Result.OK != Result)
                    {
                        break;
                    }
                    else
                    {
                        // pRadioEnum.length      = set during obj construction
                        pRadioEnum.totalLength = (UInt32)Marshal.ReadInt32(lpBuf, 4);
                        pRadioEnum.countRadios = (UInt32)Marshal.ReadInt32(lpBuf, 8);

                        pRadioEnum.radioInfo = new RadioInformation[pRadioEnum.countRadios];

                        IntPtr infoPtrArray = Marshal.ReadIntPtr(new IntPtr(lpBuf.ToInt64() + 12));

                        int index = 0;

                        for (index = 0; index < pRadioEnum.countRadios; ++index)
                        {
                            IntPtr infoPtr =
                                Marshal.ReadIntPtr
                                    (
                                        new IntPtr
                                        (
                                            lpBuf.ToInt64() +
                                            (
                                                infoPtrArray.ToInt64() - lpBuf.ToInt64() +
                                                (
                                                    IntPtr.Size * index
                                                )
                                            )
                                        )
                                    );

                            pRadioEnum.radioInfo[index] = new RadioInformation();

                            pRadioEnum.radioInfo[index].length
                                = (UInt32)Marshal.ReadInt32(infoPtr, 0);
                            pRadioEnum.radioInfo[index].driverVersion.major
                                = (UInt32)Marshal.ReadInt32(infoPtr, 4);
                            pRadioEnum.radioInfo[index].driverVersion.minor
                                = (UInt32)Marshal.ReadInt32(infoPtr, 8);
                            pRadioEnum.radioInfo[index].driverVersion.patch
                                = (UInt32)Marshal.ReadInt32(infoPtr, 12);
                            pRadioEnum.radioInfo[index].cookie
                                = (UInt32)Marshal.ReadInt32(infoPtr, 16);
#if USB_BUILD
                            pRadioEnum.radioInfo[index].idLength
                                = (UInt32)Marshal.ReadInt32(infoPtr, 20) - 1;

                            IntPtr idPtr = Marshal.ReadIntPtr(new IntPtr(infoPtr.ToInt64() + 24));

                            // We are removing the last byte ( char ) from the id as the
                            // current usb and serial drivers actually return a null
                            // terminated ( ascii ) character string...

                            pRadioEnum.radioInfo[index].uniqueId =
                                new Byte[pRadioEnum.radioInfo[index].idLength];

                            Marshal.Copy
                            (
                                idPtr,
                                pRadioEnum.radioInfo[index].uniqueId,
                                0,
                                (int)pRadioEnum.radioInfo[index].idLength
                            );
#endif
                        }

                        break;
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBuf)
            {
                Marshal.FreeHGlobal(lpBuf);
            }

            return Result;
        }




        /// <summary>
        /// Requests explicit control of a radio.
        /// </summary>
        /// <param name="cookie">the unique cookie for the radio to open.  This cookie was returned
        /// in the RFID_RADIO_INFO structure that was returned from a call to
        /// RFID_RetrieveAttachedRadiosList.</param>
        /// <param name="mode">radio open flags.  May be zero or a combination of the following:
        /// RFID_FLAG_MAC_EMULATION - instruct MAC to run in radio-emulation mode
        /// RFID_FLAG_SOFT_RESET_MAC - reset the MAC before returing the pHandle</param>
        /// <returns></returns>
        public Result RadioOpen
        (
            [In]          UInt32 cookie,
            [In]          MacMode mode
        )
        {
            return Native.RFID_RadioOpen(pHandle, cookie, mode);
        }



        /// <summary>
        /// Release control of a previously-opened radio.  On close, any currently-
        /// executing or outstanding requests are cancelled and the radio is returned
        /// to idle state.
        /// 
        /// NOTE: This function must not be called from the packet callback function.
        /// 
        /// </summary>
        /// <returns></returns>
        public Result RadioClose
        (
        )
        {
            return Native.RFID_RadioClose(pHandle);
        }



        /// <summary>
        /// Sets the low-level configuration parameter for the radio module.  Radio
        /// configuration parameters may not be set while a radio module is executing
        /// a tag-protocol operation.
        /// </summary>
        /// <param name="param">tparameter to set</param>
        /// <param name="val">the value to which the configuration parameter will be set</param>
        /// <returns></returns>
        public Result MacWriteRegister
        (
            [In] UInt16 param,
            [In] UInt32 val
        )
        {
            return Native.RFID_MacWriteRegister(pHandle, param, val);
        }



        /// <summary>
        /// Retrieves a low-level radio module configuration parameter.  Radio
        /// configuration parameters may not be retrieved while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="param">parameter to retrieve</param>
        /// <param name="val">pointer to variable that will receive configuration parameter
        /// value.  Must not be NULL.</param>
        /// <returns></returns>
        public Result MacReadRegister
        (
            [In]          UInt16 param,
            [In, Out] ref UInt32 val
        )
        {
            return Native.RFID_MacReadRegister(pHandle, param, ref val);
        }




        /// <summary>
        /// Sets the radio's operation mode.  An RFID radio module operation mode
        /// will remain in effect until it is explicitly changed via
        /// RFID_RadioSetOperationMode.  The operation mode may not be set while a
        /// radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="mode">the operation mode for the radio</param>
        /// <returns></returns>
        public Result RadioSetOperationMode
        (
            [In] RadioOperationMode mode
        )
        {
            return Native.RFID_RadioSetOperationMode(pHandle, mode);
        }



        /// <summary>
        /// Retrieves the radio's operation mode.  The operation mode may not be
        /// retrieved while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="mode">current operation mode for the radio.  Must not be NULL.</param>
        /// <returns></returns>
        public Result RadioGetOperationMode
        (
            [In, Out] ref RadioOperationMode mode
        )
        {
            try
            {
                return Native.RFID_RadioGetOperationMode(pHandle, ref mode);
            }
            catch
            {
                // Error auto-unmarshal encountered value that could
                // not be translated to a RadioOperationType

                mode = RadioOperationMode.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }




        /// <summary>
        /// Sets the radio module's power state (not to be confused with the antenna
        /// RF power).  The power state may not be set while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="state">the power state for the radio</param>
        /// <returns></returns>
        public Result RadioSetPowerState
        (
            [In] RadioPowerState state
        )
        {
            return Native.RFID_RadioSetPowerState(pHandle, state);
        }




        /// <summary>
        /// Retrieves the radio module's power state (not to be confused with the
        /// antenna RF power).  The power state may not be retrieved while a radio
        /// module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="state">a pointer to a variable that on return will contain the radio
        /// module's power state.  Must not be NULL.</param>
        /// <returns></returns>
        public Result RadioGetPowerState
        (
            [In, Out] ref RadioPowerState state
        )
        {
            try
            {
                return Native.RFID_RadioGetPowerState(pHandle, ref state);
            }
            catch
            {
                // Error auto-unmarshal encountered value that could
                // not be translated to a RadioPowerState

                state = RadioPowerState.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }




        /// <summary>
        /// Sets the current link profile for the radio module.  The curren link
        /// profile may not be set while a radio module is executing a tag-protocol
        /// operation.
        /// </summary>
        /// <param name="profile">the profile to make the current one</param>
        /// <returns></returns>
        public Result RadioSetCurrentLinkProfile
        (
            [In] UInt32 profile
        )
        {
            return Native.RFID_RadioSetCurrentLinkProfile(pHandle, profile);
        }



        /// <summary>
        /// Retrieves the current link profile for the radio module.  The current link
        /// profile may not be retrieved while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="profile">a pointer to an unsigned 32-bit integer that will receive
        /// the current profile.  Must not be NULL.</param>
        /// <returns></returns>
        public Result RadioGetCurrentLinkProfile
        (
            [In, Out] ref UInt32 profile
        )
        {
            return Native.RFID_RadioGetCurrentLinkProfile(pHandle, ref profile);
        }



        /// <summary>
        /// Retrieves the information for the specified link profile for the radio
        /// module.  A link profile may not be retrieved while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="profile">the link profile for which information will be retrieved</param>
        /// <param name="profileInfo">a pointer to a structure that will be filled in with link
        /// profile information.  Must not be NULL.</param>
        /// <returns></returns>
        public Result RadioGetLinkProfile
        (
            [In]      UInt32 profile,
            [In, Out] RadioLinkProfile profileInfo
        )
        {
            Result Result = Result.OK;
            IntPtr lpBuf = IntPtr.Zero;

            try
            {
                lpBuf = Marshal.AllocHGlobal((int)profileInfo.length);  // sizeof(RFID_RADIO_LINK_PROFILE)

                Marshal.WriteInt32(lpBuf, 0, (int)profileInfo.length);

                Result = Native.RFID_RadioGetLinkProfile(pHandle, profile, lpBuf);

                if (Result.OK == Result)
                {
                    // profile.length  =
                    //     supplied during profile object construction
                    profileInfo.enabled =
                        (UInt32)Marshal.ReadInt32(lpBuf, 4);
                    profileInfo.profileId =
                        (UInt64)Marshal.ReadInt64(lpBuf, 8);
                    profileInfo.profileVersion =
                        (UInt32)Marshal.ReadInt32(lpBuf, 16);

                    // Catch if value read in has no match in protocol type

                    try
                    {
                        profileInfo.profileProtocol =
                            (RadioProtocol)Marshal.ReadInt32(lpBuf, 20);
                    }
                    catch
                    {
                        profileInfo.profileProtocol = RadioProtocol.UNKNOWN;

                        Result = Result.DRIVER_MISMATCH;
                    }

                    // Grab all data we can even though we are returning
                    // a mis-match error...

                    profileInfo.denseReaderMode =
                        (UInt32)Marshal.ReadInt32(lpBuf, 24);
                    profileInfo.widebandRssiSamples =
                        (UInt32)Marshal.ReadInt32(lpBuf, 28);
                    profileInfo.narrowbandRssiSamples =
                        (UInt32)Marshal.ReadInt32(lpBuf, 32);

                    profileInfo.realtimeRssiEnabled =
                        (UInt32)Marshal.ReadInt32(lpBuf, 36);
                    profileInfo.realtimeWidebandRssiSamples =
                        (UInt32)Marshal.ReadInt32(lpBuf, 40);
                    profileInfo.realtimeNarrowbandRssiSamples =
                        (UInt32)Marshal.ReadInt32(lpBuf, 44);

                    if (RadioProtocol.ISO18K6C == profileInfo.profileProtocol)
                    {
                        RadioLinkProfileConfig profileConfig =
                            new RadioLinkProfileConfig();

                        try
                        {
                            Marshal.PtrToStructure
                            (
                                new IntPtr(lpBuf.ToInt64() + 48),
                                profileConfig
                            );
                        }
                        catch
                        {
                            // This can potentiall occur if any of the well defined types encounter
                            // a non-convertable int value when perform ptr to struct marshal. Just
                            // set any field(s) that can cause the error to UNKNOWN status.

                            profileConfig.modulationType = ModulationType.UNKNOWN;
                            profileConfig.data01Difference = DataDifference.UNKNOWN;
                            profileConfig.divideRatio = DivideRatio.UNKNOWN;
                            profileConfig.millerNumber = MillerNumber.UNKNOWN;

                            Result = Result.DRIVER_MISMATCH;
                        }

                        profileInfo.profileConfig = profileConfig;
                    }
                    else
                    {
                        // This will set the value of profile.profileProtocol field
                        // to be set to UNKNOWN - i.e. library / linkage mis-match

                        profileInfo.profileConfig = new ProfileConfig();
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBuf)
            {
                Marshal.FreeHGlobal(lpBuf);
            }

            return Result;
        }



        /// <summary>
        /// Writes a valut to a link-profile register for the specified link
        /// profile.  A link-profile regsiter may not be written while a radio module
        /// is executing a tag-protocol operation.
        /// </summary>
        /// <param name="profile">the link profile for link-profile register should be written</param>
        /// <param name="address">address of the link-profile register</param>
        /// <param name="value">the value to be written to the register</param>
        /// <returns></returns>
        public Result RadioWriteLinkProfileRegister
        (
            [In] UInt32 profile,
            [In] UInt16 address,
            [In] UInt16 value
        )
        {
            return Native.RFID_RadioWriteLinkProfileRegister(pHandle, profile, address, value);
        }



        /// <summary>
        /// Retrieves the contents of a link-profile register for the specified link
        /// profile.  A link-profile regsiter may not be read while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="profile">the link profile for link-profile register should be read</param>
        /// <param name="address">address of the link-profile register</param>
        /// <param name="value">a pointer to a 16-bit unsigned integer that upon return will
        /// contain the register's value</param>
        /// <returns></returns>
        public Result RadioReadLinkProfileRegister
        (
            [In]          UInt32 profile,
            [In]          UInt16 address,
            [In, Out] ref UInt16 value
        )
        {
            return Native.RFID_RadioReadLinkProfileRegister(pHandle, profile, address, ref value);
        }

        /// <summary>
        /// Turn on carrier wave
        /// </summary>
        /// <returns></returns>
        public Result RadioTurnCarrierWaveOn
        (
        )
        {
            return Native.RFID_RadioTurnCarrierWaveOn(pHandle);
        }
        /// <summary>
        /// Turn off carrier wave
        /// </summary>
        /// <returns></returns>
        public Result RadioTurnCarrierWaveOff
        (
        )
        {
            return Native.RFID_RadioTurnCarrierWaveOff(pHandle);
        }

        /// <summary>
        /// Retrieves the status of a radio module's antenna port.  The antenna port
        /// status may not be retrieved while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="port">the antenna port for which status is to be retrieved. 
        /// Antenna ports are numbered beginning with 0.</param>
        /// <param name="status">pointer to the structure which upon return will contain the
        /// antenna port's status.  Must not be NULL.</param>
        /// <returns></returns>
        public Result AntennaPortGetStatus
        (
            [In]      UInt32 port,
            [In, Out] AntennaPortStatus status
        )
        {
            try
            {
                return Native.RFID_AntennaPortGetStatus(pHandle, port, status);
            }
            catch
            {
                // Here if the port state value was unknown to us and
                // couldn't be converted automatically - set to unknown

                status.state = AntennaPortState.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }
//#if CS468
        /// <summary>
        /// Sets the status of a radio module's antenna port.  The antenna port
        /// status may not be set while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="port">the antenna port for which status is to be set. 
        /// Antenna ports are numbered beginning with 0.</param>
        /// <param name="status">pointer to the structure which upon return will contain the
        /// antenna port's status.  Must not be NULL.</param>
        /// <returns></returns>
        public Result AntennaPortSetStatus
        (
            [In]      UInt32 port,
            [In, Out] AntennaPortStatus status
        )
        {
            try
            {
                return Native.RFID_AntennaPortSetStatus(pHandle, port, status);
            }
            catch
            {
                // Here if the port state value was unknown to us and
                // couldn't be converted automatically - set to unknown

                status.state = AntennaPortState.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }
//#endif

        /// <summary>
        /// Sets the state of a radio module's antenna port.  The antenna port state
        /// may not be set while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="port">the antenna port for which state will be set.  Antenna ports
        /// are numbered beginning with 0.</param>
        /// <param name="state">the state for the antenna port</param>
        /// <returns></returns>
        public Result AntennaPortSetState
        (
            [In] UInt32 port,
            [In] AntennaPortState state
        )
        {
            return Native.RFID_AntennaPortSetState(pHandle, port, state);
        }





        /// <summary>
        /// Sets the configuration for a radio module's antenna port.  The antenna
        /// port configuration may not be set while a radio module is executing a
        /// tag-protocol operation.
        /// </summary>
        /// <param name="port">the antenna port for which configuration will be set.
        /// Antenna ports are numbered beginning with 0.</param>
        /// <param name="config">pointer to structure containing antenna port configuration.  Must
        /// not be NULL.  In version 1.0, the physicalRxPort and physicalTxPort
        /// fields must be the same.</param>
        /// <returns></returns>
        public Result AntennaPortSetConfiguration
        (
            [In] UInt32 port,
            [In] AntennaPortConfig config
        )
        {
            return Native.RFID_AntennaPortSetConfiguration(pHandle, port, config);
        }



        /// <summary>
        /// Retrieves the configuration for a radio module's antenna port.  The antenna
        /// port configuration may not be retrieved while a radio module is executing a
        /// tag-protocol operation.
        /// </summary>
        /// <param name="port">the antenna port for which configuration will be
        /// retrieved.  Antenna ports are numbered beginning with 0.</param>
        /// <param name="config">pointer to structure that on return will contain the antenna
        /// port configuration.  Must not be NULL.</param>
        /// <returns></returns>
        public Result AntennaPortGetConfiguration
        (
            [In]      UInt32 port,
            [In, Out] AntennaPortConfig config
        )
        {
            return Native.RFID_AntennaPortGetConfiguration(pHandle, port, config);
        }




        /// <summary>
        /// Configures the tag-selection criteria for the ISO 18000-6C select command.
        /// The supplied tag-selection criteria will be used for any tag-protocol
        /// operations (i.e., RFID_18K6CTagInventory, etc.) in which the application
        /// specifies that an ISO 18000-6C select command should be issued prior to
        /// executing the tag-protocol operation.  The tag-selection criteria will
        /// stay in effect until the next call to RFID_18K6CSetSelectCriteria.  The
        /// select criteria may not be set while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="pCriteria">pointer to a structure that specifies the ISO 18000-6C
        /// tag-selection criteria.  This parameter must not be NULL.</param>
        /// <param name="flags">Reserved for future use.  Set to zero.</param>
        /// <returns></returns>
        public Result Set18K6CSelectCriteria
        (
            [In] SelectCriteria pCriteria,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            // Check immediately if the specified count > than actual count
            // of criteria since that will cause null ptr exception when we
            // perform our array marshaling loop...

            if (pCriteria.countCriteria > pCriteria.pCriteria.Length)
            {
                return Result.INVALID_PARAMETER;
            }

            try
            {
                // The select mask and action do not have embedded length fields so
                // we must hand calculate for use when copying data to native block

                Int32 selectMaskSize =
                    Marshal.SizeOf(typeof(SelectMask));

                Int32 selectCriterionSize =
                    selectMaskSize +
                    Marshal.SizeOf(typeof(SelectAction));

                // This is to hold the count value, the ptr to select criterion array
                // and the select criterion array members

                lpBufPtr = Marshal.AllocHGlobal
                (
                    4 +
                    IntPtr.Size +
                    (Int32)(selectCriterionSize * pCriteria.countCriteria)
                );

                Marshal.WriteInt32(lpBufPtr, 0, (Int32)pCriteria.countCriteria);

                // pCriteria.pCriteria - ptr to array of criteria

                Marshal.WriteIntPtr
                   (
                       new IntPtr(lpBufPtr.ToInt64() + 4),
                       new IntPtr(lpBufPtr.ToInt64() + 4 + IntPtr.Size)
                   );

                // Copy the individual select criteria array elements over
                // to native memory block...

                for (int index = 0; index < pCriteria.countCriteria; ++index)
                {
                    IntPtr lpCurrentCriteriaPtr =
                        new IntPtr
                            (
                                lpBufPtr.ToInt64() + 4 + IntPtr.Size + (selectCriterionSize * index)
                            );

                    Marshal.StructureToPtr
                        (
                            pCriteria.pCriteria[index].mask,
                            lpCurrentCriteriaPtr,
                            false
                        );

                    Marshal.StructureToPtr
                        (
                            pCriteria.pCriteria[index].action,
                            new IntPtr(lpCurrentCriteriaPtr.ToInt64() + selectMaskSize),
                            false
                        );
                }
                Result = Native.RFID_18K6CSetSelectCriteria(pHandle, lpBufPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }




        /// <summary>
        /// Retrieves the configured tag-selection criteria for the ISO 18000-6C select
        /// command.  The returned tag-selection criteria are used for any tag-protocol
        /// operations (i.e., RFID_18K6CTagInventory, etc.) in which the application
        /// specifies that an ISO 18000-6C select command should be issued prior to 
        /// executing the tag-protocol operation.  The select criteria may not be
        /// retrieved while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pCriteria">pointer to a structure that specifies the ISO 18000-6C
        /// tag-selection criteria.  On entry to the function, the countCriteria 
        /// field must contain the number of entries in the array pointed to by the
        /// pCriteria field.  On return from the function, the countCriteria field
        /// will contain the number of tag-selection criteria returned in the array
        /// pointed to by pCriteria.  If the array pointed to by pCriteria is not
        /// large enough to hold the configured tag-selection criteria, on return
        /// countCriteria will contain the number of entries required and the
        /// function will return RFID_ERROR_BUFFER_TOO_SMALL.  This parameter must
        /// not be NULL.  The pCriteria field may be NULL only if the countCriteria
        /// field is zero.</param>
        /// <returns></returns>
        public Result Get18K6CSelectCriteria
        (
            [In, Out] SelectCriteria pCriteria
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            // The select mask and action do not have embedded length fields so
            // we must hand calculate for use when retrieving native block data

            Int32 selectMaskSize =
                Marshal.SizeOf(typeof(SelectMask));

            Int32 selectCriterionSize =
                selectMaskSize +
                Marshal.SizeOf(typeof(SelectAction));

            // Start by specifying single rule...

            Int32 countCriteria = 1;

            try
            {
                // This is to hold the count value, the ptr to select criterion array
                // and the select criterion array members

                lpBufPtr = Marshal.AllocHGlobal
                (
                    4 +
                    IntPtr.Size +
                    (Int32)(selectCriterionSize * countCriteria)
                );

                Marshal.WriteInt32(lpBufPtr, 0, countCriteria);

                //!! pCriteria.pCriteria - ptr to array of criteria

                Marshal.WriteIntPtr
                (
                    new IntPtr(lpBufPtr.ToInt64() + 4),
                    new IntPtr(lpBufPtr.ToInt64() + 4 + IntPtr.Size)
                );
                Result = Native.RFID_18K6CGetSelectCriteria(pHandle, lpBufPtr);

                if (Result.BUFFER_TOO_SMALL == Result)
                {
                    // On buffer too small, the returned count in native block
                    // will hold the required count: length of criterion array

                    countCriteria = Marshal.ReadInt32(lpBufPtr, 0);

                    lpBufPtr = Marshal.ReAllocHGlobal
                    (
                        lpBufPtr,
                        new IntPtr
                        (
                            4 + IntPtr.Size + (Int32)(selectCriterionSize * countCriteria)
                        )
                    );

                    // Try native call again - we should NEVER see buff too small error
                    // since set to size lib told us but we can see other errors...

                    Result = Native.RFID_18K6CGetSelectCriteria(pHandle, lpBufPtr);
                }

                if (Result.OK == Result)
                {
                    pCriteria.countCriteria = (UInt32)Marshal.ReadInt32(lpBufPtr, 0);

                    pCriteria.pCriteria = new SelectCriterion[countCriteria];

                    for (int index = 0; index < countCriteria; ++index)
                    {
                        // We need to calculate offset over the criteria count, ptr to the
                        // criteria array and into the proper array element...

                        IntPtr lpCurrentCriteriaPtr =
                            new IntPtr
                                (
                                    lpBufPtr.ToInt64() + 4 + IntPtr.Size + (selectCriterionSize * index)
                                );

                        pCriteria.pCriteria[index] = new SelectCriterion();

                        try
                        {
                            Marshal.PtrToStructure
                            (
                                lpCurrentCriteriaPtr,
                                pCriteria.pCriteria[index].mask
                            );
                        }
                        catch
                        {
                            // Non-convertable value or other error...

                            pCriteria.pCriteria[index].mask.bank = MemoryBank.UNKNOWN;

                            Result = Result.DRIVER_MISMATCH;
                        }

                        try
                        {
                            Marshal.PtrToStructure
                            (
                                new IntPtr(lpCurrentCriteriaPtr.ToInt64() + selectMaskSize),
                                pCriteria.pCriteria[index].action
                            );
                        }
                        catch
                        {
                            // Non-convertable value or other error...

                            pCriteria.pCriteria[index].action.target = Target.UNKNOWN;
                            pCriteria.pCriteria[index].action.action = CSLibrary.Constants.Action.UNKNOWN;

                            Result = Result.DRIVER_MISMATCH;
                        }
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }




        /// <summary>
        /// Configures the post-singulation match criteria to be used by the RFID
        /// radio module.  The supplied post-singulation match criteria will be used
        /// for any tag-protocol operations (i.e., RFID_18K6CTagInventory, etc.) in
        /// which the application specifies that a post-singulation match should be
        /// performed on the tags that are singulated by the tag-protocol operation.
        /// The post-singulation match criteria will stay in effect until the next call
        /// to RFID_18K6CSetPostMatchCriteria.  The post-singulation match criteria
        /// may not be set while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pCriteria">a pointer to a structure that specifies the post-singulation
        /// match mask and disposition that are to be applied to the tag Electronic
        /// Product Code after it is singulated to determine if it is to have the
        /// tag-protocol operation applied to it.  This parameter must not be NULL.</param>
        /// <param name="flags">Reserved for future use.  Set to zero.</param>
        /// <returns></returns>
        public Result Set18K6CPostMatchCriteria
        (
            [In] SingulationCriteria pCriteria,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            // Check immediately if the specified count > than actual count
            // of criteria since that will cause null ptr exception when we
            // perform our array marshaling loop...

            if (pCriteria.countCriteria > pCriteria.pCriteria.Length)
            {
                return Result.INVALID_PARAMETER;
            }

            try
            {
                // The singlation mask does not have an embedded length field so
                // hand calculte for use when retrieving native block data

                Int32 singulationCriterionSize =
                    4 +
                    Marshal.SizeOf(typeof(SingulationMask));

                lpBufPtr = Marshal.AllocHGlobal
                (
                    4 +
                    IntPtr.Size +
                    (Int32)(singulationCriterionSize * pCriteria.countCriteria)
                );

                // pCriteria.countCriteria

                Marshal.WriteInt32(lpBufPtr, 0, (Int32)pCriteria.countCriteria);

                // pCriteria.pCriteria - ptr to array of criteria

                Marshal.WriteIntPtr
                   (
                       new IntPtr(lpBufPtr.ToInt64() + 4),
                       new IntPtr(lpBufPtr.ToInt64() + 4 + IntPtr.Size)
                   );

                for (int index = 0; index < pCriteria.countCriteria; ++index)
                {
                    // Calculate pointer so we jump over the count, pointer to array
                    // and then into the desired position in the embedded array...

                    IntPtr lpCurrentCriteriaPtr =
                        new IntPtr
                            (
                                lpBufPtr.ToInt64() + 4 + IntPtr.Size + (singulationCriterionSize * index)
                            );

                    Marshal.WriteInt32
                        (
                            lpCurrentCriteriaPtr, (Int32)pCriteria.pCriteria[index].match
                        );

                    Marshal.StructureToPtr
                        (
                            pCriteria.pCriteria[index].mask,
                            new IntPtr(lpCurrentCriteriaPtr.ToInt64() + 4),
                            false
                        );
                }
                Result = Native.RFID_18K6CSetPostMatchCriteria(pHandle, lpBufPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }


        /// <summary>
        /// Retrieves the configured post-singulation match criteria to be used by the
        /// RFID radio module.  The post-singulation match criteria is used for any
        /// tag-protocol operations (i.e., RFID_18K6CTagInventory, etc.) in which the
        /// application specifies that a post-singulation match should be performed on
        /// the tags that are singulated by the tag-protocol operation.  Post-
        /// singulation match criteria may not be retrieved while a radio module is
        /// executing a tag-protocol operation.  The post-singulation match criteria
        /// may not be retrieved while a radio module is executing a tag-protocol
        /// operation.
        /// </summary>
        /// <param name="pCriteria">a pointer to a structure that upon return will contain the
        /// post-singulation match criteria that are to be applied to the tag
        /// Electronic Product Code after it is singulated to determine if it is to
        /// have the tag-protocol operation applied to it.  On entry to the function,
        /// the countCriteria field must contain the number of entries in the array
        /// pointed to by the pCriteria field.  On return from the function, the
        /// countCriteria field will contain the number of post-singulation match
        /// criteria returned in the array pointed to by pCriteria.  If the array
        /// pointed to by pCriteria is not large enough to hold the configured tag-
        /// selection criteria, on return countCriteria will contain the number of
        /// entries required and the function will return
        /// RFID_ERROR_BUFFER_TOO_SMALL.  This parameter must not be NULL.  The
        /// pCriteria field may be NULL only if the countCriteria field is zero.</param>
        /// <returns></returns>
        public Result Get18K6CPostMatchCriteria
        (
            [In, Out] SingulationCriteria pCriteria
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            // This is to hold the count value, the ptr to post singulation
            // criterion count and associated array members

            Int32 singulationCriterionSize = 74;  // marshal sizeof pads bytes in the count !

            // Max out - we currently support 1 but may eventually support 8 so...

            Int32 countCriteria = 1;

            try
            {
                // Alloc our native buffer for diving down into rfid.dll

                lpBufPtr = Marshal.AllocHGlobal
                (
                    4 +
                    IntPtr.Size +
                    (Int32)(singulationCriterionSize * countCriteria)
                );

                Marshal.WriteInt32(lpBufPtr, 0, countCriteria);

                // pCriteria.pCriteria - ptr to the loc where the native
                // version of the criterion array data will be placed

                Marshal.WriteIntPtr
                (
                    new IntPtr(lpBufPtr.ToInt64() + 4),
                    new IntPtr(lpBufPtr.ToInt64() + 4 + IntPtr.Size)
                );
                Result = Native.RFID_18K6CGetPostMatchCriteria(pHandle, lpBufPtr);


                if (Result.BUFFER_TOO_SMALL == Result)
                {
                    // On buffer too small, the returned count in native block
                    // will hold the required count: length of criterion array

                    countCriteria = Marshal.ReadInt32(lpBufPtr, 0);

                    // Resize our buffer to the required amount...

                    lpBufPtr = Marshal.ReAllocHGlobal
                    (
                        lpBufPtr,
                        new IntPtr
                        (
                            4 + IntPtr.Size + (Int32)(singulationCriterionSize * countCriteria)
                        )
                    );

                    // Make another native attempt at native call - we should never
                    // fail ( due to insufficient memory ) twice in a row...

                    Result = Native.RFID_18K6CGetPostMatchCriteria(pHandle, lpBufPtr);

                }

                if (Result.OK == Result)
                {
                    pCriteria.countCriteria = (UInt32)Marshal.ReadInt32(lpBufPtr, 0);

                    pCriteria.pCriteria = new SingulationCriterion[countCriteria];

                    for (int index = 0; index < countCriteria; ++index)
                    {
                        // We need to calculate offset over the criteria count, ptr to the
                        // criteria array and into the proper array element...

                        IntPtr lpCurrentCriteriaPtr =
                            new IntPtr
                                (
                                    lpBufPtr.ToInt64() + 4 + IntPtr.Size + (singulationCriterionSize * index)
                                );

                        pCriteria.pCriteria[index] = new SingulationCriterion();

                        // UnMarshal the post singulation object at the current array index...

                        pCriteria.pCriteria[index].match = (UInt32)Marshal.ReadInt32
                            (
                                lpCurrentCriteriaPtr,
                                0
                            );

                        Marshal.PtrToStructure
                            (
                                new IntPtr(lpCurrentCriteriaPtr.ToInt64() + 4),
                                pCriteria.pCriteria[index].mask
                            );
                    }
                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }




        /// <summary>
        /// Specifies which tag group will have subsequent tag-protocol operations
        /// (e.g., inventory, tag read, etc.) applied to it.  The tag group may not be
        /// changed while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="group">a pointer to a structure that specifies the tag group that will
        /// have subsequent tag-protocol operations applied to it.  This parameter
        /// must not be NULL.</param>
        /// <returns></returns>
        public Result Set18K6CQueryTagGroup
        (
            [In] TagGroup group
        )
        {
            return Native.RFID_18K6CSetQueryTagGroup(pHandle, group);
        }



        /// <summary>
        /// Retrieves the tag group that will have subsequent tag-protocol operations
        /// (e.g., inventory, tag read, etc.) applied to it.  The tag group may not be
        /// retrieved while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="pGroup">a pointer to a structure that upon return contains the configured
        /// tag group.  This parameter must not be NULL.</param>
        /// <returns></returns>
        public Result Get18K6CQueryTagGroup
        (
            [In, Out] TagGroup pGroup
        )
        {
            try
            {
                return Native.RFID_18K6CGetQueryTagGroup(pHandle, pGroup);
            }
            catch
            {
                // Auto unmarshal error - set all typed fields to unknown

                pGroup.selected = Selected.UNKNOWN;
                pGroup.session = Session.UNKNOWN;
                pGroup.target = SessionTarget.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }



        /// <summary>
        /// Allows the application to set the currently-active singulation algorithm
        /// (i.e., the one that is used when performing a tag-protocol operation
        /// (e.g., inventory, tag read, etc.)).  The currently-active singulation
        /// algorithm may not be changed while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="algorithm">the singulation algorithm to make currently active.</param>
        /// <returns></returns>
        public Result Set18K6CCurrentSingulationAlgorithm
        (
            [In] SingulationAlgorithm algorithm
        )
        {
            return Native.RFID_18K6CSetCurrentSingulationAlgorithm(pHandle, algorithm);
        }




        /// <summary>
        /// Allows the application to retrieve the currently-active singulation
        /// algorithm.  The currently-active singulation algorithm may not be changed
        /// while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="algorithm">a pointer to a singulation-algorithm variable that upon
        /// return will contain the currently-active singulation algorithm.  This
        /// parameter must not be NULL.</param>
        /// <returns></returns>
        public Result Get18K6CCurrentSingulationAlgorithm
        (
            [In, Out] ref SingulationAlgorithm algorithm
        )
        {
            try
            {
                return Native.RFID_18K6CGetCurrentSingulationAlgorithm(pHandle, ref algorithm);
            }
            catch
            {
                algorithm = SingulationAlgorithm.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }




        /// <summary>
        /// Allows the application to configure the settings for a particular
        /// singulation algorithm.  A singulation algorithm may not be configured while
        /// a radio module is executing a tag-protocol operation.
        /// 
        /// NOTE:  Configuring a singulation algorithm does not automatically set it as
        /// the current singulation algorithm
        /// (see RFID_18K6CSetCurrentSingulationAlgorithm). 
        /// </summary>
        /// <param name="algorithm">the singulation algorithm to be configured.  This parameter
        /// determines the type of structure to which pParms points.  For example,
        /// if this parameter is RFID_18K6C_SINGULATION_ALGORITHM_FIXEDQ, pParms must
        /// point to a RFID_18K6C_SINGULATION_FIXEDQ_PARMS structure.  If this
        /// parameter does not represent a valid singulation algorithm,
        /// RFID_ERROR_INVALID_PARAMETER is returned.</param>
        /// <param name="parms">a pointer to a structure that contains the singulation-algorithm
        /// parameters.  The type of structure this points to is determined by
        /// algorithm.  The structure length field must be filled in appropriately.
        /// This parameter must not be NULL.</param>
        /// <returns></returns>
        public Result Set18K6CSingulationAlgorithmParameters
        (
            [In] SingulationAlgorithm algorithm,
            [In] SingulationAlgorithmParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr pParms = IntPtr.Zero;

            try
            {
                pParms = Marshal.AllocHGlobal(Marshal.SizeOf(parms));

                Marshal.StructureToPtr(parms, pParms, false);

                Result = Native.RFID_18K6CSetSingulationAlgorithmParameters(pHandle, algorithm, pParms);
            }
            catch
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != pParms)
            {
                Marshal.FreeHGlobal(pParms);
            }

            return Result;
        }



        /// <summary>
        /// Allows the application to retrieve the settings for a particular
        /// singulation algorithm.  Singulation-algorithm parameters may not be
        /// retrieved while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="algorithm">The singulation algorithm for which parameters are to be
        /// retrieved.  This parameter determines the type of structure to which
        /// pParms points.  For example, if this parameter is
        /// RFID_18K6C_SINGULATION_ALGORITHM_FIXEDQ, pParms must point to a
        /// RFID_18K6C_SINGULATION_FIXEDQ_PARMS structure.  If this parameter does
        /// not represent a valid singulation algorithm,
        /// RFID_ERROR_INVALID_PARAMETER is returned.</param>
        /// <param name="parms">a pointer to a structure that upon return will contain the
        /// singulation-algorithm parameters.  The type of structure this points to
        /// is determined by algorithm.  The structure length field must be filled
        /// in appropriately.  This parameter must not be NULL.</param>
        /// <returns></returns>
        public Result Get18K6CSingulationAlgorithmParameters
        (
            [In]      SingulationAlgorithm algorithm,
            [In, Out] SingulationAlgorithmParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr pParms = IntPtr.Zero;

            if (null == parms)
            {
                return Result.INVALID_PARAMETER;
            }

            // Instead of the following, could simply make the native call
            // with given fields and catch the Resulting marshal exception
            // when trying to do ptr to structure

            Type algoType = parms.GetType();

            if
            (
                (SingulationAlgorithm.UNKNOWN == algorithm)
             || (SingulationAlgorithm.FIXEDQ == algorithm && parms.GetType() != typeof(FixedQParms))
             || (SingulationAlgorithm.DYNAMICQ == algorithm && parms.GetType() != typeof(DynamicQParms))
            )
            {
                return Result.INVALID_PARAMETER;
            }

            try
            {
                pParms = Marshal.AllocHGlobal(Marshal.SizeOf(parms));

                Marshal.StructureToPtr(parms, pParms, false);

                Result = Native.RFID_18K6CGetSingulationAlgorithmParameters(pHandle, algorithm, pParms);

                if (Result.OK == Result)
                {
                    Marshal.PtrToStructure(pParms, parms);
                }
            }
            catch(OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != pParms)
            {
                Marshal.FreeHGlobal(pParms);
            }

            return Result;
        }


        /// <summary>
        /// Configures the parameters for the ISO 18000-6C query command.  The supplied
        /// query parameters will be used for any subsequent tag-protocol operations
        /// (i.e., RFID_18K6CTagInventory, etc.) and will stay in effect until the next
        /// call to RFID_18K6CSetQueryParameters.  The query parameters may not be set
        /// while a radio module is executing a tag-protocol operation.
        /// 
        /// NOTE: Failure to call RFID_18K6CSetQueryParameters prior to executing the
        /// first tag-protocol operation (i.e., RFID_18K6CTagInventory, etc.) will
        /// Result in the RFID radio module using default values for the ISO 18000-6C
        /// query parameters.
        /// 
        /// NOTE:  As of version 1.1 of the RFID Reader Library, this function has been
        /// deprecated and replaced by the combination of RFID_18K6CSetQueryTagGroup,
        /// RFID_18K6CSetCurrentSingulationAlgorithm, and
        /// RFID_18K6CSetSingulationAlgorithmParameters.  This function remains for
        /// backwards compatibility, however new code should not use it as it will be
        /// removed in a future version.
        /// </summary>
        /// <param name="parms">a pointer to a structure that defines the inventory round ISO
        /// 18000-6C query parameters.  This parameter must not be NULL.</param>
        /// <param name="flags">Reserved for future use.  Set to zero.</param>
        /// <returns></returns>
        public Result SetQueryParameters
        (
            [In] QueryParms parms,
            [In] Int32 flags
        )
        {
            // This function implementation uses the new set tag group, set
            // current algorithm and set algorithm parameters function as
            // the native function this is based on has been deprecated

            Result Result = Result.OK;

            Result = Set18K6CQueryTagGroup(parms.tagGroup);

            if (Result.OK != Result)
            {
                return Result;
            }

            SingulationAlgorithm algorithm = SingulationAlgorithm.UNKNOWN;

            Type algoType = parms.singulationParms.GetType();

            if (algoType == typeof(FixedQParms))
                algorithm = SingulationAlgorithm.FIXEDQ;
            else if (algoType == typeof(DynamicQParms))
                algorithm = SingulationAlgorithm.DYNAMICQ;
            else
                return Result.INVALID_PARAMETER;

            Result = Set18K6CCurrentSingulationAlgorithm(algorithm);

            if (Result.OK != Result)
            {
                return Result;
            }

            Result = Set18K6CSingulationAlgorithmParameters
                (
                    algorithm,
                    parms.singulationParms
                );

            return Result;
        }


        /// <summary>
        /// Retrieves the parameters for the ISO 18000-6C query command.  These are
        /// the query parameters that used for tag-protocol operations (i.e.,
        /// RFID_18K6CTagInventory, etc.).  Query parameters may not be retrieved
        /// while a radio module is executing a tag-protocol operation.  The query
        /// parameters may not be retrieved while a radio module is executing a tag-
        /// protocol operation.
        /// 
        /// NOTE:  As of version 1.1 of the RFID Reader Library, this function has been
        /// deprecated and replaced by the combination of RFID_18K6CGetQueryTagGroup,
        /// RFID_18K6CGetCurrentSingulationAlgorithm, and
        /// RFID_18K6CGetSingulationAlgorithmParameters.  This function remains for
        /// backwards compatibility, however new code should not use it as it will be
        /// removed in a future version.
        /// </summary>
        /// <param name="parms">a pointer to a structure that on return contains the ISO
        /// 18000-6C query parameters for each inventory round.  This parameter must
        /// not be NULL.</param>
        /// <returns></returns>
        public Result GetQueryParameters
        (
            [In, Out] QueryParms parms
        )
        {
            Result Result = Result.OK;

            // This function implementation uses the new get tag group, get
            // current algorithm and get algorithm parameters function as
            // the native function this is based on has been deprecated

            // Check that the internal tag group obj exists and create if
            // necessary - alternative is to return invalid parameter...

            if (null == parms.tagGroup)
            {
                parms.tagGroup = new TagGroup();
            }

            Result = Get18K6CQueryTagGroup(parms.tagGroup);

            if (Result.OK != Result)
            {
                return Result;
            }

            SingulationAlgorithm algorithm = SingulationAlgorithm.UNKNOWN;

            Result = Get18K6CCurrentSingulationAlgorithm(ref algorithm);

            if (Result.OK != Result)
            {
                return Result;
            }

            switch (algorithm)
            {
                case SingulationAlgorithm.FIXEDQ:
                    {
                        parms.singulationParms = new FixedQParms();
                    }
                    break;
                case SingulationAlgorithm.DYNAMICQ:
                    {
                        parms.singulationParms = new DynamicQParms();
                    }
                    break;
                default:
                    {
                        return Result.DRIVER_MISMATCH; // Firmware and library mismatch ?
                    }
            }

            Result = Get18K6CSingulationAlgorithmParameters
                (
                    algorithm,
                    parms.singulationParms
                );

            return Result;
        }

        /// <summary>
        /// Executes a tag inventory for the tags of interest.  If the
        /// RFID_FLAG_PERFORM_SELECT flag is specified, the tag population is
        /// partitioned (i.e., ISO 18000-6C select) prior to the inventory operation.
        /// If the RFID_FLAG_PERFORM_POST_MATCH flag is specified, the post-singulation
        /// match mask is applied to a singulated tag's EPC to determine if the tag
        /// will be returned to the application.  The operation-response packets
        /// will be returned to the application via the application-supplied callback
        /// function.  An application may prematurely stop an inventory operation by
        /// calling RFID_Radio{Cancel|Abort}Operation on another thread or by returning
        /// a non-zero value from the callback function.  A tag inventory may not be
        /// issued while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="parms">pointer to a structure that specifies the ISO 18000-6C inventory
        /// operation parameters.  This parameter must not be NULL.</param>
        /// <param name="flags">inventory flags.  May be zero or a combination of the following:
        /// RFID_FLAG_PERFORM_SELECT - perform one or more selects before performing
        ///   the inventory.
        /// RFID_FLAG_PERFORM_POST_MATCH - perform post-singulation mask match on
        ///   singulated tags.</param>
        /// <returns></returns>
        public Result Tag18K6CInventory
        (
            [In] InventoryParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.length);

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagInventory(pHandle, lpBufPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            return Result;
        }



        /// <summary>
        /// Executes a tag read for the tags of interest.  If the
        /// RFID_FLAG_PERFORM_SELECT flag is specified, the tag population is
        /// partitioned (i.e., ISO 18000-6C select) prior to the tag-read operation.
        /// If the RFID_FLAG_PERFORM_POST_MATCH flag is specified, the post-singulation
        /// match mask is applied to a singulated tag's EPC to determine if the tag
        /// will be read from.  Reads may only be performed on 16-bit word boundaries
        /// and for multiples of 16-bit words.  If one or more of the memory words
        /// specified by the offset/count combination do not exist or are read-locked,
        /// the read from the tag will fail and this failure will be reported through
        /// the operation response packet.    The operation-response packets will
        /// be returned to the application via the application-supplied callback
        /// function.  Each tag-read record is grouped with its corresponding tag-
        /// inventory record.  An application may prematurely stop a read operation by
        /// calling RFID_Radio{Cancel|Abort}Operation on another thread or by returning
        /// a non-zero value from the callback function.  A tag read may not be
        /// issued while a radio module is executing a tag-protocol operation.
        /// 
        /// Note that read should not be confused with inventory.  A read allows for
        /// reading a sequence of one or more 16-bit words starting from an arbitrary
        /// 16-bit location in any of the tag's memory banks.
        /// </summary>
        /// <param name="parms">pointer to a structure that specifies the ISO 18000-6C tag-read
        /// operation parameters.  This parameter must not be NULL.</param>
        /// <param name="flags">read flags.  May be zero or a combination of the following:
        /// RFID_FLAG_PERFORM_SELECT - perform one or more selects before performing
        ///   the read.
        /// RFID_FLAG_PERFORM_POST_MATCH - perform post-singulation mask match on
        ///   singulated tags.</param>
        /// <returns></returns>
        public Result Tag18K6CRead
        (
            [In] ReadParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagRead(pHandle, lpBufPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }
            return Result;
        }



        /// <summary>
        /// Executes a tag write for the tags of interest.  If the
        /// RFID_FLAG_PERFORM_SELECT flag is specified, the tag population is
        /// partitioned (i.e., ISO 18000-6C select) prior to the tag-write operation.
        /// If the RFID_FLAG_PERFORM_POST_MATCH flag is specified, the post-singulation
        /// match mask is applied to a singulated tag's EPC to determine if the tag
        /// will be written to.  Writes may only be performed on 16-bit word boundaries
        /// and for multiples of 16-bit words.  If one or more of the specified memory
        /// words do not exist or are write-locked, the write to the tag will fail and
        /// this failure will be reported through the operation-response packet.  The
        /// operation-response packets will be returned to the application via
        /// the application-supplied callback function.  Each tag-write record is
        /// grouped with its corresponding tag-inventory record.  An application may
        /// prematurely stop a write operation by calling
        /// RFID_Radio{Cancel|Abort}Operation on another thread or by returning a non-
        /// zero value from the callback function.  A tag write may not be
        /// issued while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="parms">pointer to a structure that specifies the ISO 18000-6C tag-write
        /// operation parameters.  This parameter must not be NULL.</param>
        /// <param name="flags">write flags.  May be zero or a combination of the following:
        /// RFID_FLAG_PERFORM_SELECT - perform one or more selects before performing
        ///   the write.
        /// RFID_FLAG_PERFORM_POST_MATCH - perform post-singulation mask match on
        ///   singulated tags.</param>
        /// <returns></returns>
        public Result Tag18K6CWrite
        (
            [In] WriteParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.length);

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagWrite(pHandle, lpBufPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }
            if (lpBufPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }
            return Result;
        }

        public Result Tag18K6CBlockWrite
        (
            [In] BlockWriteParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.length);

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagBlockWrite(pHandle, lpBufPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }
            if (lpBufPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }
            return Result;
        }
        public Result Tag18K6CBlockErase
        (
            [In] BlockEraseParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.length);

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagBlockErase(pHandle, lpBufPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }
            if (lpBufPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }
            return Result;
        }

        public Result Tag18K6CPermaLock
        (
            [In] PermalockParms parms,
            [In] UInt32 flags
        )
        {

            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.length);

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagPermaLock(pHandle, lpBufPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }
            if (lpBufPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }
            return Result;
        }
        /// <summary>
        /// Executes a tag kill for the tags of interest.  If the
        /// RFID_FLAG_PERFORM_SELECT flag is specified, the tag population is
        /// partitioned (i.e., ISO 18000-6C select) prior to the tag-kill operation.
        /// If the RFID_FLAG_PERFORM_POST_MATCH flag is specified, the post-singulation
        /// match mask is applied to a singulated tag's EPC to determine if the tag
        /// will be killed.  The operation-response packets will be returned to the
        /// application via the application-supplied callback function.  Each tag-kill
        /// record is grouped with its corresponding tag-inventory record.  An
        /// application may prematurely stop a kill operation by calling
        /// RFID_Radio{Cancel|Abort}Operation on another thread or by returning a non-
        /// zerovalue from the callback function.  A tag kill may not be
        /// issued while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="parms">pointer to a structure that specifies the ISO 18000-6C tag-kill
        /// operation parameters.  This parameter must not be NULL.</param>
        /// <param name="flags">kill flags.  May be zero or a combination of the following:
        /// RFID_FLAG_PERFORM_SELECT - perform one or more selects before performing
        ///   the kill.
        /// RFID_FLAG_PERFORM_POST_MATCH - perform post-singulation mask match on
        ///   singulated tags.</param>
        /// <returns></returns>
        public Result Tag18K6CKill
        (
            [In] KillParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.length);

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagKill(pHandle, lpBufPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }
            if (lpBufPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }
            return Result;
        }


        /// <summary>
        /// Executes a tag lock for the tags of interest.  If the
        /// RFID_FLAG_PERFORM_SELECT flag is specified, the tag population is
        /// partitioned (i.e., ISO 18000-6C select) prior to the tag-lock operation.
        /// If the RFID_FLAG_PERFORM_POST_MATCH flag is specified, the post-singulation
        /// match mask is applied to a singulated tag's EPC to determine if the tag
        /// will be locked.  The operation-response packets will be returned to the
        /// application via the application-supplied callback function.  Each tag-lock
        /// record is grouped with its corresponding tag-inventory record.  An
        /// application may prematurely stop a lock operation by calling
        /// RFID_Radio{Cancel|Abort}Operation on another thread or by returning a non-
        /// zero value from the callback function.  A tag lock may not be
        /// issued while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="parms">pointer to a structure that specifies the ISO 18000-6C tag-lock
        /// operation parameters.  This parameter must not be NULL.</param>
        /// <param name="flags">lock flags.  May be zero or a combination of the following:
        /// RFID_FLAG_PERFORM_SELECT - perform one or more selects before performing
        ///   the lock.
        /// RFID_FLAG_PERFORM_POST_MATCH - perform post-singulation mask match on
        ///   singulated tags.</param>
        /// <returns></returns>
        public Result Tag18K6CLock
        (
            [In] LockParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.length);

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagLock(pHandle, lpBufPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }
            if (lpBufPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }
            return Result;
        }

        public Result Tag18K6CReadProtect
        (
            [In] ReadProtectParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.length);

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagReadProtect(pHandle, lpBufPtr, flags);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }
            if (lpBufPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }
            return Result;
        }
        public Result Tag18K6CResetReadProtect
        (
            [In] ReadProtectParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.length);

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagResetReadProtect(pHandle, lpBufPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }
            if (lpBufPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }
            return Result;
        }
        public Result Tag18K6CEASConfig
       (
           [In] EASParms parms,
           [In] UInt32 flags
       )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.length);

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagEASConfig(pHandle, lpBufPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }
            if (lpBufPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }
            return Result;
        }
        public Result Tag18K6CEASAlarm
        (
            [In] EASParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.length);

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagEASAlarm(pHandle, lpBufPtr, flags);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }
            if (lpBufPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }
            return Result;
        }
        /// <summary>
        /// Stops a currently-executing tag-protocol operation on a radio module.  The
        /// packet callback function will be executed until the command-end packet is
        /// received from the MAC or the packet callback returns a non-zero Result.
        /// 
        /// NOTE: This function must not be called from the packet callback function.
        /// </summary>
        /// <param name="flags">Reserved for future use.  Set to zero.</param>
        /// <returns></returns>
        public Result RadioCancelOperation
            (
                [In] UInt32 flags
            )
        {
            return Native.RFID_RadioCancelOperation(pHandle,flags);
        }

        /// <summary>
        /// Stops a currently-executing tag-protocol operation on a radio module and
        /// discards all remaining command-reponse packets.
        /// 
        /// NOTE: This function must not be called from the packet callback function.
        /// 
        /// </summary>
        /// <param name="flags">Reserved for future use.  Set to zero.</param>
        /// <returns></returns>
        public Result RadioAbortOperation
        (
            [In] UInt32 flags
        )
        {
            return Native.RFID_RadioAbortOperation(pHandle, flags);
        }

        /// <summary>
        /// Sets the operation response data reporting mode for tag-protocol
        /// operations.  By default, the reporting mode is set to "normal".  The 
        /// reporting mode will remain in effect until a subsequent call to
        /// RFID_RadioSetResponseDataMode.  The mode may not be changed while the
        /// radio is executing a tag-protocol operation.  The data response mode may
        /// not be set while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="responseType">the type of data that will have its reporting mode set.  For
        /// version 1.0 of the library, the only valid value is
        /// RFID_RESPONSE_TYPE_DATA.</param>
        /// <param name="responseMode">the operation response data reporting mode</param>
        /// <returns></returns>
        public Result RadioSetResponseDataMode
        (
            [In] ResponseType responseType,
            [In] ResponseMode responseMode
        )
        {
            return Native.RFID_RadioSetResponseDataMode(pHandle, responseType, responseMode);
        }

        /// <summary>
        /// Retrieves the operation response data reporting mode for tag-protocol
        /// operations.  The data response mode may not be retrieved while a radio
        /// module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="responseType">the type of data that will have its reporting mode
        /// retrieved.  For version 1.0 of the library, the only valid value is
        /// RFID_RESPONSE_TYPE_DATA.</param>
        /// <param name="responseMode">a pointer to a RFID_RESPONSE_MODE variable that upon return
        /// will contain the operation response data reporting mode.  Must not be
        /// NULL.</param>
        /// <returns></returns>
        public Result RadioGetResponseDataMode
        (
            [In]      ResponseType responseType,
            [In] ref  ResponseMode responseMode
        )
        {
            try
            {
                return Native.RFID_RadioGetResponseDataMode(pHandle, responseType, ref responseMode);
            }
            catch
            {
                // If encountered an error while auto-unmarshalling then
                // we have seen an int value that cannot be converted to
                // responseType or responseMode - mark both unknown.

                responseType = ResponseType.UNKNOWN;
                responseMode = ResponseMode.UNKNOWN;

                return Result.DRIVER_MISMATCH;
            }
        }



        /// <summary>
        /// Writes the specified data to the radio module nonvolatile-memory 
        /// block(s).  After a successful update, the RFID radio module resets itself
        /// and the RFID Reader Library closes and invalidates the radio pHandle so that
        /// it may no longer be used by the application.
        /// 
        /// In the case of an unsuccessful update the RFID Reader Library does not
        /// invalidate the radio pHandle ?i.e., it is the application responsibility
        /// to close the pHandle.
        /// 
        /// Alternatively, an application can perform the update in “test?mode.
        /// An application uses the “test?mode, by checking the returned status, to
        /// verify that the update would succeed before performing the destructive
        /// update of the radio module nonvolatile memory.  When a “test?update has
        /// completed, either successfully or unsuccessfully, the MAC firmware returns
        /// to its normal idle state and the radio pHandle remains valid (indicating
        /// that the application is still responsible for closing it).
        /// 
        /// The radio module nonvolatile memory may not be updated while a radio
        /// module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="countBlocks">The number of nonvolatile memory blocks in the array pointed
        /// to by pBlocks.  This value must be greater than zero.</param>
        /// <param name="pBlocks">A pointer to an array of countBlocks nonvolatile memory block
        /// structures that are used to control the update of the radio module
        /// nonvolatile memory.  This pointer must not be NULL.</param>
        /// <param name="flags">Zero, or a combination of the following:
        /// RFID_FLAG_TEST_UPDATE - Indicates that the RFID Reader Library is to
        ///   perform a non-destructive nonvolatile memory update to verify that
        ///   the update would succeed.  The RFID Reader Library will perform all
        ///   of the update operations with the exception that the data will not be
        ///   committed to nonvolatile memory.</param>
        /// <returns></returns>
        public Result MacUpdateNonvolatileMemory
        (
            [In] UInt32 countBlocks,
            [In] NonVolatileMemoryBlock[] pBlocks,
            [In] FwUpdateFlags flags
        )
        {
            Result Result = Result.OK;
            IntPtr pBlocksPtr = IntPtr.Zero;
            //IntPtr[] pDataPtrs = null;

            // Short-circuit check of parameter validity... not reason to
            // dive into native code if we know the parameters are wrong.

            if (countBlocks > pBlocks.Length)
            {
                return Result.INVALID_PARAMETER;
            }

            try
            {
                // Allocate memory to hold the pData arrays that
                // are held by the individual pBlock structures

                //pDataPtrs = new IntPtr[countBlocks];

                /*for (int idx = 0; idx < countBlocks; ++idx)
                {
                    if (pBlocks[idx].length > pBlocks[idx].pData.Length)
                    {
                        return Result.INVALID_PARAMETER;
                    }

                    pDataPtrs[idx] = Marshal.AllocHGlobal((int)pBlocks[idx].length);

                    // Copy C# byte array data into native...

                    Marshal.Copy(pBlocks[idx].pData, 0, pDataPtrs[idx], (int)pBlocks[idx].length);
                }*/

                // Allocate memory to hold countBlocks count
                // of pBlocks structures

                pBlocksPtr = Marshal.AllocHGlobal((int)(countBlocks * (12 + IntPtr.Size)));

                Int32 pBlocksPtrOffset = 0;

                // Copy pBlocks structures over to native memory

                for (int idx = 0; idx < countBlocks; ++idx)
                {
                    Marshal.WriteInt32(pBlocksPtr, pBlocksPtrOffset, (Int32)pBlocks[idx].address);
                    pBlocksPtrOffset += 4;

                    Marshal.WriteInt32(pBlocksPtr, pBlocksPtrOffset, (Int32)pBlocks[idx].length);
                    pBlocksPtrOffset += 4;

                    Marshal.WriteIntPtr(new IntPtr(pBlocksPtr.ToInt64() + pBlocksPtrOffset), pBlocks[idx].pData);
                    pBlocksPtrOffset += IntPtr.Size;

                    Marshal.WriteInt32(pBlocksPtr, pBlocksPtrOffset, (Int32)pBlocks[idx].flags);
                    pBlocksPtrOffset += 4;
                }

                Result = Native.RFID_MacUpdateNonvolatileMemory(pHandle, countBlocks, pBlocksPtr, flags);
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            /*if (null != pDataPtrs)
            {
                for (int idx = 0; idx < countBlocks; ++idx)
                {
                    if (IntPtr.Zero != pDataPtrs[idx])
                    {
                        Marshal.FreeHGlobal(pDataPtrs[idx]);
                    }
                }
            }*/
            if (IntPtr.Zero != pBlocksPtr)
            {
                Marshal.FreeHGlobal(pBlocksPtr);
            }

            return Result;
        }

        /// <summary>
        /// Retrieves the radio module's MAC firmware version information.  The MAC
        /// version may not be retrieved while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="pVersion">pointer to structure that upon return will contain the radio
        /// module's MAC firmware version information.  Must not be NULL.</param>
        /// <returns></returns>
        public Result MacGetVersion
        (
            [In, Out] MacVersion pVersion
        )
        {
            return Native.RFID_MacGetVersion(pHandle, pVersion);
        }

        /// <summary>
        /// Reads one or more 32-bit words from the MAC's OEM configuration data
        /// area.  The OEM data are may not be read while a radio module is executing
        /// a tag-protocol operation.
        /// </summary>
        /// <param name="address">the 32-bit address of the first 32-bit word to read from
        /// the MAC's OEM configuration data area.  Note that this is not a byte
        /// address - i.e., address 1 is actually byte 4, address 2 is actually byte
        /// 8, etc.  If the address is beyond the end of the OEM configuration data
        /// area, Results in an error.</param>
        /// <param name="count">the number of 32-bit words to read.   Must be greater
        /// than zero.  If count causes the read to extend beyond the end of the
        /// OEM configuration data area, Results in an error.</param>
        /// <param name="data"> pointer to the buffer into which the OEM configuration data will
        /// be placed.  The buffer must be at least (count * 4) bytes in length.
        /// Must not be NULL.  Note that the data returned will be in the MAC's
        /// native format (i.e., little endian).</param>
        /// <returns></returns>
        public  Result MacReadOemData
        (
            [In] UInt32 address,
            [In] UInt32 count,
            [In] UInt32[] data
        )
        {
            Result Result = Result.OK;
#if USE_INTPTR
            IntPtr dataPtr = IntPtr.Zero;
#endif
            // Short-circuit check of parameter validity... not reason to
            // dive into native code if we know the parameters are wrong.

            if (count > data.Length)
            {
                return Result.INVALID_PARAMETER;
            }

            try
            {
#if USE_INTPTR
                    dataPtr = Marshal.AllocHGlobal((int)(count * 4));

                    Result = Native.RFID_MacReadOemData(pHandle, address, count, dataPtr);

                    // Required due to non-castability of data to int[ ] ?

                    for (int index = 0; index < count; ++index)
                    {
                        data[index] = (UInt32)Marshal.ReadInt32(dataPtr, index * 4);
                    }
#else
                fixed (UInt32* dataPtr = data)
                {
                    Result = Native.RFID_MacReadOemData(pHandle, address, count, dataPtr);
                }
#endif
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

#if USE_INTPTR
            if (IntPtr.Zero != dataPtr)
            {
                Marshal.FreeHGlobal(dataPtr);
            }
#endif
            return Result;
        }
        /// <summary>
        /// Reads one or more 32-bit words from the MAC's OEM configuration data
        /// area.  The OEM data are may not be read while a radio module is executing
        /// a tag-protocol operation.
        /// </summary>
        /// <param name="address">the 32-bit address of the first 32-bit word to read from
        /// the MAC's OEM configuration data area.  Note that this is not a byte
        /// address - i.e., address 1 is actually byte 4, address 2 is actually byte
        /// 8, etc.  If the address is beyond the end of the OEM configuration data
        /// area, Results in an error.</param>
        /// <param name="data"> pointer to the buffer into which the OEM configuration data will
        /// be placed.  The buffer must be at least (count * 4) bytes in length.
        /// Must not be NULL.  Note that the data returned will be in the MAC's
        /// native format (i.e., little endian).</param>
        /// <returns></returns>
        public  Result MacReadOemData
        (
            [In] UInt32 address,
            [In, Out] ref UInt32 data
        )
        {
            Result Result = Result.OK;

            // Short-circuit check of parameter validity... not reason to
            // dive into native code if we know the parameters are wrong.

            try
            {
                fixed (uint* dataPtr = &data)
                {
                    Result = Native.RFID_MacReadOemData(pHandle, address, 1, dataPtr);
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            return Result;
        }

        /// <summary>
        /// Writes one or more 32-bit words to the MAC's OEM configuration data
        /// area.  The OEM data area may not be written while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="address">the 32-bit address of the first 32-bit word to write in
        /// the MAC's OEM configuration data area.  Note that this is not a byte
        /// address - i.e., address 1 is actually byte 4, address 2 is actually byte
        /// 8, etc.  If the address is beyond the end of the OEM configuration data
        /// area, Results in an error.</param>
        /// <param name="count">the number of 32-bit words to write.   Must be greater
        /// than zero.  If count causes the write to extend beyond the end of the
        /// OEM configuration data area, Results in an error.</param>
        /// <param name="data">pointer to the buffer that contains the data to write to the OEM
        /// configuration area.  The buffer must be at least (count * 4) bytes in
        /// length.  Must not be NULL.  Note that the data must be in the MAC's
        /// native format (i.e., little endian).</param>
        /// <returns></returns>
        public  Result MacWriteOemData
        (
            [In] UInt32 address,
            [In] UInt32 count,
            [In] UInt32[] data
        )
        {
            Result Result = Result.OK;
#if USE_INTPTR
            IntPtr dataPtr = IntPtr.Zero;
#endif
            try
            {
#if USE_INTPTR
                dataPtr = Marshal.AllocHGlobal((int)(count * 4));
                // Required due to non-castability of data to int[]

                for (int index = 0; index < count; ++index)
                {
                    Marshal.WriteInt32(dataPtr, index * 4, (int)data[index]);
                }
#endif
                fixed (uint* dataPtr = data)
                {
                    Result = Native.RFID_MacWriteOemData(pHandle, address, count, dataPtr);
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

#if USE_INTPTR
            if (IntPtr.Zero != dataPtr)
            {
                Marshal.FreeHGlobal(dataPtr);
            }
#endif
            return Result;
        }
        /// <summary>
        /// Writes one or more 32-bit words to the MAC's OEM configuration data
        /// area.  The OEM data area may not be written while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="address">the 32-bit address of the first 32-bit word to write in
        /// the MAC's OEM configuration data area.  Note that this is not a byte
        /// address - i.e., address 1 is actually byte 4, address 2 is actually byte
        /// 8, etc.  If the address is beyond the end of the OEM configuration data
        /// area, Results in an error.</param>
        /// <param name="data">pointer to the buffer that contains the data to write to the OEM
        /// configuration area.  The buffer must be at least (count * 4) bytes in
        /// length.  Must not be NULL.  Note that the data must be in the MAC's
        /// native format (i.e., little endian).</param>
        /// <returns></returns>
        public  Result MacWriteOemData
        (
            [In] UInt32 address,
            [In] UInt32 data
        )
        {
            return MacWriteOemData(address, 1, new uint[] { data });
        }

        /// <summary>
        /// Instructs the radio module's MAC firmware to perform the specified reset.
        /// Any currently executing tag-protocol operations will be aborted, any
        /// unconsumed data will be discarded, and tag-protocol operation functions
        /// (i.e., RFID_18K6CTagInventory, etc.) will return immediately with an
        /// error of RFID_ERROR_OPERATION_CANCELLED.
        /// Upon reset, the connection to the radio module is lost and the pHandle
        /// to the radio is invalid.  To obtain control of the radio module after it
        /// has been reset, the application must re-enumerate the radio modules, via
        /// RFID_RetrieveAttachedRadiosList, and request control via RFID_RadioOpen.
        /// 
        /// NOTE: This function must not be called from the packet callback function.
        /// 
        /// </summary>
        /// <param name="resetType">the type of reset to perform on the radio</param>
        /// <returns></returns>
        public Result MacReset
        (
            [In] MacResetType resetType
        )
        {
            return Native.RFID_MacReset(pHandle, resetType);
        }


        /// <summary>
        /// Attempts to clear the error state for the radio module MAC firmware.  The
        /// MAC error may not be cleared while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <returns></returns>
        public Result MacClearError
        (
        )
        {
            return Native.RFID_MacClearError(pHandle);
        }
        /// <summary>
        /// Allows for direct writing of registers on the radio (i.e., bypassing the
        /// MAC).  The radio registers may not be written while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="address">the address of the register to write.  An address that is beyond
        ///  the end of the radio module's register set Results in an invalid-parameter
        ///  return status.</param>
        /// <param name="value">the value to write to the register</param>
        /// <returns></returns>
        public Result MacBypassWriteRegister
        (
            [In] UInt16 address,
            [In] UInt16 value
        )
        {
            return Native.RFID_MacBypassWriteRegister(pHandle, address, value);
        }

        /// <summary>
        /// Allows for direct reading of registers on the radio (i.e., bypassing the
        /// MAC).  The radio regsiters mode may not be read while a radio module is
        /// executing a tag-protocol operation.
        /// </summary>
        /// <param name="address">the address of the register to write  An address that is beyond
        ///  the end of the radio module's register set Results in an invalid-parameter
        ///  return status.</param>
        /// <param name="value">pointer to unsigned 16-bit integer that will receive register
        /// value.  This parameter must not be NULL.</param>
        /// <returns></returns>
        public Result MacBypassReadRegister
        (
            [In]     UInt16 address,
            [In] ref UInt16 value
        )
        {
            return Native.RFID_MacBypassReadRegister(pHandle, address, ref value);
        }

        /// <summary>
        /// Sets the regulatory mode region for the MAC's operation.  The region of 
        /// operation may not be set while a radio module is executing a tag-protocol
        /// operation.
        /// </summary>
        /// <param name="region">the region to which the radio operation is to be set.</param>
        /// <param name="regionConfig">reserved for future use.  Must be NULL.</param>
        /// <returns></returns>
        public Result MacSetRegion
        (
            [In] MacRegion region,
            [In] IntPtr regionConfig
        )
        {
            // See notes - region config null currently required...
            return Native.RFID_MacSetRegion(pHandle, region, IntPtr.Zero);
        }


        /// <summary>
        /// Retrieves the regulatory mode region for the MAC's operation.  The region
        /// of operation may not be retrieved while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="region">pointer to variable that will receive region.  Must not be NULL.</param>
        /// <param name="regionConfig">reserved for future use.  Must be NULL.</param>
        /// <returns></returns>
        public Result MacGetRegion
        (
            [In] ref MacRegion region,
            [In]     IntPtr regionConfig
        )
        {
            // See notes - region config null currently required...
            return Native.RFID_MacGetRegion(pHandle, ref region, IntPtr.Zero);
        }

        /// <summary>
        /// Configures the specified radio module's GPIO pins.  For version 1.0 of the
        /// library, only GPIO pins 0-3 are valid.  The GPIO pin configuration may not
        /// be set while a radio module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="mask">a 32-bit mask which specifies which GPIO pins are to be configured.
        /// Bit 0 (i.e., the lowest-order bit) represents GPIO pin 0, bit 1
        /// represents GPIO pin 1, etc.  The presence of a 1 bit in a mask-bit
        /// location indicates that the GPIO pin is to be configured.  The presence
        /// of a 0 bit in a mask-bit location indicates that the GPIO pin
        /// configuration is to remain unchanged.</param>
        /// <param name="configuration">A 32-bit value that indicates the configuration for the
        /// bits corresponding to the ones set in mask ?bit 0 (i.e., the lowest-
        /// order bit) represents GPIO pin 0's configuration, etc.  Bits which
        /// correspond to bits set to 0 in mask are ignored.  The presence of a 1 in
        /// a bit location indicates that the GPIO pin is to be configured as an
        /// output pin.  The presence of a 0 in a bit location indicates that the
        /// GPIO pin is to be configured as an input pin.</param>
        /// <returns></returns>
        public Result RadioSetGpioPinsConfiguration
        (
            [In] UInt32 mask,
            [In] UInt32 configuration
        )
        {
            return Native.RFID_RadioSetGpioPinsConfiguration(pHandle, mask, configuration);
        }

        /// <summary>
        /// Retrieves the configuration for the radio module's GPIO pins.  For version
        /// 1.0 of the library, only GPIO pins 0-3 are valid.  The GPIO pin 
        /// configuration may not be retrieved while a radio module is executing a tag-
        /// protocol operation.
        /// </summary>
        /// <param name="configuration">A pointer to an unsigned 32-bit integer that upon return
        /// contains the configuration for the radio module GPIO pins ?bit 0
        /// (i.e., the lowest-order bit) represents GPIO pin 0, etc.  The presence
        /// of a 1 in a bit location indicates that the GPIO pin is configured as an
        /// output pin.  The presence of a 0 in a bit location indicates that the
        /// GPIO pin is configured as an input pin.</param>
        /// <returns></returns>
        public Result RadioGetGpioPinsConfiguration
        (
            [In] ref UInt32 configuration
        )
        {
            return Native.RFID_RadioGetGpioPinsConfiguration(pHandle, ref configuration);
        }

        /// <summary>
        /// Reads the specified radio module's GPIO pins.  Attempting to read from an
        /// output GPIO pin Results in an error.  For version 1.0 of the library, only
        /// GPIO pins 0-3 are valid.  The GPIO pins may not be read while a radio 
        /// module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="mask">a 32-bit mask which specifies which GPIO pins are to be read.  Bit
        ///   0 (i.e., the lowest-order bit) represents GPIO pin 0, bit 1 represents
        ///   GPIO pin 1, etc.  The presence of a 1 bit in a mask bit location
        ///   indicates that the GPIO pin is to be read.</param>
        /// <param name="value">a pointer to a 32-bit unsigned integer that upon return will
        ///   contain the bit values of the GPIO pins specified in the mask.  Bit 0 of
        ///   the *pValue corresponds to GPIO pin 0, bit 1 corresponds to GPIO
        ///   pin 1, etc.  If a GPIO pin's bit is not set in mask, then the bit value
        ///   in the corresponding bit in *pValue is undefined.</param>
        /// <returns></returns>
        public Result RadioReadGpioPins
        (
            [In]     UInt32 mask,
            [In] ref UInt32 value
        )
        {
            return Native.RFID_RadioReadGpioPins(pHandle, mask, ref value);
        }
        /// <summary>
        /// Writes the specified radio module's GPIO pins.  Attempting to write to an
        /// input GPIO pin Results in an error.  For version 1.0 of the library, only
        /// GPIO pins 0-3 are valid.  The GPIO pins may not be written while a radio
        /// module is executing a tag-protocol operation.
        /// </summary>
        /// <param name="mask">a 32-bit mask which specifies which GPIO pins are to be written.
        /// Bit 0 (i.e., the lowest-order bit) represents GPIO pin 0, bit 1
        /// represents GPIO pin 1, etc.  The presence of a 1 in a mask bit location
        /// indicates that the corresponding bit in value is to be written to the 
        /// GPIO pin.</param>
        /// <param name="value">
        /// a 32-bit unsigned integer that contains the bits to write to the
        /// GPIO pins specifed in mask.  Bit 0 of the value corresponds to the value
        /// to write to GPIO pin 0, bit 1 corresponds to the value to write to GPIO
        /// pin 1, etc.  If a GPIO pin's bit is not set in mask, then the bit value
        /// in the corresponding bit is ignored.</param>
        /// <returns></returns>
        public Result RadioWriteGpioPins
        (
            [In] UInt32 mask,
            [In] UInt32 value
        )
        {
//            return Native.RFID_RadioWriteGpioPins(pHandle, mask, value);
        }

        #region WaterYu @ 16-Sept-2009
#if NET_BUILD
        public Result GetMACAddress([In, Out] Byte[] mac)
        {
            return Native.RFID_GetMACAddress(pHandle, mac);
        }
#endif
        public Result Tag18K6CWriteEPC
        (
            [In] TagWriteEpcParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                if (parms == null || parms.count == 0)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal((int)parms.Length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.Length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.retryCount);
                lpBufOff += 4;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.offset);
                lpBufOff += 2;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.count);
                lpBufOff += 2;

                Marshal.Copy(parms.epc.ToShorts(), 0, new IntPtr(lpBufPtr.ToInt64() + lpBufOff), parms.epc.GetLength());

                Result = Native.RFID_18K6CTagWriteEPC(pHandle, lpBufPtr);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

            }

            return Result;
        }

        public Result Tag18K6CWritePC
        (
            [In] TagWritePcParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagWritePcParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagWritePC(pHandle, lpBufPtr);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CWriteAccPwd
        (
            [In] TagWritePwdParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagWritePwdParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagWriteAccPwd(pHandle, lpBufPtr);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CWriteKillPwd
        (
            [In] TagWritePwdParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagWritePwdParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagWriteKillPwd(pHandle, lpBufPtr);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }
        public Result Tag18K6CWriteUser
        (
            [In] TagWriteUserParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            IntPtr ptr_A = IntPtr.Zero;

            try
            {
                if (parms == null || parms.pData == null || parms.pData.Length != parms.count || parms.count == 0)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal((Int32)parms.Length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.Length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.retryCount);
                lpBufOff += 4;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.offset);
                lpBufOff += 2;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.count);
                lpBufOff += 2;


                ptr_A = Marshal.AllocHGlobal(parms.pData.Length * 2);

                Marshal.WriteIntPtr
                (
                    new IntPtr(lpBufPtr.ToInt64() + lpBufOff),
                    ptr_A
                );

                lpBufOff += IntPtr.Size;

                for (int index = 0; index < parms.count; ++index)
                {
                    Marshal.WriteInt16
                    (
                        ptr_A,
                        index * 2,
                        (Int16)parms.pData[index]
                    );
                }

                Result = Native.RFID_18K6CTagWriteUser(pHandle, lpBufPtr);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

                if (IntPtr.Zero != ptr_A)
                {
                    Marshal.FreeHGlobal(ptr_A);
                }
            }

            return Result;
        }

        public Result Tag18K6CReadEPC
        (
            [In] TagReadEpcParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null || parms.count == 0)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagReadEpcParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagReadEPC(pHandle, lpBufPtr);

                if (Result == Result.OK)
                {
                    Marshal.PtrToStructure(lpBufPtr, parms);
                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CReadTID
        (
            [In] TagReadTidParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null || parms.count == 0)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagReadTidParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagReadTID(pHandle, lpBufPtr);

                if (Result == Result.OK)
                {
                    Marshal.PtrToStructure(lpBufPtr, parms);

                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CReadPC
        (
            [In] TagReadPcParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagReadPcParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagReadPC(pHandle, lpBufPtr);

                if (Result == Result.OK)
                {
                    Marshal.PtrToStructure(lpBufPtr, parms);
                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CReadAccPwd
        (
            [In] TagReadPwdParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagReadPwdParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagReadAccPwd(pHandle, lpBufPtr);

                if (Result == Result.OK)
                {
                    Marshal.PtrToStructure(lpBufPtr, parms);
                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CReadKillPwd
        (
            [In] TagReadPwdParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagReadPwdParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagReadKillPwd(pHandle, lpBufPtr);

                if (Result == Result.OK)
                {
                    Marshal.PtrToStructure(lpBufPtr, parms);
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }
        public Result Tag18K6CReadUser
        (
            [In] TagReadUserParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            IntPtr ptr_A = IntPtr.Zero;

            try
            {
                if (parms == null || parms.count == 0)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.retryCount);
                lpBufOff += 4;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.offset);
                lpBufOff += 2;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.count);
                lpBufOff += 2;

                ptr_A = Marshal.AllocHGlobal(parms.count * 2);

                Marshal.WriteIntPtr
                (
                    new IntPtr(lpBufPtr.ToInt64() + lpBufOff),
                    ptr_A
                );

                lpBufOff += IntPtr.Size;

                Result = Native.RFID_18K6CTagReadUser(pHandle, lpBufPtr);

                if (Result == Result.OK)
                {
                    short[] tmp = new short[parms.count];

                    Marshal.Copy(ptr_A, tmp, 0, parms.count);

                    parms.pData = new S_DATA(tmp);
                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

                if (IntPtr.Zero != ptr_A)
                {
                    Marshal.FreeHGlobal(ptr_A);
                }
            }

            return Result;
        }

        public Result Tag18K6CRawLock
        (
            [In] TagLockParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            //Int32 lpBufOff = 0;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagLockParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                // count
                /*
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.Length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.retryCount);
                lpBufOff += 4;

                // permissions
                Marshal.StructureToPtr(parms.permissions, new IntPtr(lpBufPtr.ToInt64() + lpBufOff), false);*/

                Result = Native.RFID_18K6CTagRawLock(pHandle, lpBufPtr);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

            }

            return Result;
        }
        public Result Tag18K6CBlockLock
            (
            [In] TagBlockPermalockParms parms
            )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero, ptr_A = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // offset

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.accessPassword);
                lpBufOff += 4;
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)(parms.setPermalock ? 0x1 : 0));
                lpBufOff += 4;
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.retryCount);
                lpBufOff += 4;
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.flags);
                lpBufOff += 4;
                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.count);
                lpBufOff += 2;
                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.offset);
                lpBufOff += 2;

                //parms.mask = new ushort[parms.count];

                ptr_A = Marshal.AllocHGlobal(parms.mask.Length * 2);

                Marshal.WriteIntPtr
                (
                    new IntPtr(lpBufPtr.ToInt64() + lpBufOff),
                    ptr_A
                );

                lpBufOff += IntPtr.Size;

                if (parms.mask != null && parms.setPermalock)
                {
                    for (int index = 0; index < parms.count; ++index)
                    {
                        Marshal.WriteInt16
                        (
                            ptr_A,
                            index * 2,
                            (Int16)parms.mask[index]
                        );
                    }
                }


                Result = Native.RFID_18K6CTagBlockPermalock(pHandle, lpBufPtr);

                if (Result == Result.OK && !parms.setPermalock)
                {
                    short[] tmp = new short[parms.count];
                    Marshal.Copy(ptr_A, tmp, 0, parms.count);
                    parms.mask = (ushort[])tmp.Clone();
                }

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                if (ptr_A != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr_A);
                }

                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }
        public Result Tag18K6CRawKill
        (
            [In] TagKillParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            //Int32 lpBufOff = 0;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TagKillParms)));

                Marshal.StructureToPtr(parms, lpBufPtr, false);

                Result = Native.RFID_18K6CTagRawKill(pHandle, lpBufPtr);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);

            }

            return Result;
        }
        public Result Tag18K6CSearchAny
        (
            [In] InternalTagInventoryParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.flags);
                lpBufOff += 4;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.tagStopCount);
                lpBufOff += 2;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), Marshal.GetFunctionPointerForDelegate(parms.pCallback));

                Result = Native.RFID_18K6CTagSearchAny(pHandle, lpBufPtr);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CSearchOne
        (
            [In] InternalTagSearchOneParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, parms.avgRssi ? 1 : 0);
                lpBufOff += 4;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), Marshal.GetFunctionPointerForDelegate(parms.pCallback));

                Result = Native.RFID_18K6CTagSearchOne(pHandle, lpBufPtr);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        public Result Tag18K6CRanging
        (
            [In] InternalTagRangingParms parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            Int32 lpBufOff = 0;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                lpBufPtr = Marshal.AllocHGlobal((int)parms.length);

                // count

                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.length);
                lpBufOff += 4;

                // Common parms struct
                Marshal.WriteInt32(lpBufPtr, lpBufOff, (Int32)parms.flags);
                lpBufOff += 4;

                Marshal.WriteInt16(lpBufPtr, lpBufOff, (Int16)parms.tagStopCount);
                lpBufOff += 2;

                Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + lpBufOff), Marshal.GetFunctionPointerForDelegate(parms.pCallback));

                Result = Native.RFID_18K6CTagRanging(pHandle, lpBufPtr);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }
        public Result Tag18K6CCustCmdReadProtect
        (
            [In] InternalCustCmdTagReadProtectParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                Result = Native.RFID_18K6CCustTagReadProtect(pHandle, parms, flags);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            return Result;
        }
        
        public Result Tag18K6CCustCmdResetReadProtect
        (
            [In] InternalCustCmdTagReadProtectParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                Result = Native.RFID_18K6CCustTagResetReadProtect(pHandle, parms, flags);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            return Result;
        }

        public Result Tag18K6CCustCmdEASConfig
        (
            [In] InternalCustCmdEASParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                Result = Native.RFID_18K6CCustTagEASConfig(pHandle, parms, flags);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            return Result;
        }
       
        public Result Tag18K6CCustCmdEASAlarm
        (
            [In] InternalCustCmdEASParms parms,
            [In] UInt32 flags
        )
        {
            Result Result = Result.OK;

            try
            {
                if (parms == null)
                {
                    return Result.INVALID_PARAMETER;
                }

                Result = Native.RFID_18K6CCustTagEASAlarm(pHandle, parms, flags);

            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            return Result;
        }

        public Result RadioCLSetPassword([In] UInt32 reg1, [In] UInt32 reg2)
        {
            return Native.RFID_RadioCLSetPassword(pHandle, reg1, reg2);
        }

        public Result RadioCLSetLogMode([In] UInt32 reg1)
        {
            return Native.RFID_RadioCLSetLogMode(pHandle, reg1);
        }

        public Result RadioCLSetLogLimits([In] UInt32 reg1, [In] UInt32 reg2)
        {
            return Native.RFID_RadioCLSetLogLimits(pHandle, reg1, reg2);
        }

        public Result RadioCLGetMeasurementSetup(byte [] pParms)
        {
            return Native.RFID_RadioCLGetMeasurementSetup (pHandle, pParms);
        }

        public Result RadioCLSetSFEParameters(UInt32 reg1)
        {
            return Native.RFID_RadioCLSetSFEParameters(pHandle, reg1);
        }

        public Result RadioCLSetCalibrationData(UInt32 reg1, UInt32 reg2)
        {
            return Native.RFID_RadioCLSetCalibrationData(pHandle, reg1, reg2);
        }

        public Result RadioCLEndLog()
        {
//            return Native.RFID_RadioCLEndLog(pHandle);
        }

        public Result RadioCLStartLog(UINT32 starttime)
        {
//            return Native.RFID_RadioCLStartLog(pHandle, starttime);
        }

        public Result RadioCLGetLogState(UINT32 ShelfLifeFlag, byte[] Parms)
        {
            return Native.RFID_RadioCLGetLogState(pHandle, ShelfLifeFlag, Parms);
        }

        public Result RadioCLGetCalibrationData(byte [] pParms)
        {
            return Native.RFID_RadioCLGetCalibrationData(pHandle, pParms);
        }

        public Result RadioCLGetBatteryLevel(UInt32 reg1, byte [] Parms)
        {
            return Native.RFID_RadioCLGetBatteryLevel(pHandle, reg1, Parms);
        }

        public Result RadioCLSetShelfLife(UINT32 reg1, UINT32 reg2)
        {
            return Native.RFID_RadioCLSetShelfLife(pHandle, reg1, reg2);
        }

        public Result RadioCLInitialize(UInt32 reg1)
        {
            return Native.RFID_RadioCLInitialize(pHandle, reg1);
        }

        public Result RadioCLGetSensorValue(UInt32 reg1, byte [] Parms)
        {
            return Native.RFID_RadioCLGetSensorValue(pHandle, reg1, Parms);
        }

        public Result RadioCLOpenArea(UInt32 reg1, UInt32 reg2)
        {
            return Native.RFID_RadioCLOpenArea(pHandle, reg1, reg2);
        }

        public Result RadioCLAccessFifo(UInt32 reg1, UInt32 reg2, UInt32 reg3, byte [] Parms)
        {
            return Native.RFID_RadioCLAccessFifo(pHandle, reg1, reg2, reg3, Parms);
        }

        public Result RadioG2X_Change_EAS()
        {
            return Native.RFID_RadioG2X_Change_EAS(pHandle);
        }

        public Result RadioG2X_EAS_Alarm()
        {
            return Native.RFID_RadioG2X_EAS_Alarm(pHandle);
        }

        public Result RadioG2X_ChangeConfig()
        {
            return Native.RFID_RadioG2X_ChangeConfig(pHandle);
        }

        public Result RadioQT_Command(int RW, int TP, int SR, int MEM)
        {
            return Native.RFID_RadioQT_Command(pHandle, RW, TP, SR, MEM);
        }

        #endregion

#if NOUSE
        public Result Initialize(IntPtr hWnd)
        {
            return Native.rfidmx_Initialize(hWnd);
        }

        public Result Uninitialize()
        {
            return Native.rfidmx_Uninitialize();
        }

        public Result PostMessage
        (
            [In] RFID_OPERATION operation,
            [In] cmnparm parms
        )
        {
            Result Result = Result.OK;
            IntPtr lpBufPtr = IntPtr.Zero;
            int lpBufOff = 0;

            try
            {
                switch (operation)
                {
                    case RFID_OPERATION.TAG_BLOCK_ERASE:

                        break;
                    case RFID_OPERATION.TAG_BLOCK_WRITE:
                        break;
                    case RFID_OPERATION.TAG_INVENTORY:
                        if (parms.GetType() != typeof(CB_INV_PARMS))
                        {
                            return Result.INVALID_PARAMETER;
                        }
                        CB_INV_PARMS invparm = (CB_INV_PARMS)parms;

                        lpBufPtr = Marshal.AllocHGlobal((int)invparm.length);

                        Marshal.WriteInt32(lpBufPtr, 0, (Int32)invparm.length);
                        Marshal.WriteInt32(lpBufPtr, 4, (Int32)invparm.pHandle);
                        Marshal.WriteInt32(lpBufPtr, 8, (Int32)invparm.flags);
                        Marshal.WriteInt32(lpBufPtr, 12, (Int32)invparm.tagStopCount);
                        //Marshal.WriteIntPtr(new IntPtr(lpBufPtr.ToInt64() + 16), Marshal.GetFunctionPointerForDelegate(invparm.callback));
                        Result = Native.rfidmx_PostMessage(operation, lpBufPtr);
                        break;
                    case RFID_OPERATION.TAG_KILL:
                    case RFID_OPERATION.TAG_LOCK:
                        break;
                    case RFID_OPERATION.TAG_READ:
                        if (parms.GetType() != typeof(CB_READ_PARMS))
                        {
                            return Result.INVALID_PARAMETER;
                        }
                        CB_READ_PARMS parm = (CB_READ_PARMS)parms;

                        lpBufPtr = Marshal.AllocHGlobal((int)parm.length);

                        WriteInt32(lpBufPtr, ref lpBufOff, parm.length);

                        WriteInt32(lpBufPtr, ref lpBufOff, parm.pHandle);
                        WriteInt32(lpBufPtr, ref lpBufOff, parm.flags);

                        // count
                        WriteInt32(lpBufPtr, ref lpBufOff, parm.parms.length);

                        // Common parms struct

                        WriteInt32(lpBufPtr, ref lpBufOff, parm.parms.common.tagStopCount);

                        WriteIntPtr(lpBufPtr, ref lpBufOff, IntPtr.Zero);

                        WriteIntPtr(lpBufPtr, ref lpBufOff, IntPtr.Zero);

                        WriteIntPtr(lpBufPtr, ref lpBufOff, IntPtr.Zero);

                        // bank

                        WriteInt32(lpBufPtr, ref lpBufOff, (int)parm.parms.bank);

                        // offset

                        WriteInt16(lpBufPtr, ref lpBufOff, parm.parms.offset);

                        // count

                        WriteInt16(lpBufPtr, ref lpBufOff, parm.parms.count);

                        // access password

                        WriteInt32(lpBufPtr, ref lpBufOff, parm.parms.accessPassword);


                        Result = Native.rfidmx_PostMessage(operation, lpBufPtr);
                        break;
                    case RFID_OPERATION.TAG_WRITE:
                        break;
                    case RFID_OPERATION.TAG_SEARCH:
                        if (parms.GetType() != typeof(CB_TAG_SEARCH_PARMS))
                        {
                            return Result.INVALID_PARAMETER;
                        }
                        CB_TAG_SEARCH_PARMS searchparm = (CB_TAG_SEARCH_PARMS)parms;

                        lpBufPtr = Marshal.AllocHGlobal((int)searchparm.length);

                        WriteInt32(lpBufPtr, ref lpBufOff, (Int32)searchparm.length);
                        WriteInt32(lpBufPtr, ref lpBufOff, (Int32)searchparm.pHandle);
                        WriteInt32(lpBufPtr, ref lpBufOff, searchparm.average ? 1 : 0);
                        WriteInt32(lpBufPtr, ref lpBufOff, searchparm.bUsePc ? 1 : 0);
                        WriteInt32(lpBufPtr, ref lpBufOff, (Int32)searchparm.retryCount);
                        WriteInt32(lpBufPtr, ref lpBufOff, (Int32)searchparm.masklen);
                        Marshal.Copy(searchparm.pc_epc_mask, 0, new IntPtr(lpBufPtr.ToInt64() + lpBufOff), (Int32)searchparm.masklen);
                        Result = Native.rfidmx_PostMessage(operation, lpBufPtr);
                        break;
                }
            }
            catch (OutOfMemoryException)
            {
                Result = Result.OUT_OF_MEMORY;
            }

            if (IntPtr.Zero != lpBufPtr)
            {
                Marshal.FreeHGlobal(lpBufPtr);
            }

            return Result;
        }

        private void WriteInt32(IntPtr ptr, ref int offset, Int32 value)
        {
            Marshal.WriteInt32(ptr, offset, value);
            offset += 4;
        }
        private void WriteInt32(IntPtr ptr, ref int offset, UInt32 value)
        {
            Marshal.WriteInt32(ptr, offset, (Int32)value);
            offset += 4;
        }
        private void WriteInt16(IntPtr ptr, ref int offset, UInt16 value)
        {
            Marshal.WriteInt16(ptr, offset, (Int16)value);
            offset += 2;
        }
        private void WriteInt16(IntPtr ptr, ref int offset, Int16 value)
        {
            Marshal.WriteInt16(ptr, offset, value);
            offset += 2;
        }

        private void WriteIntPtr(IntPtr ptr, ref int offset, IntPtr value)
        {
            Marshal.WriteIntPtr(new IntPtr(ptr.ToInt64() + offset), value);
            offset += IntPtr.Size;
        }
#endif

    }  // Linkage class END
#endif

}// rfidnamespace END
