namespace HexaEngine.Core.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// Represents a virtual stream that provides access to a subset of a base stream.
    /// </summary>
    public class VirtualStream : Stream
    {
        private readonly SemaphoreSlim semaphore = new(1);
        private readonly Stream _baseStream;
        private readonly long _start;
        private readonly bool _leaveOpen;
        private long position;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualStream"/> class with the specified base stream, start position, length, and leave-open option.
        /// </summary>
        /// <param name="baseStream">The base stream.</param>
        /// <param name="start">The start position within the base stream.</param>
        /// <param name="length">The length of the virtual stream.</param>
        /// <param name="leaveOpen">A value indicating whether the base stream should be left open after disposing the virtual stream.</param>
        public VirtualStream(Stream baseStream, long start, long length, bool leaveOpen = false)
        {
            _baseStream = baseStream;
            _start = start;
            Length = length;
            _leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Gets a value indicating whether the stream supports reading.
        /// </summary>
        public override bool CanRead => _baseStream.CanRead;

        /// <summary>
        /// Gets a value indicating whether the stream supports seeking.
        /// </summary>
        public override bool CanSeek => _baseStream.CanSeek;

        /// <summary>
        /// Gets a value indicating whether the stream supports writing.
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// Gets the length of the virtual stream.
        /// </summary>
        public override long Length { get; }

        /// <summary>
        /// Gets or sets the position within the virtual stream.
        /// </summary>
        public override long Position { get => position; set => position = value; }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="VirtualStream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_leaveOpen)
            {
                _baseStream.Dispose();
            }
            semaphore.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Flushes the stream.
        /// </summary>
        public override void Flush()
        {
            semaphore.Wait();
            _baseStream.Flush();
            semaphore.Release();
        }

        /// <summary>
        /// Reads the entire virtual stream and returns it as a byte array.
        /// </summary>
        /// <returns>A byte array containing the contents of the virtual stream.</returns>
        public byte[] ReadBytes()
        {
            semaphore.Wait();
            var buffer = new byte[Length];
            if (_baseStream.CanSeek && _start != 0)
            {
                _baseStream.Position = _start;
            }

            _ = _baseStream.Read(buffer);
            semaphore.Release();
            return buffer;
        }

        /// <summary>
        /// Reads a sequence of bytes from the virtual stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">The buffer to read the data into.</param>
        /// <param name="offset">The offset in the buffer at which to start writing the read data.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            semaphore.Wait();
            if (Position == Length)
            {
                semaphore.Release();
                return 0;
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException(null, nameof(buffer));
            }

            if (_baseStream.CanSeek)
            {
                _baseStream.Position = _start + Position;
            }

            var result = _baseStream.Read(buffer, offset, (int)(count + Position > Length ? Length - Position : count));

            Position += result;
            if (Position > Length)
            {
                Position = Length;
            }

            semaphore.Release();
            return result;
        }

        /// <summary>
        /// Sets the position within the virtual stream.
        /// </summary>
        /// <param name="offset">The offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">The reference point used to obtain the new position.</param>
        /// <returns>The new position within the virtual stream.</returns>
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

        /// <summary>
        /// Sets the length of the virtual stream. This operation is not supported.
        /// </summary>
        /// <param name="value">The desired length of the virtual stream.</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes a sequence of bytes to the virtual stream. This operation is not supported.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to write.</param>
        /// <param name="offset">The offset in the buffer at which to start reading the data.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies the content of the virtual stream to another stream.
        /// </summary>
        /// <param name="destination">The stream to copy the content to.</param>
        /// <param name="bufferSize">The size of the buffer used for copying.</param>
        public override void CopyTo(Stream destination, int bufferSize)
        {
            semaphore.Wait();
            base.CopyTo(destination, bufferSize);
            semaphore.Release();
        }

        /// <summary>
        /// Locks the stream.
        /// </summary>
        /// <param name="millisecondsTimeout">The maximum time to wait for the lock. Use -1 to wait indefinitely.</param>
        /// <returns><c>true</c> if the lock is acquired; otherwise, <c>false</c>.</returns>
        public bool Lock(int millisecondsTimeout = -1)
        {
            return semaphore.Wait(millisecondsTimeout);
        }

        /// <summary>
        /// Unlocks the stream.
        /// </summary>
        public void Unlock()
        {
            semaphore.Release();
        }

        /// <summary>
        /// Begins a lock on the stream.
        /// </summary>
        /// <returns>A <see cref="LockBlock"/> representing the locked state.</returns>
        public LockBlock BeginLock()
        {
            semaphore.Wait();
            return new LockBlock(semaphore);
        }

        /// <summary>
        /// Tries to begin a lock on the stream.
        /// </summary>
        /// <param name="block">When this method returns, contains a <see cref="LockBlock"/> if the lock was acquired; otherwise, the default value.</param>
        /// <param name="millisecondsTimeout">The maximum time to wait for the lock. Use -1 to wait indefinitely.</param>
        /// <returns><c>true</c> if the lock is acquired; otherwise, <c>false</c>.</returns>
        public bool TryBeginLock(out LockBlock block, int millisecondsTimeout = -1)
        {
            if (Lock(millisecondsTimeout))
            {
                block = new LockBlock(semaphore);
                return true;
            }
            block = default;
            return false;
        }

        /// <summary>
        /// Represents a block of locked state for a <see cref="VirtualStream"/>.
        /// </summary>
        public readonly struct LockBlock : IDisposable
        {
            private readonly SemaphoreSlim semaphore;

            /// <summary>
            /// Initializes a new instance of the <see cref="LockBlock"/> struct with the specified semaphore.
            /// </summary>
            /// <param name="semaphore">The semaphore representing the locked state.</param>
            public LockBlock(SemaphoreSlim semaphore)
            {
                this.semaphore = semaphore;
            }

            /// <summary>
            /// Releases the lock.
            /// </summary>
            public void Dispose()
            {
                semaphore.Release();
            }
        }
    }
}