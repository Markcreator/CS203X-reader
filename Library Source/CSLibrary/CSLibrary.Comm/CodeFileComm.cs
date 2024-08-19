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
using System.Collections.Generic;
using System.Text;

using System.Threading;
using System.Runtime.InteropServices;

using System.Net;
using System.Net.Sockets;

using System.IO.Ports;

using CSLibrary.Text;
using CSLibrary.Constants;

namespace CSLibrary
{
    //        private bool COMM_Connect(string DeviceName, uint TimeOut)
    //        private bool COMM_Disconnect()
    //        private bool COMM_READER_Send(byte[] buffer, int offset, int size, int timeout)       // e.g. TCP Port 1515
    //        private bool COMM_READER_Recv(byte[] buffer, int offset, int size, int timeout)       // e.g. TCP Port 1515
    //        private bool COMM_INTBOARD_Send(byte[] buffer, int offset, int size, int timeout)     // e.g. TCP Port 1516
    //        private bool COMM_INTBOARD_Recv(byte[] buffer, int offset, int size, int timeout)     // e.g. TCP Port 1516
    //        private bool COMM_INTBOARDALT_Send(byte[] buffer, int offset, int size, int timeout)  // e.g. UDP Command
    //        private bool COMM_INTBOARDALT_Recv(byte[] buffer, int offset, int size, int timeout)  // e.g. UDP Command

    public partial class HighLevelInterface
    {
        private string m_DeviceName;
        private INTERFACETYPE m_DeviceInterfaceType;
        private uint m_ConnectionTimeOut = 30000;
        private string m_macAddress = String.Empty;

        private int ReaderStatus = 0;
        private bool ReaderAbort = false;

        private uint m_GPIOStatus = 0;

        // for internal data transfer
//        const int MAX_RETRY = 30;
        const int MAX_RD_CNT = 0x20;
        const int MAX_WR_CNT = 0x8;
        UInt16[] tagreadbuf = new UInt16[MAX_RD_CNT];
        UInt16[] taglockbuf = new UInt16[MAX_RD_CNT];
        private uint m_OEMReadAdd;
        private uint m_OEMReadData;
        private ushort m_TildenReadValue;
        private int m_TagAccessStatus;
        private UInt16 m_CurrentFreqChannelIndex;

        /// <summary>
        /// REGISTER NAME/ADDRESS CONSTANTS
        /// </summary>
        public enum MacRegister : ushort
        {
            MAC_VER = 0x0000,
            MAC_INFO = 0x0001,
            MAC_RFTRANSINFO = 0x0002,
            MAC_DBG1 = 0x0003,
            MAC_DBG2 = 0x0004,
            MAC_ERROR = 0x0005,

            HST_ENGTST_ARG0 = 0x0100,
            HST_ENGTST_ARG1 = 0x0101,
            HST_DBG1 = 0x0102,
            HST_EMU = 0x0103,
            HST_TX_RANDOM_DATA_CONTROL = 0x0105,
            
            HST_PWRMGMT = 0x0200,
            HST_CMNDIAGS = 0x0201,
            MAC_BLK02RES1 = 0x0202,
            HST_IMPINJ_EXTENSIONS = 0x0203,
            HST_CTR1_CFG = 0x0204,
            MAC_CTR1_VAL = 0x0205,
            HST_CTR2_CFG = 0x0206,
            MAC_CTR2_VAL = 0x0207,
            HST_CTR3_CFG = 0x0208,
            MAC_CTR3_VAL = 0x0209,
            HST_CTR4_CFG = 0x020A,
            MAC_CTR4_VAL = 0x020B,
            
            HST_PROTSCH_SMIDX = 0x0300,
            HST_PROTSCH_SMCFG = 0x0301,
            HST_PROTSCH_FTIME_SEL = 0x0302,
            HST_PROTSCH_FTIME = 0x0303,
            HST_PROTSCH_SMCFG_SEL = 0x0304,
            HST_PROTSCH_TXTIME_SEL = 0x0305,
            HST_PROTSCH_TXTIME_ON = 0x0306,
            HST_PROTSCH_TXTIME_OFF = 0x0307,
            HST_PROTSCH_CYCCFG_SEL = 0x0308,
            HST_PROTSCH_CYCCFG_DESC_ADJ1 = 0x0309,
            HST_PROTSCH_ADJCW = 0x030A,
            
            HST_MBP_ADDR = 0x0400,
            HST_MBP_DATA = 0x0401,
            HST_MBP_RFU_0x0402 = 0x0402,
            HST_MBP_RFU_0x0403 = 0x0403,
            HST_MBP_RFU_0x0404 = 0x0404,
            HST_MBP_RFU_0x0405 = 0x0405,
            HST_MBP_RFU_0x0406 = 0x0406,
            HST_MBP_RFU_0x0407 = 0x0407,
            HST_LPROF_SEL = 0x0408,
            HST_LPROF_ADDR = 0x0409,
            HST_LPROF_DATA = 0x040A,
            
            HST_OEM_ADDR = 0x0500,
            HST_OEM_DATA = 0x0501,
            
            HST_GPIO_INMSK = 0x0600,
            HST_GPIO_OUTMSK = 0x0601,
            HST_GPIO_OUTVAL = 0x0602,
            HST_GPIO_CFG = 0x0603,
            
            HST_ANT_CYCLES = 0x0700,
            HST_ANT_DESC_SEL = 0x0701,
            HST_ANT_DESC_CFG = 0x0702,
            MAC_ANT_DESC_STAT = 0x0703,
            HST_ANT_DESC_PORTDEF = 0x0704,
            HST_ANT_DESC_DWELL = 0x0705,
            HST_ANT_DESC_RFPOWER = 0x0706,
            HST_ANT_DESC_INV_CNT = 0x0707,
            
            HST_TAGMSK_DESC_SEL = 0x0800,
            HST_TAGMSK_DESC_CFG = 0x0801,
            HST_TAGMSK_BANK = 0x0802,
            HST_TAGMSK_PTR = 0x0803,
            HST_TAGMSK_LEN = 0x0804,
            HST_TAGMSK_0_3 = 0x0805,
            HST_TAGMSK_4_7 = 0x0806,
            HST_TAGMSK_8_11 = 0x0807,
            HST_TAGMSK_12_15 = 0x0808,
            HST_TAGMSK_16_19 = 0x0809,
            HST_TAGMSK_20_23 = 0x080A,
            HST_TAGMSK_24_27 = 0x080B,
            HST_TAGMSK_28_31 = 0x080C,
            
