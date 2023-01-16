namespace HexaEngine.IO
{
    using System;
    using System.IO;

    public class VirtualStream : Stream
    {
        private readonly SemaphoreSlim semaphore = new(1);
        private readonly Stream _baseStream;
        private readonly long _start;
        private readonly bool _leaveOpen;
        private long position;

        public VirtualStream(Stream baseStream, long start, long length, bool leaveOpen = false)
        {
            _baseStream = baseStream;
            _start = start;
            Length = length;
            _leaveOpen = leaveOpen;
        }

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length { get; }

        public override long Position { get => position; set => position = value; }

        protected override void Dispose(bool disposing)
        {
            if (!_leaveOpen)
            {
                _baseStream.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            semaphore.Wait();
            _baseStream.Flush();
            semaphore.Release();
        }

        public byte[] ReadBytes()
        {
            semaphore.Wait();
            var buffer = new byte[Length];
            if (_baseStream.CanSeek && _start != 0)
                _baseStream.Position = _start;
            _ = _baseStream.Read(buffer);
            semaphore.Release();
            return buffer;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            semaphore.Wait();
            if (Position == Length)
            {
                semaphore.Release();
                return 0;
            }

            if (buffer.Length - offset < count)
                throw new ArgumentException(null, nameof(buffer));
            if (_baseStream.CanSeek && _start != 0)
                _baseStream.Position = _start + Position;

            var result = _baseStream.Read(buffer, offset, (int)(count + Position > Length ? Length - Position : count));

            Position += result;
            if (Position > Length)
                Position = Length;
            semaphore.Release();
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
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            semaphore.Wait();
            base.CopyTo(destination, bufferSize);
            semaphore.Release();
        }
    }
}