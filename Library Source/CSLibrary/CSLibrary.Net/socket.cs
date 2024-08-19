using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Reader.Net
{
    public class socket : IDisposable
    {
        Socket m_sock = null;

        public IntPtr Handle
        {
            get { return m_sock.Handle; }
        }

        public socket()
        {
            
        }

        public bool Connect(String IP, Int32 Port)
        {
            try
            {
                m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //m_sock.NoDelay = false;

                m_sock.Connect(IP, Port);

                m_sock.Blocking = true;

                m_sock.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);

                uint dummy = 0;
　　            byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
　　            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);//是否启用Keep-Alive
　　            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, Marshal.SizeOf(dummy));//多长时间开始第一次探测
　　            BitConverter.GetBytes((uint)100).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);//探测时间间隔
                m_sock.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);

                byte[] c = new byte[] { (byte)'G', (byte)'P', (byte)'I', (byte)'O', (byte)'P', (byte)'O', (byte)'W', (byte)'E', (byte)'R', (byte)'_', (byte)'O', (byte)'N' };
                
                m_sock.Send(c);

                rfid.Native.RFID_TCPSetSocket(m_sock.Handle);

            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public void Disconnect()
        {
            LingerOption ling = new LingerOption(true, 1);
            m_sock.LingerState = ling;
            m_sock.Disconnect(false);
            m_sock.Close();
        }

        public rfid.Constants.Result Connected()
        {
            return rfid.Native.RFID_TCPConnected();
        }

        public void Dispose()
        {
            m_sock.Disconnect(true);
            m_sock.Close();
        }
    }
}
