namespace HexaEngine.IO
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    public class VirtualStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly long _start;
        private readonly bool disposeable;
        private long position;

        public VirtualStream(Stream baseStream, long start, long length, bool disposeable)
        {
            _baseStream = baseStream;
            _start = start;
            Length = length;
            this.disposeable = disposeable;
        }

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length { get; }

        public override long Position { get => position; set => position = value; }

        protected override void Dispose(bool disposing)
        {
            if (disposeable)
            {
                _baseStream.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public IntPtr GetIntPtr(out int length)
        {
            var buffer = new byte[Length];
            _baseStream.Position = _start;
            _ = _baseStream.Read(buffer);
            var ptr = Marshal.AllocHGlobal((int)Length);
            Marshal.Copy(buffer, 0, ptr, (int)Length);
            length = (int)Length;
            return ptr;
        }

        public byte[] GetBytes()
        {
            var buffer = new byte[Length];
            _baseStream.Position = _start;
            _ = _baseStream.Read(buffer);
            return buffer;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position == Length)
                return 0;
            if (buffer.Length - offset < count)
                throw new ArgumentException(null, nameof(buffer));
            _baseStream.Position = _start + Position;

            var result = _baseStream.Read(buffer, offset, (int)(count + Position > Length ? Length - Position : count));

            Position += result;
            if (Position > Length)
                Position = Length;
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;

                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Position = _start + Position;
            _baseStream.Write(buffer, offset, count);
        }
    }

    public class OffsetStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly long _start;
        private readonly bool disposeable;
        private long position;

        public OffsetStream(Stream baseStream, long start, bool disposeable)
        {
            _baseStream = baseStream;
            _start = start;
            this.disposeable = disposeable;
        }

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length - _start;

        public override long Position { get => position; set => position = value; }

        protected override void Dispose(bool disposing)
        {
            if (disposeable)
            {
                _baseStream.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public IntPtr GetIntPtr(out int length)
        {
            var buffer = new byte[Length];
            _baseStream.Position = _start;
            _ = _baseStream.Read(buffer);
            var ptr = Marshal.AllocHGlobal((int)Length);
            Marshal.Copy(buffer, 0, ptr, (int)Length);
            length = (int)Length;
            return ptr;
        }

        public byte[] GetBytes()
        {
            var buffer = new byte[Length];
            _baseStream.Position = _start;
            _ = _baseStream.Read(buffer);
            return buffer;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position == Length)
                return 0;
            if (buffer.Length - offset < count)
                throw new ArgumentException(null, nameof(buffer));
            _baseStream.Position = _start + Position;

            var result = _baseStream.Read(buffer, offset, (int)(count + Position > Length ? Length - Position : count));

            Position += result;
            if (Position > Length)
                Position = Length;
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;

                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(_start + value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Position = _start + Position;
            _baseStream.Write(buffer, offset, count);
        }
    }
}