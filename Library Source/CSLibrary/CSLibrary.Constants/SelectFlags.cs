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

namespace CSLibrary.Constants
{
    /// <summary>
    /// Select Flags
    /// </summary>
    [Flags]
    public enum SelectFlags
    {
        /// <summary>
        /// Normal Inventory
        /// </summary>
        //NORMAL = 0x0000000,
        ZERO = 0x0000000,
        /// <summary>
        /// Use Select Criteria
        /// </summary>
        SELECT = 0x00000001,
        /// <summary>
        /// Use Post-Match Criteria
        /// </summary>
        POSTMATCH = 0x00000002,
        /// <summary>
        /// Using Post-Match Criterion
        /// </summary>
        POST_MATCH = 0x2,
        /// <summary>
        /// Disable Inventory
        /// </summary>
        DISABLE_INVENTORY = 0x00000004,
        /// <summary>
        /// Read 1 bank after Inventory
        /// </summary>
        READ1BANK = 0x00000008,
        /// <summary>
        /// Read 2 bank after Inventory
        /// </summary>
        READ2BANK = 0x00000010,
        /// <summary>
        /// Unknown
        /// </summary>
        UNKNOWN = 0xffff,
    }
}
