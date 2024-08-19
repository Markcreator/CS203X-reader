using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS.Structures
{
    using Constants;
    /// <summary>
    /// Exception thrown when a Lock method on the SyncLock class times out.
    /// </summary>
    public class RTLSErrorException : Exception
    {
        private Result result = Result.OK;
        /// <summary>
        /// Constructs an instance with the specified message.
        /// </summary>
        /// <param name="result">Result code</param>
        public RTLSErrorException(Result result)
        {
            this.result = result;
        }
        /// <summary>
        /// Constructs an instance with the specified message.
        /// </summary>
        /// <param name="message">The message for the exception</param>
        public RTLSErrorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs an instance by formatting the specified message with
        /// the given parameters.
        /// </summary>
        /// <param name="format">The message, which will be formatted with the parameters.</param>
        /// <param name="args">The parameters to use for formatting.</param>
        public RTLSErrorException(string format, params object[] args)
            : this(string.Format(format, args))
        {
        }
        /// <summary>
        /// Get error code
        /// </summary>
        /// <returns></returns>
        public Result GetError()
        {
            return result;
        }
        /// <summary>
        /// Error message
        /// </summary>
        public override string Message
        {
            get
            {
                return "*RTLS Exception*  " +
                    result.ToString() + " [" +
                    base.Message +
                    "]";
            }
        }
    }
}
