using System;
using System.Collections.Generic;
using System.Text;

using System.Threading;
using System.Runtime.InteropServices;

using System.Net;
using System.Net.Sockets;

using System.IO.Ports;

using CSLibrary.Constants;

using Microsoft.Win32;

namespace CSLibrary
{
    public partial class HighLevelInterface
    {
        private const uint TCP_DATA_PORT = 1515;

        // paramter for IPv4
        private IPAddress hostIP;...
        private Socket IntelCMD = null;

        UInt32 totalrevcnt = 0;

        public enum UDP_CMD
        {
	        CHECK_STATUS = 0x01,
	        FORCE_RESET = 0x02,
	        UDP_KEEPALIVE_ON = 0x03,
	        UDP_KEEPALIVE_OFF = 0x04,
	        GET_GPI_STATUS = 0x5,
	        GET_GPO_STATUS = 0x6,
	        SET_GPO0_STATUS,
	        SET_GPO1_STATUS,
	        SET_GPI0_INTERRUPT,
	        SET_GPI1_INTERRUPT,
	        GET_GPI_INTERRUPT,
	        SET_LOW_LVL_API,
	        SET_HIGH_LVL_API,
	        CHECK_API,
            CHECK_GPO5V_STATUS,
            SET_GPO5V,
            CHECK_TAGREADIND_STATUS,
            SET_TAGREADIND,
            CHECK_POE_INFORMATION,
            SET_PD_POWER,
            GET_BLV,
            GET_IMV,
            SET_TCP_NOTIFICATION_MODE
        };

#if test_nouse
        public void SetKeepAliveTime(uint keepalive_time, uint keepalive_interval)
        {
            uint uBytesReturn = 0;
            byte[] keep_alive_in = new byte[12];
            byte[] keep_alive_out = new byte[12];
            ulong[] input_params = new ulong[3];
            int i1;
            int bits_per_byte = 8, bytes_per_long = 4;

            if (keepalive_time == 0 || keepalive_interval == 0)
                input_params[0] = 0;
            else
                input_params[0] = 1;
            input_params[1] = keepalive_time;
            input_params[2] = keepalive_interval;
            for (i1 = 0; i1 < input_params.Length; i1++)
            {
                keep_alive_in[i1 * bytes_per_long + 3] = (byte)(input_params[i1] >> ((bytes_per_long - 1) * bits_per_byte) & 0xff);
                keep_alive_in[i1 * bytes_per_long + 2] = (byte)(input_params[i1] >> ((bytes_per_long - 2) * bits_per_byte) & 0xff);
                keep_alive_in[i1 * bytes_per_long + 1] = (byte)(input_params[i1] >> ((bytes_per_long - 3) * bits_per_byte) & 0xff);
                keep_alive_in[i1 * bytes_per_long + 0] = (byte)(input_params[i1] >> ((bytes_per_long - 4) * bits_per_byte) & 0xff);
            }


            if (WSAIoctl(
                socketHandle,
                0x98000004,//(uint)IOControlCode.KeepAliveValues,
                keep_alive_in,
                12,
                keep_alive_out,
                12,
                out uBytesReturn,
                IntPtr.Zero,
                IntPtr.Zero) != SocketError.Success)
                throw new SocketException(WSAGetLastError());
        }

        
        public void TCP_SetKeepAlive(Socket s, ulong keepalive_time, ulong keepalive_interval)
        {
            int bytes_per_long = 32 / 8;
            byte [] keep_alive = new byte[3*bytes_per_long];
            ulong [] input_params = new ulong[3];
            int i1;
            int bits_per_byte = 8;

            if (keepalive_time == 0 || keepalive_interval == 0)
                input_params[0] = 0;
            else
                input_params[0] = 1;

            input_params[1] = keepalive_time;
            input_params[2] = keepalive_interval;

            for (i1=0; i1<input_params.Length; i1++)
            {
                keep_alive[i1*bytes_per_long+3] = (byte)(input_params[i1] >> ((bytes_per_long - 1) * bits_per_byte) & 0xff);
                keep_alive[i1*bytes_per_long+2] = (byte)(input_params[i1] >> ((bytes_per_long - 2) * bits_per_byte) & 0xff);
                keep_alive[i1*bytes_per_long+1] = (byte)(input_params[i1] >> ((bytes_per_long - 3) * bits_per_byte) & 0xff);
                keep_alive[i1*bytes_per_long+0] = (byte)(input_params[i1] >> ((bytes_per_long - 4) * bits_per_byte) & 0xff);
            }

            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, keep_alive);
            
            //s.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
            //s.IOControl(IOControlCode.KeepAliveValues, inValue, outValue);

        } /* method AsyncSocket SetKeepAlive */        
#endif

