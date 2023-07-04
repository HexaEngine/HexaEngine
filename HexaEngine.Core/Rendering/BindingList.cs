namespace HexaEngine.Core.Rendering
{
    using HexaEngine.Core.Graphics;
    using System;

    public unsafe class BindingList : IDisposable
    {
        private const uint DefaultCapacity = 4;
        private readonly List<Binding> bindings = new();
        private readonly Dictionary<uint, int> dict = new();
        private uint capacity;
        private uint count;
        private uint startSlot;
        private void** data;
        private bool disposedValue;

        public BindingList()
        {
            capacity = DefaultCapacity;
            data = AllocArray(capacity);
            ZeroMemory(data, (uint)(sizeof(nint) * capacity));
        }

        public int Count => bindings.Count;

        public uint StartSlot => startSlot;

        public uint SlotCount => count;

        public void** Data => data;

        public bool IsReadOnly => false;

        public Binding this[int index]
        {
            get
            {
                return bindings[index];
            }
            set
            {
                bindings[index] = value;
                Update();
            }
        }

        public nint this[uint slot]
        {
            get
            {
                return bindings[dict[slot]].Data;
            }
            set
            {
                var binding = bindings[dict[slot]];
                binding.Data = value;
                bindings[dict[slot]] = binding;
                Update();
            }
        }

        private void EnsureCapacity(uint newCapacity)
        {
            if (newCapacity > capacity)
            {
                var oldCapacity = capacity;
                capacity = (uint)(newCapacity * 1.5f);
                var tmpData = data;
                data = AllocArray(capacity);
                ZeroMemory(data, (uint)(sizeof(nint) * capacity));
                MemoryCopy(tmpData, data, capacity * sizeof(nint), oldCapacity * sizeof(nint));
                Free(tmpData);
            }
        }

        public void Add(Binding binding)
        {
            var index = bindings.Count;
            bindings.Add(binding);
            dict.Add(binding.Slot, index);
            Update();
        }

        public void Remove(Binding binding)
        {
            bindings.Remove(binding);
            Update();
        }

        public void Update()
        {
            startSlot = int.MaxValue;
            count = 0;
            ZeroMemory(data, (uint)(sizeof(nint) * capacity));
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];

                var slot = binding.Slot;

                EnsureCapacity(slot + 1);
                data[slot] = (void*)binding.Data;
                count = Math.Max(slot + 1, count);
                startSlot = Math.Min(slot, startSlot);
            }
        }

        public void Bind(IGraphicsContext context)
        {
            context.PSSetShaderResources(startSlot, count, data);
        }

        public void Unbind(IGraphicsContext context)
        {
            nint* temp = stackalloc nint[(int)count];
            context.PSSetShaderResources(startSlot, count, (void**)temp);
        }

        public bool Contains(Binding binding)
        {
            return bindings.Contains(binding);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                bindings.Clear();
                Free(data);
                count = 0;
                capacity = 0;
                data = null;

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

    public unsafe class BindingList<T> : IDisposable where T : IDeviceChild
    {
        private const uint DefaultCapacity = 4;
        private readonly List<Binding> bindings = new();
        private readonly Dictionary<uint, int> dict = new();
        private uint capacity;
        private uint count;
        private uint startSlot;
        private void** data;
        private bool disposedValue;

        public BindingList()
        {
            capacity = DefaultCapacity;
            data = AllocArray(capacity);
            ZeroMemory(data, (uint)(sizeof(nint) * capacity));
        }

        public int Count => bindings.Count;

        public uint StartSlot => startSlot;

        public uint SlotCount => count;

        public void** Data => data;

        public bool IsReadOnly => false;

        public Binding this[int index]
        {
            get
            {
                return bindings[index];
            }
            set
            {
                bindings[index] = value;
                Update();
            }
        }

        public nint this[uint slot]
        {
            get
            {
                return bindings[dict[slot]].Data;
            }
            set
            {
                var binding = bindings[dict[slot]];
                binding.Data = value;
                bindings[dict[slot]] = binding;
                Update();
            }
        }

        private void EnsureCapacity(uint newCapacity)
        {
            if (newCapacity > capacity)
            {
                var oldCapacity = capacity;
                capacity = (uint)(newCapacity * 1.5f);
                var tmpData = data;
                data = AllocArray(capacity);
                ZeroMemory(data, (uint)(sizeof(nint) * capacity));
                MemoryCopy(tmpData, data, capacity * sizeof(nint), oldCapacity * sizeof(nint));
                Free(tmpData);
            }
        }

        public void Add(Binding binding)
        {
            var index = bindings.Count;
            bindings.Add(binding);
            dict.Add(binding.Slot, index);
            Update();
        }

        public void Remove(Binding binding)
        {
            bindings.Remove(binding);
            Update();
        }

        public void Update()
        {
            startSlot = int.MaxValue;
            count = 0;
            ZeroMemory(data, (uint)(sizeof(nint) * capacity));
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];

                var slot = binding.Slot;

                EnsureCapacity(slot + 1);
                data[slot] = (void*)binding.Data;
                count = Math.Max(slot + 1, count);
                startSlot = Math.Min(slot, startSlot);
            }
        }

        public void Bind(IGraphicsContext context)
        {
            context.PSSetShaderResources(startSlot, count, data);
        }

        public void Unbind(IGraphicsContext context)
        {
            nint* temp = stackalloc nint[(int)count];
            context.PSSetShaderResources(startSlot, count, (void**)temp);
        }

        public bool Contains(Binding binding)
        {
            return bindings.Contains(binding);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                bindings.Clear();
                Free(data);
                count = 0;
                capacity = 0;
                data = null;

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
}