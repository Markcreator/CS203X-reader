using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Constants
{
    /// <summary>
    /// Result
    /// </summary>
    public enum Result
    {
        /// <summary>
        /// Success
        /// </summary>
        OK = 0,
        /// <summary>
        /// Attempted to open a radio that is already open
        /// </summary>
        ALREADY_OPEN = -9999,
        /// <summary>
        /// General failure 
        /// </summary>
        FAILURE,
        /// <summary>
        /// One of the parameters to the function is invalid
        /// </summary>
        INVALID_PARAMETER,
        /// <summary>
        /// Library has not been successfully initialized
        /// </summary>
        NOT_INITIALIZED,
        /// <summary>
        /// Function not supported
        /// </summary>
        NOT_SUPPORTED,
        /// <summary>
        /// Op cancelled by cancel op func, close radio, or library shutdown
        /// </summary>
        OPERATION_CANCELLED,
        /// <summary>
        /// Library encountered an error allocating memory
        /// </summary>
        OUT_OF_MEMORY,
        /// <summary>
        /// The operation cannot be performed because the radio is currently busy
        /// </summary>
        RADIO_BUSY,
        /// <summary>
        /// Device is booting up, please wait a moment
        /// </summary>
        DEVICE_NOT_READY
    }
}
