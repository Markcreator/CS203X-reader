#if nouse
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
#if WIN32
using System.IO;
using System.Reflection;
#endif
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;

using CSLibrary.Constants;
using CSLibrary.Structures;

using INT8U = System.Byte;
using UINT8 = System.Byte;
using INT32U = System.UInt32;
using UINT32 = System.UInt32;
using INT16U = System.UInt16;
using UINT16 = System.UInt16;
using INT64U = System.UInt64;
using UINT64 = System.UInt64;

namespace CSLibrary
{
    sealed class Native
    {

        /// <summary>
        /// Declare, instantiate and utilize for callbacks
        /// originating from inventory, read, write and
        /// similar operations...
        /// </summary>
        /// <param name="pHandle"> radio handle </param>
        /// <param name="bufferLength"> buffer length </param>
        /// <param name="pBuffer">buffer pointer</param>
        /// <param name="context">custom pointer</param>
        /// <returns></returns>
#if WIN32
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
        public delegate Int32 CallbackDelegate
        (
            [In]      RfidHandle pHandle,
            [In]      UInt32 bufferLength,
            [In]      IntPtr pBuffer,
            [In]      IntPtr context
        );
        /// <summary>
        /// Declare, instantiate and utilize for callbacks
        /// originating from Searching Any tags operation
        /// </summary>
        /// <param name="pc"> pc </param>
        /// <param name="epcLength"> epc length </param>
        /// <param name="epc">epc pointer</param>
        /// <returns></returns>
#if WIN32
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
        public delegate Int32 TagSearchAnyCallbackDelegate
        (
            [In]      UInt16 pc,
            [In]      UInt32 epcLength,
            [In]      IntPtr epc,
            [In]      UInt32 ms_ctr
       );
        /// <summary>
        /// Declare, instantiate and utilize for callbacks
        /// originating from Searching One Tag Operation
        /// </summary>
        /// <param name="rssi">narrow band RSSI</param>
        /// <param name="pc">pc</param>
        /// <param name="epcLength">epc length</param>
        /// <param name="epc">epc pointer</param>
        /// <returns></returns>
#if WIN32
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
        public delegate Int32 TagSearchOneCallbackDelegate
        (
            [In]      float rssi,
            [In]      UInt16 pc,
            [In]      UInt32 epcLength,
            [In]      IntPtr epc,
            [In]      UInt32 ms_ctr

        );
        /// <summary>
        /// Declare, instantiate and utilize for callbacks
        /// originating from Tag Ranging Operation.
        /// </summary>
        /// <param name="crcInvalid">determine whether CRC error</param>
        /// <param name="rssi">narrow band RSSI</param>
        /// <param name="pc">pc</param>
        /// <param name="epcLength">epc length</param>
        /// <param name="epc">epc pointer</param>
        /// <returns></returns>
#if WIN32
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
        public delegate Int32 TagRangingCallbackDelegate
        (
            [In]        bool crcInvalid,
            [In]        float rssi,
            [In]        UInt32 antennaPort,
            [In]        UInt16 pc,
            [In]        UInt32 epcLength,
            [In]        IntPtr epc,
            [In]      UInt32 ms_ctr
        );
#if NOUSE
    public delegate Int32 InventoryCallbackDelegate
    (
        [In]      bool Result,
        [In]      TAG_INV_RECORD_T record
    );

    public delegate Int32 TagAccessCallbackDelegate
    (
        [In]      bool Result,
        [In]      TAG_ACCESS_RECORD_T record
    );
#endif

#if USE_LOADLIBRARY

        //public static IntPtr dllModule = IntPtr.Zero;
        public static string RFID_DLL = "rfid.dll";