        private static void TCP_SetKeepAlive(Socket s, ulong keepalive_time, ulong keepalive_interval)
        {
#if WIN32
            ulong[] input_params = new ulong[3];
            byte[] keep_alive = new byte[12];

            if (keepalive_time == 0 || keepalive_interval == 0)
                input_params[0] = 0;
            else
                input_params[0] = 1;
            input_params[1] = keepalive_time;
            input_params[2] = keepalive_interval;

            for (int cnt = 0; cnt < input_params.Length; cnt++)
            {
                keep_alive[cnt * 4 + 3] = (byte)(input_params[cnt] >> 24 & 0xff);
                keep_alive[cnt * 4 + 2] = (byte)(input_params[cnt] >> 16 & 0xff);
                keep_alive[cnt * 4 + 1] = (byte)(input_params[cnt] >> 8 & 0xff);
                keep_alive[cnt * 4 + 0] = (byte)(input_params[cnt] & 0xff);
            }

            s.IOControl(IOControlCode.KeepAliveValues, keep_alive, null);


#else
/*            
            // Enable TCP notification mode (disable keepalive feature)
            //IPEndPoint e = (IPEndPoint)(s.RemoteEndPoint);
            //COMM_INTBOARDALT_Cmd(System.Net.IPAddress.Parse("192.168.25.205").Address, UDP_CMD.SET_TCP_NOTIFICATION_MODE, TCPNotiOn, new byte[0]);
            //COMM_INTBOARDALT_Cmd(e.Address.Address, UDP_CMD.SET_TCP_NOTIFICATION_MODE, new byte[1]{1}, new byte[0]);
            COMM_INTBOARDALT_Cmd(((IPEndPoint)(s.RemoteEndPoint)).Address.Address, UDP_CMD.SET_TCP_NOTIFICATION_MODE, new byte[1] { 1 }, new byte[0]);
*/

/*
//#elif WindowsCE
            // need to soft reboot
            RegistryKey reg;

            reg = Registry.LocalMachine.OpenSubKey(@"Comm\Tcpip\Parms", true);
            reg.SetValue("KeepAliveTime", keepalive_time, RegistryValueKind.DWord);
            reg.SetValue("KeepAliveInterval", keepalive_interval, RegistryValueKind.DWord); 

            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, Convert.ToInt32(true));
 */
#endif
        }

#if nouse
        private Socket ConnectSocket(int port)
        {
            IPEndPoint ipe = new IPEndPoint(hostIP, port);
            Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Thread.Sleep(1);

            try
            {
                tempSocket.Connect(ipe);
                Thread.Sleep(500);

                if (tempSocket.Connected)
                    return tempSocket;
            }
            catch (Exception ex)
            {
                //                Console.Out.WriteLine("Network connection fail{0}", ex.ToString());
            }

            return null;

/*            Socket s = null;
            IPEndPoint ipe = new IPEndPoint(hostIP, port);
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
*/
        }
#endif

#if oldcode
        void netfinder_OnSearchCompleted(object sender, CSLibrary.Net.DeviceFinderArgs e)
        {
            di = e.Found;
            readerFound = true;
        }
#endif

