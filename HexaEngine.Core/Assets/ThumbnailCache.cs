namespace HexaEngine.Core.Assets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a cache policy interface.
    /// </summary>
    public interface ICachePolicy
    {
        /// <summary>
        /// Gets the cache entry that should be removed according to the policy.
        /// </summary>
        /// <param name="entries">The list of cache entries.</param>
        /// <param name="ignore">The cache entry to ignore during selection.</param>
        /// <returns>The cache entry to be removed, or <see langword="null"/> if no entry should be removed.</returns>
        public ThumbnailCacheEntry? GetItemToRemove(IList<ThumbnailCacheEntry> entries, ThumbnailCacheEntry ignore);
    }

    /// <summary>
    /// Represents the persistence state of a cache entry.
    /// </summary>
    public struct CacheEntryPersistenceState : IEquatable<CacheEntryPersistenceState>
    {
        /// <summary>
        /// Indicates whether the cache entry has been modified and needs to be written back to disk.
        /// </summary>
        public bool IsDirty;

        /// <summary>
        /// The previous size of the cache entry.
        /// </summary>
        public long OldSize;

        public override readonly bool Equals(object? obj)
        {
            return obj is CacheEntryPersistenceState state && Equals(state);
        }

        public readonly bool Equals(CacheEntryPersistenceState other)
        {
            return IsDirty == other.IsDirty &&
                   OldSize == other.OldSize;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(IsDirty, OldSize);
        }

        public static bool operator ==(CacheEntryPersistenceState left, CacheEntryPersistenceState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CacheEntryPersistenceState left, CacheEntryPersistenceState right)
        {
            return !(left == right);
        }
    }

    public unsafe class ThumbnailCacheEntry : IDisposable
    {
        private readonly SemaphoreSlim semaphore = new(1);
        private bool disposedValue;

        public Guid Key;
        public long MemSize;
        public long Position;
        public DateTime LastAccess;
        public byte* Data;

        public uint VideoMemSize;
        public Ref<Texture2D> Texture = new();

        /// <summary>
        /// Gets or sets the persistence state.
        /// </summary>
        public CacheEntryPersistenceState PersistenceState;

        /// <summary>
        /// The size in bytes of a <see cref="ThumbnailCacheEntry"/>
        /// </summary>
        internal const int EntrySize = 40;

        public ThumbnailCacheEntry(Guid key, long size, long position)
        {
            Key = key;
            MemSize = size;
            Position = position;
            LastAccess = DateTime.Now;
        }

        public ThumbnailCacheEntry(Guid key, long size, long position, DateTime lastAccess)
        {
            Key = key;
            MemSize = size;
            Position = position;
            LastAccess = lastAccess;
        }

        public void Lock()
        {
            semaphore.Wait();
        }

        public void ReleaseLock()
        {
            semaphore.Release();
        }

        public void Free()
        {
            if (Data != null)
            {
                Utils.Free(Data);
                Data = null;
            }
        }

        public void FreeVideo()
        {
            if (Texture.Value != null)
            {
                Texture.Value.Dispose();
                Texture.Value = null;
            }
        }

        public Span<byte> AsSpan()
        {
            return new Span<byte>(Data, (int)MemSize);
        }

        /// <summary>
        /// Reads a <see cref="ThumbnailCacheEntry"/> from the specified stream using the provided buffer.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="buffer">The buffer to use for reading.</param>
        /// <returns>The read <see cref="ThumbnailCacheEntry"/>.</returns>
        public static ThumbnailCacheEntry Read(Stream stream, Span<byte> buffer)
        {
            stream.ReadExactly(buffer);
            Guid key = new(buffer.Slice(0, 16));
            long size = BinaryPrimitives.ReadInt64LittleEndian(buffer[16..]);
            long position = BinaryPrimitives.ReadInt64LittleEndian(buffer[24..]);
            DateTime lastAccess = new(BinaryPrimitives.ReadInt64LittleEndian(buffer[32..]));
            return new(key, size, position, lastAccess);
        }

        /// <summary>
        /// Writes the <see cref="ThumbnailCacheEntry"/> to the specified stream using the provided buffer.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="buffer">The buffer to use for writing.</param>
        public void Write(Stream stream, Span<byte> buffer)
        {
            Key.TryWriteBytes(buffer);
            BinaryPrimitives.WriteInt64LittleEndian(buffer[16..], MemSize);
            BinaryPrimitives.WriteInt64LittleEndian(buffer[24..], Position);
            BinaryPrimitives.WriteInt64LittleEndian(buffer[32..], LastAccess.Ticks);
            stream.Write(buffer);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Free();
                FreeVideo();
                semaphore.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class LRUCachePolicy : ICachePolicy
    {
        /// <summary>
        /// Gets the cache entry that should be removed according to the LRU policy.
        /// </summary>
        /// <param name="entries">The list of cache entries.</param>
        /// <param name="ignore">The cache entry to ignore during selection.</param>
        /// <returns>The cache entry to be removed, or <see langword="null"/> if no entry should be removed.</returns>
        public unsafe ThumbnailCacheEntry? GetItemToRemove(IList<ThumbnailCacheEntry> entries, ThumbnailCacheEntry ignore)
        {
            ThumbnailCacheEntry? itemToRemove = null;
            DateTime lastAccess = DateTime.MaxValue;
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry != ignore && entry.Data != null && entry.LastAccess < lastAccess)
                {
                    itemToRemove = entry;
                    lastAccess = entry.LastAccess;
                }
            }
            return itemToRemove;
        }
    }

    /// <summary>
    /// A thread-safe thumbnail cache, with lazy-disk writing.
    /// </summary>
    public class ThumbnailCache : IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly string cacheFile;
        private readonly string indexFile;

        private readonly List<ThumbnailCacheEntry> entries = [];
        private readonly Dictionary<Guid, ThumbnailCacheEntry> hashTable = [];

        private readonly ManualResetEventSlim readLock = new(true);
        private readonly SemaphoreSlim writeSemaphore = new(1);
        private readonly ManualResetEventSlim writeLock = new(true);
        private readonly SemaphoreSlim readSemaphore = new(MaxConcurrentReaders);

        private int maxVideoMemorySize = 67108864;
        private int maxMemorySize = 134217728;

        private readonly SemaphoreSlim memorySemaphore = new(1);
        private readonly SemaphoreSlim videoMemorySemaphore = new(1);

        private readonly SemaphoreSlim indexFileSemaphore = new(1);
        private readonly SemaphoreSlim cacheFileSemaphore = new(1);

        private readonly FileStream cacheStream;

        private long writtenBytes;
        private long allocatedMemory;
        private long allocatedVideoMemory;

        private ICachePolicy cachePolicy = new LRUCachePolicy();
        private bool disposedValue;
        public const int MaxConcurrentReaders = 64;

        public ThumbnailCache(IGraphicsDevice device, string folder) : this(device, Path.Combine(folder, "cache.bin"), Path.Combine(folder, "cache.index"))
        {
        }

        public ThumbnailCache(IGraphicsDevice device, string cacheFile, string indexFile)
        {
            this.device = device;
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

        public ICachePolicy CachePolicy { get => cachePolicy; set => cachePolicy = value; }

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

        /// <summary>
        /// Gets or sets maximum video memory cache size in bytes. Default value is 64MB (67108864 Bytes).
        /// </summary>
        public int MaxVideoMemorySize { get => maxVideoMemorySize; set => maxVideoMemorySize = value; }

        /// <summary>
        /// Gets or sets maximum memory cache size in bytes. Default value is 128MB (134217728 Bytes).
        /// </summary>
        public int MaxMemorySize { get => maxMemorySize; set => maxMemorySize = value; }

        private bool TryGetEntry(Guid key, [NotNullWhen(true)] out ThumbnailCacheEntry? entry)
        {
            return hashTable.TryGetValue(key, out entry);
        }

        public unsafe bool Remove(Guid key)
        {
            BeginRead();
            try
            {
                if (!TryGetEntry(key, out var entry))
                {
                    return false;
                }

                entry.Lock();
                try
                {
                    RemoveFromDisk(entry);

                    if (entry.Data != null)
                    {
                        Interlocked.Add(ref allocatedMemory, -entry.MemSize);
                        entry.Free();
                        entry.MemSize = 0;
                    }

                    if (entry.Texture != null)
                    {
                        Interlocked.Add(ref allocatedVideoMemory, -entry.VideoMemSize);
                        entry.FreeVideo();
                        entry.VideoMemSize = 0;
                    }

                    return true;
                }
                finally
                {
                    entry.ReleaseLock();
                }
            }
            finally
            {
                EndRead();
            }
        }

        public void Get(Guid key, out Ref<Texture2D> texture)
        {
            BeginRead();

            if (!TryGetEntry(key, out var entry))
            {
                throw new KeyNotFoundException($"The key '{key}' was not found.");
            }

            entry.Lock();

            entry.LastAccess = DateTime.Now;
            if (entry.Texture.IsNull)
            {
                LoadTexture(entry, null);
            }

            texture = entry.Texture;

            entry.ReleaseLock();

            EndRead();
        }

        public bool TryGet(Guid key, [MaybeNullWhen(false)] out Ref<Texture2D> texture)
        {
            BeginRead();

            if (!TryGetEntry(key, out var entry))
            {
                texture = null;
                EndRead();
                return false;
            }

            entry.Lock();

            entry.LastAccess = DateTime.Now;
            if (entry.Texture.IsNull)
            {
                LoadTexture(entry, null);
            }

            texture = entry.Texture;

            entry.ReleaseLock();

            EndRead();

            return true;
        }

        public unsafe void Set(Guid key, IScratchImage image)
        {
            BeginRead();

            if (TryGetEntry(key, out var entry))
            {
                entry.Lock();

                byte* data;
                nuint size;
                image.SaveToMemory(&data, &size, Graphics.Textures.TexFileFormat.TGA, 0);

                long oldSize = entry.MemSize;

                if (!entry.PersistenceState.IsDirty)
                {
                    entry.PersistenceState.OldSize = oldSize;
                    entry.PersistenceState.IsDirty = true;
                }

                entry.MemSize = (long)size;

                AllocateMemory(entry, oldSize, false);

                entry.Data = data;
                entry.LastAccess = DateTime.Now;

                if (!entry.Texture.IsNull)
                {
                    entry.Texture.Dispose();
                    LoadTexture(entry, entry.VideoMemSize);
                }

                entry.ReleaseLock();

                EndRead();
            }
            else
            {
                EndRead();

                BeginWrite();

                byte* data;
                nuint size;
                image.SaveToMemory(&data, &size, Graphics.Textures.TexFileFormat.TGA, 0);

                entry = new(key, (long)size, -1);

                AllocateMemory(entry, null, false);

                entry.Data = data;

                entries.Add(entry);
                hashTable.Add(key, entry);
                entry.PersistenceState.IsDirty = true;

                EndWrite();
            }
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            BeginWrite();
            Interlocked.Exchange(ref writtenBytes, 0);
            Interlocked.Exchange(ref allocatedMemory, 0);

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Dispose();
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

            Interlocked.Exchange(ref allocatedMemory, 0);

            WriteIndexFile();
            cacheStream.Flush();

            memorySemaphore.Release();

            EndRead();
        }

        /// <summary>
        /// Flushes the cache this will clear the memory cache and executes all pending IO operations.
        /// </summary>
        public void FlushVideo()
        {
            BeginRead();

            videoMemorySemaphore.Wait();

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                entry.Lock();
                entry.FreeVideo();
                entry.ReleaseLock();
            }

            Interlocked.Exchange(ref allocatedVideoMemory, 0);

            videoMemorySemaphore.Release();

            EndRead();
        }

        /// <summary>
        /// Determines whether the cache contains an entry with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns><see langword="true"/> if the cache contains an entry with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool ContainsKey(Guid key)
        {
            BeginRead();

            bool result = hashTable.ContainsKey(key);

            EndRead();

            return result;
        }

        private unsafe void LoadTexture(ThumbnailCacheEntry entry, uint? oldVideoMemSize)
        {
            if (entry.Data == null)
            {
                AllocateMemory(entry, null, true);
                ReadFromDisk(entry);
            }

            IScratchImage image = device.TextureLoader.LoadFromMemory(Graphics.Textures.TexFileFormat.TGA, entry.Data, (nuint)entry.MemSize);
            var metadata = image.Metadata;
            entry.VideoMemSize = (uint)((uint)MathF.Ceiling(FormatHelper.BitsPerColor(metadata.Format) / 8f) * metadata.Width * metadata.Height);
            AllocateVideoMemory(entry, oldVideoMemSize);

            entry.Texture.Value = new(new(metadata.Format, metadata.Width, metadata.Height, metadata.ArraySize, metadata.MipLevels, GpuAccessFlags.Read), image);
        }

        private bool AllocateVideoMemory(ThumbnailCacheEntry entry, uint? oldSize)
        {
            if (oldSize != null && entry.Texture != null)
            {
                Interlocked.Add(ref allocatedVideoMemory, -(long)oldSize);
                entry.FreeVideo();
            }

            var newSize = Interlocked.Add(ref allocatedVideoMemory, entry.VideoMemSize);
            if (newSize > maxVideoMemorySize)
            {
                videoMemorySemaphore.Wait();

                newSize = Interlocked.Read(ref allocatedVideoMemory);
                if (newSize > maxVideoMemorySize)
                {
                    var overhead = newSize - maxVideoMemorySize;
                    while (overhead > 0)
                    {
                        var itemToRemove = cachePolicy.GetItemToRemove(entries, entry);

                        if (itemToRemove == null)
                        {
                            break;
                        }

                        itemToRemove.Lock();

                        itemToRemove.FreeVideo();
                        overhead -= itemToRemove.VideoMemSize;
                        Interlocked.Add(ref allocatedVideoMemory, -itemToRemove.VideoMemSize);

                        itemToRemove.ReleaseLock();
                    }
                }

                videoMemorySemaphore.Release();
            }

            return true;
        }

        private unsafe bool AllocateMemory(ThumbnailCacheEntry entry, long? oldSize, bool createBuffer)
        {
            if (oldSize != null && entry.Data != null)
            {
                Interlocked.Add(ref allocatedMemory, -(long)oldSize);
                entry.Free();
            }

            var newSize = Interlocked.Add(ref allocatedMemory, entry.MemSize);
            if (newSize > maxMemorySize)
            {
                memorySemaphore.Wait();

                newSize = Interlocked.Read(ref allocatedMemory);
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
                        overhead -= itemToRemove.MemSize;
                        Interlocked.Add(ref allocatedMemory, -itemToRemove.MemSize);
                        itemToRemove.ReleaseLock();
                    }
                }

                memorySemaphore.Release();
            }

            if (createBuffer)
            {
                entry.Data = (byte*)Alloc((nuint)entry.MemSize);
            }

            return true;
        }

        private void ReadFromDisk(ThumbnailCacheEntry entry)
        {
            cacheFileSemaphore.Wait();

            cacheStream.Position = entry.Position;
            var span = entry.AsSpan();
            cacheStream.ReadExactly(span);

            cacheFileSemaphore.Release();
        }

        private void WriteBackToDisk(ThumbnailCacheEntry entry)
        {
            var state = entry.PersistenceState;
            cacheFileSemaphore.Wait();

            if (entry.Position == -1)
            {
                entry.Position = cacheStream.Position = cacheStream.Length;
                var span = entry.AsSpan();
                cacheStream.Write(span);

                writtenBytes += entry.MemSize;
            }
            else
            {
                if (state.OldSize == entry.MemSize)
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

                    cacheStream.MoveBlock(endPos, startPos, size);

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

                    writtenBytes += entry.MemSize;
                }
            }

            cacheFileSemaphore.Release();
            entry.PersistenceState = default;
        }

        private void RemoveFromDisk(ThumbnailCacheEntry entry)
        {
            var state = entry.PersistenceState;
            cacheFileSemaphore.Wait();

            if (entry.Position != -1)
            {
                var itemSize = state.IsDirty ? state.OldSize : entry.MemSize;
                // prevent fragmentation.
                var startPos = entry.Position;
                var endPos = entry.Position + itemSize;
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

                cacheStream.MoveBlock(endPos, startPos, size);

                // offset position and release lock again.
                for (int i = 0; i < entries.Count; i++)
                {
                    var sEntry = entries[i];
                    if (sEntry.Position > startPos)
                    {
                        sEntry.Position -= itemSize;
                        sEntry.ReleaseLock();
                    }
                }

                writtenBytes -= itemSize;
            }

            cacheFileSemaphore.Release();
            entry.PersistenceState = default;
            entry.Position = -1;
        }

        public void GenerateAndSetThumbnail(Guid key, IScratchImage image)
        {
            var metadata = image.Metadata;
            bool owns = false;
            if (FormatHelper.IsCompressed(metadata.Format))
            {
                Swap(ref owns, ref image, image.Decompress(Format.R8G8B8A8UNorm));
            }
            else if (metadata.Format != Format.R8G8B8A8UNorm)
            {
                Swap(ref owns, ref image, image.Convert(Format.R8G8B8A8UNorm, 0));
            }

            Swap(ref owns, ref image, image.Resize(256, 256, 0));

            Set(key, image);

            if (owns)
            {
                image.Dispose();
            }
        }

        private static void Swap(ref bool owns, ref IScratchImage image, IScratchImage newImage)
        {
            if (owns)
            {
                image.Dispose();
            }
            owns = true;
            image = newImage;
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
        }

        private const int Version = 1;

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
            var version = fs.ReadInt32(Hexa.NET.Mathematics.Endianness.LittleEndian);
            if (version != Version)
            {
                EndWrite();
                fs.Close();
                return;
            }

            writtenBytes = fs.ReadInt64(Hexa.NET.Mathematics.Endianness.LittleEndian);

            var count = fs.ReadInt32(Hexa.NET.Mathematics.Endianness.LittleEndian);
            entries.EnsureCapacity(count);

            Span<byte> buffer = stackalloc byte[ThumbnailCacheEntry.EntrySize];
            for (int i = 0; i < count; i++)
            {
                var entry = ThumbnailCacheEntry.Read(fs, buffer);
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

            fs.WriteInt32(Version, Hexa.NET.Mathematics.Endianness.LittleEndian);
            fs.WriteInt64(writtenBytes, Hexa.NET.Mathematics.Endianness.LittleEndian);
            fs.WriteInt32(entries.Count, Hexa.NET.Mathematics.Endianness.LittleEndian);

            Span<byte> buffer = stackalloc byte[ThumbnailCacheEntry.EntrySize];
            for (var i = 0; i < entries.Count; i++)
            {
                entries[i].Write(fs, buffer);
            }

            EndRead();

            fs.Close();
            indexFileSemaphore.Release();
        }

        /// <summary>
        /// Disposes of the resources used by the <see cref="ThumbnailCache"/>.
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
                videoMemorySemaphore.Dispose();
                indexFileSemaphore.Dispose();
                cacheFileSemaphore.Dispose();

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes of the resources used by the <see cref="ThumbnailCache"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}