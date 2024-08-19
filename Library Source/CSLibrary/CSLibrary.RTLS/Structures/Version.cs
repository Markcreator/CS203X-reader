using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    /// <summary>
    /// Versiob
    /// </summary>
    public class Version
    {
        /// <summary>
        /// Major
        /// </summary>
        public UInt32 Major = 0;
        /// <summary>
        /// Minor
        /// </summary>
        public UInt32 Minor = 0;
        /// <summary>
        /// Maintenance
        /// </summary>
        public UInt32 Maintenance = 0;
        /// <summary>
        /// Development
        /// </summary>
        public UInt32 Development = 0;
        /// <summary>
        /// Constructor
        /// </summary>
        public Version() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="version"></param>
        internal Version(byte[] version)
        {
            if (version == null || version.Length != 4)
                return;
            Major = (UInt32)version[0];
            Minor = (UInt32)version[1];
            Maintenance = (UInt32)version[2];
            Development = (UInt32)version[3];
        }
        internal static Version Parse(byte[] version)
        {
            return new Version(version);
        }
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Maintenance, Development);
        }
    }
}
