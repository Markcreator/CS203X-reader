using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    using Constants;
    /// <summary>
    /// FirmwareUpgradeArgs
    /// </summary>
    public class FirmwareUpgradeArgs
    {
        private int mProgress;
        private FirmwareUpdateResult mResult;
        /// <summary>
        /// progress
        /// </summary>
        public int progress
        {
            get { return mProgress; }
        }
        /// <summary>
        /// Firmware update result
        /// </summary>
        public FirmwareUpdateResult result
        {
            get { return mResult; }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="result"></param>
        public FirmwareUpgradeArgs(int progress, FirmwareUpdateResult result)
        {
            this.mProgress = progress;
            this.mResult = result;
        }
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Progress = {0:D3} %, result = {1}", mProgress, mResult);
        }
    }
}
