using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Constants
{
    /// <summary>
    /// TagSearchTxOptionFlags
    /// </summary>
    public enum TagSearchTxOptionFlags : byte
    {
        /// <summary>
        /// StopSearch
        /// </summary>
        StopSearch = 0,
        /// <summary>
        /// StartSearch
        /// </summary>
        StartSearch = 1,
        //public const byte AnyTime = 0;
        /// <summary>
        /// SlotTime
        /// </summary>
        SlotTime = 0x2,
        //public const byte CsmaOrTdma = 0;
        /// <summary>
        /// CsmaOnly
        /// </summary>
        CsmaOnly = 0x4,
        /// <summary>
        /// UIDOnly
        /// </summary>
        UIDOnly = 0x8,
    }
}
