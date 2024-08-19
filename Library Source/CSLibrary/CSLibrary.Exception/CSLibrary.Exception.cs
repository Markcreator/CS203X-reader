/*
Copyright (c) 2023 Convergence Systems Limited

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
