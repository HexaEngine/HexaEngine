namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.IO;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// A memory cache with the LRU strategy.
    /// </summary>
    public unsafe class Cache
    {
        private readonly Dictionary<string, CacheHandle> nameToHandle = new();
        private readonly List<CacheHandle> handles = new();
        private readonly CacheMode mode;
        private long size;
        private long free;

        public Cache(long size = 65536)
        {
            this.size = free = size;
        }

        public long Size { get => size; set => size = value; }

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

        public void FreeBuffer(MemoryHandle memory)
        {
            free += memory.Size;
            Marshal.FreeHGlobal((nint)memory.Data);
        }

        public readonly unsafe struct MemoryHandle
        {
            public readonly byte* Data;
            public readonly long Size;

            internal MemoryHandle(byte* data, long size)
            {
                Data = data;
                Size = size;
            }

            public readonly Span<byte> Span => new(Data, (int)Size);

            public readonly bool IsValid => Data != null;

            public readonly bool IsNull => Data == null;
        }

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
            CacheHandle handle = new(this, name, FileSystem.Open(name), mode);
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