            HST_QUERY_CFG = 0x0900,
            HST_INV_CFG = 0x0901,
            HST_INV_SEL = 0x0902,
            HST_INV_ALG_PARM_0 = 0x0903,
            HST_INV_ALG_PARM_1 = 0x0904,
            HST_INV_ALG_PARM_2 = 0x0905,
            HST_INV_ALG_PARM_3 = 0x0906,
            HST_INV_RFU_0x0907 = 0x0907,
            HST_INV_RFU_0x0908 = 0x0908,
            HST_INV_RFU_0x0909 = 0x0909,
            HST_INV_RFU_0x090A = 0x090A,
            HST_INV_RFU_0x090B = 0x090B,
            HST_INV_RFU_0x090C = 0x090C,
            HST_INV_RFU_0x090D = 0x090D,
            HST_INV_RFU_0x090E = 0x090E,
            HST_INV_RFU_0x090F = 0x090F,
            HST_INV_EPC_MATCH_SEL = 0x0910,
            HST_INV_EPC_MATCH_CFG = 0x0911,
            HST_INV_EPCDAT_0_3 = 0x0912,
            HST_INV_EPCDAT_4_7 = 0x0913,
            HST_INV_EPCDAT_8_11 = 0x0914,
            HST_INV_EPCDAT_12_15 = 0x0915,
            HST_INV_EPCDAT_16_19 = 0x0916,
            HST_INV_EPCDAT_20_23 = 0x0917,
            HST_INV_EPCDAT_24_27 = 0x0918,
            HST_INV_EPCDAT_28_31 = 0x0919,
            HST_INV_EPCDAT_32_35 = 0x091A,
            HST_INV_EPCDAT_36_39 = 0x091B,
            HST_INV_EPCDAT_40_43 = 0x091C,
            HST_INV_EPCDAT_44_47 = 0x091D,
            HST_INV_EPCDAT_48_51 = 0x091E,
            HST_INV_EPCDAT_52_55 = 0x091F,
            HST_INV_EPCDAT_56_59 = 0x0920,
            HST_INV_EPCDAT_60_63 = 0x0921,
            
            HST_TAGACC_DESC_SEL = 0x0A00,
            HST_TAGACC_DESC_CFG = 0x0A01,
            HST_TAGACC_BANK = 0x0A02,
            HST_TAGACC_PTR = 0x0A03,
            HST_TAGACC_CNT = 0x0A04,
            HST_TAGACC_LOCKCFG = 0x0A05,
            HST_TAGACC_ACCPWD = 0x0A06,
            HST_TAGACC_KILLPWD = 0x0A07,
            HST_TAGWRDAT_SEL = 0x0A08,
            HST_TAGWRDAT_0 = 0x0A09,
            HST_TAGWRDAT_1 = 0x0A0A,
            HST_TAGWRDAT_2 = 0x0A0B,
            HST_TAGWRDAT_3 = 0x0A0C,
            HST_TAGWRDAT_4 = 0x0A0D,
            HST_TAGWRDAT_5 = 0x0A0E,
            HST_TAGWRDAT_6 = 0x0A0F,
            HST_TAGWRDAT_7 = 0x0A10,
            HST_TAGWRDAT_8 = 0x0A11,
            HST_TAGWRDAT_9 = 0x0A12,
            HST_TAGWRDAT_10 = 0x0A13,
            HST_TAGWRDAT_11 = 0x0A14,
            HST_TAGWRDAT_12 = 0x0A15,
            HST_TAGWRDAT_13 = 0x0A16,
            HST_TAGWRDAT_14 = 0x0A17,
            HST_TAGWRDAT_15 = 0x0A18,
            
