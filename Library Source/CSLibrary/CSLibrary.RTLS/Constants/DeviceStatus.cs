using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Constants
{
    /// <summary>
    /// DeviceStatus
    /// </summary>
    public enum DeviceStatus
    {
        /// <summary>
        /// Bootloader mode
        /// </summary>
        Bootloader,
        /// <summary>
        /// Device ready
        /// </summary>
        Idle,
        /// <summary>
        /// Device busy
        /// </summary>
        Busy,
        /// <summary>
        /// Device stopping
        /// </summary>
        Stop,
        /// <summary>
        /// catch error
        /// </summary>
        Error,
        /// <summary>
        /// Unknown state
        /// </summary>
        Unknown
    }
}
