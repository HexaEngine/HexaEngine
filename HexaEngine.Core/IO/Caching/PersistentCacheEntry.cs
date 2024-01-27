namespace HexaEngine.Core.IO.Caching
{
    using HexaEngine.Core;
    using HexaEngine.Core.Security.Cryptography;
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a cache entry.
    /// </summary>
    public unsafe class PersistentCacheEntry : IDisposable
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
        /// Gets or sets a pointer to the data of the cache entry, might be null.
        /// </summary>
        public byte* Data;

        /// <summary>
        /// Gets or sets the persistence state.
        /// </summary>
        public CacheEntryPersistenceState PersistenceState;

        private bool disposedValue;

        /// <summary>
        /// The size in bytes of a <see cref="PersistentCacheEntry"/>
        /// </summary>
        public const int EntrySize = 24;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentCacheEntry"/> class with the specified key, size, position, and last access time.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="size">The size of the entry, in bytes.</param>
        /// <param name="position">The position of the entry in the cache file.</param>
        public PersistentCacheEntry(string key, uint size, long position)
        {
            Key = Crc32.Compute(MemoryMarshal.AsBytes(key.AsSpan()));
            Size = size;
            Position = position;
            LastAccess = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentCacheEntry"/> class with the specified key, size, position, and last access time.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="size">The size of the entry, in bytes.</param>
        /// <param name="position">The position of the entry in the cache file.</param>
        /// <param name="lastAccess">The last access time of the entry.</param>
        public PersistentCacheEntry(uint key, uint size, long position, DateTime lastAccess)
        {
            Key = key;
            Size = size;
            Position = position;
            LastAccess = lastAccess;
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
        /// Reads a <see cref="PersistentCacheEntry"/> from the specified stream using the provided buffer.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="buffer">The buffer to use for reading.</param>
        /// <returns>The read <see cref="PersistentCacheEntry"/>.</returns>
        public static PersistentCacheEntry Read(Stream stream, Span<byte> buffer)
        {
            stream.Read(buffer);
            uint key = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            uint size = BinaryPrimitives.ReadUInt32LittleEndian(buffer[4..]);
            long position = BinaryPrimitives.ReadInt64LittleEndian(buffer[8..]);
            DateTime lastAccess = new(BinaryPrimitives.ReadInt64LittleEndian(buffer[16..]));
            return new(key, size, position, lastAccess);
        }

        /// <summary>
        /// Writes the <see cref="PersistentCacheEntry"/> to the specified stream using the provided buffer.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="buffer">The buffer to use for writing.</param>
        public void Write(Stream stream, Span<byte> buffer)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, Key);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer[4..], Size);
            BinaryPrimitives.WriteInt64LittleEndian(buffer[8..], Position);
            BinaryPrimitives.WriteInt64LittleEndian(buffer[16..], LastAccess.Ticks);
            stream.Write(buffer);
        }

        /// <summary>
        /// Frees the memory associated with the entry.
        /// </summary>
        public void Free()
        {
            if (Data != null)
            {
                Utils.Free(Data);
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
        /// Disposes of the resources used by the <see cref="PersistentCacheEntry"/>.
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
        /// Disposes of the resources used by the <see cref="PersistentCacheEntry"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}