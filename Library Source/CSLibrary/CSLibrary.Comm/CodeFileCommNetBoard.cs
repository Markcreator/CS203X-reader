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

//
// 8051 UART <-> Network board control command
//
using System;
using System.Collections.Generic;
using System.Text;

using System.Threading;
using System.Runtime.InteropServices;

using System.Net;
using System.Net.Sockets;

using System.IO.Ports;

using CSLibrary.Constants;
using CSLibrary.Structures;


namespace CSLibrary
{
    public partial class HighLevelInterface
    {

        // paramter for IPv4
        public enum TCP_CMD
        {
            RFID_H = 0x01,
            RFID_L = 0x02,
            BOOTLDR = 0x03,
            GPO0_H = 0x04,
            GPO0_L = 0x05,
            GPO1_H = 0x06,
            GPO1_L = 0x07,
            LED_H = 0x08,
            LED_L = 0x09,
            GPO0 = 0x0a,
            GPO1 = 0x0b,
            GPI0 = 0x0c,
            GPI1 = 0x0d,
            EBOOT = 0x0e,
            FIND = 0x0f,
            LED = 0x10,
            MAC = 0x11,
            BLV = 0x12,
            IMV = 0x13,
            STDBY = 0x14,
            AUTO_RESET = 0x15,
            CRC_FILTER = 0x16,
            TCP_NOTEMODE = 0x17
        };

        private bool NETBOARD_Cmd(TCP_CMD cmd, byte[] Parameter, byte[] Out)
        {
            if (m_DeviceInterfaceType != INTERFACETYPE.IPV4)
                return false;

            return (NETBOARD_Cmd (hostIP, cmd, Parameter, Out));
        }

        private static bool NETBOARD_Cmd(IPAddress ip, TCP_CMD cmd, byte[] Parameter, byte[] Out)
        {
            Socket ContCMD = null;      // TCP 1516
            byte[] CMDBuf;
            byte[] RecvHeaderBuf = new byte[4];

            switch (cmd)
            {
                case TCP_CMD.IMV:
                case TCP_CMD.BLV:
                    CMDBuf = new byte[4];
                    CMDBuf[2] = 0x00;
                    break;

                case TCP_CMD.TCP_NOTEMODE:
                    CMDBuf = new byte[5];
                    CMDBuf[2] = 0x01;
                    CMDBuf[4] = 0x00;
                    break;

                default:
                    CMDBuf = new byte[4 + Parameter.Length];
                    CMDBuf[2] = (byte)Parameter.Length;
                    Array.Copy(Parameter, 0, CMDBuf, 4, Parameter.Length);
                    break;
            }

            CMDBuf[0] = 0x80;
            CMDBuf[1] = 0x00;
            CMDBuf[3] = (byte)cmd;


            DateTime StartTime = DateTime.Now;
            do
            {
                try
                {
                    ContCMD = ConnectSocket(ip, 1516); // Control command port;

                    if (ContCMD != null)
                        break;

                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                }
            } while (DateTime.Now.Subtract(StartTime).TotalSeconds < 5);

            if (ContCMD == null)
                return false;

            ContCMD.Blocking = true; // false;
#if WIN32
            ContCMD.NoDelay = true;
#endif

            //TCP_SetKeepAlive(ContCMD, 500U, 500U);

            //TCP_Send(ContCMD, CMDBuf, 0, CMDBuf.Length);
            //TCP_Recv(ContCMD, RecvHeaderBuf, 0, 4, 3000);
            //TCP_Recv(ContCMD, Out, 0, RecvHeaderBuf[2], 3000);

            ContCMD.Send(CMDBuf, 0, CMDBuf.Length, SocketFlags.None);

            StartTime = DateTime.Now;
            bool success = false;
            do
            {
                Thread.Sleep(10);

                if (ContCMD.Available >= 5)
                {
                    ContCMD.Receive(RecvHeaderBuf, 0, 4, SocketFlags.None);
                    ContCMD.Receive(Out, 0, RecvHeaderBuf[2], SocketFlags.None);
                    success = true;
                    break;
                }

            } while (DateTime.Now.Subtract(StartTime).TotalSeconds < 5);

            ContCMD.Close();
            Thread.Sleep(500);

            return success;
        }

