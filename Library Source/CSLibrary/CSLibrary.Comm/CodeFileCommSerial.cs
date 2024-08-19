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

using System.Threading;
using System.Runtime.InteropServices;

using System.Net;
using System.Net.Sockets;

using System.IO.Ports;

namespace CSLibrary
{
    public partial class HighLevelInterface
    {
        // paramter for Serial Port
        private SerialPort SerialPortHandler = new SerialPort();
        string ErrMsg;

        private bool SERIAL_Connect(string SerialPortName)
        {
            return SERIAL_Connect(ref SerialPortHandler, SerialPortName);
        }

        private bool SERIAL_Connect(ref SerialPort SP, string SerialPortName)
        {
            if (GetOSVersion () == OSVERSION.WINCE)
                SerialPortName += ":";

            SP.PortName = SerialPortName;
            SP.BaudRate = 115200;
            SP.DataBits = 8;
            SP.Parity = Parity.None;
            SP.StopBits = StopBits.One;
            SP.Handshake = Handshake.None;

            // Set the read/write timeouts
            SP.ReadTimeout = 1000;
            SP.WriteTimeout = 1000;

            byte[] SendBuf = new byte[8];
            byte[] RecvBuf = new byte[50];
            
            try
            {
                SP.Open();

                if (SP.IsOpen == false)
                    return false;

                SendBuf[0] = 0xc0;
                SendBuf[1] = 0x06;
                SendBuf[2] = 0x00;
                SendBuf[3] = 0x00;
                SendBuf[4] = 0x00;
                SendBuf[5] = 0x00;
                SendBuf[6] = 0x00;
                SendBuf[7] = 0x00;

                if (SERIAL_Send(SP, SendBuf, 0, 8, 100) == false)
                    return false;

                if (SERIAL_Recv(SP, RecvBuf, 0, 2, 100) == false)
                    return false;

                if (RecvBuf[1] != 0x03)
                    return false;

                if (SERIAL_Recv(SP, RecvBuf, 0, RecvBuf[0] - 2, 100) == false)
                    return false;

                RecvBuf[0] = 0x12;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return false;
            }

            return true;
        }

        private bool SERIAL_Send(byte[] Buffer, int offset, int size, int timeout)
        {
            return SERIAL_Send(SerialPortHandler, Buffer, offset, size, timeout);
        }

        private bool SERIAL_Send(SerialPort SP, byte[] Buffer, int offset, int size, int timeout)
        {
            try
            {
                SP.Write(Buffer, offset, size);
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return false;
            }

            return true;
        }


        /// <summary>
        /// Receive data from reader
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private bool SERIAL_Recv(byte[] buffer, int offset, int size, uint timeout)
        {
            return SERIAL_Recv (SerialPortHandler, buffer, offset, size, timeout);
        }

        /// <summary>
        /// Receive data from reader
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private bool SERIAL_Recv(SerialPort SP, byte[] buffer, int offset, int size, uint timeout)
        {
            int recv = 0;
            DateTime start;

            try
            {
                start = DateTime.Now;
                while (SP.BytesToRead < size)
                {
                    if (timeout != 0 && (DateTime.Now - start).TotalMilliseconds > timeout)
                        return false;

                    System.Threading.Thread.Sleep(1);
                }

                recv = SP.Read(buffer, offset, size);
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return false;
            }

            if (recv != size)
            {
                Console.WriteLine("Read Fail");
                return false;
            }
            return true;
        }

        private bool SERIAL_Close()
        {
            return SERIAL_Close(SerialPortHandler);
        }

        private bool SERIAL_Close(SerialPort SP)
        {
            try
            {
                SP.Close();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool GetSerialDevicesList(ref List<string> DeviceList)
        {
            List<ManualResetEvent> events = new List<ManualResetEvent>();
            List<string> RFIDDeviceList = new List<string>();

            foreach (string ComPort in System.IO.Ports.SerialPort.GetPortNames())
            {
                string cn = ComPort;
                var resetEvent = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(
                    arg =>
                    {
                        SerialPort SP = new SerialPort();

                        if (SERIAL_Connect(ref SP, cn))
                            RFIDDeviceList.Add(cn);

                        SERIAL_Close(SP);
                        resetEvent.Set();
                    });
                events.Add(resetEvent);
            }

            foreach (ManualResetEvent e in events)
                e.WaitOne ();

            foreach (string Device in RFIDDeviceList)
                DeviceList.Add(Device);

            return true;
        }
    }
}
