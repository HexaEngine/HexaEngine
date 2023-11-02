namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.IO;
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Represents a cache handle for managing cached data from a source stream.
    /// </summary>
    public unsafe class CacheHandle : Stream
    {
        private readonly SemaphoreSlim semaphore = new(1);
        private readonly CacheMode mode;
        private long lastAccess;

        private readonly Cache cache;
        private Cache.MemoryHandle buffer;
        private int size;
        private readonly VirtualStream stream;

        private long position;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheHandle"/> class.
        /// </summary>
        /// <param name="cache">The cache associated with this handle.</param>
        /// <param name="name">The name of the cache handle.</param>
        /// <param name="stream">The source stream from which data is cached.</param>
        /// <param name="mode">The caching mode to use.</param>
        public CacheHandle(Cache cache, string name, VirtualStream stream, CacheMode mode)
        {
            Name = name;
            this.cache = cache;
            this.stream = stream;

            if (cache.Size < stream.Length)
            {
                mode = CacheMode.ReadThough;
            }

            if (mode == CacheMode.PreLoad && cache.Size > stream.Length)
            {
                buffer = cache.AllocBuffer(stream.Length);
                stream.Position = 0;
                stream.Read(buffer.Span);
            }

            this.mode = mode;
        }

        /// <summary>
        /// Gets the name of the cache handle.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the timestamp of the last access to this cache handle.
        /// </summary>
        public long LastAccess => lastAccess;

        /// <summary>
        /// Gets the size of the cached data.
        /// </summary>
        public long Size => size;

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => stream.Length;

        /// <inheritdoc/>
        public override long Position { get => position; set => position = value; }

        /// <summary>
        /// Locks access to the cache handle.
        /// </summary>
        public void Lock()
        {
            semaphore.Wait();
        }

        /// <summary>
        /// Unlocks access to the cache handle.
        /// </summary>
        public void Unlock()
        {
            semaphore.Release();
        }

        /// <summary>
        /// Begins a lock block to access the cache handle.
        /// </summary>
        /// <returns>A <see cref="LockBlock"/> that must be disposed to release the lock.</returns>
        public IDisposable BeginLock()
        {
            semaphore.Wait();
            return new LockBlock(semaphore);
        }

        private readonly struct LockBlock : IDisposable
        {
            private readonly SemaphoreSlim semaphore;

            public LockBlock(SemaphoreSlim semaphore)
            {
                this.semaphore = semaphore;
            }

            public readonly void Dispose()
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            if (mode == CacheMode.ReadThough)
            {
                return;
            }

            if (mode == CacheMode.OnDemand && buffer.IsNull)
            {
                buffer = cache.AllocBuffer(stream.Length);
                size = (int)stream.Length;
                stream.Position = 0;
                stream.Read(buffer.Span);

                return;
            }

            stream.Position = 0;
            stream.Read(buffer.Span);
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            lastAccess = Stopwatch.GetTimestamp();
            // cache too small do read though
            if (mode == CacheMode.ReadThough)
            {
                stream.Position = position;
                var read = stream.Read(buffer, offset, count);

                return read;
            }

            if (mode == CacheMode.OnDemand && this.buffer.IsNull)
            {
                this.buffer = cache.AllocBuffer(stream.Length);
                size = (int)stream.Length;
                stream.Position = 0;
                stream.Read(this.buffer.Span);
            }

            this.buffer.Span.Slice((int)position, count).CopyTo(buffer.AsSpan(offset));
            position += count;

            return count;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;

                case SeekOrigin.Current:
                    position += offset;
                    break;

                case SeekOrigin.End:
                    position = Length - offset;
                    break;
            }

            return position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            cache.FreeBuffer(buffer);
            size = 0;
            stream.Dispose();
        }
    }
}