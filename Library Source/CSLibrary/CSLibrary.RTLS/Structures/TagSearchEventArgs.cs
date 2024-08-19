using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    using Constants;
    /// <summary>
    /// TagSearchEventArgs
    /// </summary>
    public class TagSearchEventArgs : EventArgs
    {
        /// <summary>
        /// searchIndex
        /// </summary>
        public byte searchIndex = 0;
        /// <summary>
        /// tag ID
        /// </summary>
        public byte[] tagID = new byte[6];
        /// <summary>
        /// distance, valid range from 0-65525
        /// </summary>
        public ushort distance = 0;
        /// <summary>
        /// RSSI
        /// </summary>
        public byte rssi = 0;
        /// <summary>
        /// Error code
        /// </summary>
        public ErrorCode errorCode = ErrorCode.UNKNOWN;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="searchIndex"></param>
        /// <param name="tagID"></param>
        /// <param name="errorCode"></param>
        /// <param name="distance"></param>
        /// <param name="rssi"></param>
        public TagSearchEventArgs(
            byte searchIndex,
            byte[] tagID,
            ErrorCode errorCode,
            ushort distance,
            byte rssi)
        {
            this.searchIndex = searchIndex;
            this.tagID = (byte[])tagID.Clone();
            this.distance = distance;
            this.rssi = rssi;
            this.errorCode = errorCode;
        }

        internal static TagSearchEventArgs Parse(byte[] raw)
        {
            if (raw == null || raw.Length != 11)
            {
                return null;
            }
            byte[] id = new byte[6];
            Array.Copy(raw,1, id,0,6);
            return new TagSearchEventArgs(
                raw[0],
                id,
                (ErrorCode)raw[7],
                (ushort)((raw[8] << 8 | raw[9] << 0) > 800 ? (ushort)(raw[8] << 8 | raw[9] << 0) - 800  : 0),
                raw[10]);
        }
    }
}
