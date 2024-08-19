using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    using Constants;
    /// <summary>
    /// ISO 18000-6C command parameters
    /// </summary>
    public interface IOperationParms
    {
        /// <summary>
        /// Tag operation
        /// </summary>
        Operation Operation { get;}
    }
}