            MAC_RFTC_PAPWRLEV = 0x0B00,
            HST_RFTC_PAPWRCTL_PGAIN = 0x0B01,
            HST_RFTC_PAPWRCTL_IGAIN = 0x0B02,
            HST_RFTC_PAPWRCTL_DGAIN = 0x0B03,
            MAC_RFTC_REVPWRLEV = 0x0B04,
            HST_RFTC_REVPWRTHRSH = 0x0B05,
            MAC_RFTC_AMBIENTTEMP = 0x0B06,
            HST_RFTC_AMBIENTTEMPTHRSH = 0x0B07,
            MAC_RFTC_XCVRTEMP = 0x0B08,
            HST_RFTC_XCVRTEMPTHRSH = 0x0B09,
            MAC_RFTC_PATEMP = 0x0B0A,
            HST_RFTC_PATEMPTHRSH = 0x0B0B,
            HST_RFTC_PADELTATEMPTHRSH = 0x0B0C,
            HST_RFTC_PAPWRCTL_AIWDELAY = 0x0B0D,
            MAC_RFTC_PAPWRCTL_STAT0 = 0x0B0E,
            MAC_RFTC_PAPWRCTL_STAT1 = 0x0B0F,
            MAC_RFTC_PAPWRCTL_STAT2 = 0x0B10,
            MAC_RFTC_PAPWRCTL_STAT3 = 0x0B11,
            HST_RFTC_ANTSENSRESTHRSH = 0x0B12,
            HST_RFTC_IFLNAAGCRANGE = 0x0B13,
            MAC_RFTC_LAST_ANACTRL1 = 0x0B14,
            HST_RFTC_OPENLOOPPWRCTRL = 0x0B15,
            HST_RFTC_RFU_0x0B16 = 0x0B16,
            HST_RFTC_RFU_0x0B17 = 0x0B17,
            HST_RFTC_RFU_0x0B18 = 0x0B18,
            HST_RFTC_RFU_0x0B19 = 0x0B19,
            HST_RFTC_PREDIST_COEFF0 = 0x0B1A,
            HST_RFTC_RFU_0x0B1B = 0x0B1B,
            HST_RFTC_RFU_0x0B1C = 0x0B1C,
            HST_RFTC_RFU_0x0B1D = 0x0B1D,
            HST_RFTC_RFU_0x0B1E = 0x0B1E,
            HST_RFTC_RFU_0x0B1F = 0x0B1F,
            HST_RFTC_CAL_GGNEG7 = 0x0B20,
            HST_RFTC_CAL_GGNEG5 = 0x0B21,
            HST_RFTC_CAL_GGNEG3 = 0x0B22,
            HST_RFTC_CAL_GGNEG1 = 0x0B23,
            HST_RFTC_CAL_GGPLUS1 = 0x0B24,
            HST_RFTC_CAL_GGPLUS3 = 0x0B25,
            HST_RFTC_CAL_GGPLUS5 = 0x0B26,
            HST_RFTC_CAL_GGPLUS7 = 0x0B27,
            HST_RFTC_CAL_MACADCREFV = 0x0B28,
            HST_RFTC_CAL_RFFWDPWR_C0 = 0x0B29,
            HST_RFTC_CAL_RFFWDPWR_C1 = 0x0B2A,
            HST_RFTC_CAL_RFFWDPWR_C2 = 0x0B2B,
            HST_RFTC_RFU_0x0B2C = 0x0B2C,
            HST_RFTC_RFU_0x0B2D = 0x0B2D,
            HST_RFTC_RFU_0x0B2E = 0x0B2E,
            HST_RFTC_RFU_0x0B2F = 0x0B2F,
            HST_RFTC_CLKDBLR_CFG = 0x0B30,
            HST_RFTC_CLKDBLR_SEL = 0x0B31,
            HST_RFTC_CLKDBLR_LUTENTRY = 0x0B32,
            HST_RFTC_RFU_0x0B33 = 0x0B33,
            HST_RFTC_RFU_0x0B34 = 0x0B34,
            HST_RFTC_RFU_0x0B35 = 0x0B35,
            HST_RFTC_RFU_0x0B36 = 0x0B36,
            HST_RFTC_RFU_0x0B37 = 0x0B37,
            HST_RFTC_RFU_0x0B38 = 0x0B38,
            HST_RFTC_RFU_0x0B39 = 0x0B39,
            HST_RFTC_RFU_0x0B3A = 0x0B3A,
            HST_RFTC_RFU_0x0B3B = 0x0B3B,
            HST_RFTC_RFU_0x0B3C = 0x0B3C,
            HST_RFTC_RFU_0x0B3D = 0x0B3D,
            HST_RFTC_RFU_0x0B3E = 0x0B3E,
            HST_RFTC_RFU_0x0B3F = 0x0B3F,
            HST_RFTC_FRQHOPMODE = 0x0B40,
            HST_RFTC_FRQHOPENTRYCNT = 0x0B41,
            HST_RFTC_FRQHOPTABLEINDEX = 0x0B42,
            MAC_RFTC_HOPCNT = 0x0B43,
            HST_RFTC_MINHOPDUR = 0x0B44,
            HST_RFTC_MAXHOPDUR = 0x0B45,
            HST_RFTC_FRQHOPRANDSEED = 0x0B46,
            MAC_RFTC_FRQHOPSHFTREGVAL = 0x0B47,
            MAC_RFTC_FRQHOPRANDNUMCNT = 0x0B48,
            HST_RFTC_FRQCHINDEX = 0x0B49,
            HST_RFTC_PLLLOCKTIMEOUT = 0x0B4A,
            HST_RFTC_PLLLOCK_DET_THRSH = 0x0B4B,
            HST_RFTC_PLLLOCK_DET_CNT = 0x0B4C,
            HST_RFTC_PLLLOCK_TO = 0x0B4D,
            HST_RFTC_BERREADDELAY = 0x0B4E,
            HST_RFTC_RFU_0x0B4F = 0x0B4F,
            MAC_RFTC_FWDRFPWRRAWADC = 0x0B50,
            MAC_RFTC_REVRFPWRRAWADC = 0x0B51,
            MAC_RFTC_ANTSENSERAWADC = 0x0B52,
            MAC_RFTC_AMBTEMPRAWADC = 0x0B53,
            MAC_RFTC_PATEMPRAWADC = 0x0B54,
            MAC_RFTC_XCVRTEMPRAWADC = 0x0B55,
            HST_RFTC_RFU_0x0B56 = 0x0B56,
            HST_RFTC_RFU_0x0B57 = 0x0B57,
            HST_RFTC_RFU_0x0B58 = 0x0B58,
            HST_RFTC_RFU_0x0B59 = 0x0B59,
            HST_RFTC_RFU_0x0B5A = 0x0B5A,
            HST_RFTC_RFU_0x0B5B = 0x0B5B,
            HST_RFTC_RFU_0x0B5C = 0x0B5C,
            HST_RFTC_RFU_0x0B5D = 0x0B5D,
            HST_RFTC_RFU_0x0B5E = 0x0B5E,
            HST_RFTC_RFU_0x0B5F = 0x0B5F,
            HST_RFTC_CURRENT_PROFILE = 0x0B60,
            HST_RFTC_PROF_SEL = 0x0B61,
            MAC_RFTC_PROF_CFG = 0x0B62,
            MAC_RFTC_PROF_ID_HIGH = 0x0B63,
            MAC_RFTC_PROF_ID_LOW = 0x0B64,
            MAC_RFTC_PROF_IDVER = 0x0B65,
            MAC_RFTC_PROF_PROTOCOL = 0x0B66,
            MAC_RFTC_PROF_R2TMODTYPE = 0x0B67,
            MAC_RFTC_PROF_TARI = 0x0B68,
            MAC_RFTC_PROF_X = 0x0B69,
            MAC_RFTC_PROF_PW = 0x0B6A,
            MAC_RFTC_PROF_RTCAL = 0x0B6B,
            MAC_RFTC_PROF_TRCAL = 0x0B6C,
            MAC_RFTC_PROF_DIVIDERATIO = 0x0B6D,
            MAC_RFTC_PROF_MILLERNUM = 0x0B6E,
            MAC_RFTC_PROF_T2RLINKFREQ = 0x0B6F,
            MAC_RFTC_PROF_VART2DELAY = 0x0B70,
            MAC_RFTC_PROF_RXDELAY = 0x0B71,
            MAC_RFTC_PROF_MINTOTT2DELAY = 0x0B72,
            MAC_RFTC_PROF_TXPROPDELAY = 0x0B73,
            MAC_RFTC_PROF_RSSIAVECFG = 0x0B74,
            MAC_RFTC_PROF_PREAMCMD = 0x0B75,
            MAC_RFTC_PROF_FSYNCCMD = 0x0B76,
            MAC_RFTC_PROF_T2WAITCMD = 0x0B77,
            HST_RFTC_RFU_0x0B78 = 0x0B78,
            HST_RFTC_RFU_0x0B79 = 0x0B79,
            HST_RFTC_RFU_0x0B7A = 0x0B7A,
            HST_RFTC_RFU_0x0B7B = 0x0B7B,
            HST_RFTC_RFU_0x0B7C = 0x0B7C,
            HST_RFTC_RFU_0x0B7D = 0x0B7D,
            HST_RFTC_RFU_0x0B7E = 0x0B7E,
            HST_RFTC_RFU_0x0B7F = 0x0B7F,
            HST_RFTC_RFU_0x0B80 = 0x0B80,
            HST_RFTC_RFU_0x0B81 = 0x0B81,
            HST_RFTC_RFU_0x0B82 = 0x0B82,
            HST_RFTC_RFU_0x0B83 = 0x0B83,
            HST_RFTC_RFU_0x0B84 = 0x0B84,
            
