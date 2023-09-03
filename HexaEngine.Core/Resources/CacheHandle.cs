namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.IO;
    using System;
    using System.Diagnostics;
    using System.IO;

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

        public string Name { get; }

        public long LastAccess => lastAccess;

        public long Size => size;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => stream.Length;

        public override long Position { get => position; set => position = value; }

        public void Lock()
        {
            semaphore.Wait();
        }

        public void Unlock()
        {
            semaphore.Release();
        }

        public LockBlock BeginLock()
        {
            semaphore.Wait();
            return new(semaphore);
        }

        public readonly struct LockBlock : IDisposable
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

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

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            cache.FreeBuffer(buffer);
            size = 0;
            stream.Dispose();
        }
    }
}