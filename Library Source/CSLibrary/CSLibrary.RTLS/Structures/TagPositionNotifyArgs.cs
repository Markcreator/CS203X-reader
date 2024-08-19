using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    /// <summary>
    /// TagPositionNotifyArgs
    /// </summary>
    public class TagPositionNotifyArgs
    {
        /// <summary>
        /// TagAnchorID
        /// </summary>
        public byte[] TagAnchorID = new byte[6];
        /// <summary>
        /// xPosition
        /// </summary>
        public ushort xPosition = 0;
        /// <summary>
        /// yPosition
        /// </summary>
        public ushort yPosition = 0;
        /// <summary>
        /// zPosition
        /// </summary>
        public ushort zPosition = 0;
        /// <summary>
        /// timezone
        /// </summary>
        public int timezone = 0;
        /// <summary>
        /// dayLightSaving
        /// </summary>
        public int dayLightSaving = 0;
        /// <summary>
        /// second
        /// </summary>
        public int second = 0;
        /// <summary>
        /// misecond
        /// </summary>
        public int misecond = 0;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <param name="zPos"></param>
        /// <param name="tz"></param>
        /// <param name="dls"></param>
        /// <param name="sec"></param>
        /// <param name="msec"></param>
        public TagPositionNotifyArgs(
            byte[] id,
            ushort xPos,
            ushort yPos,
            ushort zPos,
            int tz,
            int dls,
            int sec,
            int msec
            )
        {
            this.TagAnchorID = (byte[])id.Clone();
            this.xPosition = xPos;
            this.yPosition = yPos;
            this.zPosition = zPos;
            this.timezone = tz;
            this.dayLightSaving = dls;
            this.second = sec;
            this.misecond = msec;
        }

        internal static TagPositionNotifyArgs Decode(Byte[] raw)
        {
            if (raw == null || raw.Length != 20)
            {
                return null;
            }
            byte[] id = new byte[6];
            Array.Copy(raw, 0, id, 0, 6);
            return new TagPositionNotifyArgs(
                id,
                (ushort)(raw[6] << 8 | raw[7]),
                (ushort)(raw[8] << 8 | raw[9]),
                (ushort)(raw[10] << 8 | raw[11]),
                raw[12],
                raw[13],
                (int)(raw[14] << 24 | raw[15] << 16 | raw[16] << 8 | raw[17]),
                (int)(raw[18] << 8 | raw[19]));
        }
    }
}
