using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    using Constants;
    /// <summary>
    /// Ranging parameters
    /// </summary>
    public class RangingParms : IOperationParms
    {
        /// <summary>
        /// Operation
        /// </summary>
        public Operation Operation
        {
            get { return Operation.Ranging; }
        }
        /// <summary>
        /// specific Led blink upon on good read
        /// </summary>
        public byte ledBlinkOnGoodRead = 0;
        /// <summary>
        /// SelectMask flags
        /// </summary>
        public IDFilterFlags flags = IDFilterFlags.NONE;
        /// <summary>
        /// Constructor
        /// </summary>
        public RangingParms()
            : this(0, IDFilterFlags.NONE)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="flags">SelectMask flags</param>
        public RangingParms(IDFilterFlags flags)
            : this (0, flags)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ledBlinkOnGoodRead">specific Led blink upon on good read</param>
        public RangingParms(byte ledBlinkOnGoodRead)
            : this(ledBlinkOnGoodRead, IDFilterFlags.NONE)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ledBlinkOnGoodRead">specific Led blink upon on good read</param>
        /// <param name="flags">SelectMask flags</param>
        public RangingParms(byte ledBlinkOnGoodRead, IDFilterFlags flags)
        {
            this.flags = flags;
            this.ledBlinkOnGoodRead = ledBlinkOnGoodRead;
        }
    }
}
