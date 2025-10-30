namespace HexaEngine.Web.Caching
{
    using HexaEngine.Core.Security.Cryptography;
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a cache entry.
    /// </summary>
    public unsafe class WebCacheEntry : IDisposable
    {
        private readonly SemaphoreSlim Semaphore = new(1);

        /// <summary>
        /// Gets or sets the key of the cache entry.
        /// </summary>
        public uint Key;

        /// <summary>
        /// Gets or sets the size of the cache entry.
        /// </summary>
        public uint Size;

        /// <summary>
        /// Gets or sets the position on disk of the cache entry.
        /// </summary>
        public long Position;

        /// <summary>
        /// Gets or sets the last access time.
        /// </summary>
        public DateTime LastAccess;

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        public DateTime ExpirationDate;

        /// <summary>
        /// Gets or sets a pointer to the data of the cache entry, might be null.
        /// </summary>
        public byte* Data;

        /// <summary>
        /// Gets or sets the persistence state.
        /// </summary>
        public WebCacheEntryPersistenceState PersistenceState;

        private bool disposedValue;

        /// <summary>
        /// The size in bytes of a <see cref="WebCacheEntry"/>
        /// </summary>
        public const int EntrySize = 32;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebCacheEntry"/> class with the specified key, size, position, and last access time.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="size">The size of the entry, in bytes.</param>
        /// <param name="position">The position of the entry in the cache file.</param>
        public WebCacheEntry(string key, uint size, long position, DateTime expirationDate)
        {
            Key = Crc32.Compute(MemoryMarshal.AsBytes(key.AsSpan()));
            Size = size;
            Position = position;
            LastAccess = DateTime.Now;
            ExpirationDate = expirationDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebCacheEntry"/> class with the specified key, size, position, and last access time.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="size">The size of the entry, in bytes.</param>
        /// <param name="position">The position of the entry in the cache file.</param>
        /// <param name="lastAccess">The last access time of the entry.</param>
        public WebCacheEntry(uint key, uint size, long position, DateTime lastAccess, DateTime expiration)
        {
            Key = key;
            Size = size;
            Position = position;
            LastAccess = lastAccess;
            ExpirationDate = expiration;
        }

        /// <summary>
        /// Acquires a lock on the cache entry.
        /// </summary>
        public void Lock()
        {
            Semaphore.Wait();
        }

        /// <summary>
        /// Releases the lock on the cache entry.
        /// </summary>
        public void ReleaseLock()
        {
            Semaphore.Release();
        }

        /// <summary>
        /// Reads a <see cref="WebCacheEntry"/> from the specified stream using the provided buffer.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="buffer">The buffer to use for reading.</param>
        /// <returns>The read <see cref="WebCacheEntry"/>.</returns>
        public static WebCacheEntry Read(Stream stream, Span<byte> buffer)
        {
            stream.ReadExactly(buffer);
            uint key = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            uint size = BinaryPrimitives.ReadUInt32LittleEndian(buffer[4..]);
            long position = BinaryPrimitives.ReadInt64LittleEndian(buffer[8..]);
            DateTime lastAccess = new(BinaryPrimitives.ReadInt64LittleEndian(buffer[16..]));
            DateTime expirationDate = new(BinaryPrimitives.ReadInt64LittleEndian(buffer[24..]));
            return new(key, size, position, lastAccess, expirationDate);
        }

        /// <summary>
        /// Writes the <see cref="WebCacheEntry"/> to the specified stream using the provided buffer.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="buffer">The buffer to use for writing.</param>
        public void Write(Stream stream, Span<byte> buffer)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, Key);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer[4..], Size);
            BinaryPrimitives.WriteInt64LittleEndian(buffer[8..], Position);
            BinaryPrimitives.WriteInt64LittleEndian(buffer[16..], LastAccess.Ticks);
            BinaryPrimitives.WriteInt64LittleEndian(buffer[24..], ExpirationDate.Ticks);
            stream.Write(buffer);
        }

        /// <summary>
        /// Frees the memory associated with the entry.
        /// </summary>
        public void Free()
        {
            if (Data != null)
            {
                Marshal.FreeHGlobal((nint)Data);
                Data = null;
            }
        }

        /// <summary>
        /// Gets the entry data as a span of bytes.
        /// </summary>
        /// <returns>A span of bytes representing the entry data.</returns>
        public Span<byte> AsSpan()
        {
            return new(Data, (int)Size);
        }

        /// <summary>
        /// Disposes of the resources used by the <see cref="WebCacheEntry"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if called from <see cref="Dispose()"/> method; otherwise, <see langword="false"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Free();
                Semaphore.Dispose();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes of the resources used by the <see cref="WebCacheEntry"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}