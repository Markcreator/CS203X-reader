using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

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
    using CSLibrary.Constants;
    using CSLibrary.Structures;

    sealed class IReaderBase : IDisposable
    {
        private const int MAX_RADIOS = 16;

        private string ERRMSG_NO_STARTUP = "Startup function has not been called.";
        private string ERRMSG_RADIO_INVALID_INDEX = "Radio index is invalid.";
        private string ERRMSG_RADIO_NOT_OPEN = "Radio is not open.";
        private readonly string ERRMSG_NULL_POINTER = "MacUpdateNonvolatileMemory was passed a null pointer.";
        private readonly string ERRMSG_ALREADY_CALLED = "function has already called";
        private readonly string ERRMSG_MEMORY_ERROR = "Memory allocation error";
        private CSLibrary.Linkage ReaderLinkage = new Linkage();
        private CSLibrary.Structures.LibraryVersion g_pLibVersion = null;
        private CSLibrary.Structures.Version[] g_pDriverVersion = null;
        private CSLibrary.Structures.MacVersion[] g_pFirmwareVersion = null;
        private CSLibrary.Structures.RadioInformation[] g_pRadioInfo = null;
        private int[] g_pRadioHandle = null;
        private byte[] g_pRadioNames = null;
        private string g_szErrorBuffer = null;
        private uint g_nTotalNumberOfRadios = 0;
        private bool g_bStartHasBeenCalled = false;
        private CriticalSection m_cs = null;

        /// <summary>
        /// Get current system detect radio 
        /// </summary>
        public uint TotalNumberOfRadios
        {
            get { return g_nTotalNumberOfRadios; }
        }

        public int[] RadioHandle
        {
            get { return g_pRadioHandle; }
            set { g_pRadioHandle = value; }
        }

        public IReaderBase()
        {
            if (m_cs == null)
            {
                m_cs = new CriticalSection();
            }
            m_cs.Init();
        }
        
        ~IReaderBase()
        {
            Dispose();
        }
        
        public void Dispose()
        {
            if (m_cs != null)
            {
                m_cs.UnInit();
                m_cs = null;
            }
        }

        public Result Startup()
        {
#if DEBUG
            return Startup(LibraryMode.DEBUG_TRACE);
#else
            return Startup(LibraryMode.DEFAULT);
#endif
        }

        public Result Startup(String ip, uint port, uint timeout)
        {
#if DEBUG
            return Startup(LibraryMode.DEFAULT | LibraryMode.DEBUG_TCP, ip, port, timeout);
#else
            return Startup(LibraryMode.DEFAULT, ip, port, timeout);
#endif
        }

        /// <summary>
        /// Shutdown
        /// </summary> 
        /// <returns></returns>
        public Result Shutdown()
        {
            string FUNCTION = "Shutdown";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            // App is all or nothing with connected radios - close all
            // prior to doing library shutdown...

            for (int i = 0; i < g_nTotalNumberOfRadios; ++i)
            {
                if (g_pRadioHandle[i] != 0)
                {
                    RadioClose(i);
                }
            }

            Result rslt = ReaderLinkage.Shutdown();

            //FreeGlobalMemory();

            g_bStartHasBeenCalled = false;

            fnExit(FUNCTION);

            return Result.OK;
        }
        /// <summary>
        /// RetrieveAttachedRadiosList
        /// </summary>
        /// <returns></returns>
        public Result RetrieveAttachedRadiosList()
        {
            string FUNCTION = "RetrieveAttachedRadiosList";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }
            RadioEnumeration g_radio =new RadioEnumeration();

            Result status = ReaderLinkage.RetrieveAttachedRadiosList(g_radio,0);
            if( status == Result.OK)
            {
                g_nTotalNumberOfRadios = g_radio.countRadios;
                if (g_nTotalNumberOfRadios > 0)
                {
                    g_pRadioNames = (byte[])g_radio.radioInfo[0].uniqueId.Clone();
                    g_pRadioInfo = (RadioInformation[])g_radio.radioInfo.Clone();
                    g_pDriverVersion = new Version[g_nTotalNumberOfRadios];
                    for (int i = 0; i < g_nTotalNumberOfRadios; i++)
                    {
                        g_pDriverVersion[i] = new Version();
                        g_pDriverVersion[i] = g_radio.radioInfo[i].driverVersion;
                    }
                    
                }
            }
            fnExit(FUNCTION);

            return status;
        }
        /// <summary>
        /// RadioOpen
        /// </summary>
        /// <param name="radioIndex"></param>
        /// <param name="useMacEmulation"></param>
        /// <returns></returns>
        public Result RadioOpen(int radioIndex, bool useMacEmulation)
        {
            string FUNCTION = "RadioOpen";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (g_pRadioHandle[radioIndex] == 0)
            {
                Result status = ReaderLinkage.RadioOpen
                                     (
                                         g_pRadioInfo[radioIndex].cookie,
                                         useMacEmulation ? MacMode.EMULATION : MacMode.DEFAULT
                                     );

                if (status != Result.OK)
                {
                    g_pRadioHandle[radioIndex] = 0;
                    return fnErrorReturn(status, FUNCTION, status.ToString());
                }
                else
                    g_pRadioHandle[radioIndex] = 0x10000;    
            }

            fnExit(FUNCTION);

            return 0;
        }
        /// <summary>
        /// RadioClose
        /// </summary>
        /// <param name="radioIndex"></param>
        /// <returns></returns>
        public Result RadioClose(int radioIndex)
        {
            string FUNCTION = "RadioClose";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioClose();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            g_pRadioHandle[radioIndex] = 0;

            fnExit(FUNCTION);

            return Result.OK;
        }
