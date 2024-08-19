using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Constants
{
    /// <summary>
    /// Operation Flags
    /// </summary>
    public enum IDFilterFlags
    {
        /// <summary>
        /// no filter applied
        /// </summary>
        NONE = 0x0000000,
        /// <summary>
        /// Use mask filtering
        /// </summary>
        MASK = 0x00000001,
        /// <summary>
        /// Unknown flag
        /// </summary>
        UNKNOWN,
    }
}