            HST_RFTC_FRQCH_ENTRYCNT = 0x0C00,
            HST_RFTC_FRQCH_SEL = 0x0C01,
            HST_RFTC_FRQCH_CFG = 0x0C02,
            HST_RFTC_FRQCH_DESC_PLLDIVMULT = 0x0C03,
            HST_RFTC_FRQCH_DESC_PLLDACCTL = 0x0C04,
            MAC_RFTC_FRQCH_DESC_PLLLOCKSTAT0 = 0x0C05,
            MAC_RFTC_FRQCH_DESC_PLLLOCKSTAT1 = 0x0C06,
            HST_RFTC_FRQCH_DESC_PARFU3 = 0x0C07,
            HST_RFTC_FRQCH_CMDSTART = 0x0C08,
            
            HST_CMD = 0xF000
        }

        private enum HST_CMD : uint
        {
            NV_MEM_UPDATE = 0x00000001, // Enter NV MEMORY UPDATE mode
            WROEM = 0x00000002, // Write OEM Configuration Area
            RDOEM = 0x00000003, // Read OEM Configuration Area
            ENGTST1 = 0x00000004, // Engineering Test Command #1
            MBPRDREG = 0x00000005, // R1000 firmware by-pass Read Register
            MBPWRREG = 0x00000006, // R1000 firmware by-pass Write Register
            RDGPIO = 0x0000000C, // Read GPIO
            WRGPIO = 0x0000000D, // Write GPIO
            CFGGPIO = 0x0000000E, // Configure GPIO
            INV = 0x0000000F, // ISO 18000-6C Inventory
            READ = 0x00000010, // ISO 18000-6C Read
            WRITE = 0x00000011, // ISO 18000-6C Write
            LOCK = 0x00000012, // ISO 18000-6C Lock
            KILL = 0x00000013, // ISO 18000-6C Kill
            SETPWRMGMTCFG = 0x00000014, // Set Power Management Configuration
            CLRERR = 0x00000015, // Clear Error
            CWON = 0x00000017, // Engineering CMD: Powers up CW
            CWOFF = 0x00000018, // Engineering CMD: Powers down CW
            UPDATELINKPROFILE = 0x00000019, // Changes the Link Profile
            CALIBRATE_GG = 0x0000001B, // Calibrate gross-gain settings
            LPROF_RDXCVRREG = 0x0000001C, // Read R1000 reg associated with given link profile
            LPROF_WRXCVRREG = 0x0000001D, // Write R1000 reg associated with given link profile
            BLOCKERASE = 0x0000001e, // ISO 18000-6C block erase
            BLOCKWRITE = 0x0000001f, // ISO 18000-6C block write
            POPULATE_SPURWATABLE = 0x00000020, // populate a local copy of the spur workaround table
            POPRFTCSENSLUTS = 0x00000021, // map the ADC readings to sensor-appropriate units
            BLOCKPERMALOCK,
            CUSTOMM4QT,
            CUSTOMG2XREADPROTECT,
            CUSTOMG2XRESETREADPROTECT,
            CUSTOMG2XCHANGEEAS,
            CUSTOMG2XEASALARM,
            CUSTOMG2XCHANGECONFIG,
            CUSTOMSLSETPASSWORD,
            CUSTOMSLSETLOGMODE,
            CUSTOMSLSETLOGLIMITS,
            CUSTOMSLGETMEASUREMENTSETUP,
            CUSTOMSLSETSFEPARA,
            CUSTOMSLSETCALDATA,
            CUSTOMSLENDLOG,
            CUSTOMSLSTARTLOG,
            CUSTOMSLGETLOGSTATE,
            CUSTOMSLGETCALDATA,
            CUSTOMSLGETBATLV,
            CUSTOMSLSETSHELFLIFE,
            CUSTOMSLINIT,
            CUSTOMSLGETSENSORVALUE,
            CUSTOMSLOPENAREA,
            CUSTOMSLACCESSFIFO,
            CUSTOMEM4324GETUID,
            CUSTOMEM4325GETUID,
            CUSTOMEMGETSENSORDATA,
            CUSTOMEMRESETALARMS,
            CUSTOMEMSENDSPI,
            CMD_TX_RANDOM_DATA = 0x3e,
            CMD_END
        }

        /*
        */

        private enum READERCMD : byte
        {
            CANCEL = 0x01,
            SOFTRESET = 0x02,
            ABORT = 0x03,
            PAUSE = 0x04,
            RESUME = 0x05,
            GETSERIALNUMBER = 0x06,
        }

        private bool COMM_Connect(string DeviceName)
        {
            byte[] CMDBuf = new byte[10];
            byte[] RecvBuf = new byte[100];

            // Init queue

            if (ReaderStatus != 0)
                return false;

            m_DeviceName = DeviceName;
            m_DeviceInterfaceType = INTERFACETYPE.UNKNOWN;
            m_macAddress = String.Empty;

            try
            {
                if (DeviceName.Substring(0, 3) == "USB")
                {
                    switch (GetOSVersion())
                    {
                        case OSVERSION.WIN32:
                        case OSVERSION.WINCE:
                            break;

                        default: // Only support USB mode in windows
                            return false;
                    }

                    string SN = DeviceName.Substring(3);
                    m_DeviceInterfaceType = INTERFACETYPE.USB;

                    if (USB_Connect(SN) == false)
                    {
                        m_Result = Result.FAILURE;
                        return false;
                    }
                }
                else if (DeviceName.Substring(0, 3) == "COM")
                {
                    m_DeviceInterfaceType = INTERFACETYPE.SERIAL;
                    if (SERIAL_Connect(DeviceName) == false)
                    {
                        m_Result = Result.FAILURE;
                        return false;
                    }
                }
                else
                {
                    m_DeviceInterfaceType = INTERFACETYPE.IPV4;
                    if (TCP_Connect(DeviceName) == false)
                    {
                        return false;
                    }
                    //byte[] mac = new byte[6];
                    //GetMacAddress(mac);
                    //m_save_readerName = m_macAddress = Hex.ToString(mac);
                }

                // StartOperation receive process

                ReaderStatus = 1;

                // for debuging
                //EngDumpAllReg();
                
                return true;
            }
            catch (Exception ex)
            {
                string a;
                a = ex.Message;
                COMM_Disconnect();
                return false;
            }
        }

