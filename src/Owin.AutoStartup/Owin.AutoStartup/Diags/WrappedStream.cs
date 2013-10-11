namespace Owin.AutoStartup.Diags
{
    using System;
    using System.IO;
    using System.Runtime.Remoting;

    public class WrappedStream : Stream
    {
        private readonly Stream stream;

        public bool HasBeenWrittenTo { get; set; }

        public WrappedStream(Stream stream)
        {
            this.stream = stream;
        }

        public override void Close()
        {
            this.stream.Close();
        }

        protected override void Dispose(bool disposing)
        {
            this.stream.Dispose();
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.stream.EndRead(asyncResult);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.HasBeenWrittenTo = true;
            return this.stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.stream.EndWrite(asyncResult);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return this.stream.ReadByte();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.HasBeenWrittenTo = true;

            this.stream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            this.HasBeenWrittenTo = true;

            this.stream.WriteByte(value);
        }

        public override bool CanRead
        {
            get
            {
                return this.stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.stream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return this.stream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return this.stream.Length;
            }
        }

        public override long Position { get; set; }
        public override int ReadTimeout { get; set; }
        public override int WriteTimeout { get; set; }

        public override object InitializeLifetimeService()
        {
            return this.stream.InitializeLifetimeService();
        }

        public override ObjRef CreateObjRef(Type requestedType)
        {
            return this.stream.CreateObjRef(requestedType);
        }

        public override string ToString()
        {
            return this.stream.ToString();
        }

        public override bool Equals(object obj)
        {
            return this.stream.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.stream.GetHashCode();
        }

        public Stream UnWrap()
        {
            return this.stream;
        }
    }
}