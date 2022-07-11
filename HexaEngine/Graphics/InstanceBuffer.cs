namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;

    public unsafe class InstanceBuffer
    {
        private bool isDirty;
        private List<InstanceData> instances;
        private int count;
        private IBuffer instanceBuffer;
        private bool disposedValue;

        public InstanceBuffer()
        {
            instances = new List<InstanceData>();
            isDirty = true;
        }

        public InstanceBuffer(IGraphicsDevice device, IEnumerable<InstanceData> instances)
        {
            this.instances = new List<InstanceData>(instances);
            ResizeBuffers(device);
            UpdateBuffers(device.Context);
        }

        public InstanceBuffer(IGraphicsDevice device, int capacity)
        {
            instances = new List<InstanceData>(capacity);
            ResizeBuffers(device);
        }

        public int Count => count;

        public int Capacity { get; private set; }

        public InstanceData this[int index]
        {
            get { return instances[index]; }
            set
            {
                instances[index] = value;
                isDirty = true;
            }
        }

        private unsafe void ResizeBuffers(IGraphicsDevice device)
        {
            if (instances.Count <= Capacity)
            {
                return;
            }
            instanceBuffer?.Dispose();
            instanceBuffer = device.CreateBuffer(instances.ToArray(), BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            Capacity = instances.Count;
        }

        private unsafe void UpdateBuffers(IGraphicsContext context)
        {
            context.Write(instanceBuffer, instances.ToArray());
            count = instances.Count;
        }

        public InstanceSlot Add(InstanceData instance)
        {
            int index = instances.Count;
            instances.Add(instance);
            isDirty = true;
            return new() { ID = index, Data = instance };
        }

        public void AddRange(params InstanceData[] instances)
        {
            this.instances.AddRange(instances);
            isDirty = true;
        }

        public void Clear()
        {
            instances.Clear();
            isDirty = true;
        }

        public void Bind(IGraphicsContext context)
        {
            Bind(context, 0);
        }

        public void FlushMemory(IGraphicsContext context)
        {
            ResizeBuffers(context.Device);
            UpdateBuffers(context);
            instances.Clear();
            instances = null;
            isDirty = false;
        }

        public void Bind(IGraphicsContext context, int slot)
        {
            Bind(context, slot, 0);
        }

        public void Bind(IGraphicsContext context, int slot, int offset)
        {
            if (isDirty)
            {
                ResizeBuffers(context.Device);
                UpdateBuffers(context);

                isDirty = false;
            }

            context.SetVertexBuffer(slot, instanceBuffer, sizeof(InstanceData), offset);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                instanceBuffer?.Dispose();
                instanceBuffer = null;

                disposedValue = true;
            }
        }

        ~InstanceBuffer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}