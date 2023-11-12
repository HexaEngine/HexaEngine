namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.IO;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// A memory cache with the LRU (Least Recently Used) strategy.
    /// </summary>
    public unsafe class Cache
    {
        private readonly Dictionary<string, CacheHandle> nameToHandle = new();
        private readonly List<CacheHandle> handles = new();
        private readonly CacheMode mode;
        private long size;
        private long free;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache"/> class with the specified cache size.
        /// </summary>
        /// <param name="mode">The cache operation mode.</param>
        /// <param name="size">The maximum size of the cache in bytes.</param>
        public Cache(CacheMode mode = CacheMode.OnDemand, long size = 65536)
        {
            this.mode = mode;
            this.size = free = size;
        }

        /// <summary>
        /// Gets or sets the maximum size of the cache.
        /// </summary>
        public long Size { get => size; set => size = value; }

        /// <summary>
        /// Allocates a buffer from the cache for the specified length.
        /// </summary>
        /// <param name="length">The requested buffer size in bytes.</param>
        /// <returns>A memory handle to the allocated buffer.</returns>
        /// <exception cref="ArgumentException">Thrown when the requested buffer size exceeds the cache size.</exception>
        public MemoryHandle AllocBuffer(long length)
        {
            if (length > size)
            {
                throw new ArgumentException($"The requested buffer size ({length}) was larger than the cache size ({size})", nameof(length));
            }

            if (free >= length)
            {
                free -= length;

                return new((byte*)Marshal.AllocHGlobal((nint)length), length);
            }

            lock (handles)
            {
                handles.Sort(CacheAccessComparer.Instance);

                while (free < length)
                {
                    DestroyCacheHandle(handles[0]);
                }
            }

            return AllocBuffer(length);
        }

        /// <summary>
        /// Frees a previously allocated buffer, making its memory available for reuse in the cache.
        /// </summary>
        /// <param name="memory">The memory handle to the buffer being freed.</param>
        public void FreeBuffer(MemoryHandle memory)
        {
            free += memory.Size;
            Marshal.FreeHGlobal((nint)memory.Data);
        }

        /// <summary>
        /// Represents a memory handle to an allocated buffer within the cache.
        /// </summary>
        public readonly unsafe struct MemoryHandle
        {
            /// <summary>
            /// Gets a pointer to the allocated buffer.
            /// </summary>
            public readonly byte* Data;

            /// <summary>
            /// Gets the size of the allocated buffer.
            /// </summary>
            public readonly long Size;

            /// <summary>
            /// Initializes a new instance of the <see cref="MemoryHandle"/> struct with the specified data pointer and size.
            /// </summary>
            /// <param name="data">The pointer to the allocated buffer.</param>
            /// <param name="size">The size of the allocated buffer.</param>
            internal MemoryHandle(byte* data, long size)
            {
                Data = data;
                Size = size;
            }

            /// <summary>
            /// Gets a <see cref="Span{T}"/> representation of the allocated buffer.
            /// </summary>
            public readonly Span<byte> Span => new(Data, (int)Size);

            /// <summary>
            /// Gets a value indicating whether the memory handle is valid (non-null data pointer).
            /// </summary>
            public readonly bool IsValid => Data != null;

            /// <summary>
            /// Gets a value indicating whether the memory handle is null (null data pointer).
            /// </summary>
            public readonly bool IsNull => Data == null;
        }


        /// <summary>
        /// Gets a cache handle associated with the specified resource name.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <returns>A cache handle associated with the resource.</returns>
        public CacheHandle GetCacheHandle(string name)
        {
            lock (handles)
            {
                if (nameToHandle.TryGetValue(name, out CacheHandle? handle))
                {
                    return handle;
                }

                return CreateCacheHandle(name);
            }
        }

        /// <summary>
        /// Flushes the entire cache, disposing of all cached resources.
        /// </summary>
        public void Flush()
        {
            lock (handles)
            {
                for (int i = 0; i < handles.Count; i++)
                {
                    handles[i].Dispose();
                }

                handles.Clear();
                nameToHandle.Clear();
            }
        }

        /// <summary>
        /// Flushes a specific resource from the cache by its name.
        /// </summary>
        /// <param name="name">The name of the resource to flush.</param>
        public void Flush(string name)
        {
            lock (handles)
            {
                var handle = nameToHandle[name];
                DestroyCacheHandle(handle);
            }
        }

        private CacheHandle CreateCacheHandle(string name)
        {
            CacheHandle handle = new(this, name, FileSystem.OpenRead(name), mode);
            nameToHandle.Add(name, handle);
            handles.Add(handle);
            return handle;
        }

        private void DestroyCacheHandle(CacheHandle handle)
        {
            nameToHandle.Remove(handle.Name);
            handles.Remove(handle);
            handle.Dispose();
        }
    }
}