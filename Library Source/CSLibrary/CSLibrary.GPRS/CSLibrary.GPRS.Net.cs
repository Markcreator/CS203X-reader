
#if CS101
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace CSLibrary.GPRS.Net
{
    /// <summary>
    /// cs501 Socket
    /// </summary>
    public sealed class Socket : IDisposable
    {
        private System.Int32 m_handle = -1;
        private bool m_open = false;
        private bool m_disconnect = true;
        //private int m_timeout = -1;
        private List<byte> recv_buf = new List<byte>();

        internal Socket(System.Int32 handle)
        {
            if (handle == -1)
                //throw new Exception("Invalid handle");
                throw new System.ComponentModel.Win32Exception();
            m_handle = handle;
            m_open = true;
            m_disconnect = false;
            //m_timeout = -1;
        }

        #region IDisposable Members
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (m_open)
                Close();
        }

        #endregion

        [DllImport(Device.cs501_dll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern System.Int32 cs501_Socket_ConnectW(String server, uint port);

        /// <summary>
        /// create TCP connection to server:ip
        /// </summary>
        /// <param name="server">server name/address</param>
        /// <param name="port">server port number</param>
        /// <returns>NetworkStream</returns>
        static public Socket Connect(string server, int port)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return null;

            System.Int32 handle = cs501_Socket_ConnectW(server, Convert.ToUInt32(port));
            if (handle == -1)
                //throw new System.IO.IOException("cs501 socket connect");
                throw new System.ComponentModel.Win32Exception();

            return new Socket(handle);
        }

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_Socket_Close(System.Int32 handle);

        /// <summary>
        /// close socket
        /// </summary>
        /// <seealso cref="System.IO.Stream.Close"/>
        public void Close()
        {
            if (m_open)
            {
                if (!cs501_Socket_Close(m_handle))
                    throw new System.ComponentModel.Win32Exception();
                m_handle = -1;
            }
            m_open = false;
            m_disconnect = true;
        }

        private bool FillRecv()
        {
            if (!m_open)
                throw new System.ObjectDisposedException("cs501 socket not open");
            if (m_disconnect)
                return false;

            uint NumberOfBytesRead;
            byte[] buffer = new byte[4000];
            if (!cs501_Socket_Read(m_handle, buffer, Convert.ToUInt32(buffer.Length), out NumberOfBytesRead))
            {
                if (Marshal.GetLastWin32Error() == 10054) //WSAECONNRESET
                {
                    m_disconnect = true;
                    return false;
                }
                //throw new System.IO.IOException();
                throw new System.ComponentModel.Win32Exception();
            }
            if (NumberOfBytesRead > 0)
            {
#if false//DEBUG
                {
                    int i;
                    StringBuilder str = new StringBuilder(Convert.ToInt16(NumberOfBytesRead));
                    for (i = 0; i < NumberOfBytesRead; i++)
                        str.Append((char)buf[i + offset]);
                    System.Diagnostics.Debug.WriteLine("");
                    System.Diagnostics.Debug.WriteLine("<~");
                    System.Diagnostics.Debug.WriteLine(str);
                    System.Diagnostics.Debug.WriteLine("~>");
                    System.Diagnostics.Debug.WriteLine("");
                }
#endif
                for (int i = 0; i < NumberOfBytesRead; i++)
                    recv_buf.Add(buffer[i]);
                return true;
            }
            return true;
        }
        /// <summary>
        /// Poll to recv data
        /// </summary>
        /// <param name="microSeconds"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool Poll(int microSeconds, System.Net.Sockets.SelectMode mode)
        {
            switch (mode)
            {
                case System.Net.Sockets.SelectMode.SelectRead:
                    if (m_disconnect)
                        return false;
                    {
                        DateTime start = DateTime.Now;
                        for (; ; )
                        {
                            if (!FillRecv())
                                return false;

                            if (recv_buf.Count > 0)
                                return true;

                            if ((DateTime.Now - start).Ticks > microSeconds * TimeSpan.TicksPerMillisecond / 1000)
                                break;

                            System.Threading.Thread.Sleep(100);
                            //System.Diagnostics.Debug.Write(".");
                        }
                    }
                    break;
                case System.Net.Sockets.SelectMode.SelectWrite:
                    return true;
                //break;
                case System.Net.Sockets.SelectMode.SelectError:
                    return false;
                //break;
            }
            return false;
        }

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_Socket_Read(System.Int32 handle, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead);
        /// <summary>
        /// Receive Data
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int Receive(byte[] buffer)
        {
            if (!m_open)
                throw new System.ObjectDisposedException("cs501 socket not open");

            while (recv_buf.Count == 0)
            {
                if (m_disconnect)
                    return 0;

                if (!FillRecv())
                    return 0;

                System.Threading.Thread.Sleep(100);
                //System.Diagnostics.Debug.Write(".");
            }
            int size = recv_buf.Count;
            if (size > buffer.Length)
                size = buffer.Length;
            recv_buf.CopyTo(0, buffer, 0, size);
            recv_buf.RemoveRange(0, size);
            return size;
        }

        [DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_Socket_Write(System.Int32 handle, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten);
        /// <summary>
        /// Send Data
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int Send(byte[] buffer)
        {
            if (!m_open)
                throw new System.ObjectDisposedException("cs501 socket send");
            if (m_disconnect)
                throw new System.InvalidOperationException("cs501 socket send");

            {
                uint lpNumberOfBytesWritten;
                if (!cs501_Socket_Write(m_handle, buffer, Convert.ToUInt32(buffer.Length), out lpNumberOfBytesWritten))
                {
                    if (Marshal.GetLastWin32Error() == 1117) //ERROR_IO_DEVICE
                        throw new System.Net.Sockets.SocketException();
                    //throw new System.IO.IOException();
                    throw new System.ComponentModel.Win32Exception();
                }
            }
            return buffer.Length;
        }
    }

    /// <summary>
    /// NetworkStream for cs501 socket
    /// </summary>
    /// <seealso cref="System.IO.Stream"/>
    public sealed class NetworkStream : System.IO.Stream
    {
        private System.Int32 m_handle = -1;
        private bool m_open = false;
        private bool m_disconnect = true;
        private int m_timeout = -1;

        internal NetworkStream(System.Int32 handle)
        {
            if (handle == -1)
                //throw new Exception("Invalid handle");
                throw new System.ComponentModel.Win32Exception();
            m_handle = handle;
            m_open = true;
            m_disconnect = false;
            m_timeout = -1;
        }

        #region IDisposable Members
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (m_open)
                Close();

            base.Dispose(disposing);
        }

        #endregion

        /*[DllImport(Device.cs501_dll, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern System.Int32 cs501_Socket_ConnectW(String server, uint port);*/

        /// <summary>
        /// create TCP connection to server:ip
        /// </summary>
        /// <param name="server">server name/address</param>
        /// <param name="port">server port number</param>
        /// <returns>NetworkStream</returns>
        static public NetworkStream Connect(string server, int port)
        {
            if (Device.DeviceStatus != Device.Status.Ready)
                return null;

            System.Int32 handle = Socket.cs501_Socket_ConnectW(server, Convert.ToUInt32(port));
            if (handle == -1)
                //throw new System.IO.IOException("cs501 socket connect");
                throw new System.ComponentModel.Win32Exception();

            return new Net.NetworkStream(handle);
        }

        /*[DllImport(Device.cs501_dll, SetLastError = true)]
        internal static extern bool cs501_Socket_Close(System.Int32 handle);*/

        /// <summary>
        /// close socket
        /// </summary>
        /// <seealso cref="System.IO.Stream.Close"/>
        public override void Close()
        {
            base.Close();
            if (m_open)
            {
                if (!Socket.cs501_Socket_Close(m_handle))
                    throw new System.ComponentModel.Win32Exception();
                m_handle = -1;
            }
            m_open = false;
            m_disconnect = true;
        }

        /// <seealso cref="System.IO.Stream.CanRead"/>
        public override bool CanRead
        {
            //get { throw new Exception("The method or operation is not implemented."); }
            get { return true; }
        }

        /// <seealso cref="System.IO.Stream.CanSeek"/>
        public override bool CanSeek
        {
            //get { throw new Exception("The method or operation is not implemented."); }
            get { return false; }
        }

        /// <seealso cref="System.IO.Stream.CanWrite"/>
        public override bool CanWrite
        {
            //get { throw new Exception("The method or operation is not implemented."); }
            get { return true; }
        }

        /// <seealso cref="System.IO.Stream.Flush"/>
        public override void Flush()
        {
            //throw new Exception("The method or operation is not implemented.");
            //System.Diagnostics.Debug.WriteLine("cs501 socket flush");
        }

        /// <seealso cref="System.IO.Stream.Length"/>
        public override long Length
        {
            //get { throw new Exception("The method or operation is not implemented."); }
            get
            {
                if (!m_open)
                    throw new System.ObjectDisposedException("cs501 socket not open");

                throw new System.NotSupportedException();
            }
        }

        /// <seealso cref="System.IO.Stream.Position"/>
        public override long Position
        {
            get
            {
                //throw new Exception("The method or operation is not implemented.");
                if (!m_open)
                    throw new System.ObjectDisposedException("cs501 socket not open");

                throw new System.NotSupportedException();
            }
            set
            {
                //throw new Exception("The method or operation is not implemented.");
                if (!m_open)
                    throw new System.ObjectDisposedException("cs501 socket not open");

                throw new System.NotSupportedException();
            }
        }

        internal int Read0(byte[] buffer, int count)
        {
            if (!m_open)
                throw new System.ObjectDisposedException("cs501 socket not open");
            if (m_disconnect)
                return 0;

            DateTime start = DateTime.Now;
            for (; ; )
            {
                uint NumberOfBytesRead;
                if (!Socket.cs501_Socket_Read(m_handle, buffer, Convert.ToUInt32(count), out NumberOfBytesRead))
                {
                    if (Marshal.GetLastWin32Error() == 10054) //WSAECONNRESET
                    {
                        m_disconnect = true;
                        return 0;
                    }
                    //throw new System.IO.IOException();
                    throw new System.ComponentModel.Win32Exception();
                }
                if (NumberOfBytesRead > 0)
                {
#if false//DEBUG
                    {
                        int i;
                        StringBuilder str = new StringBuilder(Convert.ToInt16(NumberOfBytesRead));
                        for (i = 0; i < NumberOfBytesRead; i++)
                            str.Append((char)buf[i + offset]);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine("<~");
                        System.Diagnostics.Debug.WriteLine(str);
                        System.Diagnostics.Debug.WriteLine("~>");
                        System.Diagnostics.Debug.WriteLine("");
                    }
#endif
                    return Convert.ToInt32(NumberOfBytesRead);
                }
                if (m_timeout >= 0)
                {
                    if ((DateTime.Now - start).Ticks > m_timeout * TimeSpan.TicksPerMillisecond)
                        throw new System.InvalidOperationException();
                }
                //System.Diagnostics.Debug.Write(".");
                System.Threading.Thread.Sleep(100);
            }
        }

        /// <seealso cref="System.IO.Stream.Read"/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            /*if (offset < 0 || count <= 0)
                throw new System.ArgumentOutOfRangeException();
            if (buffer == null)
                throw new System.ArgumentNullException();
            if (offset + count > buffer.Length)
                throw new System.ArgumentException();*/
            if (offset == 0)
                return Read0(buffer, count);

            {
                byte[] buf = new byte[count];
                count = Read0(buf, count);
                Array.Copy(buf, 0, buffer, offset, count);
                return count;
            }
        }

        /// <seealso cref="System.IO.Stream.Seek"/>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new System.NotSupportedException();
        }

        /// <seealso cref="System.IO.Stream.SetLength"/>
        public override void SetLength(long value)
        {
            throw new System.NotSupportedException();
        }

        internal void Write0(byte[] buffer, int count)
        {
            if (!m_open)
                throw new System.ObjectDisposedException("cs501 socket not open");
            if (m_disconnect)
                throw new System.InvalidOperationException("cs501 socket disconnect");

            {
                uint lpNumberOfBytesWritten;
                if (!Socket.cs501_Socket_Write(m_handle, buffer, Convert.ToUInt32(count), out lpNumberOfBytesWritten))
                {
                    if (Marshal.GetLastWin32Error() == 1117) //ERROR_IO_DEVICE
                        throw new System.Net.Sockets.SocketException();
                    //throw new System.IO.IOException();
                    throw new System.ComponentModel.Win32Exception();
                }
            }
        }

        /// <seealso cref="System.IO.Stream.Write"/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            /*if (offset < 0 || count <= 0)
                throw new System.ArgumentOutOfRangeException();
            if (buffer == null)
                throw new System.ArgumentNullException();
            if (offset + count > buffer.Length)
                throw new System.ArgumentException();*/
#if DEBUG
            /*{
                StringBuilder str = new StringBuilder(count);
                for (int i = 0; i < count; i++)
                    str.Append((char)buffer[i + offset]);
                System.Diagnostics.Debug.WriteLine(str);
            }*/
#endif
            if (offset == 0)
            {
                Write0(buffer, count);
                return;
            }
            {
                byte[] buf = new byte[count];
                Array.Copy(buffer, offset, buf, 0, count);
                Write0(buf, count);
            }
        }

        /// <seealso cref="System.IO.Stream.ReadTimeout"/>
        public override int ReadTimeout
        {
            get
            {
                return m_timeout;
            }
            set
            {
                m_timeout = value;
            }
        }

        /// <seealso cref="System.IO.Stream.WriteTimeout"/>
        public override int WriteTimeout
        {
            get
            {
                return m_timeout;
            }
            set
            {
                m_timeout = value;
            }
        }
    }
}

#endif