        /// <summary>
        /// Disconnect
        /// </summary>
        /// <returns></returns>
        private bool COMM_Disconnect()
        {
            if (ReaderStatus == 0)
                return false;

            switch (m_DeviceInterfaceType)
            {
                case INTERFACETYPE.IPV4:
                    TCP_Disconnect();
                    break;

                case INTERFACETYPE.USB:
                    USB_Close();
                    break;

                case INTERFACETYPE.SERIAL:
                    break;
            }

            ReaderStatus = 0;
            return true;
        }

        /// <summary>
        /// Send data to Reader
        /// </summary>
        /// <returns></returns>
        private bool COMM_READER_Send(byte[] buffer, int offset, int size, int timeout)
        {
            bool result = false;

            DEBUGT_WriteLine(DEBUGLEVEL.READER_DATA_READ_WRITE, "Reader Write", buffer, offset, size);

            try
            {
                switch (m_DeviceInterfaceType)
                {
                    case INTERFACETYPE.SERIAL:
                        result = SERIAL_Send(buffer, offset, size, 5000);
                        break;

                    case INTERFACETYPE.IPV4:
                        result = TCP_Send(IntelCMD, buffer, offset, size);
                        break;

                    case INTERFACETYPE.USB:
                        result = USB_Send(buffer, offset, size);
                        break;
                }

                if (result)
                    return true;
            }
            catch (Exception ex)
            {
                DEBUGT_WriteLine(DEBUGLEVEL.READER_DATA_READ_WRITE, "Reader Write Fail!");
            }

            throw new ReaderException(m_Result = Result.NETWORK_LOST);
            return false;
        }

        /// <summary>
        /// Receive data from Reader
        /// </summary>
        /// <returns></returns>
        private bool COMM_READER_Recv(byte[] buffer, int offset, int size, uint timeout)
        {
            bool result = false;

            try
            {
                switch (m_DeviceInterfaceType)
                {
                    case INTERFACETYPE.SERIAL:
                        result = SERIAL_Recv(buffer, offset, size, timeout);
                        break;

                    case INTERFACETYPE.IPV4:
                        result = TCP_Recv(IntelCMD, buffer, offset, size, timeout);
                        break;

                    case INTERFACETYPE.USB:
                        result = USB_Recv(buffer, offset, size);
                        break;
                }

                if (result)
                {
                    DEBUGT_WriteLine(DEBUGLEVEL.READER_DATA_READ_WRITE, "COMM_READER_Recv " + size + " bytes", buffer, offset, size);
                }
                else
                {
                    DEBUGT_WriteLine(DEBUGLEVEL.READER_DATA_READ_WRITE, "COMM_READER_Recv No Data");
                }
            }
            catch (Exception ex)
            {
                DEBUGT_WriteLine(DEBUGLEVEL.READER_DATA_READ_WRITE, "COMM_READER_Recv Fail " + ex.Message);
                throw new ReaderException(m_Result = Result.NETWORK_LOST);
            }

            return result;
        }

        private bool COMM_INTBOARDALT_Cmd(UDP_CMD cmd, byte[] Parameter, byte[] Out)
        {
            return COMM_INTBOARDALT_Cmd(hostIP.Address, cmd, Parameter, Out);
        }

        private static bool COMM_INTBOARDALT_Cmd(long ipAdrr, long comIpAdrr, UDP_CMD cmd, byte[] Parameter, byte[] Out)
        {
            byte[] SendBuf = new byte[7 + Parameter.Length];

            SendBuf[0] = 0x80;//Header
            SendBuf[1] = (byte)(comIpAdrr >> 0);//Destination IP
            SendBuf[2] = (byte)(comIpAdrr >> 8);
            SendBuf[3] = (byte)(comIpAdrr >> 16);
            SendBuf[4] = (byte)(comIpAdrr >> 24);
            SendBuf[5] = (byte)Parameter.Length;//length
            SendBuf[6] = (byte)cmd;//Command

            if (Parameter.Length > 0)
                Array.Copy(Parameter, 0, SendBuf, 7, Parameter.Length);

            return Net_UDP_SendBCmd(ipAdrr, SendBuf, SendBuf.Length, Out);
        }

        private static bool COMM_INTBOARDALT_Cmd(long ipAdrr, UDP_CMD cmd, byte[] Parameter, byte[] Out)
        {
            byte[] SendBuf = new byte[7 + Parameter.Length];

            SendBuf[0] = 0x80;//Header
            SendBuf[1] = (byte)(ipAdrr >> 0);//Destination IP
            SendBuf[2] = (byte)(ipAdrr >> 8);
            SendBuf[3] = (byte)(ipAdrr >> 16);
            SendBuf[4] = (byte)(ipAdrr >> 24);
            SendBuf[5] = (byte)Parameter.Length;//length
            SendBuf[6] = (byte)cmd;//Command

            if (Parameter.Length > 0)
                Array.Copy(Parameter, 0, SendBuf, 7, Parameter.Length);

            return Net_UDP_SendBCmd(ipAdrr, SendBuf, SendBuf.Length, Out);
        }
        
        private Result RadioReadGpioPins (uint cmd, ref uint value)
	    {
            //UInt16 HST_GPIO_INMSK = 0x0600;

		    // Tell the MAC which GPIO pins to read
    		MacWriteRegister (MacRegister.HST_GPIO_INMSK, cmd);
            if (COMM_HostCommand (HST_CMD.RDGPIO) != Result.OK)
                return Result.FAILURE;

            value = m_GPIOStatus;

            return Result.OK;
        }

        private bool COMM_AdaprtCommand(READERCMD Cmd, ref byte [] CmdPacket)
        {
            if (CmdPacket.Length < 8)
                return false;

            CmdPacket[0] = 0x40;
            CmdPacket[1] = (byte)Cmd;
            for (int cnt = 2; cnt < 8; cnt++)
                CmdPacket[cnt] = 0;

            return true;
        }
        
