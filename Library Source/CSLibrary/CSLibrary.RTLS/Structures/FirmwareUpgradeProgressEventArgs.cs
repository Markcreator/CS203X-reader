using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    /// <summary>
    /// FirmwareUpgradeProgressEventArgs
    /// </summary>
    public class FirmwareUpgradeProgressEventArgs : EventArgs
    {
        /// <summary>
        /// upgrade percentage
        /// </summary>
        public uint percent = 0;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="percent"></param>
        public FirmwareUpgradeProgressEventArgs(uint percent)
        {
            this.percent = percent;
        }
    }
}
