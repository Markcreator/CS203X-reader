#if CS101
using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.GPRS
{
    public class GpsStream : System.IO.Stream
    {
        public GpsStream()
        {
            GPS.cs501_Unsolicited_NMEA_Mode(2, true, true, true, true, true, true);
        }
        public override void Close()
        {
            GPS.cs501_Unsolicited_NMEA_Mode(0, false, false, false, false, false, false);
        }

        public override bool CanRead
        {
            get { return true; }
        }
        public override bool CanTimeout
        {
            get
            {
                return base.CanTimeout;
            }
        }
        public override bool CanSeek
        {
            get { return false; }
        }
        public override bool CanWrite
        {
            get { return false; }
        }
        public override long Length
        {
            get { return -1; }
        }
        public override long Position
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }
        public override void SetLength(long value)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            uint recv = 0;
            GPS.cs501_Unsolicited_NMEA_Read(buffer, count, out recv);
            return (int)recv;
        }
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override void Flush()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        
    }
}
#endif