        /// <summary>
        /// Send Command to Reader
        /// </summary>
        private bool COMM_AdapterCommand(READERCMD Cmd)
        {
            byte[] CMDBuf = new byte[8];

            if (Cmd == READERCMD.ABORT || Cmd == READERCMD.CANCEL)
                ReaderAbort = true;

            switch (m_DeviceInterfaceType)
            {
                case INTERFACETYPE.SERIAL:
                case INTERFACETYPE.IPV4:
                    if (!COMM_AdaprtCommand(Cmd, ref CMDBuf))
                        return false;
/*
                    cmdlen = 8;
                    CMDBuf[0] = 0x40;
                    CMDBuf[1] = (byte)Cmd;
                    for (int cnt = 2; cnt < cmdlen; cnt++)
                        CMDBuf[cnt] = 0;
*/
                    COMM_READER_Send(CMDBuf, 0, 8, 2000);

/* for old network proccessor firmware
                    if (m_DeviceInterfaceType == INTERFACETYPE.IPV4)
                    {
                        //System.Threading.Thread.Sleep(1);
                        COMM_READER_Send(CMDBuf, 0, 8, 2000);
                    }
*/
                    break;

                case INTERFACETYPE.USB:
                    if (USB_Control(Cmd) != true)
                        return false;
                    break;

                default:
                    return false;
            }

            System.Threading.Thread.Sleep(500);
            return true;
        }

        /// <summary>
        /// Execute Host Command
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>


        Single R2000RSSINB(byte rawRSSI)
        {
            int iMantissa = rawRSSI & 0x7;
            int iExponent = (rawRSSI >> 3) & 0x1F;

            double dRSSI = 20.0 * Math.Log10(Math.Pow(2.0, (double)iExponent) * (1.0 + ((double)iMantissa / 8.0)));
            return (Single)dRSSI;
        }

        Single R2000RSSIWB(byte rawRSSI)
        {
            int iMantissa = rawRSSI & 0x0f;
            int iExponent = (rawRSSI >> 4) & 0x0F;

            double dRSSI = 20.0 * Math.Log10(Math.Pow(2.0, (double)iExponent) * (1.0 + ((double)iMantissa / 16.0)));
            return (Single)dRSSI;
        }

        uint _debugcnt = 0;
        uint StopInventory = 0;

