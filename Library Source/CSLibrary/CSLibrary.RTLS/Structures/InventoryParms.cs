using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    using Constants;
    /// <summary>
    /// Inventory parameters
    /// </summary>
    public class InventoryParms : IOperationParms
    {
        /// <summary>
        /// Operation
        /// </summary>
        public Operation Operation
        {
            get { return Operation.Inventory; }
        }
        /// <summary>
        /// specific Led blink upon on good read
        /// </summary>
        public byte ledBlinkOnGoodRead = 0;
        /// <summary>
        /// how many tag you want to search
        /// </summary>
        public uint tagStopCount = 0;
        /// <summary>
        /// SelectMask flags
        /// </summary>
        public IDFilterFlags flags = IDFilterFlags.NONE;
        /// <summary>
        /// constructor
        /// </summary>
        public InventoryParms()
            : this(0, 0, IDFilterFlags.NONE)
        {
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="flags">SelectMask flags</param>
        public InventoryParms(IDFilterFlags flags)
            : this(0, 0, flags)
        {
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="tagStopCount">how many tag you want to search</param>
        public InventoryParms(uint tagStopCount)
            : this(0, tagStopCount, IDFilterFlags.NONE)
        {
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ledBlinkOnGoodRead">specific Led blink upon on good read</param>
        public InventoryParms(byte ledBlinkOnGoodRead)
            : this(ledBlinkOnGoodRead, 0, IDFilterFlags.NONE)
        {
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="tagStopCount">how many tag you want to search</param>
        /// <param name="flags">SelectMask flags</param>
        public InventoryParms(uint tagStopCount, IDFilterFlags flags)
            : this(0, tagStopCount, flags)
        {
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ledBlinkOnGoodRead">specific Led blink upon on good read</param>
        /// <param name="tagStopCount">how many tag you want to search</param>
        /// <param name="flags">SelectMask flags</param>
        public InventoryParms(byte ledBlinkOnGoodRead, uint tagStopCount, IDFilterFlags flags)
        {
            this.tagStopCount = tagStopCount;
            this.flags = flags;
            this.ledBlinkOnGoodRead = ledBlinkOnGoodRead;
        }
    }
}
