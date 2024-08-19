using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Constants
{
    /// <summary>
    /// Error code
    /// </summary>
    public enum ErrorCode : byte
    {
        /// <summary>
        /// 0x00 - No Error
        /// </summary>
        NO_ERROR = 0x00,
        /// <summary>
        /// 0x01 - No Response
        /// </summary>
        NO_RESPONSE = 0x01,
        /// <summary>
        /// 0x02 - Fail 2
        /// </summary>
        FAIL_2 = 0x02,
        /// <summary>
        /// 0x03 - Fail 3
        /// </summary>
        FAIL_3 = 0x03,
        /// <summary>
        /// 0x04 - Fail 4
        /// </summary>
        FAIL_4 = 0x04,
        /// <summary>
        /// 0x20 - Error of Operation mode for that WDP
        /// </summary>
        ERROR_WDP_MODE_NG = 0x20,
        /// <summary>
        /// 0x21 - Error of WDP type
        /// </summary>
        ERROR_WDP_TYPE_NG = 0x21,
        /// <summary>
        /// 0x22 - Error of sub-data of an WDP type
        /// </summary>
        ERROR_WDP_SUBTYPE_NG = 0x22,
        /// <summary>
        /// 0x23 - Error in payload range for that WDP
        /// </summary>
        ERROR_WDP_PAYLOAD_RANGE_NG = 0x23,
        /// <summary>
        /// 0x24 - Error in payload data in WDP
        /// </summary>
        ERROR_WDP_PAYLOAD_DATA_NG = 0x24,
        /// <summary>
        /// 0x25 - Error in repeated payload for that WDP
        /// </summary>
        ERROR_WDP_PAYLOAD_REP_NG = 0x25,
        /// <summary>
        /// 0x30 - Ok in writing the last information flash
        /// </summary>
        ERROR_WRITEINFO_OK = 0x30,
        /// <summary>
        /// 0x31 - Error in writing the last information flash
        /// </summary>
        ERROR_WRITEINFO_NG = 0x31,
        /// <summary>
        /// 0x32 - Error in retry writing information flash
        /// </summary>
        ERROR_RETRY_WRITEINFO_NG = 0x32,
        /// <summary>
        /// 0x33 - Error in receiving the break loop request from server as outside the loop
        /// </summary>
        ERROR_REQ_LOOPBREAK_NG = 0x33,
        /// <summary>
        /// 0x34 - Ok in receiving the break loop request from server at firmware updating
        /// </summary>
        ERROR_REQ_LOOPBREAK_AT_FWUG_OK = 0x34,
        /// <summary>
        /// 0x40 - Error in receiving RECD
        /// </summary>
        ERROR_WDP_RECD_REC_NG = 0x40,
        /// <summary>
        /// 0x41 - Error of payload data of 1st Rec¡¦d
        /// </summary>
        ERROR_WDP_RECD_REC_DATA1_NG = 0x41,
        /// <summary>
        /// 0x42 - Error of payload data of 2nd Rec¡¦d
        /// </summary>
        ERROR_WDP_RECD_REC_DATA2_NG = 0x42,
        /// <summary>
        /// 0x43 ¡V 0x45 - Error in receiving RECD in 2nd time instance of the process
        /// </summary>
        ERROR_WDP_RECD2_43 = 0x43,
        /// <summary>
        /// 0x43 ¡V 0x45 - Error in receiving RECD in 2nd time instance of the process
        /// </summary>
        ERROR_WDP_RECD2_44 = 0x44,
        /// <summary>
        /// 0x43 ¡V 0x45 - Error in receiving RECD in 2nd time instance of the process
        /// </summary>
        ERROR_WDP_RECD2_45 = 0x45,
        /// <summary>
        /// 0x46 ¡V 0x48 - Error in receiving RECD in 3rd time instance of the process
        /// </summary>
        ERROR_WDP_RECD3_46 = 0x46,
        /// <summary>
        /// 0x46 ¡V 0x48 - Error in receiving RECD in 3rd time instance of the process
        /// </summary>
        ERROR_WDP_RECD3_47 = 0x47,
        /// <summary>
        /// 0x46 ¡V 0x48 - Error in receiving RECD in 3rd time instance of the process
        /// </summary>
        ERROR_WDP_RECD3_48 = 0x48,
        /// <summary>
        /// 0x49 - Error in sending RECD
        /// </summary>
        ERROR_WDP_RECD_SEND_NG = 0x49,
        /// <summary>
        /// 0x50 - Error in address range
        /// </summary>
        ERROR_WRITEFLASH_ADDRANGE_NG = 0x50,
        /// <summary>
        /// 0x51 - Error in not-blank area
        /// </summary>
        ERROR_WRITEFLASH_NOTBLANK_NG = 0x51,
        /// <summary>
        /// 0x52 - Error in read back not equal
        /// </summary>
        ERROR_WRITEFLASH_READNEQ_NG = 0x52,
        /// <summary>
        /// 0x53 ¡V 0x55 - Error in writing information flash
        /// </summary>
        ERROR_WRITEFLASH_INFO_53 = 0x53,
        /// <summary>
        /// 0x53 ¡V 0x55 - Error in writing information flash
        /// </summary>
        ERROR_WRITEFLASH_INFO_54 = 0x54,
        /// <summary>
        /// 0x53 ¡V 0x55 - Error in writing information flash
        /// </summary>
        ERROR_WRITEFLASH_INFO_55 = 0x55,
        /// <summary>
        /// 0x5C - Error due to busy on Flash writing
        /// </summary>
        ERROR_FWUG_BUSY_NG = 0x5C,
        /// <summary>
        /// 0x5D - Error in different Block Size
        /// </summary>
        ERROR_FWUG_BSIZE_DIFF_NG = 0x5D,
        /// <summary>
        /// 0x5E - Error not in appropriate firmware upgrade mode for firmware upgrade
        /// </summary>
        ERROR_FWUG_MODE_NG = 0x5E,
        /// <summary>
        /// 0x5F - Error in slot location for firmware upgrade
        /// </summary>
        ERROR_FWUG_SLOT_LIMIT_NG = 0x5F,
        /// <summary>
        /// 0x60 - Error in block size
        /// </summary>
        ERROR_FWUG_BSIZE_LIMIT_NG = 0x60,
        /// <summary>
        /// 0x61 - Error in module limit
        /// </summary>
        ERROR_FWUG_MODULE_LIMIT_NG = 0x61,
        /// <summary>
        /// 0x62 - Error in block index limit
        /// </summary>
        ERROR_FWUG_INDEX_LIMIT_NG = 0x62,
        /// <summary>
        /// 0x63 - Error in different modules for different firmware upgrade block
        /// </summary>
        ERROR_FWUG_MODULE_DIFF_NG = 0x63,
        /// <summary>
        /// 0x64 - Error in different total for different firmware upgrade block
        /// </summary>
        ERROR_FWUG_TOTAL_DIFF_NG = 0x64,
        /// <summary>
        /// 0x65 - Error in block index greater than block total
        /// </summary>
        ERROR_FWUG_INDEX_GTTOTAL_NG = 0x65,
        /// <summary>
        /// 0x66 - Error in having different source id from different block
        /// </summary>
        ERROR_FWUG_ID_DIFF_NG = 0x66,
        /// <summary>
        /// 0x67 - Error in total limit
        /// </summary>
        ERROR_FWUG_TOTAL_LIMIT_NG = 0x67,
        /// <summary>
        /// 0x68 - Error in checksum of the data block
        /// </summary>
        ERROR_FWUG_CHECKSUM_NG_DATAIN = 0x68,
        /// <summary>
        /// 0x69 - Error in total checksum of the all data blocks
        /// </summary>
        ERROR_FWUG_CHECKSUM_NG_ALL_DATAIN = 0x69,
        /// <summary>
        /// 0x6A - Error in checksum2 with total checksum of the downloaded data
        /// </summary>
        ERROR_FWUG_CHECKSUM2_NG_DOWNLOAD = 0x6A,
        /// <summary>
        /// 0x6B - Ok and reset after checking checksum 2 against total downloaded data
        /// </summary>
        ERROR_FWUG_CHECKSUM2_OK_RESET = 0x6B,
        /// <summary>
        /// 0x6C - Ok and start burning after checking checksum 2 against total download data
        /// </summary>
        ERROR_FWUG_CHECKSUM2_OK_START = 0x6C,
        /// <summary>
        /// 0x6D - Invalid AnchorTagID
        /// </summary>
        ERROR_FWUG_ANCHORTAGID_NG = 0x6D,
        /// <summary>
        /// 0x6E - Invalid RelayAnchor number
        /// </summary>
        ERROR_FWUG_RELAYANCHOR_NG = 0x6E,
        /// <summary>
        /// 0x6F - Invalid Firmware type
        /// </summary>
        ERROR_FWUG_FIRMWARETYPE_NG = 0x6F,
        /*/// <summary>
        /// 0x00 - Ok Receive at Server-Master data
        /// </summary>
        ERROR_OKREC_AT_SMDATA = 0x00,
        /// <summary>
        /// 0x00 - Ok receive at Tag-Master data
        /// </summary>
        ERROR_OKREC_AT_TMDATA = 0x00,*/
        /// <summary>
        /// 0x70 - Error of busy at previous update
        /// </summary>
        ERROR_BUSY_AT_PREV_UPDATE = 0x70,
        /// <summary>
        /// 0x71 - Error of length at Server-Master data
        /// </summary>
        ERROR_LENGTH_AT_SMDATA = 0x71,
        /// <summary>
        /// 0x72 - Error of range at Server-Master data
        /// </summary>
        ERROR_RANGE_AT_SMDATA = 0x72,
        /// <summary>
        /// 0x73 - Ok at end of procedurc
        /// </summary>
        ERROR_OK_AT_ENDPROC = 0x73,
        /// <summary>
        /// 0x80 - Error of Retry at Master-Tag data transmission
        /// </summary>
        ERROR_RETRY_AT_MTDATASEND = 0x80,
        /// <summary>
        /// 0x81 - Error at non-idle when Master-Tag data transmission
        /// </summary>
        ERROR_NIDLE_AT_MTDATASEND = 0x81,
        /// <summary>
        /// 0x82 ¡V 0x83 - Error at data transmission in 2nd time instance of the process
        /// </summary>
        ERROR_AT_MTDATASEND1_82 = 0x82,
        /// <summary>
        /// 0x82 ¡V 0x83 - Error at data transmission in 2nd time instance of the process
        /// </summary>
        ERROR_AT_MTDATASEND1_83 = 0x83,
        /// <summary>
        /// unknown error
        /// </summary>
        UNKNOWN = 0xff
    }
}
