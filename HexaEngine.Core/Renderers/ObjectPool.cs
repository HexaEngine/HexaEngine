namespace HexaEngine.Core.Renderers
{
    using HexaEngine.Core.Unsafes;

    public struct ObjectHandle
    {
        public int Id;

        public bool IsValid => Id > 0;
    }

    public unsafe class ObjectPool<T>
    {
        private readonly UnsafeList<Pointer<ObjectHandle>>* handles;
        private readonly List<T> objects = new();
        private readonly object _lock = new();

        public ObjectPool()
        {
            handles = Alloc<UnsafeList<Pointer<ObjectHandle>>>();
        }

        public unsafe T this[Pointer<ObjectHandle> handle]
        {
            get
            {
                lock (_lock)
                {
                    return (objects)[handle.Data->Id - 1];
                }
            }
            set
            {
                lock (_lock)
                {
                    (objects)[handle.Data->Id - 1] = value;
                }
            }
        }

        public unsafe Pointer<ObjectHandle> Add(T obj)
        {
            objects.Add(obj);
            return CreateHandle();
        }

        public unsafe void Remove(Pointer<ObjectHandle> handle)
        {
            var hnd = (ObjectHandle*)handle;
            objects.RemoveAt(hnd->Id - 1);
            DestroyHandle(handle);
        }

        private unsafe ObjectHandle* CreateHandle()
        {
            lock (_lock)
            {
                ObjectHandle* handle = Alloc<ObjectHandle>();
                var id = handles->Size + 1;
                handle->Id = (int)id;
                handles->Add(handle);
                return handle;
            }
        }

        private unsafe void DestroyHandle(ObjectHandle* handle)
        {
            lock (_lock)
            {
                var baseIndex = handle->Id;
                for (int i = baseIndex + 1, j = 0; i < handles->Size; i++, j++)
                {
                    (*handles)[i].Data->Id = baseIndex + j;
                }
                handles->RemoveAt(baseIndex);
                handle->Id = -1;
            }
        }
    }
}