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
using System.Runtime.InteropServices;

using CSLibrary.Constants;
namespace CSLibrary.Structures
{
    /// <summary>
    /// Tag Kill structure, configure this before do tag kill
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TagKillParms
    {
/*        /// <summary>
        /// Structure size
        /// </summary>
        protected readonly UInt32 Length = 21;
*/
        /// <summary>
        /// The access password for the tags.  A value of zero indicates no 
        /// access password. 
        /// </summary>
        public UInt32 accessPassword;
        /// <summary>
        /// The kill password for the tags.  A value of zero indicates no 
        /// kill password. 
        /// </summary>
        public UInt32 killPassword;
        /// <summary>
        /// Number of retries attemp to read. This field must be between 0 and 15, inclusive.
        /// </summary>
        public UInt32 retryCount;
        /// <summary>
        /// Flag - Zero or combination of  Select or Post-Match
        /// </summary>
        public SelectFlags flags = SelectFlags.UNKNOWN;
        /// <summary>
        /// Extended Kill command
        /// </summary>
        public ExtendedKillCommand extCommand = ExtendedKillCommand.NORMAL;
        /// <summary>
        /// constructor
        /// </summary>
        public TagKillParms()
        {
            // NOP
        }
    }
}