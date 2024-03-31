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

        private readonly List<PersistentCacheEntry> entries = [];
        private readonly Dictionary<uint, PersistentCacheEntry> hashTable = [];

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
                hashTable.Clear();
                cacheStream = File.Open(cacheFile, FileMode.Create);
            }
            else
            {
                cacheStream = File.Open(cacheFile, FileMode.Open);
            }
        }

        private const int Version = 1;

        /// <summary>
        /// Gets or sets maximum memory cache size in bytes. Default value is 512MB (536870912 Bytes).
        /// </summary>
        public int MaxMemorySize { get => maxMemorySize; set => maxMemorySize = value; }

        /// <summary>
        /// Gets or sets the cache policy. Default policy is LRU (Least recent used)
        /// </summary>
        public ICachePolicy CachePolicy { get => cachePolicy; set => cachePolicy = value; }

        private readonly ManualResetEventSlim readLock = new(true);
        private readonly SemaphoreSlim writeSemaphore = new(1);
        private readonly ManualResetEventSlim writeLock = new(true);
        private readonly SemaphoreSlim readSemaphore = new(MaxConcurrentReaders);

        /// <summary>
        /// Maximum concurrent readers/threads.
        /// </summary>
        public const int MaxConcurrentReaders = 64;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BeginWrite()
        {
            // wait for exclusive write lock.
            writeSemaphore.Wait();

            // reset write lock to block readers.
            writeLock.Reset();

            // wait for readers to end their read process.
            readLock.Wait();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndWrite()
        {
            // sets the write lock to allow readers again to read.
            writeLock.Set();

            // release exclusive write lock.
            writeSemaphore.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BeginRead()
        {
            // blocks the reader if a write is pending.
            writeLock.Wait();

            // resets the read lock to block writers until the read process is done.
            readLock.Reset();

            // waits for reader semaphore.
            readSemaphore.Wait();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndRead()
        {
            // releases reader semaphore.
            readSemaphore.Release();

            // checks if all readers exited the reading process, note that CurrentCount is read with Volatile.Read internally.
            if (readSemaphore.CurrentCount == MaxConcurrentReaders)
            {
                // sets the read lock to inform writers that all read processes are done.
                readLock.Set();
            }
        }

        private bool TryGetEntry(string key, [NotNullWhen(true)] out PersistentCacheEntry? entry)
        {
            var hash = Crc32.Compute(MemoryMarshal.AsBytes(key.AsSpan()));
            return hashTable.TryGetValue(hash, out entry);
        }

        private bool TryGetEntry(uint key, [NotNullWhen(true)] out PersistentCacheEntry? entry)
        {
            return hashTable.TryGetValue(key, out entry);
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
            hashTable.Clear();

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
            uint hash = Crc32.Compute(MemoryMarshal.AsBytes(key.AsSpan()));

            BeginRead();

            bool result = hashTable.ContainsKey(hash);

            EndRead();

            return result;
        }

        /// <summary>
        /// Determines whether the cache contains an entry with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns><see langword="true"/> if the cache contains an entry with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool ContainsKey(uint key)
        {
            BeginRead();

            bool result = hashTable.ContainsKey(key);

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

            BeginRead();
            if (TryGetEntry(hash, out PersistentCacheEntry? entry))
            {
                // we don't need exclusive cache access if the entry already exists locking the entry is enough.
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

                entry.ReleaseLock();

                EndRead();
            }
            else
            {
                EndRead();

                BeginWrite(); // exclusive cache access is needed since we insert a new entry.

                entry = new(key, size, -1);

                entry.PersistenceState.IsDirty = true;
                entry.PersistenceState.OldSize = unchecked((uint)-1);

                AllocateSpace(entry, null);
                MemcpyT(data, entry.Data, size);
                entry.LastAccess = DateTime.Now;

                entries.Add(entry);
                hashTable.Add(hash, entry);

                EndWrite();
            }
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
            BeginRead();
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
            EndRead();

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
                var entry = PersistentCacheEntry.Read(fs, buffer);
                entries.Add(entry);
                hashTable.Add(entry.Key, entry);
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
                hashTable.Clear();

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

    public interface IDiskWriteable
    {
        public int Write(Span<byte> destination);

        public int Read(ReadOnlySpan<byte> source);
    }

    /// <summary>
    /// A persistent memory cache with lazy disk writing.
    /// </summary>
    public class PersistentCache<TKey> : IDisposable where TKey : unmanaged, IEquatable<TKey>, IDiskWriteable
    {
        private readonly string cacheFile;
        private readonly string indexFile;

        private readonly List<PersistentCacheEntry<TKey>> entries = [];
        private readonly Dictionary<TKey, PersistentCacheEntry<TKey>> hashTable;

        private readonly SemaphoreSlim memorySemaphore = new(1);
        private readonly SemaphoreSlim indexFileSemaphore = new(1);
        private readonly SemaphoreSlim cacheFileSemaphore = new(1);

        private readonly FileStream cacheStream;
        private long writtenBytes;
        private long memoryBytes;
        private bool disposedValue;
        private int maxMemorySize = 536870912;
        private ICachePolicy<TKey> cachePolicy = new LRUCachePolicy<TKey>();

        /// <summary>
        /// Creates a new instance of <see cref="PersistentCache"/>.
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="folder">The folder where to save the cache data.</param>
        public PersistentCache(IEqualityComparer<TKey> comparer, string folder) : this(comparer, Path.Combine(folder, "cache.bin"), Path.Combine(folder, "cache.index"))
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="PersistentCache"/>.
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="cacheFile">The path to the cache file.</param>
        /// <param name="indexFile">The path to the index file.</param>
        public PersistentCache(IEqualityComparer<TKey> comparer, string cacheFile, string indexFile)
        {
            hashTable = new(comparer);
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
                hashTable.Clear();
                cacheStream = File.Open(cacheFile, FileMode.Create);
            }
            else
            {
                cacheStream = File.Open(cacheFile, FileMode.Open);
            }
        }

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
            hashTable = [];
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
                hashTable.Clear();
                cacheStream = File.Open(cacheFile, FileMode.Create);
            }
            else
            {
                cacheStream = File.Open(cacheFile, FileMode.Open);
            }
        }

        private const int Version = 1;

        /// <summary>
        /// Gets or sets maximum memory cache size in bytes. Default value is 512MB (536870912 Bytes).
        /// </summary>
        public int MaxMemorySize { get => maxMemorySize; set => maxMemorySize = value; }

        /// <summary>
        /// Gets or sets the cache policy. Default policy is LRU (Least recent used)
        /// </summary>
        public ICachePolicy<TKey> CachePolicy { get => cachePolicy; set => cachePolicy = value; }

        private readonly ManualResetEventSlim readLock = new(true);
        private readonly SemaphoreSlim writeSemaphore = new(1);
        private readonly ManualResetEventSlim writeLock = new(true);
        private readonly SemaphoreSlim readSemaphore = new(MaxConcurrentReaders);

        /// <summary>
        /// Maximum concurrent readers/threads.
        /// </summary>
        public const int MaxConcurrentReaders = 64;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BeginWrite()
        {
            // wait for exclusive write lock.
            writeSemaphore.Wait();

            // reset write lock to block readers.
            writeLock.Reset();

            // wait for readers to end their read process.
            readLock.Wait();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndWrite()
        {
            // sets the write lock to allow readers again to read.
            writeLock.Set();

            // release exclusive write lock.
            writeSemaphore.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BeginRead()
        {
            // blocks the reader if a write is pending.
            writeLock.Wait();

            // resets the read lock to block writers until the read process is done.
            readLock.Reset();

            // waits for reader semaphore.
            readSemaphore.Wait();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndRead()
        {
            // releases reader semaphore.
            readSemaphore.Release();

            // checks if all readers exited the reading process, note that CurrentCount is read with Volatile.Read internally.
            if (readSemaphore.CurrentCount == MaxConcurrentReaders)
            {
                // sets the read lock to inform writers that all read processes are done.
                readLock.Set();
            }
        }

        private bool TryGetEntry(TKey key, [NotNullWhen(true)] out PersistentCacheEntry<TKey>? entry)
        {
            return hashTable.TryGetValue(key, out entry);
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
            hashTable.Clear();

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
        public bool ContainsKey(TKey key)
        {
            BeginRead();

            bool result = hashTable.ContainsKey(key);

            EndRead();

            return result;
        }

        /// <summary>
        /// Sets the data associated with the specified key in the cache.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="data">The byte array representing the data to be stored.</param>
        public unsafe void Set(TKey key, byte[] data)
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
        public unsafe void Set(TKey key, byte* data, uint size)
        {
            BeginWrite();

            if (TryGetEntry(key, out PersistentCacheEntry<TKey>? entry))
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
                hashTable.Add(key, entry);
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
        public unsafe void Get(TKey key, byte** data, uint* size)
        {
            BeginRead();

            if (!TryGetEntry(key, out var entry))
            {
                throw new KeyNotFoundException($"Key \"{key}\", was not found!");
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
        public unsafe bool TryGet(TKey key, byte** data, uint* size)
        {
            BeginRead();

            bool result = false;

            if (TryGetEntry(key, out var entry))
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
        /// <remarks>Call <see cref="PersistentCacheEntry{TKey}.Lock"/> before calling this method.</remarks>
        /// <param name="entry">The cache entry.</param>
        /// <param name="oldSize">Optional the old size of the entry.</param>
        private unsafe bool AllocateSpace(PersistentCacheEntry<TKey> entry, uint? oldSize)
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
            BeginRead();
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
            EndRead();

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
            hashTable.EnsureCapacity(count);

            Span<byte> buffer = stackalloc byte[PersistentCacheEntry<TKey>.EntrySize];
            for (int i = 0; i < count; i++)
            {
                var entry = PersistentCacheEntry<TKey>.Read(fs, buffer);
                entries.Add(entry);
                hashTable.Add(entry.Key, entry);
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

            Span<byte> buffer = stackalloc byte[PersistentCacheEntry<TKey>.EntrySize];
            for (var i = 0; i < entries.Count; i++)
            {
                entries[i].Write(fs, buffer);
            }

            EndRead();

            fs.Close();
            indexFileSemaphore.Release();
        }

        private void ReadFromDisk(PersistentCacheEntry<TKey> entry)
        {
            cacheFileSemaphore.Wait();

            cacheStream.Position = entry.Position;
            var span = entry.AsSpan();
            cacheStream.Read(span);

            cacheFileSemaphore.Release();
        }

        private void WriteBackToDisk(PersistentCacheEntry<TKey> entry)
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
        /// Disposes of the resources used by the <see cref="PersistentCache{TKey}"/>.
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
                hashTable.Clear();

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
        /// Disposes of the resources used by the <see cref="PersistentCache{TKey}"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}