using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    /// <summary>
    /// UDControlArgs
    /// </summary>
    public class UDControlArgs
    {
        private BitVector32 flags;
        private static readonly int RF_POWER_ASSIGN;
        private static readonly int RF_POWER;
        private static readonly int RF_RANGING_ASSIGN;
        private static readonly int RF_RANGING;
        private static readonly int RF_ALERT;
        private static readonly int RF_APPLY;
        /// <summary>
        /// IsRFAssigned
        /// </summary>
        public bool IsRFAssigned
        {
            get { return flags[RF_POWER_ASSIGN]; }
            set { flags[RF_POWER_ASSIGN] = value; }
        }
        /// <summary>
        /// IsRFPowerON
        /// </summary>
        public bool IsRFPowerON
        {
            get { return flags[RF_POWER]; }
            set { flags[RF_POWER] = value; }
        }
        /// <summary>
        /// IsRangingON
        /// </summary>
        public bool IsRangingON
        {
            get { return !flags[RF_RANGING_ASSIGN]; }
            set { flags[RF_RANGING_ASSIGN] = !value; }
        }
        /// <summary>
        /// IsRangingDataON
        /// </summary>
        public bool IsRangingDataON
        {
            get { return flags[RF_RANGING]; }
            set { flags[RF_RANGING] = value; }
        }
        /// <summary>
        /// IsAlertON
        /// </summary>
        public bool IsAlertON
        {
            get { return flags[RF_ALERT]; }
            set { flags[RF_ALERT] = value; }
        }
        /// <summary>
        /// IsApplyChanged
        /// </summary>
        public bool IsApplyChanged
        {
            get { return flags[RF_APPLY]; }
            set { flags[RF_APPLY] = value; }
        }

        static UDControlArgs()
        {
            RF_POWER_ASSIGN = BitVector32.CreateMask();
            RF_POWER = BitVector32.CreateMask(RF_POWER_ASSIGN);
            RF_RANGING_ASSIGN = BitVector32.CreateMask(RF_POWER);
            RF_RANGING = BitVector32.CreateMask(RF_RANGING_ASSIGN);
            RF_ALERT = BitVector32.CreateMask(RF_RANGING);
            RF_ALERT = BitVector32.CreateMask(RF_ALERT);
            RF_ALERT = BitVector32.CreateMask(RF_ALERT);
            RF_APPLY = BitVector32.CreateMask(RF_ALERT);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="raw"></param>
        public UDControlArgs(Byte raw)
        {
            flags = new BitVector32(raw);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public UDControlArgs()
        {
        }
        internal Byte Encode()
        {
            return (Byte)flags.Data;
        }

        internal static UDControlArgs Decode(Byte raw)
        {
            UDControlArgs arg = new UDControlArgs(raw);
            return arg;
        }
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("IsRFAssigned:{0},IsRFPowerON:{1},IsRangingON:{2},IsRangingDataON:{3},IsAlertON:{4},IsApplyChanged:{5},",
                IsRFAssigned,
                IsRFPowerON,
                IsRangingON,
                IsRangingDataON,
                IsAlertON,
                IsApplyChanged);
        }
    }
}