        object _HostCommandLock = new object();
        byte[] COMM_HostCommand_RecvBuf = new byte[500];
        private Result COMM_HostCommand(HST_CMD cmd)
        {
            DateTime timeOut;

            //DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Enter function : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            //DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Parameter cmd : " + cmd.ToString ());

            try
            {

                byte pkt_ver;
                byte flags;
                UInt16 pkt_type;
                UInt16 pkt_len;
                int extdatalen;

                try
                {
                    lock (_HostCommandLock)//  try
                    {
                        ReaderAbort = false;
                        LastMacErrorCode = 0;

/*
                        // for Smart antenna debug
                        if (cmd == HST_CMD.INV)
                        {
                            for (uint cnt = 0; cnt < 16; cnt++)
                            {
                                uint value = 0;

                                MacWriteRegister(MacRegister.HST_ANT_DESC_SEL, cnt);
                                MacReadRegister(MacRegister.HST_ANT_DESC_INV_CNT, ref value);
                            }
                        }
*/

                        if (cmd == HST_CMD.INV)
                            m_TagAccessStatus = 0;

                        CurrentOperationResult = Result.NO_TAG_FOUND;                        
                        if ((MacWriteRegister(MacRegister.HST_CMD /*0xf000*/, (UInt32)cmd)) != Result.OK)
                            return Result.FAILURE;

                        timeOut = DateTime.Now.AddSeconds (5);

                        // Reader Command Process
                        int ProcFlow = 0; // 0 = waiting command begin, 1 = command process and waiting command end, 2 = finish
                        m_TagAccessStatus = 0;

                        while (ProcFlow != 2)
                        {
                            if (ProcFlow == 0 && timeOut < DateTime.Now)
                                throw new ReaderException(m_Result = Result.NETWORK_LOST);

                            if (StopInventory == 1)
                            {
                                RadioAbortOperation();
                                StopInventory = 0;
                            }
                            else if (StopInventory == 2)
                            {
                                RadioCancelOperation();
                                StopInventory = 0;
                            }

                            DEBUGT_WriteLine(DEBUGLEVEL.READER_DATA_READ_WRITE, "Read Header");

                            if (COMM_READER_Recv(COMM_HostCommand_RecvBuf, 0, 8, 2000) == false)
                            {
                                if (ReaderAbort == true)
                                {
                                    ProcFlow = 2;
                                    //continue;
                                }
                                continue;
                                //                    LastError = ERRORCODE.TIMEOUT;
                                //return Result.FAILURE;
                            }

                            //a2 = DateTime.Now;
                            //if ((a2 - a1).TotalMilliseconds > 100)
                            //Console.WriteLine("abc");

                            if (COMM_HostCommand_RecvBuf[0] == 0x98 && COMM_HostCommand_RecvBuf[1] == 0x98 && COMM_HostCommand_RecvBuf[2] == 0x98 && COMM_HostCommand_RecvBuf[3] == 0x98 && COMM_HostCommand_RecvBuf[4] == 0x98 && COMM_HostCommand_RecvBuf[5] == 0x98 && COMM_HostCommand_RecvBuf[6] == 0x98 && COMM_HostCommand_RecvBuf[7] == 0x98)
                                continue;

                            // Abort command response
                            if (COMM_HostCommand_RecvBuf[0] == 0x40 && COMM_HostCommand_RecvBuf[1] == 0x03 && COMM_HostCommand_RecvBuf[2] == 0xbf && COMM_HostCommand_RecvBuf[3] == 0xfc && COMM_HostCommand_RecvBuf[4] == 0xbf && COMM_HostCommand_RecvBuf[5] == 0xfc && COMM_HostCommand_RecvBuf[7] == 0xfc)
                            {
                                if (ReaderAbort == true)
                                    ProcFlow = 2;

                                if (m_DeviceInterfaceType == INTERFACETYPE.IPV4)
                                {
                                    if (COMM_READER_Recv(CheckAbortRespBuffer, 0, 8, 0) == true)
                                    {
                                        if (CheckAbortRespBuffer[0] != 0x40 || CheckAbortRespBuffer[1] != 0x03)
                                        {
                                            CheckAbortRespBufferSize = 8;
                                        }
                                    }
                                }

                                continue;
                            }

                            pkt_ver = COMM_HostCommand_RecvBuf[0];
                            flags = COMM_HostCommand_RecvBuf[1];
                            pkt_type = (UInt16)((COMM_HostCommand_RecvBuf[2] + (COMM_HostCommand_RecvBuf[3] << 8)) & 0x7fff);
                            pkt_len = (UInt16)(COMM_HostCommand_RecvBuf[4] + (COMM_HostCommand_RecvBuf[5] << 8));
                            extdatalen = (pkt_len) * 4 - ((flags >> 6) & 3);

                            //#if DEBUG
                            //Console.Write("Read Body size {0} * ", pkt_len);
                            //#endif
                            //a1 = DateTime.Now;
                            DEBUGT_WriteLine(DEBUGLEVEL.READER_DATA_READ_WRITE, "Read Body");

                            if (pkt_ver < 0x01 || pkt_ver > 0x04)
                                return Result.FAILURE;
                            
                            if (pkt_len != 0)
                                if (COMM_READER_Recv(COMM_HostCommand_RecvBuf, 8, pkt_len * 4, 2000) == false)
                                {
                                    if (ReaderAbort == true)
                                    {
                                        ProcFlow = 2;
                                        continue;
                                    }
                                    return Result.FAILURE;
                                }

                            //a2 = DateTime.Now;
                            //if ((a2 - a1).TotalMilliseconds > 100)
                            //  Console.WriteLine("abc");

                            //a2 = Environment.TickCount;
                            //DEBUG_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, "Read Data Time (ms): Time:(" + (a2 - a1) + ")");

                            //a1 = Environment.TickCount;

                            switch (pkt_type)
                            {
                                // Command-Begin Packet
                                case 0x0000:
                                    ProcFlow = 1;
                                    break;

                                // Command-End Packet
                                case 0x0001:
                                    ErrorPort = (UInt16)(COMM_HostCommand_RecvBuf[14] | (COMM_HostCommand_RecvBuf[15] << 8));
                                    LastMacErrorCode = (UInt16)(COMM_HostCommand_RecvBuf[12] | (COMM_HostCommand_RecvBuf[13] << 8));

                                    if (ProcFlow == 1)
                                        ProcFlow = 2;

                                    if (_EngineeringTest_Operation != 0 && LastMacErrorCode != 0)
                                    {
                                        _EngineeringTest_Operation = 0;
                                        FireStateChangedEvent(RFState.IDLE);
                                    }

                                    break;

                                // Inventory-Response Packet
                                case 0x0005:
                                    {
                                        if (m_TagAccessStatus == 0)
                                            m_TagAccessStatus = 1;

                                        InventoryResponseCallBack(COMM_HostCommand_RecvBuf);

                                        //                                a2 = DateTime.Now;
                                        //if ((a2 - a1).TotalMilliseconds > 100)
                                        //                                    DEBUG_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, (a2 - a1).ToString ());
                                    }
                                    break;

                                // Tag-Access Packet
                                case 0x0006:
                                    if ((COMM_HostCommand_RecvBuf[1] & 0x0f) == 0x00)
                                    {
                                        if (m_TagAccessStatus == 0)
                                            m_TagAccessStatus = 1;

                                        if ((flags & 0x01) == 0)
                                        {
                                            m_TagAccessStatus = 2;
                                            TagAccessCallBack(COMM_HostCommand_RecvBuf);
                                        }
                                    }
                                    break;

                                // Antenna-Cycle-End Packet
                                case 0x0007:
                                    FireStateChangedEvent(RFState.ANT_CYCLE_END);
                                    State = RFState.BUSY;
                                    break;

                                case 0x000a: // RFID_PACKET_TYPE.INVENTORY_CYCLE_BEGIN
                                    LastMacErrorCode = (UInt16)(COMM_HostCommand_RecvBuf[6] | (COMM_HostCommand_RecvBuf[7] << 8));
                                    if (LastMacErrorCode == 0x00)
                                        FireStateChangedEvent(RFState.INVENTORY_CYCLE_BEGIN);
                                    else
                                        FireStateChangedEvent(RFState.INVENTORY_MAC_ERROR);
                                    State = RFState.BUSY;
                                    break;

                                // Read Tilden Register
                                case 0x3005:
                                    m_TildenReadValue = (ushort)(COMM_HostCommand_RecvBuf[10] | COMM_HostCommand_RecvBuf[11] << 8);
                                    break;

                                // GPIO
                                case 0x3006:
                                    m_GPIOStatus = (UInt32)(COMM_HostCommand_RecvBuf[8] | COMM_HostCommand_RecvBuf[9] << 8 | COMM_HostCommand_RecvBuf[10] << 16 | COMM_HostCommand_RecvBuf[11] << 24);
                                    break;

                                // RFID_PACKET_OEMCFG_READ
                                case 0x3007:
                                    m_OEMReadAdd = (UInt32)(COMM_HostCommand_RecvBuf[8] | COMM_HostCommand_RecvBuf[9] << 8 | COMM_HostCommand_RecvBuf[10] << 16 | COMM_HostCommand_RecvBuf[11] << 24);
                                    m_OEMReadData = (UInt32)(COMM_HostCommand_RecvBuf[12] | COMM_HostCommand_RecvBuf[13] << 8 | COMM_HostCommand_RecvBuf[14] << 16 | COMM_HostCommand_RecvBuf[15] << 24);
                                    break;

                                case 0x3008:
                                    if (_debugcnt++ >= 100)
                                    {
                                        _debugcnt = 0;
                                        _Debug_EngineeringTest_RSSI = R2000RSSIWB(COMM_HostCommand_RecvBuf[30]);
                                        _Debug_EngineeringTest_RSSI1 = COMM_HostCommand_RecvBuf[30];
                                        _Debug_EngineeringTest_RSSI2 = R2000RSSINB(COMM_HostCommand_RecvBuf[28]);
                                        _Debug_EngineeringTest_RSSI3 = COMM_HostCommand_RecvBuf[28];
                                        //FireStateChangedEvent(RFState.ENGTESTRESULT);
                                    }
                                    break;

                                case (UInt16)RFID_PACKET_TYPE.ANTENNA_END:
                                    {
                                        UInt16 res0 = (UInt16)(COMM_HostCommand_RecvBuf[6] + (COMM_HostCommand_RecvBuf[7] << 8));

                                        switch (res0)
                                        {
                                            case 0x00ff:
                                                if (ChannelStatus[COMM_HostCommand_RecvBuf[8]] != RFState.CH_BUSY)
                                                {
                                                    LBTPortBusy(COMM_HostCommand_RecvBuf[8]);
                                                }
                                                break;
                                        }
                                    }
                                    break;

                                case (UInt16)RFID_PACKET_TYPE.CARRIER_INFO:
                                    m_CurrentFreqChannelIndex = (UInt16)(COMM_HostCommand_RecvBuf[16] | COMM_HostCommand_RecvBuf[17] << 8);

                                    _Debug_Carrier_Info_Atmel_Time = (UInt32)(COMM_HostCommand_RecvBuf[8] | COMM_HostCommand_RecvBuf[9] << 8 | COMM_HostCommand_RecvBuf[10] << 16 | COMM_HostCommand_RecvBuf[11] << 24);
                                    _Debug_Carrier_Info_PLLDIVMULT = (UInt32)(COMM_HostCommand_RecvBuf[12] | COMM_HostCommand_RecvBuf[13] << 8 | COMM_HostCommand_RecvBuf[14] << 16 | COMM_HostCommand_RecvBuf[15] << 24);
                                    _Debug_Carrier_Info_CW_STATE = (UInt16)(COMM_HostCommand_RecvBuf[18] | COMM_HostCommand_RecvBuf[19] << 8);

                                    FireStateChangedEvent(RFState.CARRIER_INFO);
                                    State = RFState.BUSY;
                                    break;

/*
                                case 0x04: // Inventory Round Begin
                                    FireStateChangedEvent(RFState.INVENTORY_ROUND_BEGIN);
                                    State = RFState.BUSY;
                                    break;
                                
                                case 0x09:  // Inventory Round End
                                    FireStateChangedEvent(RFState.INVENTORY_ROUND_END);
                                    State = RFState.BUSY;
                                    break;
*/
                                case 0x1004: // Inventory Round Begin Diagnostics
                                    _Debug_Inventory_Round_Begin_Diagnostics_Atmel_Time = (UInt32)(COMM_HostCommand_RecvBuf[8] | COMM_HostCommand_RecvBuf[9] << 8 | COMM_HostCommand_RecvBuf[10] << 16 | COMM_HostCommand_RecvBuf[11] << 24);
                                    FireStateChangedEvent(RFState.INVENTORY_ROUND_BEGIN_DIAGNOSTICS);
                                    State = RFState.BUSY;
                                    break;

                                case 0x1005:  // Inventory Round End Diagnostics
                                    _Debug_Inventory_Round_End_Diagnostics_Atmel_Time = (UInt32)(COMM_HostCommand_RecvBuf[8] | COMM_HostCommand_RecvBuf[9] << 8 | COMM_HostCommand_RecvBuf[10] << 16 | COMM_HostCommand_RecvBuf[11] << 24);
                                    _Debug_Inventory_Round_End_Diagnostics_EPC_successfully_read = (UInt32)(COMM_HostCommand_RecvBuf[28] | COMM_HostCommand_RecvBuf[29] << 8 | COMM_HostCommand_RecvBuf[30] << 16 | COMM_HostCommand_RecvBuf[31] << 24);
                                    _Debug_Inventory_Round_End_Diagnostics_RN16 = (UInt32)(COMM_HostCommand_RecvBuf[16] | COMM_HostCommand_RecvBuf[17] << 8 | COMM_HostCommand_RecvBuf[18] << 16 | COMM_HostCommand_RecvBuf[19] << 24);
                                    _Debug_Inventory_Round_End_Diagnostics_RN16_timeout = (UInt32)(COMM_HostCommand_RecvBuf[20] | COMM_HostCommand_RecvBuf[21] << 8 | COMM_HostCommand_RecvBuf[22] << 16 | COMM_HostCommand_RecvBuf[23] << 24);
                                    _Debug_Inventory_Round_End_Diagnostics_EPC_timeout = (UInt32)(COMM_HostCommand_RecvBuf[24] | COMM_HostCommand_RecvBuf[25] << 8 | COMM_HostCommand_RecvBuf[26] << 16 | COMM_HostCommand_RecvBuf[27] << 24);
                                    _Debug_Inventory_Round_End_Diagnostics_CRC = (UInt32)(COMM_HostCommand_RecvBuf[32] | COMM_HostCommand_RecvBuf[33] << 8 | COMM_HostCommand_RecvBuf[34] << 16 | COMM_HostCommand_RecvBuf[35] << 24);

                                    FireStateChangedEvent(RFState.INVENTORY_ROUND_END_DIAGNOSTICS);
                                    State = RFState.BUSY;
                                    break;

                                case (UInt16)RFID_PACKET_TYPE.INVENTORY_CYCLE_END_DIAGS: // Inventory Cycle End Diagnostics
                                    _Debug_Inventory_Cycle_End_Diagnostics_EPC_RX = (UInt32)(COMM_HostCommand_RecvBuf[24] | COMM_HostCommand_RecvBuf[25] << 8 | COMM_HostCommand_RecvBuf[26] << 16 | COMM_HostCommand_RecvBuf[27] << 24);
                                    _Debug_Inventory_Cycle_End_Diagnostics_RN16 = (UInt32)(COMM_HostCommand_RecvBuf[12] | COMM_HostCommand_RecvBuf[13] << 8 | COMM_HostCommand_RecvBuf[14] << 16 | COMM_HostCommand_RecvBuf[15] << 24);
                                    FireStateChangedEvent(RFState.INVENTORY_CYCLE_END_DIAGNOSTICS);
                                    State = RFState.BUSY;
                                    break;

                                default:
                                    break;
                            }
                            //a2 = Environment.TickCount;
                            //DEBUG_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, "Proc Time (ms): Type(" + pkt_type+ ") Time:(" + (a2 - a1) + ")");
                        }
                    }
                }
                catch (Exception ex)
                {
                    //               Console.Write(ex.ToString());
                    DEBUG_WriteLine(DEBUGLEVEL.USB_IO_CONTROL, ex.Message);
                    return Result.FAILURE;
                }

                //Thread.Sleep(150); // 100-150
                return Result.OK;
            }
            finally
            {
                //DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Exit function : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Reset device
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private void COMM_Reboot()
        {
            switch (m_DeviceInterfaceType)
            {
                case INTERFACETYPE.SERIAL:
                    break;

                case INTERFACETYPE.IPV4:
                    COMM_Disconnect();
                    System.Threading.Thread.Sleep(4000);
                    {
                        DateTime timeout = DateTime.Now.AddMinutes(1);
                        while (DateTime.Now < timeout)
                        {
                            if (COMM_Connect(m_DeviceName))
                                break;

                            Thread.Sleep(1000);
                        }
                    }
                    break;

                case INTERFACETYPE.USB:

                    if (m_oem_machine == Machine.CS101)
                    {
                        USB_Close();
                        RFID_PowerOnOff(1); // Power Off RFID Reader
                        System.Threading.Thread.Sleep(100);
                        RFID_PowerOnOff(0); // Power On RFID Reader
                    }
                    else
                    {
                        USB_Control(READERCMD.SOFTRESET);
                        USB_Close();
                    }

                    System.Threading.Thread.Sleep(4000);
                    ReaderStatus = 0;
                    COMM_Connect(m_DeviceName);
                    break;
            }
        }
    }
}
