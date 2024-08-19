//#define __USE_MUTEX__

using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using BOOL = System.Boolean;

namespace CSLibrary
{
    unsafe class RingBuffer : IDisposable
    {
        IntPtr m_store = IntPtr.Zero;
        Byte* store = null;
        int bufferSize = 0;   // the size of the ring buffer
        volatile int read_pos = 0;   // the read pointer
        volatile int write_pos = 0;  // the write pointer
        volatile int writtenCount = 0;
        bool m_FlowCtl = false;
        object m_lock = null;
        ManualResetEvent m_notEmptyEvent = null;
        ManualResetEvent m_notFullEvent = null;
#if __USE_MUTEX__
        Mutex rwBufferBusy = null;
#endif
        bool disposed = false;

#if WindowsCE
        [DllImport("coredll.dll", SetLastError = false)]
#else
        [DllImport("msvcrt.dll", SetLastError = false)]
#endif
        static extern void memset(IntPtr dest, int val, int size);

#if WindowsCE
        [DllImport("coredll.dll", SetLastError = false)]
#else
        [DllImport("msvcrt.dll", SetLastError = false)]
#endif
        static extern void memcpy(IntPtr dst, IntPtr src, int size);

#if WindowsCE
        [DllImport("coredll.dll", SetLastError = false)]
#else
        [DllImport("msvcrt.dll", SetLastError = false)]
#endif
        static extern void memcpy(IntPtr dst, byte* src, int size);

#if WindowsCE
        [DllImport("coredll.dll", SetLastError = false)]
#else
        [DllImport("msvcrt.dll", SetLastError = false)]
#endif
        static extern void memcpy(byte* dst, byte* src, int size);


#if WindowsCE
        [DllImport("coredll.dll", SetLastError = false)]
#else
        [DllImport("msvcrt.dll", SetLastError = false)]
#endif
        static extern void memcpy(IntPtr dst, Byte[] src, int size);

#if WindowsCE
        [DllImport("coredll.dll", SetLastError = false)]
#else
        [DllImport("msvcrt.dll", SetLastError = false)]
#endif
        static extern void memcpy(Byte[] dst, IntPtr src, int size);

#if WindowsCE
        [DllImport("coredll.dll", SetLastError = false)]
#else
        [DllImport("msvcrt.dll", SetLastError = false)]
#endif
        static extern void memcpy(Byte* dst, IntPtr src, int size);

        /// <summary>
        /// Gets a value indicating wether the buffer is empty or not.
        /// </summary>
        public bool IsEmpty
        {
            get { return writtenCount == 0; }
        }
        /// <summary>
        /// Is buffer full
        /// </summary>
        public bool IsFull
        {
            get
            {
                return writtenCount == bufferSize;
            }
        }

        public RingBuffer(int iBufSize)
        {
            m_store = IntPtr.Zero;
            bufferSize = 0;
            read_pos = 0;
            write_pos = 0;
            m_lock = new object();
            m_notEmptyEvent = new ManualResetEvent(false);
            m_notFullEvent = new ManualResetEvent(true);
#if __USE_MUTEX__
            rwBufferBusy = new Mutex();
#endif
            if (!Create(iBufSize))
            {
                throw new Exception();
            }
        }

        ~RingBuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                if (disposing)
                {
                    //Dispose Managed resources
#if __USE_MUTEX__
                    if (rwBufferBusy != null)
                    {
                        rwBufferBusy.Close();
                        rwBufferBusy = null;
                    }
#endif
                    bufferSize = 0;
                    read_pos = 0;
                    write_pos = 0;
                    m_lock = null;
                }
                //Dispose Unmanaged resources
                m_notEmptyEvent.Set();

                if (m_store != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(m_store);
                    m_store = IntPtr.Zero;
                }