        public static Result RFID_LoadLibrary(ref IntPtr dllModule)
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Native)).Location);
            if (dllModule == IntPtr.Zero)
            {
                dllModule = DLLWrapper.LoadLibrary(System.IO.Path.Combine(path, RFID_DLL));
            }
            return dllModule != IntPtr.Zero ? Result.OK : Result.DRIVER_LOAD;
        }

        public static Result RFID_UnloadLibrary(ref IntPtr dllModule)
        {
            if (dllModule != IntPtr.Zero)
            {
                DLLWrapper.FreeLibrary(dllModule);
                dllModule = IntPtr.Zero;
            }
            return Result.OK;
        }


        private delegate Result LL_TCPStartup
        (

            [In]      String IP,
            [In]      UInt32 Port
        );

        private delegate Result LL_Sleep(UInt32 milliseconds);
        public static Result RFID_Sleep(IntPtr dllModule, UInt32 milliseconds)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_Sleep libRun = (LL_Sleep)DLLWrapper.GetFunctionAddress(dllModule, "RFID_Sleep", typeof(LL_Sleep));
                return libRun(milliseconds);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_POWER([In, Out]String target, int HL);
        public static Result RFID_POWER(IntPtr dllModule, [In, Out]String target, int HL)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_POWER libRun = (LL_POWER)DLLWrapper.GetFunctionAddress(dllModule, "RFID_POWER", typeof(LL_POWER));
                return libRun(target, HL);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_SetGPO0([In, Out]String target, int HL);
        public static Result RFID_SetGPO0(IntPtr dllModule, [In, Out]String target, int HL)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_SetGPO0 libRun = (LL_SetGPO0)DLLWrapper.GetFunctionAddress(dllModule, "RFID_SetGPO0", typeof(LL_SetGPO0));
                return libRun(target, HL);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_SetGPO1([In, Out]String target, int HL);
        public static Result RFID_SetGPO1(IntPtr dllModule, [In, Out]String target, int HL)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_SetGPO1 libRun = (LL_SetGPO1)DLLWrapper.GetFunctionAddress(dllModule, "RFID_SetGPO1", typeof(LL_SetGPO1));
                return libRun(target, HL);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_SetLED([In, Out]String target, int HL);
        public static Result RFID_SetLED(IntPtr dllModule, [In, Out]String target, int HL)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_SetLED libRun = (LL_SetLED)DLLWrapper.GetFunctionAddress(dllModule, "RFID_SetLED", typeof(LL_SetLED));
                return libRun(target, HL);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_GetGPO0([In, Out]String target, [In, Out]ref int HL);
        public static Result RFID_GetGPO0(IntPtr dllModule, [In, Out]String target, [In, Out]ref int HL)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_GetGPO0 libRun = (LL_GetGPO0)DLLWrapper.GetFunctionAddress(dllModule, "RFID_GetGPO0", typeof(LL_GetGPO0));
                return libRun(target, ref HL);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_GetGPO1([In, Out]String target, [In, Out]ref int HL);
        public static Result RFID_GetGPO1(IntPtr dllModule, [In, Out]String target, [In, Out]ref int HL)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_GetGPO1 libRun = (LL_GetGPO1)DLLWrapper.GetFunctionAddress(dllModule, "RFID_GetGPO1", typeof(LL_GetGPO1));
                return libRun(target, ref HL);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_GetGPI0([In, Out]String target, [In, Out]ref int HL);
        public static Result RFID_GetGPI0(IntPtr dllModule, [In, Out]String target, [In, Out]ref int HL)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_GetGPI0 libRun = (LL_GetGPI0)DLLWrapper.GetFunctionAddress(dllModule, "RFID_GetGPI0", typeof(LL_GetGPI0));
                return libRun(target, ref HL);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_GetGPI1([In, Out]String target, [In, Out]ref int HL);
        public static Result RFID_GetGPI1(IntPtr dllModule, [In, Out]String target, [In, Out]ref int HL)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_GetGPI1 libRun = (LL_GetGPI1)DLLWrapper.GetFunctionAddress(dllModule, "RFID_GetGPI1", typeof(LL_GetGPI1));
                return libRun(target, ref HL);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_GetLED([In, Out]String target, [In, Out]ref int HL);
        public static Result RFID_GetLED(IntPtr dllModule, [In, Out]String target, [In, Out]ref int HL)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_GetLED libRun = (LL_GetLED)DLLWrapper.GetFunctionAddress(dllModule, "RFID_GetLED", typeof(LL_GetLED));
                return libRun(target, ref HL);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_BTLOADER([In, Out]String target);
        public static Result RFID_BTLOADER(IntPtr dllModule, [In, Out]String target)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_BTLOADER libRun = (LL_BTLOADER)DLLWrapper.GetFunctionAddress(dllModule, "RFID_BTLOADER", typeof(LL_BTLOADER));
                return libRun(target);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_EBOOT([In, Out]String target);
        public static Result RFID_EBOOT(IntPtr dllModule, [In, Out]String target)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_EBOOT libRun = (LL_EBOOT)DLLWrapper.GetFunctionAddress(dllModule, "RFID_EBOOT", typeof(LL_EBOOT));
                return libRun(target);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_AutoReset([In, Out]String target, bool turnOn);
        public static Result RFID_AutoReset(IntPtr dllModule, [In, Out]String target, bool turnOn)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_AutoReset libRun = (LL_AutoReset)DLLWrapper.GetFunctionAddress(dllModule, "RFID_AutoReset", typeof(LL_AutoReset));
                return libRun(target, turnOn);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_GetMACAddress([In, Out]String target, [In, Out]Byte[] mac);
        public static Result RFID_GetMACAddress(IntPtr dllModule, [In, Out]String target, [In, Out]Byte[] mac)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_GetMACAddress libRun = (LL_GetMACAddress)DLLWrapper.GetFunctionAddress(dllModule, "RFID_GetMACAddress", typeof(LL_GetMACAddress));
                return libRun(target, mac);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_GetBTLVersion([In, Out]String target, [In, Out]Byte[] version);
        public static Result RFID_GetBTLVersion(IntPtr dllModule, [In, Out]String target, [In, Out]Byte[] version)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_GetBTLVersion libRun = (LL_GetBTLVersion)DLLWrapper.GetFunctionAddress(dllModule, "RFID_GetBTLVersion", typeof(LL_GetBTLVersion));
                return libRun(target, version);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_GetIMGVersion([In, Out]String target, [In, Out]Byte[] version);
        public static Result RFID_GetIMGVersion(IntPtr dllModule, [In, Out]String target, [In, Out]Byte[] version)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_GetIMGVersion libRun = (LL_GetIMGVersion)DLLWrapper.GetFunctionAddress(dllModule, "RFID_GetIMGVersion", typeof(LL_GetIMGVersion));
                return libRun(target, version);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_StandbyMode([In, Out]String target);
        public static Result RFID_StandbyMode(IntPtr dllModule, [In, Out]String target)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_StandbyMode libRun = (LL_StandbyMode)DLLWrapper.GetFunctionAddress(dllModule, "RFID_StandbyMode", typeof(LL_StandbyMode));
                return libRun(target);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_CheckStatus([In, Out]String target, [In, Out] ref DEVICE_STATUS status);
        public static Result RFID_CheckStatus(IntPtr dllModule, [In, Out]String target, [In, Out] ref DEVICE_STATUS status)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_CheckStatus libRun = (LL_CheckStatus)DLLWrapper.GetFunctionAddress(dllModule, "RFID_CheckStatus", typeof(LL_CheckStatus));
                return libRun(target, ref status);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_ForceReset([In, Out]String target);
        public static Result RFID_ForceReset(IntPtr dllModule, [In, Out]String target)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_ForceReset libRun = (LL_ForceReset)DLLWrapper.GetFunctionAddress(dllModule, "RFID_ForceReset", typeof(LL_ForceReset));
                return libRun(target);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_UDPKeepAliveOn([In, Out]String target);
        public static Result RFID_UDPKeepAliveOn(IntPtr dllModule, [In, Out]String target)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_UDPKeepAliveOn libRun = (LL_UDPKeepAliveOn)DLLWrapper.GetFunctionAddress(dllModule, "RFID_UDPKeepAliveOn", typeof(LL_UDPKeepAliveOn));
                return libRun(target);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_UDPKeepAliveOff([In, Out]String target);
        public static Result RFID_UDPKeepAliveOff(IntPtr dllModule, [In, Out]String target)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_UDPKeepAliveOff libRun = (LL_UDPKeepAliveOff)DLLWrapper.GetFunctionAddress(dllModule, "RFID_UDPKeepAliveOff", typeof(LL_UDPKeepAliveOff));
                return libRun(target);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_CrcFilterOn([In, Out]String target);
        public static Result RFID_CrcFilterOn(IntPtr dllModule, [In, Out]String target)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_CrcFilterOn libRun = (LL_CrcFilterOn)DLLWrapper.GetFunctionAddress(dllModule, "RFID_CrcFilterOn", typeof(LL_CrcFilterOn));
                return libRun(target);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_CrcFilterOff([In, Out]String target);
        public static Result RFID_CrcFilterOff(IntPtr dllModule, [In, Out]String target)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_CrcFilterOff libRun = (LL_CrcFilterOff)DLLWrapper.GetFunctionAddress(dllModule, "RFID_CrcFilterOff", typeof(LL_CrcFilterOff));
                return libRun(target);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_Startup
        (
            [Out]   out RfidHandle pHandle,
            [In, Out]   LibraryVersion pVersion,
            [In]        LibraryMode mode,
            [In]        UInt64 ip,
            [In]        UInt32 port,
            [In]        UInt32 timeout
        );
        public static Result RFID_Startup
        (
            [In]        IntPtr dllModule,
            [Out]   out RfidHandle pHandle,
            [In, Out]   LibraryVersion pVersion,
            [In]        LibraryMode mode,
            [In]        UInt64 ip,
            [In]        UInt32 port,
            [In]        UInt32 timeout)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_Startup libRun = (LL_Startup)DLLWrapper.GetFunctionAddress(dllModule, "RFID_Startup", typeof(LL_Startup));
                return libRun(out pHandle, pVersion, mode, ip, port, timeout);
            }
            else
            {
                pHandle = new RfidHandle();
                return Result.NOT_INITIALIZED;
            }

        }

        private delegate Result LL_Shutdown([In] RfidHandle pHandle);
        public static Result RFID_Shutdown(
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle)
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_Shutdown libRun = (LL_Shutdown)DLLWrapper.GetFunctionAddress(dllModule, "RFID_Shutdown", typeof(LL_Shutdown));
                return libRun(pHandle);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_RetrieveAttachedRadiosList
        (
            [In] RfidHandle pHandle,
            [In] IntPtr pRadioEnum,
            [In] UInt32 flags
        );
        public static Result RFID_RetrieveAttachedRadiosList
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr pRadioEnum,
            [In] UInt32 flags
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RetrieveAttachedRadiosList libRun = (LL_RetrieveAttachedRadiosList)DLLWrapper.GetFunctionAddress(dllModule, "RFID_RetrieveAttachedRadiosList", typeof(LL_RetrieveAttachedRadiosList));
                return libRun(pHandle, pRadioEnum, flags);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        delegate Result LL_RadioOpen
        (
            [In] RfidHandle pHandle,
            [In]          UInt32 cookie,
            [In]          MacMode mode
        );
        public static Result RFID_RadioOpen
                (
                    [In]        IntPtr dllModule,
                    [In]        RfidHandle pHandle,
                    [In]        UInt32 cookie,
                    [In]        MacMode mode
                )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioOpen libRun = (LL_RadioOpen)DLLWrapper.GetFunctionAddress(dllModule, "RFID_RadioOpen", typeof(LL_RadioOpen));
                return libRun(pHandle, cookie, mode);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_RadioClose
        (
            [In] RfidHandle pHandle
        );
        public static Result RFID_RadioClose
        (
                    [In]        IntPtr dllModule,
            [In] RfidHandle pHandle
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioClose libRun = (LL_RadioClose)DLLWrapper.GetFunctionAddress(dllModule, "RFID_RadioClose", typeof(LL_RadioClose));
                return libRun(pHandle);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_RadioSetConfigurationParameter
        (
            [In] RfidHandle pHandle,
            [In] UInt16 param,
            [In] UInt32 val
        );
        public static Result RFID_RadioSetConfigurationParameter
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] UInt16 param,
            [In] UInt32 val
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioSetConfigurationParameter libRun = (LL_RadioSetConfigurationParameter)DLLWrapper.GetFunctionAddress(dllModule, "RFID_RadioSetConfigurationParameter", typeof(LL_RadioSetConfigurationParameter));
                return libRun(pHandle, param, val);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_RadioGetConfigurationParameter
        (
            [In]          RfidHandle pHandle,
            [In]          UInt16 param,
            [In, Out] ref UInt32 val
        );
        public static Result RFID_RadioGetConfigurationParameter
        (
            [In]        IntPtr dllModule,
            [In]          RfidHandle pHandle,
            [In]          UInt16 param,
            [In, Out] ref UInt32 val
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioGetConfigurationParameter libRun = (LL_RadioGetConfigurationParameter)DLLWrapper.GetFunctionAddress(dllModule, "RFID_RadioGetConfigurationParameter", typeof(LL_RadioGetConfigurationParameter));
                return libRun(pHandle, param, ref val);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_RadioSetOperationMode
        (
            [In] RfidHandle pHandle,
            [In] RadioOperationMode mode
        );

        public static Result RFID_RadioSetOperationMode
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] RadioOperationMode mode
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioSetOperationMode libRun = (LL_RadioSetOperationMode)DLLWrapper.GetFunctionAddress(dllModule, "RFID_RadioSetOperationMode", typeof(LL_RadioSetOperationMode));
                return libRun(pHandle, mode);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_RadioGetOperationMode
        (
            [In]          RfidHandle pHandle,
            [In, Out] ref RadioOperationMode mode
        );

        public static Result RFID_RadioGetOperationMode
        (
            [In]        IntPtr dllModule,
            [In]          RfidHandle pHandle,
            [In, Out] ref RadioOperationMode mode
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioGetOperationMode libRun = (LL_RadioGetOperationMode)DLLWrapper.GetFunctionAddress(dllModule, "RFID_RadioGetOperationMode", typeof(LL_RadioGetOperationMode));
                return libRun(pHandle, ref mode);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_RadioSetPowerState
        (
            [In]          RfidHandle pHandle,
            [In] RadioPowerState state
        );
        public static Result RFID_RadioSetPowerState
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] RadioPowerState state
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioSetPowerState libRun = (LL_RadioSetPowerState)DLLWrapper.GetFunctionAddress(dllModule, "RFID_RadioSetPowerState", typeof(LL_RadioSetPowerState));
                return libRun(pHandle, state);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_RadioGetPowerState
        (
            [In]          RfidHandle pHandle,
            [In, Out] ref RadioPowerState state
        );
        public static Result RFID_RadioGetPowerState
        (
            [In]        IntPtr dllModule,
            [In]          RfidHandle pHandle,
            [In, Out] ref RadioPowerState state
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioGetPowerState libRun = (LL_RadioGetPowerState)DLLWrapper.GetFunctionAddress(dllModule, "RFID_RadioGetPowerState", typeof(LL_RadioGetPowerState));
                return libRun(pHandle, ref state);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_RadioSetCurrentLinkProfile
        (
            [In] RfidHandle pHandle,
            [In] UInt32 profile
        );
        public static Result RFID_RadioSetCurrentLinkProfile
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] UInt32 profile
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioSetCurrentLinkProfile libRun = (LL_RadioSetCurrentLinkProfile)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioSetCurrentLinkProfile", typeof(LL_RadioSetCurrentLinkProfile));
                return libRun(pHandle, profile);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_RadioTurnCarrierWaveOn
        (
            [In] RfidHandle pHandle
        );
        public static Result RFID_RadioTurnCarrierWaveOn
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioTurnCarrierWaveOn libRun = (LL_RadioTurnCarrierWaveOn)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioTurnCarrierWaveOn", typeof(LL_RadioTurnCarrierWaveOn));
                return libRun(pHandle);
            }
            else
                return Result.NOT_INITIALIZED;
        }



        private delegate Result LL_RadioTurnCarrierWaveOff
        (
            [In] RfidHandle pHandle
        );
        public static Result RFID_RadioTurnCarrierWaveOff
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioTurnCarrierWaveOff libRun = (LL_RadioTurnCarrierWaveOff)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioTurnCarrierWaveOff", typeof(LL_RadioTurnCarrierWaveOff));
                return libRun(pHandle);
            }
            else
                return Result.NOT_INITIALIZED;

        }


        private delegate Result LL_RadioGetCurrentLinkProfile
        (
            [In]          RfidHandle pHandle,
            [In, Out] ref UInt32 profile
        );
        public static Result RFID_RadioGetCurrentLinkProfile
        (
            [In]        IntPtr dllModule,
            [In]          RfidHandle pHandle,
            [In, Out] ref UInt32 profile
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioGetCurrentLinkProfile libRun = (LL_RadioGetCurrentLinkProfile)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioGetCurrentLinkProfile", typeof(LL_RadioGetCurrentLinkProfile));
                return libRun(pHandle, ref profile);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_RadioGetLinkProfile
        (
            [In]      RfidHandle pHandle,
            [In]      UInt32 num,
            [In, Out] IntPtr profile
        );

        public static Result RFID_RadioGetLinkProfile
        (
            [In]        IntPtr dllModule,
            [In]      RfidHandle pHandle,
            [In]      UInt32 num,
            [In, Out] IntPtr profile
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioGetLinkProfile libRun = (LL_RadioGetLinkProfile)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioGetLinkProfile", typeof(LL_RadioGetLinkProfile));
                return libRun(pHandle, num, profile);
            }
            else
                return Result.NOT_INITIALIZED;

        }


        private delegate Result LL_RadioWriteLinkProfileRegister
        (
            [In]          RfidHandle pHandle,
            [In] UInt32 profile,
            [In] UInt16 address,
            [In] UInt16 value
        );
        public static Result RFID_RadioWriteLinkProfileRegister
                (
                    [In]        IntPtr dllModule,
                    [In] RfidHandle pHandle,
                    [In] UInt32 profile,
                    [In] UInt16 address,
                    [In] UInt16 value
                )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioWriteLinkProfileRegister libRun = (LL_RadioWriteLinkProfileRegister)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioWriteLinkProfileRegister", typeof(LL_RadioWriteLinkProfileRegister));
                return libRun(pHandle, profile, address, value);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_RadioReadLinkProfileRegister
        (
            [In]          RfidHandle pHandle,
            [In]          UInt32 profile,
            [In]          UInt16 address,
            [In, Out] ref UInt16 value
        );
        public static Result RFID_RadioReadLinkProfileRegister
        (
            [In]        IntPtr dllModule,
            [In]          RfidHandle pHandle,
            [In]          UInt32 profile,
            [In]          UInt16 address,
            [In, Out] ref UInt16 value
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioReadLinkProfileRegister libRun = (LL_RadioReadLinkProfileRegister)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioReadLinkProfileRegister", typeof(LL_RadioReadLinkProfileRegister));
                return libRun(pHandle, profile, address, ref value);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_AntennaPortGetStatus
        (
            [In]      RfidHandle pHandle,
            [In]      UInt32 port,
            [In, Out] AntennaPortStatus status
        );
        public static Result RFID_AntennaPortGetStatus
        (
            [In]        IntPtr dllModule,
            [In]      RfidHandle pHandle,
            [In]      UInt32 port,
            [In, Out] AntennaPortStatus status
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_AntennaPortGetStatus libRun = (LL_AntennaPortGetStatus)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_AntennaPortGetStatus", typeof(LL_AntennaPortGetStatus));
                return libRun(pHandle, port, status);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_AntennaPortSetState
        (
            [In]          RfidHandle pHandle,
            [In] UInt32 port,
            [In] AntennaPortState state
        );
        public static Result RFID_AntennaPortSetState
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] UInt32 port,
            [In] AntennaPortState state
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_AntennaPortSetState libRun = (LL_AntennaPortSetState)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_AntennaPortSetState", typeof(LL_AntennaPortSetState));
                return libRun(pHandle, port, state);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_AntennaPortSetConfiguration
        (
            [In]          RfidHandle pHandle,
            [In] UInt32 port,
            [In] AntennaPortConfig config
        );
        public static Result RFID_AntennaPortSetConfiguration
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] UInt32 port,
            [In] AntennaPortConfig config
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_AntennaPortSetConfiguration libRun = (LL_AntennaPortSetConfiguration)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_AntennaPortSetConfiguration", typeof(LL_AntennaPortSetConfiguration));
                return libRun(pHandle, port, config);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_AntennaPortGetConfiguration
        (
            [In]          RfidHandle pHandle,
            [In]      UInt32 port,
            [In, Out] AntennaPortConfig config
        );
        public static Result RFID_AntennaPortGetConfiguration
        (
            [In]        IntPtr dllModule,
            [In]      RfidHandle pHandle,
            [In]      UInt32 port,
            [In, Out] AntennaPortConfig config
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_AntennaPortGetConfiguration libRun = (LL_AntennaPortGetConfiguration)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_AntennaPortGetConfiguration", typeof(LL_AntennaPortGetConfiguration));
                return libRun(pHandle, port, config);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_18K6CSetSelectCriteria
        (
            [In]          RfidHandle pHandle,
            [In] IntPtr pCriteria,
            [In] UInt32 flags
        );
        public static Result RFID_18K6CSetSelectCriteria
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr pCriteria,
            [In] UInt32 flags
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CSetSelectCriteria libRun = (LL_18K6CSetSelectCriteria)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CSetSelectCriteria", typeof(LL_18K6CSetSelectCriteria));
                return libRun(pHandle, pCriteria, flags);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_18K6CGetSelectCriteria
        (
            [In] RfidHandle pHandle,
           [In, Out] IntPtr pCriteria
       );
        public static Result RFID_18K6CGetSelectCriteria
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In, Out] IntPtr pCriteria
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CGetSelectCriteria libRun = (LL_18K6CGetSelectCriteria)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CGetSelectCriteria", typeof(LL_18K6CGetSelectCriteria));
                return libRun(pHandle, pCriteria);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_18K6CSetPostMatchCriteria
        (
            [In] RfidHandle pHandle,
            [In] IntPtr pCriteria,
            [In] UInt32 flags
        );
        public static Result RFID_18K6CSetPostMatchCriteria
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr pCriteria,
            [In] UInt32 flags
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CSetPostMatchCriteria libRun = (LL_18K6CSetPostMatchCriteria)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CSetPostMatchCriteria", typeof(LL_18K6CSetPostMatchCriteria));
                return libRun(pHandle, pCriteria, flags);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_18K6CGetPostMatchCriteria
        (
            [In]          RfidHandle pHandle,
            [In, Out] IntPtr pCriteria
        );

        public static Result RFID_18K6CGetPostMatchCriteria
        (
            [In]        IntPtr dllModule,
            [In]      RfidHandle pHandle,
            [In, Out] IntPtr pCriteria
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CGetPostMatchCriteria libRun = (LL_18K6CGetPostMatchCriteria)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CGetPostMatchCriteria", typeof(LL_18K6CGetPostMatchCriteria));
                return libRun(pHandle, pCriteria);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_18K6CSetQueryTagGroup
        (
            [In]          RfidHandle pHandle,
           [In] TagGroup group
        );
        public static Result RFID_18K6CSetQueryTagGroup
        (
           [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
           [In] TagGroup group
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CSetQueryTagGroup libRun = (LL_18K6CSetQueryTagGroup)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CSetQueryTagGroup", typeof(LL_18K6CSetQueryTagGroup));
                return libRun(pHandle, group);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_18K6CGetQueryTagGroup
        (
            [In]          RfidHandle pHandle,
            [In, Out] TagGroup pGroup
        );
        public static Result RFID_18K6CGetQueryTagGroup
        (
            [In]        IntPtr dllModule,
            [In]      RfidHandle pHandle,
            [In, Out] TagGroup pGroup
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CGetQueryTagGroup libRun = (LL_18K6CGetQueryTagGroup)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CGetQueryTagGroup", typeof(LL_18K6CGetQueryTagGroup));
                return libRun(pHandle, pGroup);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_18K6CSetCurrentSingulationAlgorithm
        (
            [In]          RfidHandle pHandle,
           [In] SingulationAlgorithm algorithm
        );

        public static Result RFID_18K6CSetCurrentSingulationAlgorithm
       (
          [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
          [In] SingulationAlgorithm algorithm
       )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CSetCurrentSingulationAlgorithm libRun = (LL_18K6CSetCurrentSingulationAlgorithm)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CSetCurrentSingulationAlgorithm", typeof(LL_18K6CSetCurrentSingulationAlgorithm));
                return libRun(pHandle, algorithm);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_18K6CGetCurrentSingulationAlgorithm
        (
            [In]          RfidHandle pHandle,
           [In, Out] ref SingulationAlgorithm algorithm
        );

        public static Result RFID_18K6CGetCurrentSingulationAlgorithm
        (
           [In]        IntPtr dllModule,
            [In]          RfidHandle pHandle,
           [In, Out] ref SingulationAlgorithm algorithm
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CGetCurrentSingulationAlgorithm libRun = (LL_18K6CGetCurrentSingulationAlgorithm)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CGetCurrentSingulationAlgorithm", typeof(LL_18K6CGetCurrentSingulationAlgorithm));
                return libRun(pHandle, ref algorithm);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_18K6CSetSingulationAlgorithmParameters
        (
            [In]          RfidHandle pHandle,
           [In] SingulationAlgorithm algorithm,
           [In] IntPtr pParms
        );
        public static Result RFID_18K6CSetSingulationAlgorithmParameters
        (
           [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
           [In] SingulationAlgorithm algorithm,
           [In] IntPtr pParms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CSetSingulationAlgorithmParameters libRun = (LL_18K6CSetSingulationAlgorithmParameters)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CSetSingulationAlgorithmParameters", typeof(LL_18K6CSetSingulationAlgorithmParameters));
                return libRun(pHandle, algorithm, pParms);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_18K6CGetSingulationAlgorithmParameters
        (
            [In]          RfidHandle pHandle,
           [In]      SingulationAlgorithm algorithm,
           [In, Out] IntPtr pParms
        );
        public static Result RFID_18K6CGetSingulationAlgorithmParameters
        (
           [In]        IntPtr dllModule,
            [In]      RfidHandle pHandle,
           [In]      SingulationAlgorithm algorithm,
           [In, Out] IntPtr pParms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CGetSingulationAlgorithmParameters libRun = (LL_18K6CGetSingulationAlgorithmParameters)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CGetSingulationAlgorithmParameters", typeof(LL_18K6CGetSingulationAlgorithmParameters));
                return libRun(pHandle, algorithm, pParms);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        //!! RFID_18K6CGetQueryParameters
        //!! is deprecated and does not
        //!! have a C-Sharp wrapper

        private delegate Result LL_18K6CTagInventory
        (
            [In]          RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );
        public static Result RFID_18K6CTagInventory
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagInventory libRun = (LL_18K6CTagInventory)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagInventory", typeof(LL_18K6CTagInventory));
                return libRun(pHandle, parms, flags);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagSearchAny
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagSearchAny
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagSearchAny libRun = (LL_18K6CTagSearchAny)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagSearchAny", typeof(LL_18K6CTagSearchAny));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_18K6CTagSearchOne
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagSearchOne
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagSearchOne libRun = (LL_18K6CTagSearchOne)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagSearchOne", typeof(LL_18K6CTagSearchOne));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_18K6CTagRanging
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagRanging
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagRanging libRun = (LL_18K6CTagRanging)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagRanging", typeof(LL_18K6CTagRanging));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_18K6CTagRead
        (
            [In]          RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );
        public static Result RFID_18K6CTagRead
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagRead libRun = (LL_18K6CTagRead)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagRead", typeof(LL_18K6CTagRead));
                return libRun(pHandle, parms, flags);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagReadEPC
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagReadEPC
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagReadEPC libRun = (LL_18K6CTagReadEPC)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagReadEPC", typeof(LL_18K6CTagReadEPC));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagReadPC
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagReadPC
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagReadPC libRun = (LL_18K6CTagReadPC)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagReadPC", typeof(LL_18K6CTagReadPC));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagReadTID
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagReadTID
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagReadTID libRun = (LL_18K6CTagReadTID)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagReadTID", typeof(LL_18K6CTagReadTID));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagReadAccPwd
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagReadAccPwd
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagReadAccPwd libRun = (LL_18K6CTagReadAccPwd)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagReadAccPwd", typeof(LL_18K6CTagReadAccPwd));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagReadKillPwd
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagReadKillPwd
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagReadKillPwd libRun = (LL_18K6CTagReadKillPwd)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagReadKillPwd", typeof(LL_18K6CTagReadKillPwd));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagReadUser
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagReadUser
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagReadUser libRun = (LL_18K6CTagReadUser)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagReadUser", typeof(LL_18K6CTagReadUser));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }

#if Library1300
#if WIN32
        [DllImport("rfid.dll")]
#elif WindowsCE
        [DllImport( "rfid.dll")]
#endif
        public static extern Result RFID_18K6CTagBlockWrite
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );
#endif

        private delegate Result LL_18K6CTagWrite
        (
            [In]          RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );

        public static Result RFID_18K6CTagWrite
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagWrite libRun = (LL_18K6CTagWrite)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagWrite", typeof(LL_18K6CTagWrite));
                return libRun(pHandle, parms, flags);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_18K6CTagWriteEPC
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagWriteEPC
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagWriteEPC libRun = (LL_18K6CTagWriteEPC)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagWriteEPC", typeof(LL_18K6CTagWriteEPC));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagWritePC
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagWritePC
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagWritePC libRun = (LL_18K6CTagWritePC)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagWritePC", typeof(LL_18K6CTagWritePC));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagWriteAccPwd
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagWriteAccPwd
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagWriteAccPwd libRun = (LL_18K6CTagWriteAccPwd)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagWriteAccPwd", typeof(LL_18K6CTagWriteAccPwd));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagWriteKillPwd
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagWriteKillPwd
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagWriteKillPwd libRun = (LL_18K6CTagWriteKillPwd)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagWriteKillPwd", typeof(LL_18K6CTagWriteKillPwd));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagWriteUser
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagWriteUser
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagWriteUser libRun = (LL_18K6CTagWriteUser)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagWriteUser", typeof(LL_18K6CTagWriteUser));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_18K6CTagKill
        (
            [In]          RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );
        public static Result RFID_18K6CTagKill
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagKill libRun = (LL_18K6CTagKill)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagKill", typeof(LL_18K6CTagKill));
                return libRun(pHandle, parms, flags);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagRawKill
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagRawKill
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagRawKill libRun = (LL_18K6CTagRawKill)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagRawKill", typeof(LL_18K6CTagRawKill));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagLock
        (
            [In]          RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );
        public static Result RFID_18K6CTagLock
                (
                    [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
                    [In] IntPtr parms,
                    [In] UInt32 flags
                )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagLock libRun = (LL_18K6CTagLock)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagLock", typeof(LL_18K6CTagLock));
                return libRun(pHandle, parms, flags);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_18K6CTagRawLock
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagRawLock
                (
                    [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
                    [In] IntPtr parms
                )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagRawLock libRun = (LL_18K6CTagRawLock)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagRawLock", typeof(LL_18K6CTagRawLock));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_18K6CTagBlockPermalock
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        public static Result RFID_18K6CTagBlockPermalock
                (
                    [In]        IntPtr dllModule,
                    [In] RfidHandle pHandle,
                    [In] IntPtr parms
                )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_18K6CTagBlockPermalock libRun = (LL_18K6CTagBlockPermalock)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_18K6CTagBlockPermalock", typeof(LL_18K6CTagBlockPermalock));
                return libRun(pHandle, parms);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_RadioCancelOperation
        (
            [In]          RfidHandle pHandle,
            [In] UInt32 flags
        );
        public static Result RFID_RadioCancelOperation
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] UInt32 flags
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioCancelOperation libRun = (LL_RadioCancelOperation)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioCancelOperation", typeof(LL_RadioCancelOperation));
                return libRun(pHandle, flags);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_RadioAbortOperation
        (
            [In]          RfidHandle pHandle,
            [In] UInt32 flags
        );

        public static Result RFID_RadioAbortOperation
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] UInt32 flags
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioAbortOperation libRun = (LL_RadioAbortOperation)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioAbortOperation", typeof(LL_RadioAbortOperation));
                return libRun(pHandle, flags);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_RadioSetResponseDataMode
        (
            [In]          RfidHandle pHandle,
            [In] ResponseType responseType,
            [In] ResponseMode responseMode
        );
        public static Result RFID_RadioSetResponseDataMode
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] ResponseType responseType,
            [In] ResponseMode responseMode
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioSetResponseDataMode libRun = (LL_RadioSetResponseDataMode)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioSetResponseDataMode", typeof(LL_RadioSetResponseDataMode));
                return libRun(pHandle, responseType, responseMode);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_RadioGetResponseDataMode
        (
            [In]          RfidHandle pHandle,
            [In]          ResponseType responseType,
            [In, Out] ref ResponseMode responseMode
        );
        public static Result RFID_RadioGetResponseDataMode
        (
            [In]        IntPtr dllModule,
            [In]          RfidHandle pHandle,
            [In]          ResponseType responseType,
            [In, Out] ref ResponseMode responseMode
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioGetResponseDataMode libRun = (LL_RadioGetResponseDataMode)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioGetResponseDataMode", typeof(LL_RadioGetResponseDataMode));
                return libRun(pHandle, responseType, ref responseMode);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_MacUpdateNonvolatileMemory
        (
            [In]          RfidHandle pHandle,
            [In] UInt32 countBlocks,
            [In] IntPtr pBlocks,
            [In] UInt32 flags
        );
        public static Result RFID_MacUpdateNonvolatileMemory
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] UInt32 countBlocks,
            [In] IntPtr pBlocks,
            [In] UInt32 flags
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_MacUpdateNonvolatileMemory libRun = (LL_MacUpdateNonvolatileMemory)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_MacUpdateNonvolatileMemory", typeof(LL_MacUpdateNonvolatileMemory));
                return libRun(pHandle, countBlocks, pBlocks, flags);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_MacGetVersion
        (
            [In]      RfidHandle pHandle,
            [In, Out] MacVersion pVersion
        );
        public static Result RFID_MacGetVersion
        (
            [In]        IntPtr dllModule,
            [In]      RfidHandle pHandle,
            [In, Out] MacVersion pVersion
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_MacGetVersion libRun = (LL_MacGetVersion)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_MacGetVersion", typeof(LL_MacGetVersion));
                return libRun(pHandle, pVersion);
            }
            else
                return Result.NOT_INITIALIZED;
        }


        private delegate Result LL_MacReadOemData
        (
            [In]          RfidHandle pHandle,
            [In] UInt32 address,
            [In] UInt32 count,
            [In] IntPtr data
        );
        public static Result RFID_MacReadOemData
       (
           [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
           [In] UInt32 address,
           [In] UInt32 count,
           [In] IntPtr data
       )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_MacReadOemData libRun = (LL_MacReadOemData)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_MacReadOemData", typeof(LL_MacReadOemData));
                return libRun(pHandle, address, count, data);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_MacWriteOemData
        (
            [In]          RfidHandle pHandle,
            [In] UInt32 address,
            [In] UInt32 count,
            [In] IntPtr data
        );
        public static Result RFID_MacWriteOemData
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] UInt32 address,
            [In] UInt32 count,
            [In] IntPtr data
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_MacWriteOemData libRun = (LL_MacWriteOemData)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_MacWriteOemData", typeof(LL_MacWriteOemData));
                return libRun(pHandle, address, count, data);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_MacReset
        (
            [In]          RfidHandle pHandle,
            [In] MacResetType resetType
        );

        public static Result RFID_MacReset
                (
                   [In]        IntPtr dllModule,
             [In] RfidHandle pHandle,
                    [In] MacResetType resetType
                )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_MacReset libRun = (LL_MacReset)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_MacReset", typeof(LL_MacReset));
                return libRun(pHandle, resetType);
            }
            else
                return Result.NOT_INITIALIZED;
        }
        private delegate Result LL_MacClearError
        (
            [In] RfidHandle pHandle
        );
        public static Result RFID_MacClearError
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_MacClearError libRun = (LL_MacClearError)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_MacClearError", typeof(LL_MacClearError));
                return libRun(pHandle);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_MacBypassWriteRegister
        (
            [In]          RfidHandle pHandle,
            [In] UInt16 address,
            [In] UInt16 value
        );
        public static Result RFID_MacBypassWriteRegister
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] UInt16 address,
            [In] UInt16 value
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_MacBypassWriteRegister libRun = (LL_MacBypassWriteRegister)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_MacBypassWriteRegister", typeof(LL_MacBypassWriteRegister));
                return libRun(pHandle, address, value);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_MacBypassReadRegister
        (
            [In]          RfidHandle pHandle,
            [In]     UInt16 address,
            [In] ref UInt16 value
        );
        public static Result RFID_MacBypassReadRegister
        (
            [In]        IntPtr dllModule,
            [In]     RfidHandle pHandle,
            [In]     UInt16 address,
            [In] ref UInt16 value
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_MacBypassReadRegister libRun = (LL_MacBypassReadRegister)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_MacBypassReadRegister", typeof(LL_MacBypassReadRegister));
                return libRun(pHandle, address, ref value);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_MacSetRegion
        (
            [In]          RfidHandle pHandle,
            [In] MacRegion region,
            [In] IntPtr regionConfig
        );
        public static Result RFID_MacSetRegion
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] MacRegion region,
            [In] IntPtr regionConfig
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_MacSetRegion libRun = (LL_MacSetRegion)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_MacSetRegion", typeof(LL_MacSetRegion));
                return libRun(pHandle, region, regionConfig);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_MacGetRegion
        (
            [In]     RfidHandle pHandle,
            [In] ref MacRegion region,
            [In]     IntPtr regionConfig
        );
        public static Result RFID_MacGetRegion
        (
            [In]        IntPtr dllModule,
            [In]     RfidHandle pHandle,
            [In] ref MacRegion region,
            [In]     IntPtr regionConfig
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_MacGetRegion libRun = (LL_MacGetRegion)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_MacGetRegion", typeof(LL_MacGetRegion));
                return libRun(pHandle, ref region, regionConfig);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_RadioSetGpioPinsConfiguration
        (
            [In]          RfidHandle pHandle,
            [In] UInt32 mask,
            [In] UInt32 configuration
        );
        public static Result RFID_RadioSetGpioPinsConfiguration
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] UInt32 mask,
            [In] UInt32 configuration
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioSetGpioPinsConfiguration libRun = (LL_RadioSetGpioPinsConfiguration)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioSetGpioPinsConfiguration", typeof(LL_RadioSetGpioPinsConfiguration));
                return libRun(pHandle, mask, configuration);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_RadioGetGpioPinsConfiguration
        (
            [In]          RfidHandle pHandle,
            [In] ref UInt32 configuration
        );
        public static Result RFID_RadioGetGpioPinsConfiguration
        (
            [In]        IntPtr dllModule,
            [In]     RfidHandle pHandle,
            [In] ref UInt32 configuration
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioGetGpioPinsConfiguration libRun = (LL_RadioGetGpioPinsConfiguration)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioGetGpioPinsConfiguration", typeof(LL_RadioGetGpioPinsConfiguration));
                return libRun(pHandle, ref configuration);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_RadioReadGpioPins
        (
            [In]          RfidHandle pHandle,
            [In]     UInt32 mask,
            [In] ref UInt32 value
        );
        public static Result RFID_RadioReadGpioPins
        (
            [In]        IntPtr dllModule,
            [In]     RfidHandle pHandle,
            [In]     UInt32 mask,
            [In] ref UInt32 value
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioReadGpioPins libRun = (LL_RadioReadGpioPins)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioReadGpioPins", typeof(LL_RadioReadGpioPins));
                return libRun(pHandle, mask, ref value);
            }
            else
                return Result.NOT_INITIALIZED;
        }

        private delegate Result LL_RadioWriteGpioPins
        (
            [In]          RfidHandle pHandle,
            [In] UInt32 mask,
            [In] UInt32 value
        );
        public static Result RFID_RadioWriteGpioPins
        (
            [In]        IntPtr dllModule,
            [In] RfidHandle pHandle,
            [In] UInt32 mask,
            [In] UInt32 value
        )
        {
            if (dllModule != IntPtr.Zero)
            {
                LL_RadioWriteGpioPins libRun = (LL_RadioWriteGpioPins)DLLWrapper.GetFunctionAddress(
                    dllModule, "RFID_RadioWriteGpioPins", typeof(LL_RadioWriteGpioPins));
                return libRun(pHandle, mask, value);
            }
            else
                return Result.NOT_INITIALIZED;
        }
#endif
#if true


#if NET_BUILD
        [DllImport(rfid_dll)]
        public static extern Result RFID_TCPStartup
        (
            [In]      String IP,
            [In]      UInt32 Port
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_TCPShutdown();

        [DllImport(rfid_dll)]
        public static extern Result RFID_TCPReconnect();

        [DllImport(rfid_dll)]
        public static extern Result RFID_TCPConnected();

        [DllImport(rfid_dll)]
        public static extern Result RFID_TCPSend
        (
            [In]      String pData,
            [In]      UInt32 len
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_TCPRecv
        (
            [In, Out]      String pData,
            [In, Out]      UInt32 len
        );
        [DllImport(rfid_dll)]
        public static extern Result RFID_PowerUp();

        [DllImport(rfid_dll)]
        public static extern Result RFID_PowerDown();

        [DllImport(rfid_dll)]
        public static extern Result RFID_Sleep(UInt32 milliseconds);

        [DllImport(rfid_dll)]
        public static extern Result RFID_POWER([In, Out]String target, int HL);
        [DllImport(rfid_dll)]
        public static extern Result RFID_SetGPO0([In, Out]String target, int HL);
        [DllImport(rfid_dll)]
        public static extern Result RFID_SetGPO1([In, Out]String target, int HL);
        [DllImport(rfid_dll)]
        public static extern Result RFID_SetLED([In, Out]String target, int HL);
        [DllImport(rfid_dll)]
        public static extern Result RFID_GetGPO0([In, Out]String target, [In, Out]ref int HL);
        [DllImport(rfid_dll)]
        public static extern Result RFID_GetGPO1([In, Out]String target, [In, Out]ref int HL);
        [DllImport(rfid_dll)]
        public static extern Result RFID_GetGPI0([In, Out]String target, [In, Out]ref int HL);
        [DllImport(rfid_dll)]
        public static extern Result RFID_GetGPI1([In, Out]String target, [In, Out]ref int HL);
        [DllImport(rfid_dll)]
        public static extern Result RFID_GetLED([In, Out]String target, [In, Out]ref int HL);
        [DllImport(rfid_dll)]
        public static extern Result RFID_BTLOADER([In, Out]String target);
        [DllImport(rfid_dll)]
        public static extern Result RFID_EBOOT([In, Out]String target);
        [DllImport(rfid_dll)]
        public static extern Result RFID_GetMACAddress([In] RfidHandle pHandle, [In, Out]Byte[] mac);
        [DllImport(rfid_dll)]
        public static extern Result RFID_GetBTLVersion([In, Out]String target, [In, Out]Byte[] version);
        [DllImport(rfid_dll)]
        public static extern Result RFID_GetIMGVersion([In, Out]String target, [In, Out]Byte[] version);
        [DllImport(rfid_dll)]
        public static extern Result RFID_StandbyMode([In, Out]String target);
        [DllImport(rfid_dll)]
        public static extern Result RFID_AutoReset([In, Out]String target, [In] bool on);
        [DllImport(rfid_dll)]
        public static extern Result RFID_CheckStatus([In, Out]String target, [In, Out] ref DEVICE_STATUS status);
        [DllImport(rfid_dll)]
        public static extern Result RFID_ForceReset([In, Out]String target);
        [DllImport(rfid_dll)]
        public static extern Result RFID_UDPKeepAliveOn([In, Out]String target);
        [DllImport(rfid_dll)]
        public static extern Result RFID_UDPKeepAliveOff([In, Out]String target);
        [DllImport(rfid_dll)]
        public static extern Result RFID_CrcFilterOn([In, Out]String target);
        [DllImport(rfid_dll)]
        public static extern Result RFID_CrcFilterOff([In, Out]String target);
        [DllImport(rfid_dll)]
        public static extern Result RFID_GetGPIStatus([In, Out]String target, ref bool GPI0, ref bool GPI1);
        [DllImport(rfid_dll)]
        public static extern Result RFID_GetGPOStatus([In, Out]String target, ref bool GPO0, ref bool GPO1);
        [DllImport(rfid_dll)]
        public static extern Result RFID_SetGPO0Status([In, Out]String target, bool GPO0);
        [DllImport(rfid_dll)]
        public static extern Result RFID_SetGPO1Status([In, Out]String target, bool GPO1);
        [DllImport(rfid_dll)]
        public static extern Result RFID_SetGPI0Interrupt([In, Out]String target, GPIOTrigger trigger);
        [DllImport(rfid_dll)]
        public static extern Result RFID_SetGPI1Interrupt([In, Out]String target, GPIOTrigger trigger);
        [DllImport(rfid_dll)]
        public static extern Result RFID_GetGPIInterrupt([In, Out]String target, ref GPIOTrigger gpi0Trigger, ref GPIOTrigger gpi1Trigger);
        [DllImport(rfid_dll)]
        public static extern Result RFID_StartPollGPIStatus([In] GPI_INTERRUPT_CALLBACK callback, [In] IntPtr handle);
        [DllImport(rfid_dll)]
        public static extern Result RFID_StopPollGPIStatus([In] IntPtr handle);
#endif
        const string rfid_dll = "rfid.dll";

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
#if USB_BUILD
        public static extern Result RFID_Startup
        (
            [Out]   out RfidHandle pHandle,
            [In, Out] LibraryVersion pVersion,
            [In]      LibraryMode    mode
        );
#elif NET_BUILD
        public static extern Result RFID_Startup
        (
            [Out]   out RfidHandle pHandle,
            [In, Out]   LibraryVersion pVersion,
            [In]        LibraryMode mode,
            [In]        UInt64 ip,
            [In]        UInt32 port,
            [In]        UInt32 timeout
        );
#endif

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_Shutdown([In] RfidHandle pHandle);

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RetrieveAttachedRadiosList
        (
             [In] RfidHandle pHandle,
           [In] IntPtr pRadioEnum,
            [In] UInt32 flags
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioOpen
        (
            [In] RfidHandle pHandle,
            [In]          UInt32 cookie,
            [In]          MacMode mode
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioClose
        (
            [In] RfidHandle pHandle
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_MacWriteRegister
        (
            [In] RfidHandle pHandle,
            [In] UInt16 param,
            [In] UInt32 val
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_MacReadRegister
        (
            [In] RfidHandle pHandle,
            [In]          UInt16 param,
            [In, Out] ref UInt32 val
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioSetOperationMode
        (
            [In] RfidHandle pHandle,
            [In] RadioOperationMode mode
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioGetOperationMode
        (
            [In] RfidHandle pHandle,
            [In, Out] ref RadioOperationMode mode
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioSetPowerState
        (
            [In] RfidHandle pHandle,
            [In] RadioPowerState state
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioGetPowerState
        (
            [In] RfidHandle pHandle,
            [In, Out] ref RadioPowerState state
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioSetCurrentLinkProfile
        (
            [In] RfidHandle pHandle,
            [In] UInt32 profile
        );
#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioTurnCarrierWaveOn
        (
            [In] RfidHandle pHandle
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioTurnCarrierWaveOff
        (
            [In] RfidHandle pHandle
        );



#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioGetCurrentLinkProfile
        (
            [In] RfidHandle pHandle,
            [In, Out] ref UInt32 profile
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioGetLinkProfile
        (
            [In] RfidHandle pHandle,
            [In]      UInt32 num,
            [In, Out] IntPtr profile
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioWriteLinkProfileRegister
        (
            [In] RfidHandle pHandle,
            [In] UInt32 profile,
            [In] UInt16 address,
            [In] UInt16 value
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioReadLinkProfileRegister
        (
            [In] RfidHandle pHandle,
            [In]          UInt32 profile,
            [In]          UInt16 address,
            [In, Out] ref UInt16 value
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_AntennaPortGetStatus
        (
            [In] RfidHandle pHandle,
            [In]      UInt32 port,
            [In, Out] AntennaPortStatus status
        );
//#if CS468
        [DllImport(rfid_dll)]
        public static extern Result RFID_AntennaPortSetStatus(
            [In] RfidHandle pHandle,
            [In]      UInt32 port,
            [In, Out] AntennaPortStatus status
	    );
//#endif
        [DllImport(rfid_dll)]
        public static extern Result RFID_AntennaPortSetState
        (
            [In] RfidHandle pHandle,
            [In] UInt32 port,
            [In] AntennaPortState state
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_AntennaPortSetConfiguration
        (
            [In] RfidHandle pHandle,
            [In] UInt32 port,
            [In] AntennaPortConfig config
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_AntennaPortGetConfiguration
        (
            [In] RfidHandle pHandle,
            [In]      UInt32 port,
            [In, Out] AntennaPortConfig config
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CSetSelectCriteria
        (
            [In] RfidHandle pHandle,
            [In] IntPtr pCriteria,
            [In] UInt32 flags
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CGetSelectCriteria
        (
            [In] RfidHandle pHandle,
            [In, Out] IntPtr pCriteria
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CSetPostMatchCriteria
        (
            [In] RfidHandle pHandle,
            [In] IntPtr pCriteria,
            [In] UInt32 flags
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CGetPostMatchCriteria
        (
            [In] RfidHandle pHandle,
            [In, Out] IntPtr pCriteria
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CSetQueryTagGroup
        (
            [In] RfidHandle pHandle,
           [In] TagGroup group
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CGetQueryTagGroup
        (
            [In] RfidHandle pHandle,
            [In, Out] TagGroup pGroup
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CSetCurrentSingulationAlgorithm
        (
            [In] RfidHandle pHandle,
           [In] SingulationAlgorithm algorithm
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CGetCurrentSingulationAlgorithm
        (
            [In] RfidHandle pHandle,
           [In, Out] ref SingulationAlgorithm algorithm
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CSetSingulationAlgorithmParameters
        (
            [In] RfidHandle pHandle,
           [In] SingulationAlgorithm algorithm,
           [In] IntPtr pParms
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CGetSingulationAlgorithmParameters
        (
            [In] RfidHandle pHandle,
            [In]      SingulationAlgorithm algorithm,
            [In, Out] IntPtr pParms
        );

        //!! RFID_18K6CGetQueryParameters
        //!! is deprecated and does not
        //!! have a C-Sharp wrapper

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CTagInventory
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CTagRead
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );
#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CTagBlockWrite
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );
#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport(rfid_dll)]
#endif
        public static extern Result RFID_18K6CTagBlockErase
        (
            [In] RfidHandle pHandle,
           [In] IntPtr parms,
            [In] UInt32 flags
        );
#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport(rfid_dll)]
#endif
        public static extern Result RFID_18K6CTagPermaLock
        (
            [In] RfidHandle pHandle,
           [In] IntPtr parms,
            [In] UInt32 flags
        );
#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CTagWrite
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CTagKill
        (
            [In] RfidHandle pHandle,
           [In] IntPtr parms,
            [In] UInt32 flags
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CTagLock
        (
            [In] RfidHandle pHandle,
           [In] IntPtr parms,
            [In] UInt32 flags
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_18K6CTagBlockPermalock
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagReadProtect
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );
        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagResetReadProtect
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );
        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagEASConfig
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );
        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagEASAlarm
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms,
            [In] UInt32 flags
        );  
#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioCancelOperation
        (
            [In] RfidHandle pHandle,
            [In] UInt32 flags
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioAbortOperation
        (
            [In] RfidHandle pHandle,
            [In] UInt32 flags
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioSetResponseDataMode
        (
            [In] RfidHandle pHandle,
            [In] ResponseType responseType,
            [In] ResponseMode responseMode
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioGetResponseDataMode
        (
            [In] RfidHandle pHandle,
            [In]          ResponseType responseType,
            [In, Out] ref ResponseMode responseMode
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_MacUpdateNonvolatileMemory
        (
            [In] RfidHandle pHandle,
            [In] UInt32 countBlocks,
            [In] IntPtr pBlocks,
            [In] FwUpdateFlags flags
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_MacGetVersion
        (
            [In] RfidHandle pHandle,
            [In, Out] MacVersion pVersion
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public  static extern Result RFID_MacReadOemData
        (
            [In] RfidHandle pHandle,
            [In] UInt32 address,
            [In] UInt32 count,
            [In] UInt32* data
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public  static extern Result RFID_MacWriteOemData
        (
            [In] RfidHandle pHandle,
            [In] UInt32 address,
            [In] UInt32 count,
            [In] UInt32* data
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_MacReset
        (
            [In] RfidHandle pHandle,
            [In] MacResetType resetType
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_MacClearError
        (
            [In] RfidHandle pHandle
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_MacBypassWriteRegister
        (
            [In] RfidHandle pHandle,
            [In] UInt16 address,
            [In] UInt16 value
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_MacBypassReadRegister
        (
            [In] RfidHandle pHandle,
            [In]     UInt16 address,
            [In] ref UInt16 value
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_MacSetRegion
        (
            [In] RfidHandle pHandle,
            [In] MacRegion region,
            [In] IntPtr regionConfig
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_MacGetRegion
        (
            [In] RfidHandle pHandle,
            [In] ref MacRegion region,
            [In]     IntPtr regionConfig
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioSetGpioPinsConfiguration
        (
            [In] RfidHandle pHandle,
            [In] UInt32 mask,
            [In] UInt32 configuration
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioGetGpioPinsConfiguration
        (
            [In] RfidHandle pHandle,
            [In] ref UInt32 configuration
        );

#if WIN32
        [DllImport(rfid_dll)]
#elif WindowsCE
        [DllImport( rfid_dll)]
#endif
        public static extern Result RFID_RadioReadGpioPins
        (
            [In] RfidHandle pHandle,
            [In]     UInt32 mask,
            [In] ref UInt32 value
        );


        [DllImport( rfid_dll)]
        public static extern Result RFID_RadioWriteGpioPins
        (
            [In] RfidHandle pHandle,
            [In] UInt32 mask,
            [In] UInt32 value
        );

        #region WaterYu @ 16-Sept-2009

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagWriteEPC
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagWritePC
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagWriteAccPwd
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagWriteKillPwd
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagWriteUser
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagReadEPC
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagReadPC
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagReadAccPwd
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagReadKillPwd
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagReadUser
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagReadTID
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagRawLock
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagRawKill
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagSelected
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagSearchAny
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagSearchOne
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CTagRanging
        (
            [In] RfidHandle pHandle,
            [In] IntPtr parms
        );
   
        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CCustTagReadProtect
        (
            [In] RfidHandle pHandle,
            [In] InternalCustCmdTagReadProtectParms parms,
            [In] UInt32 flags
        );

      
        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CCustTagResetReadProtect
        (
            [In] RfidHandle pHandle,
            [In] InternalCustCmdTagReadProtectParms parms,
            [In] UInt32 flags
        );

  
        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CCustTagEASConfig
        (
            [In] RfidHandle pHandle,
            [In] InternalCustCmdEASParms parms,
            [In] UInt32 flags
        );


        [DllImport(rfid_dll)]
        public static extern Result RFID_18K6CCustTagEASAlarm
        (
            [In] RfidHandle pHandle,
            [In] InternalCustCmdEASParms parms,
            [In] UInt32 flags
        );
        #endregion

        [DllImport( rfid_dll)]
        public static extern Result RFID_RadioCLSetPassword
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1,
            [In] UInt32 reg2
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLSetLogMode
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLSetLogLimits
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1,
            [In] UInt32 reg2
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLGetMeasurementSetup
        (
            [In] RfidHandle pHandle,
            [In, Out] byte [] pParms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLSetSFEParameters
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLSetCalibrationData
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1,
            [In] UInt32 reg2
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLEndLog
        (
            [In] RfidHandle pHandle
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLStartLog
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLGetLogState
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1,
            [In, Out] byte [] Parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLGetCalibrationData
        (
            [In] RfidHandle pHandle,
            [In, Out] byte [] pParms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLGetBatteryLevel
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1,
            [In, Out] byte [] Parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLSetShelfLife
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1, 
            [In] UInt32 reg2
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLInitialize
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLGetSensorValue
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1,
            [In, Out] byte [] Parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLOpenArea
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1,
            [In] UInt32 reg2
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioCLAccessFifo
        (
            [In] RfidHandle pHandle,
            [In] UInt32 reg1, 
            [In] UInt32 reg2,
            [In] UInt32 reg3,
            [In, Out] byte [] Parms
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioG2X_Change_EAS
        (
            [In] RfidHandle pHandle
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioG2X_EAS_Alarm
        (
            [In] RfidHandle pHandle
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioG2X_ChangeConfig
        (
            [In] RfidHandle pHandle
        );

        [DllImport(rfid_dll)]
        public static extern Result RFID_RadioQT_Command
        (
            [In] RfidHandle pHandle,
            [In] int RW,
            [In] int TP,
            [In] int SR,
            [In] int MEM
        );

#if NOUSE

        [DllImport("rfidmx.dll")]
        public static extern Result rfidmx_Initialize
        (
            [In] IntPtr hWnd
        );

        [DllImport("rfidmx.dll")]
        public static extern Result rfidmx_Uninitialize();

        [DllImport("rfidmx.dll")]
        public static extern Result rfidmx_PostMessage
        (
            [In] RFID_OPERATION operation,
            [In] IntPtr parms
        );

        [DllImport("rfidmx.dll")]
        public static extern void DeleteUnmanagedPtr
        (
            [In] IntPtr ptr
        );
        [DllImport("rfidmx.dll")]
        public static extern void LocalFreeUnmanaged
        (
            [In] IntPtr ptr
        );
#endif
#endif
    }  // Functions class END

#if __NOT_USED__
    sealed class DLLWrapper
    {
        //public static readonly uint LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040;
        ///<summary>
        /// API LoadLibrary
        ///</summary>
        [DllImport("Kernel32")]
        public static extern IntPtr LoadLibrary(String funcname);

        ///<summary>
        /// API GetProcAddress
        ///</summary>
        [DllImport("Kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr handle, String funcname);

        ///<summary>
        /// API FreeLibrary
        ///</summary>
        [DllImport("Kernel32")]
        public static extern bool FreeLibrary(IntPtr handle);

        ///<summary>
        /// Get Function Address pointer
        ///</summary>
        ///<param name="dllModule">this value can obtain by calling LoadLibrary</param>
        ///<param name="functionName">function name</param>
        ///<param name="t">function type</param>
        ///<returns>return delegate function</returns>
        public static Delegate GetFunctionAddress(IntPtr dllModule, string functionName, Type t)
        {
            IntPtr address = GetProcAddress(dllModule, functionName);
            if (address == IntPtr.Zero)
                return null;
            else
                return Marshal.GetDelegateForFunctionPointer(address, t);
        }

        ///<summary>
        ///将表示函数地址的IntPtr实例转换成对应的委托, by jingzhongrong
        ///</summary>
        public static Delegate GetDelegateFromIntPtr(IntPtr address, Type t)
        {
            if (address == IntPtr.Zero)
                return null;
            else
                return Marshal.GetDelegateForFunctionPointer(address, t);
        }
    }
#endif

} // rfid namespace END

#endif