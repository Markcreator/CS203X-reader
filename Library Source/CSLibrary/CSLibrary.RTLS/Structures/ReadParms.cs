using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    using Constants;
    /// <summary>
    /// ReadParms
    /// </summary>
    public class ReadParms : IOperationParms
    {
        /// <summary>
        /// Tag operation
        /// </summary>
        public Operation Operation
        { 
            get { return Operation.Read;} 
        }
        /// <summary>
        /// specific Led blink upon on good read
        /// </summary>
        public byte ledBlinkOnGoodRead = 0;
        /// <summary>
        /// Constructor
        /// </summary>
        public ReadParms()
            : this(0)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ledBlinkOnGoodRead">specific Led blink upon on good read</param>
        public ReadParms(byte ledBlinkOnGoodRead)
        {
            this.ledBlinkOnGoodRead = ledBlinkOnGoodRead;
        }
    }
}