                disposed = true;
            }
        }
        ///////////////////////////////////////////////////////////////////
        // Method: Create
        // Purpose: Initializes the ring buffer for use.
        // Parameters:
        //     [in] iBufSize -- maximum size of the ring buffer
        // Return Value: TRUE if successful, otherwise FALSE.
        //
        BOOL Create(int iBufSize)
        {
            BOOL bResult = false;
            {
                m_store = Marshal.AllocHGlobal(iBufSize);
                if (m_store != IntPtr.Zero)
                {
                    bufferSize = iBufSize;
                    memset(m_store, 0x0, bufferSize);
                    store = (Byte*)m_store.ToPointer();
                    bResult = true;
                }
            }
            return bResult;
        }


        public void Clear()
        {
            read_pos = 0;
            write_pos = 0;
            m_notFullEvent.Set();
            m_notEmptyEvent.Reset();
        }

        public bool FlowCtl
        {
            get { lock (m_lock) return m_FlowCtl; }
            set { lock (m_lock) m_FlowCtl = value; }
        }

        ///////////////////////////////////////////////////////////////////
        // Method: GetMaxReadSize
        // Purpose: Returns the amount of data (in bytes) available for
        //     reading from the buffer.
        // Parameters: (None)
        // Return Value: Amount of data (in bytes) available for reading.
        //
        public int MaxReadSize
        {
            get
            {
                return writtenCount;
            }
        }

        ///////////////////////////////////////////////////////////////////
        // Method: GetMaxWriteSize
        // Purpose: Returns the amount of space (in bytes) available for
        //     writing into the buffer.
        // Parameters: (None)
        // Return Value: Amount of space (in bytes) available for writing.
        //
        public int MaxWriteSize
        {
            get
            {
                return bufferSize - writtenCount;
            }

        }

        ///////////////////////////////////////////////////////////////////
        // Method: WriteBinary
        // Purpose: Writes binary data into the ring buffer.
        // Parameters:
        //     [in] pBuf - Pointer to the data to write.
        //     [in] nBufLen - Size of the data to write (in bytes).
        // Return Value: TRUE upon success, otherwise FALSE.
        // 
        public int Write(Byte* data, int count)
        {
            int size = -1;
            if (disposed)
            {
                throw new ApplicationException("Buffer is closed");
            }

            m_notFullEvent.WaitOne();

            // Gauranteed to not be full at this point, however readers may sill read
            // from the buffer first.
            int max_size = MaxWriteSize;
            //--max_size; // write pointer isn't allowed to make itself >= read_pos
            // The alternative is that the buffer somehow remembers wether the write pos is in
            // front of the read pos or not, but this seemed easier and usually you won't be
            // filling the buffer to the last byte anyway (or you won't care about one byte).
            size = count >= max_size ? max_size : count;

            int new_write_pos = (write_pos + size) % bufferSize;
#if __USE_MUTEX__
            rwBufferBusy.WaitOne();
#endif
            if (write_pos + size <= bufferSize)
            {
                memcpy(store + write_pos, data, size);
            }
            else
            {
                int first_size = bufferSize - write_pos;
                memcpy(store + write_pos, data, first_size);
                memcpy(store, data + first_size, size - first_size);
            }
#pragma warning disable 420
            Interlocked.Exchange(ref write_pos, new_write_pos);
            Interlocked.Exchange(ref writtenCount, writtenCount + size);
#pragma warning restore 420
#if __USE_MUTEX__
            rwBufferBusy.ReleaseMutex();
#endif
            if (IsFull)
            {
                m_notFullEvent.Reset();
            }

            if (!IsEmpty/*setEmpty*/)
            {
                m_notEmptyEvent.Set();
            }
            return size;
        }

        public bool WriteByte(Byte data)
        {
            int size = -1;
            if (disposed)
            {
                throw new ApplicationException("Buffer is closed");
            }

            m_notFullEvent.WaitOne();

            // Gauranteed to not be full at this point, however readers may sill read
            // from the buffer first.
            int max_size = MaxWriteSize;
            //--max_size; // write pointer isn't allowed to make itself >= read_pos
            // The alternative is that the buffer somehow remembers wether the write pos is in
            // front of the read pos or not, but this seemed easier and usually you won't be
            // filling the buffer to the last byte anyway (or you won't care about one byte).
            size = 1;// >= max_size ? max_size : 1;

            int new_write_pos = (write_pos + size) % bufferSize;

#if __USE_MUTEX__
            rwBufferBusy.WaitOne();
#endif

            memcpy(store + write_pos, &data, 1);

#pragma warning disable 420
            Interlocked.Exchange(ref write_pos, new_write_pos);
            Interlocked.Increment(ref writtenCount);
#pragma warning restore 420

#if __USE_MUTEX__
            rwBufferBusy.ReleaseMutex();
#endif
            if (IsFull)
            {
                m_notFullEvent.Reset();
            }

            if (!IsEmpty/*setEmpty*/)
            {
                m_notEmptyEvent.Set();
            }

            return size == 1;
        }

        ///////////////////////////////////////////////////////////////////
        // Method: ReadBinary
        // Purpose: Reads (and extracts) data from the ring buffer.
        // Parameters:
        //     [in/out] pBuf - Pointer to where read data will be stored.
        //     [in] nBufLen - Size of the data to be read (in bytes).
        // Return Value: TRUE upon success, otherwise FALSE.
        // 
        public int Read(Byte* data, int offset, int count, int timeout)
        {
            int size = 0;

            if (!m_notEmptyEvent.WaitOne(timeout, false))
            {
                return size;
            }

            if (IsEmpty)
            {
                count = 0;
            }
            else
            {

                int max_size = MaxReadSize;
                size = max_size > count ? count : max_size;

                int new_read_pos = (read_pos + size) % bufferSize;

#if __USE_MUTEX__
                rwBufferBusy.WaitOne();
#endif
                if (read_pos + size <= bufferSize)
                {
                    memcpy(data, store + read_pos, size);
                }
                else
                {
                    int first_size = bufferSize - read_pos;
                    memcpy(data, store + read_pos, first_size);
                    memcpy(data + first_size, store, size - first_size);
                }

#pragma warning disable 420
                Interlocked.Exchange(ref read_pos, new_read_pos);
                Interlocked.Exchange(ref writtenCount, writtenCount - size);
#pragma warning restore 420

#if __USE_MUTEX__
                rwBufferBusy.ReleaseMutex();
#endif
                if (!disposed && IsEmpty)
                {
                    m_notEmptyEvent.Reset();
                }

                if (!IsFull)
                {
                    m_notFullEvent.Set();
                }
            }

            return size;
        }

        public int ReadByte()
        {
            int result = -1;

            if (!m_notEmptyEvent.WaitOne())
            {
                return result;
            }

            if (IsEmpty)
            {
                return result;
            }
            else
            {
                byte tmpValue = 0;
                int max_size = MaxReadSize;

                int new_read_pos = (read_pos + 1) % bufferSize;

#if __USE_MUTEX__
                rwBufferBusy.WaitOne();
#endif
                memcpy(&tmpValue, store + read_pos, 1);
                result = tmpValue;

#pragma warning disable 420
                Interlocked.Exchange(ref read_pos, new_read_pos);
                Interlocked.Decrement(ref writtenCount);
#pragma warning restore 420

#if __USE_MUTEX__
                rwBufferBusy.ReleaseMutex();
#endif
                if (!disposed && IsEmpty)
                {
                    m_notEmptyEvent.Reset();
                }

                if (!IsFull)
                {
                    m_notFullEvent.Set();
                }
            }

            return result;
        }

        int distance(int reference, int pos)
        {
            return pos >= reference ? pos - reference : bufferSize - (reference - pos);
        }
    }
}

