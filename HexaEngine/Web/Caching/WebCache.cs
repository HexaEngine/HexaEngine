namespace HexaEngine.Web.Caching
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Security.Cryptography;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// A persistent memory cache with lazy disk writing.
    /// </summary>
    public class WebCache : IDisposable
    {
        private readonly string cacheFile;
        private readonly string indexFile;

        private readonly List<WebCacheEntry> entries = [];
        private readonly SemaphoreSlim writeSemaphore = new(1);
        private readonly ManualResetEventSlim writeLock = new(true);
        private readonly SemaphoreSlim readSemaphore = new(MaxConcurrentReader);

        private readonly SemaphoreSlim memorySemaphore = new(1);
        private readonly SemaphoreSlim indexFileSemaphore = new(1);
        private readonly SemaphoreSlim cacheFileSemaphore = new(1);
        private readonly SemaphoreSlim cleanupSemaphore = new(1);

        private readonly FileStream cacheStream;
        private long diskBytes;
        private long memoryBytes;
        private bool disposedValue;
        private int maxMemorySize = 536870912;
        private int maxDiskSize = 1073741824;
        private ICachePolicy cachePolicy = new LRUCachePolicy();

        private long nextExpirationDate;

        public static readonly WebCache Shared = new("./cache/webcache.cache", "./cache/webcache.index");

        static WebCache()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainProcessExit;
        }

        private static void CurrentDomainProcessExit(object? sender, EventArgs e)
        {
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomainProcessExit;
            Shared.Save();
        }

        /// <summary>
        /// Creates a new instance of <see cref="WebCache"/>.
        /// </summary>
        /// <param name="folder">The folder where to save the cache data.</param>
        public WebCache(string folder) : this(Path.Combine(folder, "cache.bin"), Path.Combine(folder, "cache.index"))
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="WebCache"/>.
        /// </summary>
        /// <param name="cacheFile">The path to the cache file.</param>
        /// <param name="indexFile">The path to the index file.</param>
        public WebCache(string cacheFile, string indexFile)
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

            CheckForExpiredEntries();
        }

        private const int Version = 2;

        /// <summary>
        /// Maximum concurrent readers/threads.
        /// </summary>
        public const int MaxConcurrentReader = 64;

        /// <summary>
        /// Gets or sets maximum memory cache size in bytes. Default value is 512MB (536870912 Bytes).
        /// </summary>
        public int MaxMemorySize { get => maxMemorySize; set => maxMemorySize = value; }

        /// <summary>
        /// Gets or sets maximum disk cache size in bytes. Default value is 1GB (1073741824 Bytes).
        /// </summary>
        public int MaxDiskSize { get => maxDiskSize; set => maxDiskSize = value; }

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

        private unsafe void CheckForExpiredEntries()
        {
            var nextExpirationDate = Interlocked.Read(ref this.nextExpirationDate);
            var now = DateTime.UtcNow.Ticks;
            if (now >= nextExpirationDate)
            {
                bool acquired = cleanupSemaphore.Wait(0);

                if (acquired)
                {
                    Sweep(now);

                    RecalculateNextExpirationDate();
                }

                cleanupSemaphore.Release();
            }
        }

        /// <summary>
        /// Call only after acquiring <see cref="cleanupSemaphore"/>.
        /// </summary>
        /// <param name="now">The current time in ticks.</param>
        private unsafe void Sweep(long now)
        {
            BeginWrite();

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                entry.Lock();
                if (now >= entry.ExpirationDate.Ticks)
                {
                    if (entry.Data != null)
                    {
                        entry.Free();
                        Interlocked.Add(ref memoryBytes, -entry.Size);
                    }

                    RemoveFromDiskInternal(entry);

                    entries.RemoveAt(i);
                    i--;

                    entry.ReleaseLock();
                    entry.Dispose();
                }
                else
                {
                    entry.ReleaseLock();
                }
            }

            EndWrite();
        }

        /// <summary>
        /// Call only after acquiring <see cref="cleanupSemaphore"/>.
        /// </summary>
        private unsafe void RecalculateNextExpirationDate()
        {
            BeginRead();

            long min = long.MaxValue;
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                entry.Lock();
                min = Math.Min(min, entry.ExpirationDate.Ticks);
                entry.ReleaseLock();
            }

            Interlocked.Exchange(ref nextExpirationDate, min);

            EndRead();
        }

        private bool TryGetEntry(string key, [NotNullWhen(true)] out WebCacheEntry? entry)
        {
            var hash = Crc32.Compute(MemoryMarshal.AsBytes(key.AsSpan()));
            for (int i = 0; i < entries.Count; i++)
            {
                var sEntry = entries[i];
                if (sEntry.Key == hash && !sEntry.PersistenceState.IsGhost)
                {
                    entry = sEntry;

                    return true;
                }
            }
            entry = null;

            return false;
        }

        private bool TryGetEntry(uint key, [NotNullWhen(true)] out WebCacheEntry? entry, bool allowGhosts = false)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var sEntry = entries[i];
                if (sEntry.Key == key && (allowGhosts || !sEntry.PersistenceState.IsGhost))
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
            Interlocked.Exchange(ref diskBytes, 0);
            Interlocked.Exchange(ref memoryBytes, 0);
            Interlocked.Exchange(ref nextExpirationDate, long.MaxValue);

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Dispose();
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
            CheckForExpiredEntries();

            BeginRead();

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.PersistenceState.IsGhost)
                {
                    continue;
                }
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
            // trim down.
            cacheStream.SetLength(diskBytes);
            cacheStream.Flush();

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

            CheckForExpiredEntries();

            BeginRead();

            bool result = false;
            for (int i = 0; i < entries.Count; i++)
            {
                var sEntry = entries[i];
                if (sEntry.Key == hash && !sEntry.PersistenceState.IsGhost)
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
        /// <param name="expirationDate"></param>
        public unsafe void Set(string key, byte[] data, DateTime expirationDate)
        {
            fixed (byte* ptr = data)
            {
                Set(key, ptr, (uint)data.Length, expirationDate);
            }
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
                Set(key, ptr, (uint)data.Length, DateTime.MaxValue);
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
            Set(key, data, size, DateTime.MaxValue);
        }

        /// <summary>
        /// Sets the data associated with the specified key in the cache.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="data">A pointer to the memory location where the data is stored.</param>
        /// <param name="size">The size of the data, in bytes.</param>
        public unsafe void Set(string key, byte* data, uint size, DateTime expirationDate)
        {
            CheckForExpiredEntries();

            uint hash = Crc32.Compute(MemoryMarshal.AsBytes(key.AsSpan()));

            BeginWrite();

            if (TryGetEntry(hash, out WebCacheEntry? entry, true))
            {
                entry.Lock();

                uint oldSize = entry.Size;

                // preserve old size even if we do two overwrites in a row to preserve the disk space.
                if (!entry.PersistenceState.IsDirty)
                {
                    entry.PersistenceState.OldSize = oldSize;
                    entry.PersistenceState.IsDirty = true;
                }

                // recycle ghost entries.
                entry.PersistenceState.IsGhost = false;

                entry.Size = size;
                AllocateSpace(entry, oldSize);

                Memcpy(data, entry.Data, size);

                entry.LastAccess = DateTime.Now;
                entry.ExpirationDate = expirationDate;
            }
            else
            {
                entry = new(key, size, -1, expirationDate);
                entry.Lock();
                entry.PersistenceState.IsDirty = true;
                entry.PersistenceState.OldSize = unchecked((uint)-1);

                AllocateSpace(entry, null);
                Memcpy(data, entry.Data, size);
                entry.LastAccess = DateTime.Now;

                entries.Add(entry);
            }

            entry.ReleaseLock();

            EndWrite();
        }

        private static unsafe byte* Alloc(uint size)
        {
            return (byte*)Marshal.AllocHGlobal((nint)size);
        }

        private static unsafe void Memcpy(byte* src, byte* dst, uint size)
        {
            Buffer.MemoryCopy(src, dst, size, size);
        }

        private static unsafe byte* AllocCopy(byte* data, uint size)
        {
            var newPtr = Alloc(size);
            Memcpy(data, newPtr, size);
            return newPtr;
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
            CheckForExpiredEntries();

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

            *data = AllocCopy(entry.Data, entry.Size);
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
            CheckForExpiredEntries();

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

                *data = AllocCopy(entry.Data, entry.Size);
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
        /// <remarks>Call <see cref="WebCacheEntry.Lock"/> before calling this method.</remarks>
        /// <param name="entry">The cache entry.</param>
        /// <param name="oldSize">Optional the old size of the entry.</param>
        private unsafe bool AllocateSpace(WebCacheEntry entry, uint? oldSize)
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

            entry.Data = Alloc(entry.Size);

            return true;
        }

        private unsafe void AllocateDiskSpace(WebCacheEntry entry, uint? oldSize)
        {
            if (oldSize != null && entry.Data != null)
            {
                Interlocked.Add(ref diskBytes, -(long)oldSize);
            }

            var newSize = Interlocked.Add(ref diskBytes, entry.Size);
            entry.Position = newSize - entry.Size;
            if (newSize > maxDiskSize)
            {
                memorySemaphore.Wait();

                newSize = Interlocked.Read(ref diskBytes);

                if (newSize > maxDiskSize)
                {
                    var overhead = newSize - maxDiskSize;

                    while (overhead > 0)
                    {
                        var itemToRemove = cachePolicy.GetItemToRemoveDisk(entries, entry);

                        if (itemToRemove == null)
                        {
                            break;
                        }

                        itemToRemove.Lock();

                        RemoveFromDiskInternal(itemToRemove);
                        if (itemToRemove.Data != null)
                        {
                            Interlocked.Add(ref memoryBytes, -itemToRemove.Size);
                        }

                        itemToRemove.PersistenceState.IsGhost = true;
                        itemToRemove.ReleaseLock();
                        itemToRemove.Dispose();

                        overhead -= itemToRemove.Size;
                        Interlocked.Add(ref diskBytes, -itemToRemove.Size);
                    }
                }

                memorySemaphore.Release();
            }
        }

        private const int HeaderSize = 16;

        /// <summary>
        /// Saves the cache to disk without clearing the memory cache.
        /// </summary>
        public void Save()
        {
            CheckForExpiredEntries();

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.PersistenceState.IsGhost)
                {
                    continue;
                }

                entry.Lock();
                if (entry.PersistenceState.IsDirty)
                {
                    WriteBackToDisk(entry);
                }
                entry.ReleaseLock();
            }
            WriteIndexFile();

            // trim down.
            cacheStream.SetLength(diskBytes);
            cacheStream.Flush();
        }

        /// <summary>
        /// Asynchronously saves the cache to disk without clearing the memory cache.
        /// </summary>
        /// <returns>The Task for the async save operation.</returns>
        public async Task SaveAsync()
        {
            CheckForExpiredEntries();

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.PersistenceState.IsGhost)
                {
                    continue;
                }

                entry.Lock();
                if (entry.PersistenceState.IsDirty)
                {
                    WriteBackToDisk(entry);
                }
                entry.ReleaseLock();
            }
            await WriteIndexFileAsync();

            cacheStream.SetLength(diskBytes);
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

            Span<byte> headerBuffer = stackalloc byte[HeaderSize];
            fs.Read(headerBuffer);

            var version = BinaryPrimitives.ReadInt32LittleEndian(headerBuffer);
            if (version != Version)
            {
                EndWrite();
                fs.Close();
                return;
            }

            diskBytes = BinaryPrimitives.ReadInt64LittleEndian(headerBuffer[4..]);

            var count = BinaryPrimitives.ReadInt32LittleEndian(headerBuffer[12..]);
            entries.EnsureCapacity(count);

            Span<byte> buffer = stackalloc byte[WebCacheEntry.EntrySize];
            for (int i = 0; i < count; i++)
            {
                entries.Add(WebCacheEntry.Read(fs, buffer));
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

            Span<byte> headerBuffer = stackalloc byte[HeaderSize];
            BinaryPrimitives.WriteInt32LittleEndian(headerBuffer, Version);
            BinaryPrimitives.WriteInt64LittleEndian(headerBuffer[4..], diskBytes);
            BinaryPrimitives.WriteInt32LittleEndian(headerBuffer[12..], entries.Count);
            fs.Write(headerBuffer);

            Span<byte> buffer = stackalloc byte[WebCacheEntry.EntrySize];
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.PersistenceState.IsGhost)
                {
                    continue;
                }
                entry.Write(fs, buffer);
            }

            EndRead();

            fs.Close();
            indexFileSemaphore.Release();
        }

        private void ReadFromDisk(WebCacheEntry entry)
        {
            cacheFileSemaphore.Wait();

            cacheStream.Position = entry.Position;
            var span = entry.AsSpan();
            cacheStream.Read(span);

            cacheFileSemaphore.Release();
        }

        private void WriteBackToDisk(WebCacheEntry entry)
        {
            var state = entry.PersistenceState;
            cacheFileSemaphore.Wait();

            if (entry.Position == -1)
            {
                AllocateDiskSpace(entry, null);
                cacheStream.Position = entry.Position;
                cacheStream.Write(entry.AsSpan());
            }
            else
            {
                if (state.OldSize == entry.Size)
                {
                    cacheStream.Position = entry.Position;
                    cacheStream.Write(entry.AsSpan());
                }
                else
                {
                    // prevent fragmentation.
                    var startPos = entry.Position;
                    var endPos = entry.Position + state.OldSize;
                    var size = diskBytes - endPos;

                    cacheStream.MoveBlock(endPos, startPos, size);

                    for (int i = 0; i < entries.Count; i++)
                    {
                        var sEntry = entries[i];
                        if (sEntry.Position > startPos)
                        {
                            sEntry.Position -= state.OldSize;
                        }
                    }

                    AllocateDiskSpace(entry, state.OldSize);

                    cacheStream.Position = entry.Position;
                    cacheStream.Write(entry.AsSpan());
                }
            }

            cacheFileSemaphore.Release();
            entry.PersistenceState = default;
        }

        /// <summary>
        /// Only call after you acquired cacheFileSemaphore before.
        /// </summary>
        /// <param name="entry"></param>
        private void RemoveFromDiskInternal(WebCacheEntry entry)
        {
            var state = entry.PersistenceState;

            if (entry.Position == -1)
            {
                return;
            }
            else
            {
                if (!state.IsDirty)
                {
                    // prevent fragmentation.
                    var startPos = entry.Position;
                    var endPos = entry.Position + entry.Size;
                    var size = diskBytes - endPos;

                    cacheStream.MoveBlock(endPos, startPos, size);

                    for (int i = 0; i < entries.Count; i++)
                    {
                        var sEntry = entries[i];
                        if (sEntry.Position > startPos)
                        {
                            sEntry.Position -= size;
                        }
                    }
                }
                else
                {
                    // prevent fragmentation.
                    var startPos = entry.Position;
                    var endPos = entry.Position + state.OldSize;
                    var size = diskBytes - endPos;

                    cacheStream.MoveBlock(endPos, startPos, size);

                    for (int i = 0; i < entries.Count; i++)
                    {
                        var sEntry = entries[i];
                        if (sEntry.Position > startPos)
                        {
                            sEntry.Position -= size;
                        }
                    }
                }
            }

            entry.PersistenceState = default;
        }

        /// <summary>
        /// Disposes of the resources used by the <see cref="WebCache"/>.
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

        /// <summary>
        /// Disposes of the resources used by the <see cref="WebCache"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}