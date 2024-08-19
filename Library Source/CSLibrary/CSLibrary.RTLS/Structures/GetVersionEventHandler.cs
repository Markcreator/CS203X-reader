using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    /// <summary>
    /// GetVersionEventArgs
    /// </summary>
    public class GetVersionEventArgs : EventArgs
    {
        /// <summary>
        /// MSP430Version
        /// </summary>
        public Version MSP430Version;
        /// <summary>
        /// BootloaderVersion
        /// </summary>
        public Version BootloaderVersion;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="MSP430Version"></param>
        /// <param name="BootloaderVersion"></param>
        public GetVersionEventArgs(Version MSP430Version, Version BootloaderVersion)
        {
            this.MSP430Version = MSP430Version;
            this.BootloaderVersion = BootloaderVersion;
        }
        internal static GetVersionEventArgs Parse(byte[] raw)
        {
                if (raw == null || raw.Length != 8)
                {
                    return null;
                }
                Byte[] MSP430V = new byte[4];
                Byte[] BLVersion = new byte[4];
                Array.Copy(raw, 0, MSP430V, 0, 4);
                Array.Copy(raw, 4, BLVersion, 0, 4);

                return new GetVersionEventArgs(
                    Version.Parse(MSP430V),
                    Version.Parse(BLVersion));
        }
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("MSP430[{0}],BL[{1}]", MSP430Version, BootloaderVersion);
        }
    }
}