        private static Socket ConnectSocket(IPAddress ip, int port)
        {
            Socket s = null;
            IPEndPoint ipe = new IPEndPoint(ip, port);
            Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Thread.Sleep(1);

            try
            {
                tempSocket.Connect(ipe);
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                //                Console.Out.WriteLine("Network connection fail{0}", ex.ToString());
            }

            if (tempSocket.Connected)
            {
                s = tempSocket;
            }

            return s;
        }


        #region GPI Interrupt Polling

        static bool startPollGPI;
        static GPI_INTERRUPT_CALLBACK pollGPICallback;
        static int stopPollGPI = 0;
        static AutoResetEvent pollGPIThreadDead;
        static Socket udpSocket = null;
        static Thread pollGPIThread = null;



        /// <summary>
        /// StartPollGPIStatus
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Boolean StartUdpPollGPIStatus(GPI_INTERRUPT_CALLBACK func)
        {
            if (startPollGPI)
                return true;
            try
            {
                pollGPIThreadDead = new AutoResetEvent(false);
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                pollGPICallback = func;
                udpSocket.Bind(new IPEndPoint (System.Net.IPAddress.Any, 3041));
                udpSocket.Blocking = false;

                //if (pollGPIThread == null)
                {
                    pollGPIThread = new Thread(new ThreadStart(StartPollGPIStatusThreadProc));
                }

                pollGPIThread.IsBackground = true;
                pollGPIThread.Start();

            }
            catch (Exception ex)
            {
/*
                DEBUG_WriteLine(DEBUGLEVEL.API , string.Format(
//                PrintMessage.Logger.DebugException(string.Format(
                    "{0},{1}",
                    "StartPollGPIStatus",
                    TransStatus.CPL_ERROR_NOTFOUND),
                    ex);
*/                goto ERROR_EXIT;
            }
            startPollGPI = true;

            return true;
        ERROR_EXIT:
            udpSocket.Close();
            pollGPIThreadDead.Close();
            return false;
        }

        static void StartPollGPIStatusThreadProc()
        {
            //SocketError sockErr = SocketError.Success;
            while (Interlocked.Equals(stopPollGPI, 0))
            {
                EndPoint SenderAddr = (EndPoint)(new IPEndPoint(System.Net.IPAddress.Any, 3041));

                Byte[] revBuff = new byte[6];

                try 
                {
                    if (udpSocket.ReceiveFrom(revBuff, ref SenderAddr) == -1)
                    {
                        System.Threading.Thread.Sleep(100);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(100);
                    continue;
                    //goto ERROR_EXIT;
                }
                    
                if (revBuff[0] == 0x81 && revBuff[3] == 0x5/*GET_GPI_STATUS*/) // Vaild header
                {
                    if (pollGPICallback != null && revBuff[1] != 0)
                    {
                        int i0, i1;

                        switch (revBuff[4])
                        {
                            case 0:
                                i0 = -1;
                                break;
                            case 1:
                                i0 = 1;
                                break;
                            default:
                                i0 = 0;
                                break;
                        }

                        switch (revBuff[5])
                        {
                            case 0:
                                i1 = -1;
                                break;
                            case 1:
                                i1 = 1;
                                break;
                            default:
                                i1 = 0;
                                break;
                        }

                        if (!pollGPICallback(((IPEndPoint)SenderAddr).Address.ToString(), i0, i1))
                        {
                            break;
                        }
                    }
                }
            }

        ERROR_EXIT:
            udpSocket.Close();
            startPollGPI = false;
            stopPollGPI = 0;
            pollGPIThreadDead.Set();
        }

        # endregion


    }
}