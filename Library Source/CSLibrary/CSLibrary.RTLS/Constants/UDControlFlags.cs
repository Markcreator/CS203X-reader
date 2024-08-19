using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Constants
{
    /// <summary>
    /// UDControlFlags
    /// </summary>
    [Flags]
    public enum UDControlFlags : byte
    {
        /// <summary>
        /// RFDefault
        /// </summary>
        RFDefault = 0,
        /// <summary>
        /// RFAssign
        /// </summary>
        RFAssign = 1,
        /// <summary>
        /// RFPowerON
        /// </summary>
        RFPowerON = 0,
        /// <summary>
        /// RFPowerOFF
        /// </summary>
        RFPowerOFF = 2,
        /// <summary>
        /// RangingON
        /// </summary>
        RangingON = 0,
        /// <summary>
        /// RangingOFF
        /// </summary>
        RangingOFF = 4,
        /// <summary>
        /// RangingDataOFF
        /// </summary>
        RangingDataOFF = 0,
        /// <summary>
        /// RangingDataON
        /// </summary>
        RangingDataON = 8,
        /// <summary>
        /// AlertOff
        /// </summary>
        AlertOff = 0,
        /// <summary>
        /// AlertOn
        /// </summary>
        AlertOn = 16,
        /// <summary>
        /// GetStatus
        /// </summary>
        GetStatus = 0,
        /// <summary>
        /// SetStatus
        /// </summary>
        SetStatus = 128
    }
}