#if NOUSE
        /// <summary>
        /// RadioCloseAndReopen
        /// </summary>
        /// <param name="radioIndex"></param>
        /// <returns></returns>
        public Result RadioCloseAndReopen(int radioIndex)
        {
            string FUNCTION = "RadioCloseAndReopen";
            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            Result status = Shutdown();
            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            status = Startup();
            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            for (int i = 0; i < 150; i++)
            {

                CSLibrary.HighLevelInterface.Sleep(100);

                if ((status = RetrieveAttachedRadiosList()) != Result.OK)
                {
                    return fnErrorReturn(status, FUNCTION, status.ToString());
                }
                if (TotalNumberOfRadios > 0)
                {
                    break;
                }
            }
            if (TotalNumberOfRadios > 0)
            {
                if ((status = RadioOpen(radioIndex, false)) != Result.OK)
                    return fnErrorReturn(status, FUNCTION, status.ToString());
            }
            else
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, status.ToString());

            fnExit(FUNCTION);
            return Result.OK;
        }
#endif
        /// <summary>
        /// RadioSetConfigurationParameter
        /// </summary>
        /// <param name="radioIndex"></param>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Result MacWriteRegister
        (
            int radioIndex,
            UInt16 parameter,
            UInt32 value
        )
        {
            string FUNCTION = "RadioSetConfigurationParameter";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.MacWriteRegister(parameter, value);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        /// <summary>
        /// RadioGetConfigurationParameter
        /// </summary>
        /// <param name="radioIndex"></param>
        /// <param name="parameter"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public Result MacReadRegister
        (
            int radioIndex,
            UInt16 parameter,
            ref UInt32 pValue
        )
        {
            string FUNCTION = "RadioGetConfigurationParameter";
            
            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.MacReadRegister(parameter, ref pValue);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioSetOperationMode
        (
            int radioIndex,
            RadioOperationMode mode
        )
        {
            string FUNCTION = "RadioSetOperationMode";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioSetOperationMode(mode);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioGetOperationMode
        (
            int radioIndex,
            ref RadioOperationMode pMode
        )
        {
            string FUNCTION = "RadioGetOperationMode";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioGetOperationMode(ref pMode);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioSetPowerState
        (
            int radioIndex,
            RadioPowerState state
        )
        {
            string FUNCTION = "RadioSetPowerState";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioSetPowerState(state);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioGetPowerState
        (
            int radioIndex,
            ref RadioPowerState pState
        )
        {
            string FUNCTION = "RadioGetPowerState";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioGetPowerState(ref pState);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioSetCurrentLinkProfile
        (
            int radioIndex,
            UInt32 profile
        )
        {
            string FUNCTION = "RadioSetCurrentLinkProfile";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioSetCurrentLinkProfile(profile);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }



        public Result RadioGetCurrentLinkProfile
        (
            int radioIndex,
            ref UInt32 pProfile
        )
        {
            string FUNCTION = "RadioGetCurrentLinkProfile";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioGetCurrentLinkProfile(ref pProfile);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }



        public Result RadioGetLinkProfile
        (
            int radioIndex,
            UInt32 profile,
            RadioLinkProfile pProfileInfo
        )
        {
            string FUNCTION = "RadioGetLinkProfile";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioGetLinkProfile(profile, pProfileInfo);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }



        public Result AntennaPortGetStatus
        (
            int radioIndex,
            UInt32 antennaPort,
            AntennaPortStatus pStatus
        )
        {
            string FUNCTION = "AntennaPortGetStatus";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.AntennaPortGetStatus(antennaPort, pStatus);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

//#if CS468
        public Result AntennaPortSetStatus
        (
            int radioIndex,
            UInt32 antennaPort,
            AntennaPortStatus pStatus
        )
        {
            string FUNCTION = "AntennaPortSetStatus";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.AntennaPortSetStatus(antennaPort, pStatus);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
//#endif

        public Result AntennaPortSetState
        (
            int radioIndex,
            UInt32 antennaPort,
            AntennaPortState state
        )
        {
            string FUNCTION = "AntennaPortSetState";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.AntennaPortSetState(antennaPort, state);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }


        public Result AntennaPortSetConfiguration
        (
            int radioIndex,
            UInt32 antennaPort,
            AntennaPortConfig pConfig
        )
        {
            string FUNCTION = "AntennaPortSetConfiguration";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.AntennaPortSetConfiguration(antennaPort, pConfig);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }


        public Result AntennaPortGetConfiguration
        (
            int radioIndex,
            UInt32 antennaPort,
            AntennaPortConfig pConfig
        )
        {
            string FUNCTION = "AntennaPortGetConfiguration";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.AntennaPortGetConfiguration(antennaPort, pConfig);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        
/*
public Result RadioTurnCarrierWaveOn(int radioIndex)
{
	if (fnEnter(FUNCTION)) return fnErrorReturn(-1, FUNCTION, _T("Startup function has not been called."));

	if (radioIndex < 0 || radioIndex >= g_nTotalNumberOfRadios)
		return fnErrorReturn(-1, FUNCTION, _T("Invalid radio index."));

	if (g_pRadioHandle[radioIndex] == INVALID_RADIO_HANDLE) 
		return fnErrorReturn(-1, FUNCTION, _T("Reader is not open."));

	Result  status = RFID_RadioTurnCarrierWaveOn(g_pRadioHandle[radioIndex]);
	if (status != Result _OK)
		return fnErrorReturn(-1, FUNCTION, GetErrorString(status, g_szErrorBuffer, ERROR_MESSAGE_SIZE));

	fnExit(FUNCTION);
	return 0;
}

*/
/*
public Result RadioTurnCarrierWaveOff(int radioIndex)
{
	if (fnEnter(FUNCTION)) return fnErrorReturn(-1, FUNCTION, _T("Startup function has not been called."));

	if (radioIndex < 0 || radioIndex >= g_nTotalNumberOfRadios)
		return fnErrorReturn(-1, FUNCTION, _T("Invalid radio index."));

	if (g_pRadioHandle[radioIndex] == INVALID_RADIO_HANDLE) 
		return fnErrorReturn(-1, FUNCTION, _T("Reader is not open."));

	Result  status = RFID_RadioTurnCarrierWaveOff(g_pRadioHandle[radioIndex]);
	if (status != Result _OK)
		return fnErrorReturn(-1, FUNCTION, GetErrorString(status, g_szErrorBuffer, ERROR_MESSAGE_SIZE));

	fnExit(FUNCTION);
	return 0;
}
*/

/*
public Result RadioGetCarrierWaveState(int radioIndex, int* pState)
{
	if (fnEnter(FUNCTION)) return fnErrorReturn(-1, FUNCTION, _T("Startup function has not been called."));

	if (radioIndex < 0 || radioIndex >= g_nTotalNumberOfRadios)
		return fnErrorReturn(-1, FUNCTION, _T("Invalid radio index."));

	if (g_pRadioHandle[radioIndex] == INVALID_RADIO_HANDLE) 
		return fnErrorReturn(-1, FUNCTION, _T("Reader is not open."));

	INT16U pVal = 0;

	Result  status = MacBypassReadRegister(radioIndex, 0xC0, &pVal);
	if (status != Result _OK)
		return fnErrorReturn(-1, FUNCTION, GetErrorString(status, g_szErrorBuffer, ERROR_MESSAGE_SIZE));

	*pState = pVal & 0x00000001;

	fnExit(FUNCTION);
	return 0;
}
*/


/*
public Result ShuffleFrequencyHopTable(int radioIndex)
{
	if (fnEnter(FUNCTION)) return fnErrorReturn(-1, FUNCTION, _T("Startup function has not been called."));

	if (radioIndex < 0 || radioIndex >= g_nTotalNumberOfRadios) 
		return fnErrorReturn(-1, FUNCTION, _T("Radio index is invalid."));

	if (g_pRadioHandle[radioIndex] == INVALID_RADIO_HANDLE) 
		return fnErrorReturn(-1, FUNCTION, _T("Reader is not open."));


	Result  status = RFID_MacShuffleFrequencyHopTable(g_pRadioHandle[radioIndex]);
	if (status != Result _OK)
		return fnErrorReturn(-1, FUNCTION, GetErrorString(status, g_szErrorBuffer, ERROR_MESSAGE_SIZE));


	fnExit(FUNCTION);
	return 0;
}
*/
/*
public Result UpdateFrequencyHopRandomSeed(int radioIndex)
{
	if (fnEnter(FUNCTION)) return fnErrorReturn(-1, FUNCTION, _T("Startup function has not been called."));

	if (radioIndex < 0 || radioIndex >= g_nTotalNumberOfRadios) 
		return fnErrorReturn(-1, FUNCTION, _T("Radio index is invalid."));

	if (g_pRadioHandle[radioIndex] == INVALID_RADIO_HANDLE) 
		return fnErrorReturn(-1, FUNCTION, _T("Reader is not open."));

	Result  status = RFID_RadioUpdateFrequencyHopRandomSeed(g_pRadioHandle[radioIndex]);
	if (status != Result _OK)
		return fnErrorReturn(-1, FUNCTION, GetErrorString(status, g_szErrorBuffer, ERROR_MESSAGE_SIZE));


	fnExit(FUNCTION);
	return 0;
}
*/


        public Result SetSelectCriteria_18K6C
        (
            int radioIndex,
            SelectCriteria pCriteria,
            UInt32 flags
        )
        {
            string FUNCTION = "SetSelectCriteria_18K6C";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            flags = 0;  // currently reserved field

            Result status = ReaderLinkage.Set18K6CSelectCriteria(pCriteria, flags);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result GetSelectCriteria_18K6C
        (
            int radioIndex,
            SelectCriteria pCriteria
        )
        {
            string FUNCTION = "GetSelectCriteria_18K6C";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.Get18K6CSelectCriteria(pCriteria);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result SetPostMatchCriteria_18K6C
        (
            int radioIndex,
            SingulationCriteria pCriteria,
            UInt32 flags
        )
        {
            string FUNCTION = "SetPostMatchCriteria_18K6C";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            flags = 0;  // currently reserved field

            Result status = ReaderLinkage.Set18K6CPostMatchCriteria(pCriteria, flags);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result GetPostMatchCriteria_18K6C
        (
            int radioIndex,
            SingulationCriteria pCriteria
        )
        {
            string FUNCTION = "GetPostMatchCriteria_18K6C";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.Get18K6CPostMatchCriteria(pCriteria);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result SetQueryParameters_18K6C
        (
            int radioIndex,
            QueryParms pParms
        )
        {
            string FUNCTION = "SetQueryParameters_18K6C";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            //flags = 0;  // currently reserved field

            Result status = ReaderLinkage.SetQueryParameters(pParms, 0);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result GetQueryParameters_18K6C
        (
            int radioIndex,
            QueryParms pParms
        )
        {
            string FUNCTION = "GetQueryParameters_18K6C";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.GetQueryParameters(pParms);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result SetCurrentSingulationAlgorithm_18K6C
        (
            int radioIndex,
            SingulationAlgorithm algorithm
        )
        {
            string FUNCTION = "SetCurrentSingulationAlgorithm_18K6C";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.Set18K6CCurrentSingulationAlgorithm(algorithm);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result GetCurrentSingulationAlgorithm_18K6C
        (
            int radioIndex,
            ref SingulationAlgorithm algorithm
        )
        {
            string FUNCTION = "GetCurrentSingulationAlgorithm_18K6C";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.Get18K6CCurrentSingulationAlgorithm(ref algorithm);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result SetSingulationAlgorithmParameters_18K6C
        (
            int radioIndex,
            SingulationAlgorithm algorithm,
            SingulationAlgorithmParms parms
        )
        {
            string FUNCTION = "SetSingulationAlgorithmParameters_18K6C";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.Set18K6CSingulationAlgorithmParameters(algorithm, parms);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }


        public Result GetSingulationAlgorithmParameters_18K6C
        (
            int radioIndex,
            SingulationAlgorithm algorithm,
            SingulationAlgorithmParms parms
        )
        {
            string FUNCTION = "GetSingulationAlgorithmParameters_18K6C";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.Get18K6CSingulationAlgorithmParameters(algorithm, parms);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result SetQueryTagGroup_18K6C
        (
            int radioIndex,
            TagGroup tgroup
        )
        {
            string FUNCTION = "SetQueryTagGroup_18K6C";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.Set18K6CQueryTagGroup(tgroup);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result GetQueryTagGroup_18K6C
        (
            int radioIndex,
            TagGroup tgroup
        )
        {
            string FUNCTION = "GetQueryTagGroup_18K6C";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.Get18K6CQueryTagGroup(tgroup);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }


        public Result RadioCancelOperation(int radioIndex)
        {
            string FUNCTION = "RadioCancelOperation";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCancelOperation(0);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioAbortOperation(int radioIndex)
        {
            string FUNCTION = "RadioAbortOperation";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioAbortOperation(0);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioSetResponseDataMode
        (
            int radioIndex,
            ResponseType responseType,
            ResponseMode responseMode
        )
        {
            string FUNCTION = "RadioSetResponseDataMode";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioSetResponseDataMode(responseType, responseMode);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioTurnCarrierWaveOn
        (
            int radioIndex
        )
        {
            string FUNCTION = "RadioTurnCarrierWaveOn";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioTurnCarrierWaveOn();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioTurnCarrierWaveOff
        (
            int radioIndex
        )
        {
            string FUNCTION = "RadioTurnCarrierWaveOff";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioTurnCarrierWaveOff();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }


        public Result RadioGetResponseDataMode
        (
            int radioIndex,
            ResponseType responseType,
            ref ResponseMode pResponseMode
        )
        {
            string FUNCTION = "RadioGetResponseDataMode";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioGetResponseDataMode(responseType, ref pResponseMode);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result MacUpdateNonvolatileMemory
        (
            int radioIndex,
            UInt32 countBlocks,
            NonVolatileMemoryBlock[] pBlocks,
            FwUpdateFlags flags
        )
        {
            string FUNCTION = "MacUpdateFirmware";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            if (pBlocks == null)
            {
                return fnErrorReturn(Result.INVALID_PARAMETER, FUNCTION, ERRMSG_NULL_POINTER);
            }

            Result status = ReaderLinkage.MacUpdateNonvolatileMemory(countBlocks, pBlocks, flags);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result MacGetVersion
        (
            int radioIndex
        )
        {
            string FUNCTION = "MacGetVersion";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }
            g_pFirmwareVersion[radioIndex] = new MacVersion();
            Result status = ReaderLinkage.MacGetVersion(g_pFirmwareVersion[radioIndex]);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result MacReadOemData
        (
            int radioIndex,
            UInt32 address,
            UInt32 count,
            UInt32[] pData
        )
        {
            string FUNCTION = "MacReadOemData";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }
            if (pData == null)
            {
                return fnErrorReturn(Result.INVALID_PARAMETER, FUNCTION, ERRMSG_NULL_POINTER);
            }

            Result status = ReaderLinkage.MacReadOemData(address, count, pData);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result MacReadOemData
        (
            int radioIndex,
            UInt32 address,
            ref UInt32 data
        )
        {
            string FUNCTION = "MacReadOemData";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.MacReadOemData(address, ref data);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result MacWriteOemData
        (
            int radioIndex,
            UInt32 address,
            UInt32 count,
            UInt32[] pData
        )
        {
            string FUNCTION = "MacWriteOemData";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.MacWriteOemData(address, count, pData);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result MacWriteOemData
        (
            int radioIndex,
            UInt32 address,
            UInt32 data
        )
        {
            string FUNCTION = "MacWriteOemData";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.MacWriteOemData(address, data);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result MacReset
        (
            int radioIndex
        )
        {
            string FUNCTION = "MacReset";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.MacReset(MacResetType.SOFT);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result MacClearError
        (
            int radioIndex
        )
        {
            string FUNCTION = "MacClearError";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.MacClearError();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result MacBypassWriteRegister
        (
            int radioIndex,
            UInt16 parameter,
            UInt16 value
        )
        {
            string FUNCTION = "MacBypassWriteRegister";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.MacBypassWriteRegister(parameter, value);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result MacBypassReadRegister
        (
            int radioIndex,
            UInt16 parameter,
            ref UInt16 pValue
        )
        {
            string FUNCTION = "MacBypassReadRegister";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.MacBypassReadRegister(parameter, ref pValue);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result MacSetRegion
        (
            int radioIndex,
            MacRegion region,
            IntPtr pRegionConfig
        )
        {
            string FUNCTION = "MacSetRegion";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.MacSetRegion(region, pRegionConfig);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result MacGetRegion
        (
            int radioIndex,
            ref MacRegion pRegion,
            IntPtr pRegionConfig
        )
        {
            string FUNCTION = "MacGetRegion";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.MacGetRegion(ref pRegion, pRegionConfig);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioSetGpioPinsConfiguration
        (
            int radioIndex,
            UInt32 mask,
            UInt32 configuration
        )
        {
            string FUNCTION = "RadioSetGpioPinsConfiguration";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioSetGpioPinsConfiguration(mask, configuration);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioGetGpioPinsConfiguration
        (
            int radioIndex,
            ref UInt32 pConfiguration
        )
        {
            string FUNCTION = "RadioGetGpioPinsConfiguration";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioGetGpioPinsConfiguration(ref pConfiguration);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }


        public Result RadioReadGpioPins
        (
            int radioIndex,
            UInt32 mask,
            ref UInt32 pValue
        )
        {
            string FUNCTION = "RadioReadGpioPins";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioReadGpioPins(mask, ref pValue);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }


        public Result RadioWriteGpioPins
        (
            int radioIndex,
            UInt32 mask,
            UInt32 value
        )
        {
            string FUNCTION = "RadioWriteGpioPins";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioWriteGpioPins(mask, value);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result RadioReadLinkProfileRegister
        (
            int radioIndex,
            [In]          UInt32 profile,
            [In]          UInt16 address,
            [In, Out] ref UInt16 value
        )
        {
            string FUNCTION = "RadioReadLinkProfileRegister";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioReadLinkProfileRegister(profile, address, ref value);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result RadioWriteLinkProfileRegister
        (
            int radioIndex,
            [In] UInt32 profile,
            [In] UInt16 address,
            [In] UInt16 value
        )
        {
            string FUNCTION = "RadioWriteLinkProfileRegister";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioWriteLinkProfileRegister(profile, address, value);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result TagInventory(int radioIndex, InventoryParms parms, SelectFlags flags)
        {
            string FUNCTION = "TagInventory";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CInventory(parms, (uint)flags);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagRead(int radioIndex, ReadParms parms, SelectFlags flags)
        {
            string FUNCTION = "TagRead";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            //m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CRead(parms, (uint)flags);

            //m_cs.Lock();
            
            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagWrite(int radioIndex, WriteParms parms, SelectFlags flags)
        {
            string FUNCTION = "TagWrite";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            //m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CWrite(parms, (uint)flags);

            //m_cs.Lock();
            
            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagBlockWrite(int radioIndex, BlockWriteParms parms, SelectFlags flags)
        {
            string FUNCTION = "TagBlockWrite";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CBlockWrite(parms, (uint)flags);
 
            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result TagBlockErase(int radioIndex, BlockEraseParms parms, SelectFlags flags)
        {
            string FUNCTION = "TagBlockErase";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CBlockErase(parms, (uint)flags);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagLock(int radioIndex, LockParms parms, SelectFlags flags)
        {
            string FUNCTION = "TagLock";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

           // m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CLock(parms, (uint)flags);

            //m_cs.Lock();
            
            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagKill(int radioIndex, KillParms parms, SelectFlags flags)
        {
            string FUNCTION = "TagKill";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CKill(parms, (uint)flags);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result GetMACAddress(int radioIndex, [In, Out] Byte[] mac)
        {
            string FUNCTION = "GetMACAddress";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.GetMACAddress(mac);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagWriteEPC(int radioIndex, TagWriteEpcParms parms)
        {
            string FUNCTION = "TagWriteEPC";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CWriteEPC(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagWritePC(int radioIndex, TagWritePcParms parms)
        {
            string FUNCTION = "TagWritePC";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CWritePC(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagWriteKillPwd(int radioIndex, TagWritePwdParms parms)
        {
            string FUNCTION = "TagWriteKillPwd";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CWriteKillPwd(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagWriteAccPwd(int radioIndex, TagWritePwdParms parms)
        {
            string FUNCTION = "TagWriteAccPwd";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CWriteAccPwd(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagWriteUser(int radioIndex, TagWriteUserParms parms)
        {
            string FUNCTION = "TagWriteUser";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CWriteUser(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagReadEPC(int radioIndex, TagReadEpcParms parms)
        {
            string FUNCTION = "TagReadEPC";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CReadEPC(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagReadPC(int radioIndex, TagReadPcParms parms)
        {
            string FUNCTION = "TagReadPC";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CReadPC(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagReadKillPwd(int radioIndex, TagReadPwdParms parms)
        {
            string FUNCTION = "TagReadKillPwd";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CReadKillPwd(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagReadAccPwd(int radioIndex, TagReadPwdParms parms)
        {
            string FUNCTION = "TagRead";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CReadAccPwd(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagReadUser(int radioIndex, TagReadUserParms parms)
        {
            string FUNCTION = "TagReadUser";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CReadUser(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagReadTid(int radioIndex, TagReadTidParms parms)
        {
            string FUNCTION = "TagReadTid";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CReadTID(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result TagRawLock(int radioIndex, TagLockParms parms)
        {
            string FUNCTION = "TagRawLock";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CRawLock(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result TagBlockLock(int radioIndex, TagBlockPermalockParms parms)
        {
            string FUNCTION = "TagBlockLock";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CBlockLock(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result TagPermalock(int radioIndex, PermalockParms parms, SelectFlags flags)
        {
            string FUNCTION = "TagBlockLock";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CPermaLock(parms, (uint)flags);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result TagRawKill(int radioIndex, TagKillParms parms)
        {
            string FUNCTION = "TagRawKill";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CRawKill(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagSearchAny(int radioIndex, InternalTagInventoryParms parms)
        {
            string FUNCTION = "TagSearchAny";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CSearchAny(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result TagSearchOne(int radioIndex, InternalTagSearchOneParms parms)
        {
            string FUNCTION = "TagSearchOne";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CSearchOne(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result TagRanging(int radioIndex, InternalTagRangingParms parms)
        {
            string FUNCTION = "TagRanging";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CRanging(parms);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result TagReadProtect(int radioIndex, ReadProtectParms parms, SelectFlags flags)
        {
            string FUNCTION = "TagReadProtect";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CReadProtect(parms, (uint)flags);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result CustTagReadProtect(int radioIndex, InternalCustCmdTagReadProtectParms parms, SelectFlags flags)
        {
            string FUNCTION = "CustTagReadProtect";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CCustCmdReadProtect(parms, (uint)flags);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result TagResetReadProtect(int radioIndex, ReadProtectParms parms, SelectFlags flags)
        {
            string FUNCTION = "TagResetReadProtect";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CResetReadProtect(parms, (uint)flags);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result CustTagResetReadProtect(int radioIndex, InternalCustCmdTagReadProtectParms parms, SelectFlags flags)
        {
            string FUNCTION = "CustTagResetReadProtect";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CCustCmdResetReadProtect(parms, (uint)flags);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result TagEASConfig(int radioIndex, EASParms parms, SelectFlags flags)
        {
            string FUNCTION = "TagEASConfig";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CEASConfig(parms, (uint)flags);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result CustTagEASConfig(int radioIndex, InternalCustCmdEASParms parms, SelectFlags flags)
        {
            string FUNCTION = "CustTagEASConfig";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CCustCmdEASConfig(parms, (uint)flags);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result TagEASAlarm(int radioIndex, EASParms parms, SelectFlags flags)
        {
            string FUNCTION = "TagEASAlarm";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CEASAlarm(parms, (uint) flags);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
        public Result CustTagEASAlarm(int radioIndex, InternalCustCmdEASParms parms, SelectFlags flags)
        {
            string FUNCTION = "CustTagEASAlarm";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            m_cs.Unlock();

            Result status = ReaderLinkage.Tag18K6CCustCmdEASAlarm(parms, (uint)flags);

            m_cs.Lock();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }
//        #endregion


        public Result GetReaderName
        (
            int radioIndex,
            ref string szReaderName
        )
        {
            string FUNCTION = "GetReaderName";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }
            System.Text.Encoding enc = System.Text.Encoding.ASCII;
            szReaderName = enc.GetString(g_pRadioInfo[radioIndex].uniqueId, 0, g_pRadioInfo[radioIndex].uniqueId.Length);

            fnExit(FUNCTION);

            return 0;
        }

        public LibraryVersion GetLibraryVersion()
        {
            return g_pLibVersion;
        }

        public MacVersion GetFirmwareVersion(int radioindex)
        {
            if (MacGetVersion(radioindex) != Result.OK)
                return null;
            if (g_pFirmwareVersion == null)
                return null;
            if (g_pFirmwareVersion.Length <= radioindex || radioindex < 0)
                return null;
            return g_pFirmwareVersion[radioindex];
        }

        public Version GetDriverVersion(int radioindex)
        {

            if (g_pDriverVersion == null)
                return null;
            if (g_pDriverVersion.Length <= radioindex)
                return null;
            return g_pDriverVersion[radioindex];
        }

        public void SetLastErrorText(string szMessage, int nSize)
        {
            if (szMessage == null || szMessage.Length == 0) return;
            if (nSize < 5) return;

            if (!g_bStartHasBeenCalled) return;

            m_cs.Lock();

            g_szErrorBuffer = szMessage;

            m_cs.Unlock();
        }


        public string GetLastErrorText()
        {
            string pBuffer;

            m_cs.Lock();

            if (g_szErrorBuffer != null)
            {
                pBuffer = g_szErrorBuffer;
            }
            else
            {
                pBuffer = "Startup() was not called or Shutdown() has been called.";
            }
            m_cs.Unlock();
            return pBuffer;
        }

        public Result RadioCLSetPassword(int radioIndex, UInt32 reg1, UInt32 reg2)
        {
            string FUNCTION = "RadioCLSetPassword";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLSetPassword(reg1, reg2);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLSetLogMode(int radioIndex, UInt32 reg1)
        {
            string FUNCTION = "RadioCLSetLogMode";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLSetLogMode(reg1);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLSetLogLimits(int radioIndex, UInt32 reg1, UInt32 reg2)
        {
            string FUNCTION = "RadioCLSetLogLimits";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLSetLogLimits (reg1, reg2);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLGetMeasurementSetup(int radioIndex, byte [] pParms)
        {
            string FUNCTION = "RadioCLGetMeasurementSetup";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLGetMeasurementSetup (pParms);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLSetSFEParameters(int radioIndex, UInt32 reg1)
        {
            string FUNCTION = "RadioCLSetSFEParameters";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLSetSFEParameters(reg1);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLSetCalibrationData(int radioIndex, UInt32 reg1, UInt32 reg2)
        {
            string FUNCTION = "RadioCLSetCalibrationData";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLSetCalibrationData(reg1, reg2);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLEndLog(int radioIndex)
        {
            string FUNCTION = "RadioCLEngLog";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLEndLog();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLStartLog(int radioIndex, UINT32 reg1)
        {
            string FUNCTION = "RadioCLStartLog";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLStartLog (reg1);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLGetLogState(int radioIndex, UInt32 InternalRegister, byte[] Parms)
        {
            string FUNCTION = "RadioCLGetLogState";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }


//            ReaderLinkage.MacWriteRegister(0x90D, InternalRegister);

            Result status = ReaderLinkage.RadioCLGetLogState(InternalRegister, Parms);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLGetCalibrationData(int radioIndex, byte [] pParms)
        {
            string FUNCTION = "RadioCLGetMeasurementSetup";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLGetCalibrationData(pParms);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLGetBatteryLevel(int radioIndex, UInt32 reg1, byte [] Parms)
        {
            string FUNCTION = "RadioCLGetBatteryLevel";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLGetBatteryLevel(reg1, Parms);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLSetShelfLife(int radioIndex, UINT32 reg1, UINT32 reg2)
        {
            string FUNCTION = "RadioCLSetShelfLife";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLSetShelfLife(reg1, reg2);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLInitialize (int radioIndex, UInt32 reg1)
        {
            string FUNCTION = "RadioCLInitialize";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLInitialize(reg1);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLGetSensorValue(int radioIndex, UInt32 reg1, byte [] Parms)
        {
            string FUNCTION = "RadioCLGetSensorValue";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLGetSensorValue(reg1, Parms);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLOpenArea(int radioIndex, UInt32 reg1, UInt32 reg2)
        {
            string FUNCTION = "RadioCLOpenArea";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLOpenArea(reg1, reg2);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioCLAccessFifo(int radioIndex, UInt32 reg1, UInt32 reg2, UInt32 reg3, byte [] Parms)
        {
            string FUNCTION = "RadioCLOpenFifo";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioCLAccessFifo (reg1, reg2, reg3, Parms);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioG2X_Change_EAS(int radioIndex)
        {
            string FUNCTION = "RadioG2X_Change_EAS";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioG2X_Change_EAS();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioG2X_EAS_Alarm(int radioIndex)
        {
            string FUNCTION = "RadioG2X_EAS_Alarm";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioG2X_EAS_Alarm();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioG2X_ChangeConfig(int radioIndex)
        {
            string FUNCTION = "RadioG2X_EAS_Alarm";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioG2X_ChangeConfig();

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        public Result RadioQT_Command(int radioIndex, int RW, int TP, int SR, int MEM)
        {
            string FUNCTION = "RadioQT_Command";

            if (fnEnter(FUNCTION))
            {
                return fnErrorReturn(Result.NOT_INITIALIZED, FUNCTION, ERRMSG_NO_STARTUP);
            }

            if (0 > radioIndex || radioIndex >= g_nTotalNumberOfRadios)
            {
                return fnErrorReturn(Result.NO_SUCH_RADIO, FUNCTION, ERRMSG_RADIO_INVALID_INDEX);
            }

            if (0 == g_pRadioHandle[radioIndex])
            {
                return fnErrorReturn(Result.RADIO_NOT_PRESENT, FUNCTION, ERRMSG_RADIO_NOT_OPEN);
            }

            Result status = ReaderLinkage.RadioQT_Command(RW, TP, SR, MEM);

            if (status != Result.OK)
            {
                return fnErrorReturn(status, FUNCTION, status.ToString());
            }

            fnExit(FUNCTION);

            return Result.OK;
        }

        #region Private Function
        private Result Startup(LibraryMode flags)
        {
            string FUNCTION = "Startup";

            fnEnter(FUNCTION);
            CSLibrary.Structures.LibraryVersion LibraryVersion = new LibraryVersion();
            if (g_bStartHasBeenCalled)
            {
                return fnErrorReturn(Result.ALREADY_OPEN, FUNCTION, ERRMSG_ALREADY_CALLED);
            }

            if (InitializeGlobals() != Result.OK)
            {
                return fnErrorReturn(Result.OUT_OF_MEMORY, FUNCTION, ERRMSG_MEMORY_ERROR);
            }

            Result Res = ReaderLinkage.Startup(LibraryVersion, flags);
            
            g_pLibVersion = LibraryVersion;

            g_bStartHasBeenCalled = true;

            fnExit(FUNCTION);
            return Res;

        }

        private Result Startup(LibraryMode flags, string ip, uint port, uint timeout)
        {
            string FUNCTION = "Startup";

            fnEnter(FUNCTION);
            CSLibrary.Structures.LibraryVersion LibraryVersion = new LibraryVersion();
            if (g_bStartHasBeenCalled)
            {
                return fnErrorReturn(Result.ALREADY_OPEN, FUNCTION, ERRMSG_ALREADY_CALLED);
            }

            if (InitializeGlobals() != Result.OK)
            {
                return fnErrorReturn(Result.OUT_OF_MEMORY, FUNCTION, ERRMSG_MEMORY_ERROR);
            }

            Result Res = ReaderLinkage.Startup(LibraryVersion, flags, ip, port, timeout);
            
            g_pLibVersion = LibraryVersion;

            g_bStartHasBeenCalled = true;

            fnExit(FUNCTION);
            return Res;

        }

        private Result InitializeGlobals()
        {
            string FUNCTION = "InitializeGlobals";

            m_cs.Lock();

            //ReaderLinkage = new CSLibrary.Linkage();
            g_pLibVersion = new LibraryVersion();
            g_pRadioInfo = new RadioInformation[MAX_RADIOS];
            g_pRadioHandle = new int[MAX_RADIOS];
            g_pDriverVersion = new Version[MAX_RADIOS];
            g_pFirmwareVersion = new MacVersion[MAX_RADIOS];

            if (g_pLibVersion == null ||
                g_pRadioInfo == null ||
                g_pRadioHandle == null ||
                g_pDriverVersion == null ||
                g_pFirmwareVersion == null)
                return fnErrorReturn(Result.OUT_OF_MEMORY, FUNCTION, ERRMSG_MEMORY_ERROR);

            m_cs.Unlock();

            //	_ASSERT(_CrtCheckMemory());
            return Result.OK;
        }

        #endregion

        #region Error Log
#if NOUSE
        [Conditional("DEBUG")]
        private static string FUNCTION
        {
            get
            {
                System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(1);
                return sf.GetMethod().Name;
            }
        }
#endif
        private void fnMessage(string fnName, string msg)
        {
#if DEBUG
            CSLibrary.Diagnostics.CoreDebug.Logger.Trace(String.Format("{0}() Info: {1}\r", fnName, msg));
#endif
        }

        // Return true of function function is NOT ok to enter;
        private bool fnEnter(string fnName)
        {
            m_cs.Lock();

#if DEBUG
            CSLibrary.Diagnostics.CoreDebug.Logger.Trace(String.Format("{0}() Enter\r", fnName));
#endif
            return !g_bStartHasBeenCalled;
        }

        private void fnExit(string fnName)
        {

#if DEBUG
            CSLibrary.Diagnostics.CoreDebug.Logger.Trace(String.Format("{0}() Exit\r", fnName));
#endif
            m_cs.Unlock();
        }


        private Result fnErrorReturn(Result n, string fnName, string sMessage)
        {

#if DEBUG
            if (sMessage != g_szErrorBuffer && g_szErrorBuffer != null)
            {
                g_szErrorBuffer = sMessage;
            }

            if (n == Result.OK)
                CSLibrary.Diagnostics.CoreDebug.Logger.Trace(String.Format("{0}() Exit:{1}\r", fnName, sMessage));
            else
                CSLibrary.Diagnostics.CoreDebug.Logger.Trace(String.Format("{0}() ERROR:{1}\r", fnName, sMessage));

#endif
            m_cs.Unlock();

            return n;
        }

        #endregion
    }

#if nouse
    sealed class CriticalSection
    {
#if WindowsCE
        const string DLL = "coredll.dll";
#else
        const string DLL = "kernel32.dll";
#endif
        struct CRITICAL_SECTION {
            public UInt32 LockCount;         /* Nesting count on critical section */
            public IntPtr OwnerThread;         	/* Handle of owner thread */
            public IntPtr hCrit;					/* Handle to this critical section */
            public UInt32 needtrap;					/* Trap in when freeing critical section */
            public UInt32 dwContentions;			/* Count of contentions */
        }

        [DllImport(DLL)]
        static extern void InitializeCriticalSection(out CRITICAL_SECTION lpCriticalSection);
        [DllImport(DLL)]
        static extern void EnterCriticalSection(ref CRITICAL_SECTION lpCriticalSection);
        [DllImport(DLL)]
        static extern void DeleteCriticalSection(ref CRITICAL_SECTION lpCriticalSection);
        [DllImport(DLL)]
        static extern void LeaveCriticalSection(ref CRITICAL_SECTION lpCriticalSection);

        private CRITICAL_SECTION cs;

        public void Lock()
        {
            EnterCriticalSection(ref cs);
        }

        public void Unlock()
        {
            LeaveCriticalSection(ref cs);
        }

        public void Init()
        {
            InitializeCriticalSection(out cs);
        }

        public void UnInit()
        {
            DeleteCriticalSection(ref cs);
        }
    }
}
#endif