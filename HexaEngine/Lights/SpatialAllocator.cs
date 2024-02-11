using HexaEngine.Core;

namespace HexaEngine.Lights
{
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    public class SpatialCacheHandle
    {
        public readonly SpatialAllocatorHandle Handle;
        public bool IsTaken;

        public SpatialCacheHandle(SpatialAllocatorHandle handle)
        {
            Handle = handle;
        }
    }

    public class SpatialCache : IDisposable
    {
        private readonly Dictionary<Vector2, List<SpatialCacheHandle>> cache = [];
        private readonly List<List<SpatialCacheHandle>> subCaches = [];
        private readonly object _lock = new();
        private bool disposedValue;

        public SpatialCache(Vector2 size, int subdivisions)
        {
            Vector2 targetSize = size;
            for (int i = 0; i < subdivisions; i++)
            {
                List<SpatialCacheHandle> handles = [];
                subCaches.Add(handles);
                cache.Add(targetSize, handles);
                targetSize /= 2;
            }
        }

        private List<SpatialCacheHandle> GetSubCache(Vector2 size)
        {
            if (cache.TryGetValue(size, out var subCache))
            {
                return subCache;
            }
            throw new InvalidOperationException($"The given size ({size}) is not supported by the cache");
        }

        private static int IndexOf(List<SpatialCacheHandle> cache, SpatialAllocatorHandle handle)
        {
            for (int i = 0; i < cache.Count; i++)
            {
                if (handle == cache[i].Handle)
                {
                    return i;
                }
            }
            return -1;
        }

        public SpatialCacheHandle AddToCache(SpatialAllocatorHandle handle)
        {
            SpatialCacheHandle cacheHandle = new(handle);
            lock (_lock)
            {
                GetSubCache(handle.Size).Add(cacheHandle);
            }
            return cacheHandle;
        }

        public bool TryRemoveFromCache(SpatialAllocatorHandle handle)
        {
            lock (_lock)
            {
                var cache = GetSubCache(handle.Size);
                int index = IndexOf(cache, handle);

                if (index == -1)
                {
                    return false;
                }

                var cacheHandle = cache[index];

                if (cacheHandle.IsTaken)
                {
                    return false;
                }

                cacheHandle.IsTaken = true;
                cache.RemoveAt(index);
                return true;
            }
        }

        public bool TryRemoveFromCache(SpatialCacheHandle handle)
        {
            lock (_lock)
            {
                if (handle.IsTaken)
                {
                    return false;
                }

                var cache = GetSubCache(handle.Handle.Size);
                int index = cache.IndexOf(handle);

                if (index == -1)
                {
                    return false;
                }

                handle.IsTaken = true;
                cache.RemoveAt(index);
                return true;
            }
        }

        public SpatialAllocatorHandle? AllocFromCache(Vector2 size)
        {
            lock (_lock)
            {
                var cache = GetSubCache(size);
                if (cache.Count == 0)
                {
                    return null;
                }

                SpatialCacheHandle handle = cache[0];
                handle.IsTaken = true;

                cache.RemoveAt(0);
                return handle.Handle;
            }
        }

