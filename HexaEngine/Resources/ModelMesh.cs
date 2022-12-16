namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes;
    using System.Numerics;
    using System.Reflection;

    public class ModelMesh : IDisposable
    {
        private readonly SemaphoreSlim semaphore = new(1);
        private readonly List<ModelInstance> Instances = new();
        private Matrix4x4[] transforms = Array.Empty<Matrix4x4>();
        private bool disposedValue;
        private const int CapacityStepSize = 64;
        private IBuffer? VB;
        private IBuffer? IB;
        private IBuffer? ISB;
        private int VertexCount;
        private int IndexCount;
        private int instanceCount;

        public ModelMaterial? Material;

        private int instanceCapacity;

        public BoundingBox AABB;

        public ModelMesh(IGraphicsDevice device, Span<MeshVertex> vertices, Span<int> indices, BoundingBox box)
        {
            AABB = box;
            if (!vertices.IsEmpty)
            {
                VB = device.CreateBuffer(vertices, BindFlags.VertexBuffer, Usage.Immutable);
                VertexCount = vertices.Length;
            }
            if (!indices.IsEmpty)
            {
                IB = device.CreateBuffer(indices, BindFlags.IndexBuffer, Usage.Immutable);
                IndexCount = indices.Length;
            }

            instanceCount = 0;
            instanceCapacity = CapacityStepSize;
            transforms = new Matrix4x4[instanceCapacity];
            ISB = device.CreateBuffer(transforms, BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
        }

        public int InstanceCapacity => instanceCapacity;

        public int InstanceCount => instanceCount;

        public void Update(IGraphicsDevice device, Span<MeshVertex> vertices, Span<int> indices, BoundingBox box)
        {
            semaphore.Wait();
            VB?.Dispose();
            VB = null;
            IB?.Dispose();
            IB = null;
            AABB = box;
            semaphore.Release();
            if (!vertices.IsEmpty)
            {
                VB = device.CreateBuffer(vertices, BindFlags.VertexBuffer, Usage.Immutable);
                VertexCount = vertices.Length;
            }
            if (!indices.IsEmpty)
            {
                IB = device.CreateBuffer(indices, BindFlags.IndexBuffer, Usage.Immutable);
                IndexCount = indices.Length;
            }
        }

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
                semaphore.Wait();
                var before = ISB;
                ISB = null;
                before?.Dispose();
                semaphore.Release();
                transforms = new Matrix4x4[newCapacity];
                ISB = device.CreateBuffer(transforms, BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                instanceCapacity = newCapacity;
            }
            else if (instanceCapacity > capacity)
            {
                int currentStep = instanceCapacity / CapacityStepSize;
                int capaStep = (int)Math.Ceiling(capacity / (float)CapacityStepSize);

                if (capaStep < currentStep)
                {
                    var newCapacity = capaStep * CapacityStepSize;
                    semaphore.Wait();
                    var before = ISB;
                    ISB = null;
                    before?.Dispose();
                    semaphore.Release();
                    transforms = new Matrix4x4[newCapacity];
                    ISB = device.CreateBuffer(transforms, BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                    instanceCapacity = newCapacity;
                }
            }
        }

        public ModelInstance CreateInstance(IGraphicsDevice device, GameObject node)
        {
            ModelInstance instance = new(this, node);
            Instances.Add(instance);
            instanceCount++;
            EnsureCapacity(device, instanceCount);
            return instance;
        }

        public void DestroyInstance(IGraphicsDevice device, ModelInstance instance)
        {
            Instances.Remove(instance);
            instanceCount--;
            EnsureCapacity(device, instanceCount);
        }

        public unsafe int UpdateInstanceBuffer(IGraphicsContext context)
        {
            if (ISB == null) return -1;
            int count = 0;
            for (int i = 0; i < instanceCount; i++)
            {
                transforms[i] = Instances[i].Transform;
                count++;
            }

            fixed (void* p = transforms)
            {
                context.Write(ISB, p, sizeof(Matrix4x4) * instanceCount);
            }
            return count;
        }

        public unsafe bool DrawAuto(IGraphicsContext context, IEffect effect)
        {
            if (VB == null) return false;
            if (IB == null) return false;
            if (Material == null) return false;
            if (ISB == null) return false;
            if (semaphore.CurrentCount == 0) return false;

            effect.Draw(context);
            Material.Bind(context);
            context.SetVertexBuffer(VB, sizeof(MeshVertex));
            context.SetVertexBuffer(1, ISB, sizeof(Matrix4x4), 0);
            context.SetIndexBuffer(IB, Format.R32UInt, 0);
            context.DrawIndexedInstanced(IndexCount, instanceCount, 0, 0, 0);
            context.ClearState();
            return true;
        }

        public unsafe void DrawAuto(IGraphicsContext context, GraphicsPipeline pipeline, Viewport viewport)
        {
            if (VB == null) return;
            if (IB == null) return;
            if (Material == null) return;
            if (ISB == null) return;
            if (semaphore.CurrentCount == 0) return;

            Material.Bind(context);
            context.SetVertexBuffer(VB, sizeof(MeshVertex));
            context.SetVertexBuffer(1, ISB, sizeof(Matrix4x4), 0);
            context.SetIndexBuffer(IB, Format.R32UInt, 0);
            pipeline.DrawIndexedInstanced(context, viewport, IndexCount, instanceCount, 0, 0, 0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                VB?.Dispose();
                IB?.Dispose();
                ISB?.Dispose();
                disposedValue = true;
            }
        }

        ~ModelMesh()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class ModelInstances
    {
        private readonly List<GameObject> Instances = new();
        private readonly ModelMaterial Material;

        private Matrix4x4[] transforms = Array.Empty<Matrix4x4>();
        private IBuffer instanceBuffer;

        private const int CapacityStepSize = 64;
        private int instanceCapacity;
        private int instanceCount;

        public ModelInstances(IGraphicsDevice device, ModelMaterial material)
        {
            Material = material;

            instanceCount = 0;
            instanceCapacity = CapacityStepSize;
            transforms = new Matrix4x4[instanceCapacity];
            instanceBuffer = device.CreateBuffer(transforms, BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
        }

        public int InstanceCapacity => instanceCapacity;

        public int InstanceCount => instanceCount;

        public void EnsureCapacity(IGraphicsDevice device, int capacity)
        {
            if (instanceCapacity < capacity)
            {
                var newCapacity = instanceCapacity;
                while (newCapacity < capacity)
                {
                    newCapacity += CapacityStepSize;
                }

                instanceBuffer?.Dispose();
                instanceBuffer = null;
                transforms = new Matrix4x4[newCapacity];
                instanceBuffer = device.CreateBuffer(transforms, BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                instanceCapacity = newCapacity;
            }
            else if (instanceCapacity > capacity)
            {
                int currentStep = instanceCapacity / CapacityStepSize;
                int capaStep = (int)Math.Ceiling(capacity / (float)CapacityStepSize);

                if (capaStep < currentStep)
                {
                    var newCapacity = capaStep * CapacityStepSize;
                    instanceBuffer?.Dispose();
                    instanceBuffer = null;
                    transforms = new Matrix4x4[newCapacity];
                    instanceBuffer = device.CreateBuffer(transforms, BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                    instanceCapacity = newCapacity;
                }
            }
        }

        public void Add(IGraphicsDevice device, GameObject node)
        {
            Instances.Add(node);
            instanceCount++;
            EnsureCapacity(device, instanceCount);
        }

        public void Remove(IGraphicsDevice device, GameObject node)
        {
            Instances.Remove(node);
            instanceCount--;
            EnsureCapacity(device, instanceCount);
        }

        public void Update(IGraphicsContext context)
        {
            if (instanceBuffer == null) return;
            for (int i = 0; i < instanceCount; i++)
            {
                transforms[i] = Instances[i].Transform;
            }
            context.Write(instanceBuffer, transforms);
        }

        public void Draw(IGraphicsContext context, int indexCount, GraphicsPipeline pipeline, Viewport viewport)
        {
            Update(context);
            Material.Bind(context);
            pipeline.DrawIndexedInstanced(context, viewport, indexCount, instanceCount, 0, 0, 0);
            context.ClearState();
        }
    }
}