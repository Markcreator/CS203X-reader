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
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary
{
    internal class NativeRegistry
    {
        internal static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);

        internal enum KeyType
        {
            REG_NONE = 0,
            REG_SZ = 1,
            REG_EXPAND_SZ = 2,
            REG_BINARY = 3,
            REG_DWORD = 4,
            REG_DWORD_LITTLE_ENDIAN = 4,
            REG_DWORD_BIG_ENDIAN = 5,
            REG_LINK = 6,
            REG_MULTI_SZ = 7,
        }
        [DllImport("coredll.dll")]
        internal static extern int RegOpenKeyEx(
           UIntPtr hKey,
            Byte[] lpSubKey,
            uint ulOptions,
            int samDesired,
            out UIntPtr phkResult);
        [DllImport("coredll.dll")]
        internal static extern int RegOpenKeyEx(
           UIntPtr hKey,
            String lpSubKey,
            uint ulOptions,
            int samDesired,
            out UIntPtr phkResult);
        [DllImport("coredll.dll")]
        internal extern static int RegEnumKeyEx(UIntPtr hkey,
            uint index,
            Byte[] lpName,
            ref uint lpcbName,
            IntPtr reserved,
            IntPtr lpClass,
            IntPtr lpcbClass,
            out long lpftLastWriteTime);

        [DllImport("coredll.dll", SetLastError = true)]
        internal static extern int RegQueryValueEx(
            UIntPtr hkey, 
            String lpValueName, 
            IntPtr lpReserved, 
            ref KeyType lpType, 
            Byte[] lpData, 
            ref uint lpcbData);

        [DllImport("coredll.dll")]
        internal static extern int RegCloseKey(UIntPtr hKey);
    }
}
