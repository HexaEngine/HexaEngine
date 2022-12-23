namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using System.Collections.Concurrent;
    using System.Numerics;

    public class ModelInstanceType : IDisposable
    {
        private readonly SemaphoreSlim semaphore = new(1);
        private readonly List<ModelInstance> instances = new();
        private readonly ConcurrentQueue<ModelInstance> addQueue = new();
        private Matrix4x4[] transforms = Array.Empty<Matrix4x4>();
        private bool disposedValue;
        private const int CapacityStepSize = 64;
        private IBuffer? ISB;
        private int visibleCount;
        private int instanceCapacity;
        private int idCounter;

        public readonly string Name;
        public readonly Mesh Mesh;
        public readonly Material Material;

        public ModelInstanceType(IGraphicsDevice device, Mesh mesh, Material material)
        {
            Name = $"{mesh},{material}";
            Mesh = mesh;
            Material = material;
            instanceCapacity = CapacityStepSize;
            transforms = new Matrix4x4[instanceCapacity];
            ISB = device.CreateBuffer(transforms, BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
        }

        public int Count => instances.Count;

        public int Capacity => instanceCapacity;

        public bool IsEmpty => instances.Count == 0;

        public void EnsureCapacity(IGraphicsDevice device, int capacity)
        {
            if (capacity == 0) return;
            if (instanceCapacity < capacity)
            {
                var newCapacity = instanceCapacity;
                while (newCapacity < capacity)
                {
                    newCapacity += CapacityStepSize;
                }

                var before = ISB;
                ISB = null;
                before?.Dispose();

                transforms = new Matrix4x4[newCapacity];
                ISB = device.CreateBuffer(transforms, BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                instanceCapacity = newCapacity;
            }
            else if (instanceCapacity > capacity && capacity > 0)
            {
                int currentStep = instanceCapacity / CapacityStepSize;
                int capaStep = (int)Math.Ceiling(capacity / (float)CapacityStepSize);

                if (capaStep < currentStep)
                {
                    var newCapacity = capaStep * CapacityStepSize;

                    var before = ISB;
                    ISB = null;
                    before?.Dispose();

                    transforms = new Matrix4x4[newCapacity];
                    ISB = device.CreateBuffer(transforms, BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                    instanceCapacity = newCapacity;
                }
            }
        }

        public ModelInstance CreateInstance(IGraphicsDevice device, Transform node)
        {
            var value = Interlocked.Increment(ref idCounter);
            ModelInstance instance = new(value, this, node);
            addQueue.Enqueue(instance);
            return instance;
        }

        public void DestroyInstance(IGraphicsDevice device, ModelInstance instance)
        {
            semaphore.Wait();
            instances.Remove(instance);
            EnsureCapacity(device, instances.Count);
            semaphore.Release();
        }

        public unsafe int UpdateInstanceBuffer(IGraphicsContext context, BoundingFrustum frustum)
        {
            if (ISB == null) return -1;
            int count = 0;

            while (addQueue.TryDequeue(out var modelInstance))
            {
                instances.Add(modelInstance);
                EnsureCapacity(context.Device, instances.Count);
            }

            for (int i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];
                if (instance.VisibilityTest(frustum))
                {
                    transforms[count++] = instances[i].Transform;
                }
            }

            fixed (void* p = transforms)
            {
                context.Write(ISB, p, sizeof(Matrix4x4) * count);
            }
            visibleCount = count;
            return count;
        }

        public unsafe bool DrawAuto(IGraphicsContext context, int indexCount)
        {
            if (visibleCount == 0) return false;
            if (Material == null) return false;
            if (ISB == null) return false;
            if (semaphore.CurrentCount == 0) return false;

            if (!Material.Bind(context)) return false;
            context.SetVertexBuffer(1, ISB, sizeof(Matrix4x4), 0);
            context.DrawIndexedInstanced(indexCount, visibleCount, 0, 0, 0);
            return true;
        }

        public unsafe void DrawAuto(IGraphicsContext context, GraphicsPipeline pipeline, Viewport viewport, int indexCount)
        {
            if (visibleCount == 0) return;
            if (Material == null) return;
            if (ISB == null) return;
            if (semaphore.CurrentCount == 0) return;

            if (!Material.Bind(context)) return;
            context.SetVertexBuffer(1, ISB, sizeof(Matrix4x4), 0);
            pipeline.DrawIndexedInstanced(context, viewport, indexCount, visibleCount, 0, 0, 0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                ISB?.Dispose();
                disposedValue = true;
            }
        }

        ~ModelInstanceType()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}