        public bool TryAllocFromCache(Vector2 size, [NotNullWhen(true)] out SpatialAllocatorHandle? handle)
        {
            lock (_lock)
            {
                var cache = GetSubCache(size);
                if (cache.Count == 0)
                {
                    handle = null;
                    return false;
                }

                var cacheHandle = cache[0];
                cacheHandle.IsTaken = true;

                handle = cacheHandle.Handle;
                cache.RemoveAt(0);
                return true;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                for (int i = 0; i < subCaches.Count; i++)
                {
                    var cache = subCaches[i];
                    for (int j = 0; j < cache.Count; j++)
                    {
                        cache[j].IsTaken = true;
                    }
                    cache.Clear();
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Clear();
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

    public unsafe class SpatialAllocator
    {
        private Space* rootSpace;

        public SpatialAllocator(Vector2 size, int subdivisions)
        {
            Vector2 targetSize = size;
            for (int i = 0; i < subdivisions; i++)
            {
                targetSize /= 2;
            }

            rootSpace = AllocT<Space>();
            rootSpace->Size = size;
            rootSpace->Offset = default;
            rootSpace->InitSubSpaces(rootSpace, targetSize);
        }

        public bool IsDisposed => rootSpace == null;

        public event Action<SpatialAllocator>? OnDisposing;

        internal unsafe struct Space
        {
            public Vector2 Size;
            public Vector2 Offset;
            public Space* Parent;
            public Space* SubSpaces;
            public byte Flag;

            public bool IsUsed
            {
                readonly get => (Flag & 0x1) != 0;
                set
                {
                    if (value)
                    {
                        Flag |= 0x1;
                    }
                    else
                    {
                        Flag = (byte)(Flag & ~0x1);
                    }
                }
            }

            public bool IsPartiallyUsed
            {
                readonly get => (Flag & 0x2) != 0;
                set
                {
                    if (value)
                    {
                        Flag |= 0x2;
                    }
                    else
                    {
                        Flag = (byte)(Flag & ~0x2);
                    }
                }
            }

            public const int SubSpaceCount = 4;
            public const int SubSpaceWidth = 2;

            public void InitSubSpaces(Space* self, Vector2 endSize)
            {
                if (Size == endSize)
                    return;

                SubSpaces = AllocT<Space>(SubSpaceCount);
                ZeroMemoryT(SubSpaces, SubSpaceCount);

                Vector2 halfSize = Size / 2;
                for (int i = 0; i < SubSpaceCount; i++)
                {
                    int x = i / SubSpaceWidth;
                    int y = i % SubSpaceWidth;
                    Space* subSpace = &SubSpaces[i];
                    subSpace->Parent = self;
                    subSpace->Size = halfSize;
                    subSpace->Offset = Offset + new Vector2(x, y) * halfSize;
                    subSpace->InitSubSpaces(subSpace, endSize);
                }
            }

            public void Clear()
            {
                Flag = 0;
                if (SubSpaces != null)
                {
                    for (int i = 0; i < SubSpaceCount; i++)
                    {
                        Space* subSpace = &SubSpaces[i];
                        subSpace->Clear();
                    }
                }
            }

            public void Release()
            {
                if (SubSpaces != null)
                {
                    for (int i = 0; i < SubSpaceCount; i++)
                    {
                        Space* subSpace = &SubSpaces[i];
                        subSpace->Release();
                    }
                }

                Utils.Free(SubSpaces);
                SubSpaces = null;
            }

            public override readonly string ToString()
            {
                return $"Offset: {Offset}, Size: {Size}";
            }
        }

        public bool TryAlloc(Vector2 size, [NotNullWhen(true)] out SpatialAllocatorHandle? handle)
        {
            Space* space;
            bool result = TryAlloc(size, rootSpace, &space);
            if (!result)
            {
                handle = null;
                return false;
            }
            handle = new(this, space);
            return true;
        }

        internal bool TryAlloc(Vector2 size, Space* space, Space** outSpace)
        {
            if (rootSpace == null)
            {
                throw new ObjectDisposedException(nameof(SpatialAllocator));
            }

            *outSpace = null;
            if (size == space->Size)
            {
                // we cant use a used or partially used space.
                if (space->IsUsed || space->IsPartiallyUsed)
                {
                    return false;
                }

                // mark space as used.
                space->IsUsed = true;
                *outSpace = space;
                return true;
            }
            else
            {
                // we reached the end of the tree there is nothing left here.
                if (space->SubSpaces == null)
                {
                    return false;
                }

                for (int i = 0; i < Space.SubSpaceCount; i++)
                {
                    Space* subSpace = &space->SubSpaces[i];

                    if (subSpace->IsUsed)
                    {
                        continue;
                    }

                    // traverse down the tree.
                    if (TryAlloc(size, subSpace, outSpace))
                    {
                        // mark space as partially used.
                        space->IsPartiallyUsed = true;
                        return true;
                    }
                }

                return false;
            }
        }

        public SpatialAllocatorHandle? Alloc(Vector2 size)
        {
            Space* space = Alloc(size, rootSpace);
            if (space == null)
            {
                return null;
            }

            return new(this, space);
        }

        internal Space* AllocInternal(Vector2 size)
        {
            return Alloc(size, rootSpace);
        }

        private Space* Alloc(Vector2 size, Space* space)
        {
            if (rootSpace == null)
            {
                throw new ObjectDisposedException(nameof(SpatialAllocator));
            }

            if (size == space->Size)
            {
                // we can't use a used or partially used space.
                if (space->IsUsed || space->IsPartiallyUsed)
                {
                    return null;
                }

                // mark space as used.
                space->IsUsed = true;
                return space;
            }
            else
            {
                // we reached the end of the tree there is nothing left here.
                if (space->SubSpaces == null)
                {
                    return null;
                }

                for (int i = 0; i < Space.SubSpaceCount; i++)
                {
                    Space* subSpace = &space->SubSpaces[i];

                    if (subSpace->IsUsed)
                    {
                        continue;
                    }

                    // traverse down the tree.
                    var allocatedSpace = Alloc(size, subSpace);
                    if (allocatedSpace != null)
                    {
                        // mark space as partially used.
                        space->IsPartiallyUsed = true;
                        return allocatedSpace;
                    }
                }

                return null;
            }
        }

        internal void Free(Space* space)
        {
            if (rootSpace == null)
            {
                return;
            }

            if (space == null)
            {
                return;
            }

            if (!space->IsUsed)
            {
                return;
            }

            // mark space as unused.
            space->IsUsed = false;
            var parent = space->Parent;

            // clean up partially used.
            while (parent != null)
            {
                bool isUsed = false;
                for (int i = 0; i < Space.SubSpaceCount; i++)
                {
                    Space* subSpace = &parent->SubSpaces[i];

                    if (subSpace->IsUsed || subSpace->IsPartiallyUsed)
                    {
                        isUsed = true;
                        // we can skip the checks here because everything above will be partially used anyway.
                        break;
                    }
                }

                // we can skip the checks here because everything above will be partially used anyway.
                if (isUsed)
                {
                    break;
                }

                parent->IsPartiallyUsed = false;
                parent = parent->Parent;
            }
        }

        public void Clear()
        {
            if (rootSpace == null)
            {
                throw new ObjectDisposedException(nameof(SpatialAllocator));
            }

            rootSpace->Clear();
        }

        public void Dispose()
        {
            if (rootSpace != null)
            {
                OnDisposing?.Invoke(this);
                rootSpace->Release();
                Utils.Free(rootSpace);
                rootSpace = null;
            }
        }
    }
}