        public Boolean TCP_Connect(string ipaddress)
        {
            bool readerFound = false;
            CSLibrary.Net.DeviceInfomation di = new CSLibrary.Net.DeviceInfomation ();

            bool connection = false;
            CSLibrary.Structures.DEVICE_STATUS deviceStatus = new CSLibrary.Structures.DEVICE_STATUS ();
            
            try
            {
                hostIP = System.Net.IPAddress.Parse(ipaddress);

#if oldcode
                CSLibrary.Net.NetFinder nf = new CSLibrary.Net.NetFinder();
                DateTime searchTimeout = DateTime.Now.AddSeconds (3);

                byte[] CMDBuf = new byte[10];
                byte[] RecvBuf = new byte[10];

                // use netfinder to search reader
                readerFound = false;
                nf.OnSearchCompleted += new EventHandler<CSLibrary.Net.DeviceFinderArgs>(netfinder_OnSearchCompleted);
                nf.SearchDevice(hostIP);
                while (searchTimeout > DateTime.Now && !readerFound)
                {
                    Thread.Sleep(10);
                }
                nf.Stop();
                nf.OnSearchCompleted -= new EventHandler<CSLibrary.Net.DeviceFinderArgs>(netfinder_OnSearchCompleted);
                nf.ClearDeviceList();

                // if reader found and in bootloader mode
                if (readerFound && di.Mode == CSLibrary.Net.Mode.Bootloader)
                {
                    ForceReset(ipaddress);
                    m_Result = Result.FAILURE;
                    return false;
                }
#else
                readerFound = DirectSearch(hostIP, ref di);

                if (readerFound && di.Mode == CSLibrary.Net.Mode.Bootloader)
                {
                    ForceReset(ipaddress);
                    m_Result = Result.FAILURE;
                    return false;
                }
#endif

                // check device status
                if (CheckStatus(ipaddress, ref deviceStatus) == Result.OK)
                {
                    if (deviceStatus.IsConnected)
                    {
                        m_Result = Result.DEVICE_CONNECTED;
                        return false;
                    }
                }
                else 
                {
                    if (!readerFound)
                    {
                        m_Result = Result.NETWORK_LOST;
                        return false;
                    }
                    else
                    {
                        m_Result = Result.CURRENTLY_NOT_ALLOWED;
                        return false;
                    }
                }

                // if can not get Silicon (UDP command and 1516 command)
                if (!GetSiliconVersion())
                {
                    //ForceReset(ipaddress);
                    m_Result = Result.FIRMWARE_TOO_OLD;
                    return false;
                }

                try
                {
                    Thread.Sleep(1);
                    CheckAbortRespBufferSize = 0;

#if !WIN32
                    // Disable Auto reset mode
                    if (NETBOARD_Cmd(TCP_CMD.AUTO_RESET, new byte[1], new byte[1]) != true)
                    //    return false;

#endif

                    IntelCMD = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);     // TCP 1515
                    IntelCMD.Connect(new IPEndPoint(hostIP, 1515));
                    Thread.Sleep(300);
                }
                catch (Exception ex)
                {
                    //                Console.Out.WriteLine("Network connection fail{0}", ex.ToString());
                }

                if (!IntelCMD.Connected)
                {
                    ForceReset(ipaddress);
                    m_Result = Result.FAILURE;
                    return false;
                }

                m_macAddress = di.MACAddress;
                
                IntelCMD.Blocking = true;

#if !WindowsCE
                // Set the receive buffer size to 4M
                IntelCMD.ReceiveBufferSize = 4 * 1024 * 1024;
                IntelCMD.NoDelay = true;
#endif

                switch (GetOSVersion())
                {
                    case OSVERSION.WIN32:
                    case OSVERSION.WINCE:
                        TCP_SetKeepAlive(IntelCMD, 500U, 500U);
                        break;

                    default:
#if PORT_1516
                        if (NETBOARD_Cmd(TCP_CMD.TCP_NOTEMODE, new byte[1], new byte[1]) != true)
                            return false;
#endif
                        break;
                }

                NetWakeup();

                
                //#if WIN32 || WindowsCE
                //            TCP_SetKeepAlive(IntelCMD, 500U, 500U);
                //#else
                //            if (NETBOARD_Cmd(TCP_CMD.TCP_NOTEMODE, new byte[1], new byte[1]) != true)
                //                return false;
                //#endif

#if testing
#if WIN32
            TCP_SetKeepAlive(IntelCMD, 500U, 500U);
#else
            // disable Auto Reset
            if (NETBOARD_Cmd(TCP_CMD.AUTO_RESET, new byte[1], new byte[1]) != true)
                return false;
#endif
            // disable TCP notification mode
            if (NETBOARD_Cmd(TCP_CMD.TCP_NOTEMODE, new byte[1], new byte[1]) != true)
                return false;
#endif
                connection = true;
                return true;
            }
            finally
            {
                if (connection == false)
                    if (IntelCMD != null)
                        IntelCMD.Close();
            }
        }

        public void TCP_Disconnect()
        {
            try
            {
                // TCP 1515
                IntelCMD.Close();
                IntelCMD = null;
            }
            catch {}
        }

        private void SendCallback(IAsyncResult ar)
        {
            var client = (Socket)ar.AsyncState;
            if (client != null)
                client.EndSend(ar); 
        }

        private Boolean TCP_Send(Socket socket, byte[] buffer, int offset, int size)
        {
            DateTime EndTime;
            int sent = 0;  // how many bytes is already sent

            if (socket == null)
                return false;

            if (socket.Connected == false)
                return false;

            Thread.Sleep(5);
            try
            {
                EndTime = DateTime.Now.AddSeconds (5);
                
                while (DateTime.Now < EndTime)
                {
                    sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
                    if (sent >= size)
                        return true;

                    Thread.Sleep(10);
                }
            }
            catch (SocketException ex)
            {
                throw new ReaderException(m_Result = Result.NETWORK_LOST);
            }

            return false;
        }



        bool bufferfull = false;
        byte[] CheckAbortRespBuffer = new byte[8];
        int CheckAbortRespBufferSize = 0;
        private Boolean TCP_Recv(Socket socket, byte[] buffer, int offset, int size, uint timeout)
        {
            //DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Enter function : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            //DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Parameter size (dec): " + size.ToString());

            try
            {
                if (size == 0)
                    return true;

                if (socket == null || !socket.Connected)
                    return false;

                try
                {
                    if (CheckAbortRespBufferSize > 0)
                    {
                        if (size > CheckAbortRespBufferSize)
                        {
                            Array.Copy(CheckAbortRespBuffer, 0, buffer, offset, CheckAbortRespBufferSize);
                            CheckAbortRespBufferSize = 0;
                            offset += CheckAbortRespBufferSize;
                            size -= CheckAbortRespBufferSize;
                        }
                        else
                        {
                            Array.Copy(CheckAbortRespBuffer, 0, buffer, offset, size);
                            CheckAbortRespBufferSize -= size;
                            if (CheckAbortRespBufferSize > 0)
                                Array.Copy(CheckAbortRespBuffer, size, CheckAbortRespBuffer, 0, CheckAbortRespBufferSize);
                            return true;
                        }
                    }

#if WindowsCE
                    if (bufferfull == false && socket.Available >= 60000)
#else
                    if (bufferfull == false && socket.Available >= socket.ReceiveBufferSize)
#endif
                    {
                        bufferfull = true;
                        COMM_AdapterCommand(READERCMD.PAUSE);
                    }

                    DateTime RecvTimeout = DateTime.Now.AddMilliseconds(timeout);

                    do
                    {
                        if (socket.Available >= size)
                        {
                            if (socket.Receive(buffer, offset, size, SocketFlags.None) != size)
                                return false;
                            else
                                return true;
                        }

                        socket.Send(new byte[0]);

                        //if (!socket.Connected)
                            //throw new ReaderException(m_Result = Result.NETWORK_LOST);

                        if (bufferfull)
                        {
                            COMM_AdapterCommand(READERCMD.RESUME);
                            bufferfull = false;
                        }

                        Thread.Sleep(10);

                    } while (DateTime.Now < RecvTimeout);
                }
                catch (Exception ex)
                {
                    DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Error : " + ex.Message);
                    throw new ReaderException(m_Result = Result.NETWORK_LOST);
                }

                return false;
            }
            finally
            {
                //DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Exit function : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

#if oldgoodcode
        private Boolean TCP_Recv(Socket socket, byte[] buffer, int offset, int size, uint timeout)
        {
            //DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Enter function : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            //DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Parameter size (dec): " + size.ToString());

            try
            {
                if (size == 0)
                    return true;

                if (socket == null)
                    return false;

                try
                {
                    if (CheckAbortRespBufferSize > 0)
                    {
                        if (size > CheckAbortRespBufferSize)
                        {
                            Array.Copy(CheckAbortRespBuffer, 0, buffer, offset, CheckAbortRespBufferSize);
                            CheckAbortRespBufferSize = 0;
                            offset += CheckAbortRespBufferSize;
                            size -= CheckAbortRespBufferSize;
                        }
                        else
                        {
                            Array.Copy(CheckAbortRespBuffer, 0, buffer, offset, size);
                            CheckAbortRespBufferSize -= size;
                            if (CheckAbortRespBufferSize > 0)
                                Array.Copy(CheckAbortRespBuffer, size, CheckAbortRespBuffer, 0, CheckAbortRespBufferSize);
                            return true;
                        }
                    }

                    if (socket.Available < size)
                    {
                        if (timeout == 0)
                            return false;

                        if (bufferfull)
                        {
                            COMM_AdapterCommand(READERCMD.RESUME);
                            bufferfull = false;
                        }

                        DateTime RecvTimeout = DateTime.Now.AddMilliseconds(timeout);

                        do
                        {
                            Thread.Sleep(10);
                        } while (DateTime.Now < RecvTimeout && socket.Available < size);
                        

                        IAsyncResult AsyncResult = socket.BeginReceive(buffer, offset, size, SocketFlags.None, null, null);

                        Thread.Sleep(10);

                        if (AsyncResult.IsCompleted)
                        {
                            if (socket.EndReceive(AsyncResult) != size)
                                return false;

                            return true;
                        }
                        
                        return false;
                    }
                    else
                    {
#if WindowsCE
                        if (bufferfull == false && socket.Available >= 60000)
#else
                        if (bufferfull == false && socket.Available >= socket.ReceiveBufferSize)
#endif
                        {
                            bufferfull = true;
                            COMM_AdapterCommand(READERCMD.PAUSE);
                        }
                    
                        if (socket.Receive(buffer, offset, size, SocketFlags.None) != size)
                            return false;

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    int bc = 0;

                    DEBUGT_WriteLine(DEBUGLEVEL.API, bc.ToString());

                    DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Error : " + ex.Message);
                    throw new ReaderException(m_Result = Result.NETWORK_LOST);
                }

                return false;
            }
            finally
            {
                //DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Exit function : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
#endif






# if oldcode

        bool bufferfull = false;
        byte[] CheckAbortRespBuffer = new byte[8];
        int CheckAbortRespBufferSize = 0;
        private Boolean TCP_Recv(Socket socket, byte[] buffer, int offset, int size, uint timeout)
        {
            DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Enter function : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Parameter size (dec): " + size.ToString());

            try
            {
                if (size == 0)
                    return true;

                if (socket == null)
                    return false;

                try
                {
                    if (CheckAbortRespBufferSize > 0)
                    {
                        if (size > CheckAbortRespBufferSize)
                        {
                            Array.Copy(CheckAbortRespBuffer, 0, buffer, offset, CheckAbortRespBufferSize);
                            CheckAbortRespBufferSize = 0;
                            offset += CheckAbortRespBufferSize;
                            size -= CheckAbortRespBufferSize;
                        }
                        else
                        {
                            Array.Copy(CheckAbortRespBuffer, 0, buffer, offset, size);
                            CheckAbortRespBufferSize -= size;
                            if (CheckAbortRespBufferSize > 0)
                                Array.Copy(CheckAbortRespBuffer, size, CheckAbortRespBuffer, 0, CheckAbortRespBufferSize);
                            return true;
                        }
                    }

                    if (timeout == 0 && socket.Available < size)
                        return false;

                    DateTime RecvTimeout = DateTime.Now.AddMilliseconds(timeout);

                    IAsyncResult AsyncResult = socket.BeginReceive(buffer, offset, size, SocketFlags.None, null, null);

                    do
                    {
#if WindowsCE
                    if (bufferfull == false && socket.Available >= 60000)
#else
                        if (bufferfull == false && socket.Available >= socket.ReceiveBufferSize)
#endif
                        {
                            bufferfull = true;
                            COMM_AdapterCommand(READERCMD.PAUSE);
                        }

                        if (AsyncResult.IsCompleted)
                        {
                            if (socket.EndReceive(AsyncResult) != size)
                            {
                                return false;
                            }

                            return true;
                        }

                        if (bufferfull && socket.Available < 100)
                        {
                            COMM_AdapterCommand(READERCMD.RESUME);
                            bufferfull = false;
                        }

                        Thread.Sleep(10);

                    } while (DateTime.Now < RecvTimeout);

                    DateTime.Now.Ticks 


                }
                catch (Exception ex)
                {
                    DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Error : " + ex.Message);
                    throw new ReaderException(m_Result = Result.NETWORK_LOST);
                }

                return false;
            }
            finally
            {
                DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Exit function : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
#endif

#if oldcode
        bool bufferfull = false;
        byte[] CheckAbortRespBuffer = new byte[8];
        int CheckAbortRespBufferSize = 0;
        private Boolean TCP_Recv(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {

            //int received = 0;  // how many bytes is already received
            DateTime EndTime;

            if (size == 0)
                return true;

            if (socket == null)
                return false;

            EndTime = DateTime.Now.AddMilliseconds(timeout);
            try
            {
                if (CheckAbortRespBufferSize > 0)
                {
                    if (size > CheckAbortRespBufferSize)
                    {
                        Array.Copy(CheckAbortRespBuffer, 0, buffer, offset, CheckAbortRespBufferSize);
                        CheckAbortRespBufferSize = 0;
                        offset += CheckAbortRespBufferSize;
                        size -= CheckAbortRespBufferSize;
                    }
                    else
                    {
                        Array.Copy(CheckAbortRespBuffer, 0, buffer, offset, size);
                        CheckAbortRespBufferSize -= size;
                        if (CheckAbortRespBufferSize > 0)
                            Array.Copy(CheckAbortRespBuffer, size, CheckAbortRespBuffer, 0, CheckAbortRespBufferSize);
                        return true;
                    }
                }

                while (DateTime.Now < EndTime)
                {
#if WindowsCE
                    if (bufferfull == false && socket.Available >= 60000)
#else
                    if (bufferfull == false && socket.Available >= socket.ReceiveBufferSize)
#endif
                    {
                        bufferfull = true;
                        COMM_AdapterCommand(READERCMD.PAUSE);
                    }
                    else if (bufferfull && socket.Available < 8)
                    {
                        COMM_AdapterCommand(READERCMD.RESUME);
                        bufferfull = false;
                    }

                    if (socket.Available >= size)
                    {
                        //received = socket.Receive(buffer, offset, size, SocketFlags.None);
                        //if (received != size)
                        if (socket.Receive(buffer, offset, size, SocketFlags.None) != size)
                            return false;

                        return true;
                    }

                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new ReaderException(m_Result = Result.NETWORK_LOST);
            }

            return false;
        }
#endif


        private Boolean TCP_SendCMDwRet(Socket s, byte[] SendBuf, int SendLen, byte[] RecvBuf, int RecvLen)
        {
#if DEBUG
            //            Console.Out.WriteLine("{0} : Entry", System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            if (TCP_Send(s, SendBuf, 0, SendLen) == false)
                return false;

            if (RecvBuf != null && RecvLen > 0)
                if (TCP_Recv(s, RecvBuf, 0, RecvLen, 5000) == false)
                    return false;

            return true;
        }

        private int Net_Netfinder()
        {
            byte[] CMDBuf = new byte[10];
            Random myRand = new Random();
            UdpClient NetFind = new UdpClient();
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(System.Net.IPAddress.Parse("255.255.255.255"), 3040);

            CMDBuf[0] = 0x00;
            CMDBuf[1] = 0x00;
            CMDBuf[2] = (byte)myRand.Next(0, 255);
            CMDBuf[3] = (byte)myRand.Next(0, 255);

            NetFind.Send(CMDBuf, 4, RemoteIpEndPoint);

            Byte[] receiveBytes = NetFind.Receive(ref RemoteIpEndPoint);

            return 1;
        }


#if oldcode
        private static bool Net_UDP_SendBCmd(byte[] sendbuf, int len, byte[] RecvBuf)
        {
            try
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(System.Net.IPAddress.Parse("255.255.255.255"), 3041);
                UdpClient udpClient = new UdpClient();          // UDP 3041
                DateTime timeout = DateTime.Now.AddSeconds(1);

                byte[] UdpBuf = new byte[4 + RecvBuf.Length];

                udpClient.Send(sendbuf, len, RemoteIpEndPoint);
                IAsyncResult AsyncResult = udpClient.Client.BeginReceive(UdpBuf, 0, UdpBuf.Length, SocketFlags.None, null, null);
                do
                {
                    Thread.Sleep(100);

                    if (AsyncResult.IsCompleted)
                    {
                        if (udpClient.Client.EndReceive(AsyncResult) != (4 + RecvBuf.Length))
                            return false;

                        Array.Copy(UdpBuf, 4, RecvBuf, 0, RecvBuf.Length);
                        return true;
                    }
                } while (DateTime.Now < timeout);
            }
            catch (Exception ex)
            {
            }

            return false;
        }

        private static bool Net_UDP_SendBCmd(string IPaddress, byte[] sendbuf, int len, byte[] RecvBuf)
        {
            try
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(System.Net.IPAddress.Parse(IPaddress), 3041);
                UdpClient udpClient = new UdpClient();          // UDP 3041
                DateTime timeout = DateTime.Now.AddSeconds(1);

                Byte[] UdpBuf = new byte[4 + RecvBuf.Length];

                udpClient.Send(sendbuf, len, RemoteIpEndPoint);

                IAsyncResult AsyncResult = udpClient.Client.BeginReceive(UdpBuf, 0, UdpBuf.Length, SocketFlags.None, null, null);
                do
                {
                    Thread.Sleep(10);

                    if (AsyncResult.IsCompleted)
                    {
                        if (udpClient.Client.EndReceive(AsyncResult) != (4 + RecvBuf.Length))
                            return false;

                        Array.Copy(UdpBuf, 4, RecvBuf, 0, RecvBuf.Length);
                        return true;
                    }
                } while (DateTime.Now < timeout);
            }
            catch (Exception ex)
            {
            }

            return false;
        }
#endif

        private static bool Net_UDP_SendBCmd(long IPaddress, byte[] sendbuf, int len, byte[] RecvBuf)
        {
            UdpClient udpClient = new UdpClient();          // UDP 3041
            IAsyncResult AsyncResult = null;
            Byte[] UdpBuf = new byte[4 + RecvBuf.Length];

            try
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPaddress, 3041);
                DateTime timeout = DateTime.Now.AddSeconds(1);


                udpClient.Send(sendbuf, len, RemoteIpEndPoint);

                AsyncResult = udpClient.Client.BeginReceive(UdpBuf, 0, UdpBuf.Length, SocketFlags.None, null, null);
                do
                {
                    Thread.Sleep(10);

                    if (AsyncResult.IsCompleted)
                    {
                        if (udpClient.Client.EndReceive(AsyncResult) != (4 + RecvBuf.Length))
                            return false;

                        if (RecvBuf.Length > 0)
                            Array.Copy(UdpBuf, 4, RecvBuf, 0, RecvBuf.Length);

                        return true;
                    }
                } while (DateTime.Now < timeout);
            }
            catch (Exception ex)
            {
            }

            udpClient.Client.Close();

            return false;
        }

#if nouse
        /// <summary>
        /// Direct search reader and return mode status
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        bool DirectSearch(IPAddress IP, ref CSLibrary.Net.DeviceInfomation di)
        {
            int retry = 100000;
            Random rand = new Random();
            int m_rand;
            UdpClient udpClient = new UdpClient();          // UDP 3040
            IAsyncResult AsyncResult = null;
            Byte[] UdpBuf = new byte[4096];
            Byte[] bdirectSeatchCmd = new byte[4] { 0, 0, 0, 0 };

            while (retry > 0)
            {
                m_rand = rand.Next(1, 0x7fff); // rand returns a number between 0 and 0x7FFF
                bdirectSeatchCmd[2] = (byte)(m_rand >> 8);
                bdirectSeatchCmd[3] = (byte)(m_rand & 0x00FF);

                try
                {
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IP, 3040);
                    //DateTime timeout = DateTime.Now.AddSeconds(1);
                    DateTime timeout = DateTime.Now.AddMilliseconds (100);

                    udpClient.Send(bdirectSeatchCmd, 4, RemoteIpEndPoint);

                    AsyncResult = udpClient.Client.BeginReceive(UdpBuf, 0, UdpBuf.Length, SocketFlags.None, null, null);
                    do
                    {
                        Thread.Sleep(10);

                        if (AsyncResult.IsCompleted)
                        {
                            if ((udpClient.Client.EndReceive(AsyncResult) < 32) || (UdpBuf[2] != bdirectSeatchCmd[2]) || (UdpBuf[3] != bdirectSeatchCmd[3]))
                                return false;

                            di.Mode = Enum.IsDefined(typeof(CSLibrary.Net.Mode), (int)UdpBuf[1]) ? (CSLibrary.Net.Mode)UdpBuf[1] : CSLibrary.Net.Mode.Unknown;

                            return true;
                        }
                    } while (DateTime.Now < timeout);
                }
                catch (Exception ex)
                {
                }

                udpClient.Client.Close();

                retry--;
            }

            return false;
        }
#endif
        /// <summary>
        /// Direct search reader and return mode status
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        bool DirectSearch(IPAddress IP, ref CSLibrary.Net.DeviceInfomation di)
        {
            int retry = 3;
            Random rand = new Random();
            int m_rand;
            UdpClient udpClient = null;
            IAsyncResult AsyncResult = null;
            Byte[] UdpBuf = new byte[4096];
            Byte[] bdirectSeatchCmd = new byte[4] { 0, 0, 0, 0 };
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IP, 3040);
            DateTime timeout;


            while (retry > 0)
            {
                m_rand = rand.Next(1, 0x7fff); // rand returns a number between 0 and 0x7FFF
                bdirectSeatchCmd[2] = (byte)(m_rand >> 8);
                bdirectSeatchCmd[3] = (byte)(m_rand & 0x00FF);

                try
                {
                    udpClient = new UdpClient();          // UDP 3040
                    udpClient.Send(bdirectSeatchCmd, 4, RemoteIpEndPoint);
                    AsyncResult = udpClient.Client.BeginReceive(UdpBuf, 0, UdpBuf.Length, SocketFlags.None, null, null);

                    timeout = DateTime.Now.AddSeconds(1);
                    do
                    {
                        Thread.Sleep(10);

                        if (AsyncResult.IsCompleted)
                        {
                            if ((udpClient.Client.EndReceive(AsyncResult) < 32) || (UdpBuf[2] != bdirectSeatchCmd[2]) || (UdpBuf[3] != bdirectSeatchCmd[3]))
                                return false;

                            di.Mode = Enum.IsDefined(typeof(CSLibrary.Net.Mode), (int)UdpBuf[1]) ? (CSLibrary.Net.Mode)UdpBuf[1] : CSLibrary.Net.Mode.Unknown;

                            return true;
                        }
                    } while (DateTime.Now < timeout);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    AsyncResult.AsyncWaitHandle.Close();
                    udpClient.Client.Close();
                }

                retry--;
            }

            return false;
        }


    }




    internal class RFIDREADERCONTROL
    {
        enum MACHINESTATE{
            DISCONNECT,
            CONNECTED,
        }

        MACHINESTATE _state;
        
        public RFIDREADERCONTROL()
        {
        }

        ~RFIDREADERCONTROL()
        {
        }




        System.Threading.Thread TCPRecvHandle;

        void CheckRecvBuffer()
        {
            if (TCPRecvHandle.IsAlive)
                return;
        }










        Socket _LoopSocket = null;

        private void TCP_Recv(Socket s)
        {
            if (s == null)
                break;

            if (_state != MACHINESTATE.CONNECTED)
            {

            }

            while (_state == MACHINESTATE.CONNECTED)
            {
                //socket.Send(new byte[0]);
                if (!socket.Connected)
                {
                    strRetPage = "Unable to connect to host";
                }
                // Use the SelectWrite enumeration to obtain Socket status.
                if (socket.Poll(-1, SelectMode.SelectWrite))
                {
                    Console.WriteLine("This Socket is writable.");
                }
                else if (socket.Poll(-1, SelectMode.SelectRead))
                {
                    Console.WriteLine("This Socket is readable.");
                }
                else if (socket.Poll(-1, SelectMode.SelectError))
                {
                    Console.WriteLine("This Socket has an error.");
                }
                else if (socket.Available > 0)
                {
                    int len = socket.Available;
                    byte[] buf = new data[len];
                    socket.Receive(buffer, offset, len, SocketFlags.None);
                    buf.inset(buf);
                }
                else
                {
                    System.Threading.Thread.Sleep(50);
                }
            }

            _state == MACHINESTATE.DISCONNECT;
        }


        private Boolean TCP_Recv(Socket socket, byte[] buffer, int offset, int size, uint timeout)
        {
            //DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Enter function : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            //DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Parameter size (dec): " + size.ToString());

            try
            {
                if (size == 0)
                    return true;

                if (socket == null || !socket.Connected)
                    return false;

                try
                {
                    if (CheckAbortRespBufferSize > 0)
                    {
                        if (size > CheckAbortRespBufferSize)
                        {
                            Array.Copy(CheckAbortRespBuffer, 0, buffer, offset, CheckAbortRespBufferSize);
                            CheckAbortRespBufferSize = 0;
                            offset += CheckAbortRespBufferSize;
                            size -= CheckAbortRespBufferSize;
                        }
                        else
                        {
                            Array.Copy(CheckAbortRespBuffer, 0, buffer, offset, size);
                            CheckAbortRespBufferSize -= size;
                            if (CheckAbortRespBufferSize > 0)
                                Array.Copy(CheckAbortRespBuffer, size, CheckAbortRespBuffer, 0, CheckAbortRespBufferSize);
                            return true;
                        }
                    }

                    if (bufferfull == false && socket.Available >= socket.ReceiveBufferSize)
                    {
                        bufferfull = true;
                        COMM_AdapterCommand(READERCMD.PAUSE);
                    }

                    DateTime RecvTimeout = DateTime.Now.AddMilliseconds(timeout);

                    do
                    {
                        if (socket.Available >= size)
                        {
                            if (socket.Receive(buffer, offset, size, SocketFlags.None) != size)
                                return false;
                            else
                                return true;
                        }

                        socket.Send(new byte[0]);

                        //if (!socket.Connected)
                        //throw new ReaderException(m_Result = Result.NETWORK_LOST);

                        if (bufferfull)
                        {
                            COMM_AdapterCommand(READERCMD.RESUME);
                            bufferfull = false;
                        }

                        Thread.Sleep(10);

                    } while (DateTime.Now < RecvTimeout);
                }
                catch (Exception ex)
                {
                    DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Error : " + ex.Message);
                    throw new ReaderException(m_Result = Result.NETWORK_LOST);
                }

                return false;
            }
            finally
            {
                //DEBUGT_WriteLine(DEBUGLEVEL.PERFORMANCE, "Exit function : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        
        
        
        
        private static void TCP_SetKeepAlive(Socket s, ulong keepalive_time, ulong keepalive_interval)
        {
#if WIN32
            ulong[] input_params = new ulong[3];
            byte[] keep_alive = new byte[12];

            if (keepalive_time == 0 || keepalive_interval == 0)
                input_params[0] = 0;
            else
                input_params[0] = 1;
            input_params[1] = keepalive_time;
            input_params[2] = keepalive_interval;

            for (int cnt = 0; cnt < input_params.Length; cnt++)
            {
                keep_alive[cnt * 4 + 3] = (byte)(input_params[cnt] >> 24 & 0xff);
                keep_alive[cnt * 4 + 2] = (byte)(input_params[cnt] >> 16 & 0xff);
                keep_alive[cnt * 4 + 1] = (byte)(input_params[cnt] >> 8 & 0xff);
                keep_alive[cnt * 4 + 0] = (byte)(input_params[cnt] & 0xff);
            }

            s.IOControl(IOControlCode.KeepAliveValues, keep_alive, null);
#endif
        }



    }



}



// First Connect to reader
// 