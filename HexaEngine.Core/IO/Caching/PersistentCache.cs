namespace HexaEngine.Core.IO.Caching
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Security.Cryptography;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// A persistent memory cache with lazy disk writing.
    /// </summary>
    public class PersistentCache : IDisposable
    {
        private readonly string cacheFile;
        private readonly string indexFile;

        private readonly List<PersistentCacheEntry> entries = new();
        private readonly SemaphoreSlim writeSemaphore = new(1);
        private readonly ManualResetEventSlim writeLock = new(true);
        private readonly SemaphoreSlim readSemaphore = new(MaxConcurrentReader);

        private readonly SemaphoreSlim memorySemaphore = new(1);
        private readonly SemaphoreSlim indexFileSemaphore = new(1);
        private readonly SemaphoreSlim cacheFileSemaphore = new(1);

        private readonly FileStream cacheStream;
        private long writtenBytes;
        private long memoryBytes;
        private bool disposedValue;
        private int maxMemorySize = 536870912;
        private ICachePolicy cachePolicy = new LRUCachePolicy();

        /// <summary>
        /// Creates a new instance of <see cref="PersistentCache"/>.
        /// </summary>
        /// <param name="folder">The folder where to save the cache data.</param>
        public PersistentCache(string folder) : this(Path.Combine(folder, "cache.bin"), Path.Combine(folder, "cache.index"))
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="PersistentCache"/>.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file.</param>
        /// <param name="indexFile">The path to the index file.</param>
        public PersistentCache(string cacheFile, string indexFile)
        {
            this.cacheFile = cacheFile;
            this.indexFile = indexFile;
            var cacheFileDir = Path.GetDirectoryName(cacheFile);
            var indexFileDir = Path.GetDirectoryName(indexFile);

            if (cacheFileDir != null)
            {
                Directory.CreateDirectory(cacheFileDir);
            }

            if (indexFileDir != null)
            {
                Directory.CreateDirectory(indexFileDir);
            }

            ReadIndexFile();
            if (!File.Exists(this.cacheFile))
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    entries[i].Dispose();
                }
                entries.Clear();
                cacheStream = File.Open(cacheFile, FileMode.Create);
            }
            else
            {
                cacheStream = File.Open(cacheFile, FileMode.Open);
            }
        }

        private const int Version = 1;

        /// <summary>
        /// Maximum concurrent readers/threads.
        /// </summary>
        public const int MaxConcurrentReader = 64;

        /// <summary>
        /// Gets or sets maximum memory cache size in bytes. Default value is 512MB (536870912 Bytes).
        /// </summary>
        public int MaxMemorySize { get => maxMemorySize; set => maxMemorySize = value; }

        /// <summary>
        /// Gets or sets the cache policy. Default policy is LRU (Least recent used)
        /// </summary>
        public ICachePolicy CachePolicy { get => cachePolicy; set => cachePolicy = value; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BeginWrite()
        {
            writeSemaphore.Wait();

            writeLock.Reset();

            while (readSemaphore.CurrentCount != MaxConcurrentReader)
            {
                Thread.Yield();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndWrite()
        {
            writeLock.Set();
            writeSemaphore.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BeginRead()
        {
            writeLock.Wait();
            readSemaphore.Wait();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndRead()
        {
            readSemaphore.Release();
        }

        private bool TryGetEntry(string key, [NotNullWhen(true)] out PersistentCacheEntry? entry)
        {
            var hash = Crc32.Compute(MemoryMarshal.AsBytes(key.AsSpan()));
            for (int i = 0; i < entries.Count; i++)
            {
                var sEntry = entries[i];
                if (sEntry.Key == hash)
                {
                    entry = sEntry;

                    return true;
                }
            }
            entry = null;

            return false;
        }

        private bool TryGetEntry(uint key, [NotNullWhen(true)] out PersistentCacheEntry? entry)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var sEntry = entries[i];
                if (sEntry.Key == key)
                {
                    entry = sEntry;

                    return true;
                }
            }
            entry = null;

            return false;
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            BeginWrite();
            Interlocked.Exchange(ref writtenBytes, 0);
            Interlocked.Exchange(ref memoryBytes, 0);

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Free();
            }
            entries.Clear();

            cacheStream.Position = 0;
            cacheStream.SetLength(0);

            EndWrite();
        }

        /// <summary>
        /// Flushes the cache this will clear the memory cache and executes all pending IO operations.
        /// </summary>
        public void Flush()
        {
            BeginRead();

            memorySemaphore.Wait();

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                entry.Lock();
                if (entry.PersistenceState.IsDirty)
                {
                    WriteBackToDisk(entry);
                }
                entry.Free();
                entry.ReleaseLock();
            }

            Interlocked.Exchange(ref memoryBytes, 0);

            WriteIndexFile();
            cacheStream.Flush();

            memorySemaphore.Release();

            EndRead();
        }

        /// <summary>
        /// Determines whether the cache contains an entry with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns><see langword="true"/> if the cache contains an entry with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool ContainsKey(string key)
        {
            int hash = key.GetHashCode();

            BeginRead();

            bool result = false;
            for (int i = 0; i < entries.Count; i++)
            {
                var sEntry = entries[i];
                if (sEntry.Key == hash)
                {
                    result = true;
                    break;
                }
            }

            EndRead();

            return result;
        }

        /// <summary>
        /// Sets the data associated with the specified key in the cache.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="data">The byte array representing the data to be stored.</param>
        public unsafe void Set(string key, byte[] data)
        {
            fixed (byte* ptr = data)
            {
                Set(key, ptr, (uint)data.Length);
            }
        }

        /// <summary>
        /// Sets the data associated with the specified key in the cache.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="data">A pointer to the memory location where the data is stored.</param>
        /// <param name="size">The size of the data, in bytes.</param>
        public unsafe void Set(string key, byte* data, uint size)
        {
            uint hash = Crc32.Compute(MemoryMarshal.AsBytes(key.AsSpan()));

            BeginWrite();

            if (TryGetEntry(hash, out PersistentCacheEntry? entry))
            {
                entry.Lock();

                uint oldSize = entry.Size;

                // preserve old size even if we do two overwrites in a row to preserve the disk space.
                if (!entry.PersistenceState.IsDirty)
                {
                    entry.PersistenceState.OldSize = oldSize;
                    entry.PersistenceState.IsDirty = true;
                }

                entry.Size = size;
                AllocateSpace(entry, oldSize);

                MemcpyT(data, entry.Data, size);

                entry.LastAccess = DateTime.Now;
            }
            else
            {
                entry = new(key, size, -1);
                entry.Lock();
                entry.PersistenceState.IsDirty = true;
                entry.PersistenceState.OldSize = unchecked((uint)-1);

                AllocateSpace(entry, null);
                MemcpyT(data, entry.Data, size);
                entry.LastAccess = DateTime.Now;

                entries.Add(entry);
            }

            entry.ReleaseLock();

            EndWrite();
        }

        /// <summary>
        /// Retrieves the data associated with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="data">A pointer to the memory location where the retrieved data will be stored.</param>
        /// <param name="size">A pointer to store the size of the retrieved data, in bytes.</param>
        /// <exception cref="KeyNotFoundException"></exception>
        public unsafe void Get(string key, byte** data, uint* size)
        {
            uint hash = Crc32.Compute(MemoryMarshal.AsBytes(key.AsSpan()));

            BeginRead();

            if (!TryGetEntry(hash, out var entry))
            {
                throw new KeyNotFoundException($"Key \"{key}\" ({hash}), was not found!");
            }

            entry.Lock();

            if (entry.Data == null)
            {
                AllocateSpace(entry, null);
                ReadFromDisk(entry);
            }

            *data = AllocCopyT(entry.Data, entry.Size);
            *size = entry.Size;

            entry.LastAccess = DateTime.Now;

            entry.ReleaseLock();

            EndRead();
        }

        /// <summary>
        /// Tries to retrieve the data associated with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="data">A pointer to the memory location where the retrieved data will be stored.</param>
        /// <param name="size">A pointer to store the size of the retrieved data, in bytes.</param>
        /// <returns><see langword="true"/> if the data was successfully retrieved; otherwise, <see langword="false"/>.</returns>
        public unsafe bool TryGet(string key, byte** data, uint* size)
        {
            uint hash = Crc32.Compute(MemoryMarshal.AsBytes(key.AsSpan()));

            BeginRead();

            bool result = false;

            if (TryGetEntry(hash, out var entry))
            {
                entry.Lock();

                if (entry.Data == null)
                {
                    AllocateSpace(entry, null);
                    ReadFromDisk(entry);
                }

                *data = AllocCopyT(entry.Data, entry.Size);
                *size = entry.Size;

                entry.LastAccess = DateTime.Now;

                entry.ReleaseLock();

                result = true;
            }

            EndRead();

            return result;
        }

        /// <summary>
        /// Allocates new memory cache space.
        /// </summary>
        /// <remarks>Call <see cref="PersistentCacheEntry.Lock"/> before calling this method.</remarks>
        /// <param name="entry">The cache entry.</param>
        /// <param name="oldSize">Optional the old size of the entry.</param>
        private unsafe bool AllocateSpace(PersistentCacheEntry entry, uint? oldSize)
        {
            if (oldSize != null && entry.Data != null)
            {
                Interlocked.Add(ref memoryBytes, -(long)oldSize);
                entry.Free();
            }

            var newSize = Interlocked.Add(ref memoryBytes, entry.Size);
            if (newSize > maxMemorySize)
            {
                memorySemaphore.Wait();

                newSize = Interlocked.Read(ref memoryBytes);

                if (newSize > maxMemorySize)
                {
                    var overhead = newSize - maxMemorySize;

                    while (overhead > 0)
                    {
                        var itemToRemove = cachePolicy.GetItemToRemove(entries, entry);

                        if (itemToRemove == null)
                        {
                            break;
                        }

                        itemToRemove.Lock();

                        if (itemToRemove.PersistenceState.IsDirty)
                        {
                            WriteBackToDisk(itemToRemove);
                        }

                        itemToRemove.Free();
                        overhead -= itemToRemove.Size;
                        Interlocked.Add(ref memoryBytes, -itemToRemove.Size);
                        itemToRemove.ReleaseLock();
                    }
                }
                memorySemaphore.Release();
            }

            entry.Data = AllocT<byte>(entry.Size);

            return true;
        }

        private const int HeaderSize = 8;

        /// <summary>
        /// Saves the cache to disk without clearing the memory cache.
        /// </summary>
        public void Save()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                entry.Lock();
                if (entry.PersistenceState.IsDirty)
                {
                    WriteBackToDisk(entry);
                }
                entry.ReleaseLock();
            }
            WriteIndexFile();
            cacheStream.Flush();
        }

        /// <summary>
        /// Asynchronously saves the cache to disk without clearing the memory cache.
        /// </summary>
        /// <returns>The Task for the async save operation.</returns>
        public async Task SaveAsync()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                entry.Lock();
                if (entry.PersistenceState.IsDirty)
                {
                    WriteBackToDisk(entry);
                }
                entry.ReleaseLock();
            }
            await WriteIndexFileAsync();
            await cacheStream.FlushAsync();
        }

        private void ReadIndexFile()
        {
            if (!File.Exists(indexFile))
            {
                return;
            }

            var fs = File.OpenRead(indexFile);

            BeginWrite();

            if (fs.Length < HeaderSize)
            {
                EndWrite();
                fs.Close();
                return;
            }
            var version = fs.ReadInt32(Mathematics.Endianness.LittleEndian);
            if (version != Version)
            {
                EndWrite();
                fs.Close();
                return;
            }

            writtenBytes = fs.ReadInt64(Mathematics.Endianness.LittleEndian);

            var count = fs.ReadInt32(Mathematics.Endianness.LittleEndian);
            entries.EnsureCapacity(count);

            Span<byte> buffer = stackalloc byte[PersistentCacheEntry.EntrySize];
            for (int i = 0; i < count; i++)
            {
                entries.Add(PersistentCacheEntry.Read(fs, buffer));
            }

            EndWrite();

            fs.Close();
        }

        private Task WriteIndexFileAsync()
        {
            return Task.Run(WriteIndexFile);
        }

        private void WriteIndexFile()
        {
            indexFileSemaphore.Wait();
            BeginRead();

            var fs = File.Create(indexFile);

            fs.WriteInt32(Version, Mathematics.Endianness.LittleEndian);
            fs.WriteInt64(writtenBytes, Mathematics.Endianness.LittleEndian);
            fs.WriteInt32(entries.Count, Mathematics.Endianness.LittleEndian);

            Span<byte> buffer = stackalloc byte[PersistentCacheEntry.EntrySize];
            for (var i = 0; i < entries.Count; i++)
            {
                entries[i].Write(fs, buffer);
            }

            EndRead();

            fs.Close();
            indexFileSemaphore.Release();
        }

        private void ReadFromDisk(PersistentCacheEntry entry)
        {
            cacheFileSemaphore.Wait();

            cacheStream.Position = entry.Position;
            var span = entry.AsSpan();
            cacheStream.Read(span);

            cacheFileSemaphore.Release();
        }

        private void WriteBackToDisk(PersistentCacheEntry entry)
        {
            var state = entry.PersistenceState;
            cacheFileSemaphore.Wait();

            if (entry.Position == -1)
            {
                entry.Position = cacheStream.Position = cacheStream.Length;
                var span = entry.AsSpan();
                cacheStream.Write(span);

                writtenBytes += entry.Size;
            }
            else
            {
                if (state.OldSize == entry.Size)
                {
                    cacheStream.Position = entry.Position;
                    var span = entry.AsSpan();
                    cacheStream.Write(span);
                }
                else
                {
                    // prevent fragmentation.
                    var startPos = entry.Position;
                    var endPos = entry.Position + state.OldSize;
                    var size = writtenBytes - endPos;

                    // lock affected entries before moving.
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var sEntry = entries[i];
                        if (sEntry.Position > startPos)
                        {
                            sEntry.Lock();
                        }
                    }

                    MoveBlock(endPos, startPos, size);

                    // offset position and release lock again.
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var sEntry = entries[i];
                        if (sEntry.Position > startPos)
                        {
                            sEntry.Position -= state.OldSize;
                            sEntry.ReleaseLock();
                        }
                    }

                    writtenBytes -= state.OldSize;

                    entry.Position = cacheStream.Position = writtenBytes;
                    var span = entry.AsSpan();
                    cacheStream.Write(span);

                    writtenBytes += entry.Size;
                }
            }

            cacheFileSemaphore.Release();
            entry.PersistenceState = default;
        }

        private void MoveBlock(long from, long to, long size)
        {
            const int BufferSize = 8192;
            Span<byte> buffer = stackalloc byte[BufferSize];

            long positionFrom = from;
            long positionTo = to;
            while (size > 0)
            {
                int bytesToRead = (int)Math.Min(size, BufferSize);

                cacheStream.Position = positionFrom;
                int read = cacheStream.Read(buffer[..bytesToRead]);
                positionFrom += read;

                cacheStream.Position = to;
                cacheStream.Write(buffer[..read]);
                positionTo += read;

                size -= read;
            }
        }

        /// <summary>
        /// Disposes of the resources used by the <see cref="PersistentCache"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if called from <see cref="Dispose()"/> method; otherwise, <see langword="false"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Save();
                Flush();
                cacheStream.Dispose();
                for (int i = 0; i < entries.Count; i++)
                {
                    entries[i].Dispose();
                }
                entries.Clear();

                writeSemaphore.Dispose();
                writeLock.Dispose();
                readSemaphore.Dispose();
                memorySemaphore.Dispose();
                indexFileSemaphore.Dispose();
                cacheFileSemaphore.Dispose();

                disposedValue = true;
            }
        }

        ~PersistentCache()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Disposes of the resources used by the <see cref="PersistentCache"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}