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
using System.IO;
using CSLibrary.Constants;

namespace CSLibrary
{
    public partial class HighLevelInterface
    {
        public enum DEBUGLEVEL : ulong
        {
            READER_DATA_READ_WRITE = 0x01,  // bit 0
            USB_IO_CONTROL = 0x02,          // bit 1
            REGISTER = 0x04,                // bit 2
            API = 0x08,                     // bit 3
            PERFORMANCE = 0x8000000000000000    // bit 63 
        }

        ulong DebugModeLevel = 0;
        string DebugLogFileName = "";

        private bool DebugFileValidate(string FileName)
        {
            try
            {
                TextWriter tw = new StreamWriter(FileName, true);
                tw.WriteLine("DEBUG LOG START : " + DateTime.Now);
                tw.Close();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        
        public Result EngDebugModeSetLogFile(string FileName)
        {
            if (DebugFileValidate(FileName))
            {
                DebugLogFileName = FileName;
                return Result.OK;
            }
            else
            {
                DebugLogFileName = "";
                return Result.FAILURE;
            }
        }

        public ulong EngDebugModeEnable(DEBUGLEVEL Mode)
        {
            if (DebugLogFileName.Length == 0)
            {
                DebugModeLevel = 0;
            }
            else
            {
                DebugModeLevel |= (ulong)Mode;
            }

            return DebugModeLevel;
        }

        public void EngDebugModeDisable()
        {
            DebugModeLevel = 0;
        }

        private void DEBUG_Write(DEBUGLEVEL Level, string message)
        {
            if (((UInt32)Level & DebugModeLevel) != 0)
            {
                lock (DebugLogFileName)
                {
                    try
                    {
                        TextWriter tw = new StreamWriter(DebugLogFileName, true);
                        tw.Write(message);
                        tw.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void DEBUGT_Write(DEBUGLEVEL Level, string message)
        {
            if (((UInt32)Level & DebugModeLevel) != 0)
            {
                lock (DebugLogFileName)
                {
                    try
                    {
                        TextWriter tw = new StreamWriter(DebugLogFileName, true);
                        tw.Write(DateTime.Now + ":" + message);
                        tw.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void DEBUG_WriteLine(DEBUGLEVEL Level, byte[] buffer, int offset, int size)
        {
            if (((UInt32)Level & DebugModeLevel) != 0)
            {
                lock (DebugLogFileName)
                {
                    try
                    {
                        int end = offset + size;
                        int cnt;

                        TextWriter tw = new StreamWriter(DebugLogFileName, true);
                        for (cnt = offset; cnt < end; cnt++)
                            tw.Write(buffer[cnt].ToString("X2"));
                        tw.WriteLine();
                        tw.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void DEBUGT_WriteLine(DEBUGLEVEL Level, byte[] buffer, int offset, int size)
        {
            if (((UInt32)Level & DebugModeLevel) != 0)
            {
                lock (DebugLogFileName)
                {
                    try
                    {
                        int end = offset + size;
                        int cnt;

                        TextWriter tw = new StreamWriter(DebugLogFileName, true);
                        tw.Write(DateTime.Now + ":");
                        for (cnt = offset; cnt < end; cnt++)
                            tw.Write(buffer[cnt].ToString("X2"));
                        tw.WriteLine();
                        tw.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void DEBUGT_WriteLine(DEBUGLEVEL Level, string msg, byte[] buffer, int offset, int size)
        {
            if (((UInt32)Level & DebugModeLevel) != 0)
            {
                lock (DebugLogFileName)
                {
                    try
                    {
                        int end = offset + size;
                        int cnt;

                        TextWriter tw = new StreamWriter(DebugLogFileName, true);
                        tw.Write(DateTime.Now + ":" + msg + ":");
                        for (cnt = offset; cnt < end; cnt++)
                            tw.Write(buffer[cnt].ToString("X2"));
                        tw.WriteLine();
                        tw.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void DEBUG_WriteLine()
        {
            if (DebugLogFileName.Length > 0)
            {
                lock (DebugLogFileName)
                {
                    try
                    {
                        TextWriter tw = new StreamWriter(DebugLogFileName, true);
                        tw.WriteLine();
                        tw.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void DEBUG_WriteLine(DEBUGLEVEL Level, string message)
        {
            if (((UInt32)Level & DebugModeLevel) != 0)
            {
                lock (DebugLogFileName)
                {
                    try
                    {
                        TextWriter tw = new StreamWriter(DebugLogFileName, true);
                        tw.WriteLine(message);
                        tw.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void DEBUGT_WriteLine(DEBUGLEVEL Level, string message)
        {
            if (((ulong)Level & DebugModeLevel) != 0)
            {
                lock (DebugLogFileName)
                {
                    try
                    {
                        TextWriter tw = new StreamWriter(DebugLogFileName, true);
                        tw.WriteLine(DateTime.Now + ":" + message);
                        tw.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}
