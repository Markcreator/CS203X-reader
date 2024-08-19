using System;
using System.Collections.Generic;
using System.Text;

using CSLibrary.Constants;

namespace CSLibrary
{
    internal class ReaderException : System.Exception
    {
        private Result _errorCode = Result.OK;

        public Result ErrorCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }
        public ReaderException() : base() { }
        public ReaderException(string message) : base(message) { }
        public ReaderException(Result err)
            : base()
        {
            _errorCode = err;
        }

        public ReaderException(Result err, String message)
            : base(message)
        {
            _errorCode = err;
        }

        public ReaderException(System.Exception innerException)
            : base("See inner Exception", innerException)
        {

        }
        public override string Message
        {
            get
            {
                return "Reader Error: " +
                    _errorCode.ToString() + " [" +
                    base.Message +
                    "]";
            }
        }
    } // public class rfidException : SystemException

}
