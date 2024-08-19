using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    using Constants;
    /// <summary>
    /// Searching parameters
    /// </summary>
    public class SearchingParms : IOperationParms
    {
        /// <summary>
        /// Operation
        /// </summary>
        public Operation Operation
        {
            get { return Operation.Searching; }
        }
        /// <summary>
        /// specific Led blink upon on good read
        /// </summary>
        public byte ledBlinkOnGoodRead = 0;
        /// <summary>
        /// this threshold use for stabilzed rssi and distance
        /// <para>Default value is 8</para>
        /// </summary>
        public int threshold = 8;
        /// <summary>
        /// SelectMask, must be in 6 bytes
        /// </summary>
        public Byte[] mask = new byte[6];
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mask">SelectMask, must be in 6 bytes</param>
        public SearchingParms(Byte[] mask) 
            : this(0, 8, mask)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="threshold">
        /// this threshold use for stabilzed rssi and distance</param>
        /// <param name="mask">SelectMask, must be in 6 bytes</param>
        public SearchingParms(int threshold, Byte[] mask) 
            : this(0, threshold, mask)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ledBlinkOnGoodRead">specific Led blink upon on good read</param>
        /// <param name="mask">SelectMask, must be in 6 bytes</param>
        public SearchingParms(byte ledBlinkOnGoodRead, Byte[] mask)
            : this(ledBlinkOnGoodRead, 8, mask)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ledBlinkOnGoodRead">specific Led blink upon on good read</param>
        /// <param name="threshold">
        /// this threshold use for stabilzed rssi and distance</param>
        /// <param name="mask">SelectMask, must be in 6 bytes</param>
        public SearchingParms(byte ledBlinkOnGoodRead, int threshold, Byte[] mask)
        {
            this.mask = (Byte[])mask.Clone();
            this.threshold = threshold;
            this.ledBlinkOnGoodRead = ledBlinkOnGoodRead;
        }
    }
}
