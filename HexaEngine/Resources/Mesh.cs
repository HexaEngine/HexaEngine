namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using System.Collections.Concurrent;

    public class Mesh : IDisposable
    {
        private readonly SemaphoreSlim semaphore = new(1);
        private readonly ConcurrentDictionary<string, ModelInstanceType> materialToType = new();
        private readonly List<ModelInstanceType> instanceTypes = new();
        private readonly string name;
        private bool disposedValue;
        public IBuffer? VB;
        public IBuffer? IB;
        public int VertexCount;
        public int IndexCount;
        public BoundingBox BoundingBox;

        public Mesh(IGraphicsDevice device, string name, Span<MeshVertex> vertices, Span<int> indices, BoundingBox box)
        {
            this.name = name;
            BoundingBox = box;
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

        public string Name => name;

        public bool IsUsed => instanceTypes.Count > 0;

        public void Update(IGraphicsDevice device, Span<MeshVertex> vertices, Span<int> indices, BoundingBox box)
        {
            semaphore.Wait();
            VB?.Dispose();
            VB = null;
            IB?.Dispose();
            IB = null;
            BoundingBox = box;
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

        public ModelInstanceType CreateInstanceType(IGraphicsDevice device, Material material)
        {
            semaphore.Wait();
            lock (materialToType)
            {
                if (!materialToType.TryGetValue(material.Name, out var type))
                {
                    type = new(device, this, material);
                    if (!materialToType.TryAdd(material.Name, type))
                        throw new Exception();
                    lock (instanceTypes)
                    {
                        instanceTypes.Add(type);
                    }
                }

                semaphore.Release();
                return type;
            }
        }

        public void DestroyInstanceType(ModelInstanceType type)
        {
            if (instanceTypes.Contains(type))
            {
                lock (instanceTypes)
                {
                    instanceTypes.Remove(type);
                }
                materialToType.Remove(type.Material.Name, out _);
                type.Dispose();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public unsafe void UpdateInstanceBuffer(IGraphicsContext context, BoundingFrustum frustum)
        {
            lock (instanceTypes)
            {
                for (int i = 0; i < instanceTypes.Count; i++)
                {
                    instanceTypes[i].UpdateInstanceBuffer(context, frustum);
                }
            }
        }

        public unsafe bool DrawAuto(IGraphicsContext context, IEffect effect)
        {
            if (VB == null) return false;
            if (IB == null) return false;
            if (semaphore.CurrentCount == 0) return false;

            effect.Draw(context);
            context.SetVertexBuffer(VB, sizeof(MeshVertex));
            context.SetIndexBuffer(IB, Format.R32UInt, 0);
            lock (instanceTypes)
            {
                for (int i = 0; i < instanceTypes.Count; i++)
                {
                    instanceTypes[i].DrawAuto(context, IndexCount);
                }
            }
            context.ClearState();
            return true;
        }

        public unsafe void DrawAuto(IGraphicsContext context, GraphicsPipeline pipeline, Viewport viewport)
        {
            if (VB == null) return;
            if (IB == null) return;
            if (semaphore.CurrentCount == 0) return;

            context.SetVertexBuffer(VB, sizeof(MeshVertex));
            context.SetIndexBuffer(IB, Format.R32UInt, 0);
            lock (instanceTypes)
            {
                for (int i = 0; i < instanceTypes.Count; i++)
                {
                    instanceTypes[i].DrawAuto(context, pipeline, viewport, IndexCount);
                }
            }
            context.ClearState();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                VB?.Dispose();
                IB?.Dispose();
                lock (instanceTypes)
                {
                    for (int i = 0; i < instanceTypes.Count; i++)
                    {
                        instanceTypes[i].Dispose();
                    }
                    instanceTypes.Clear();
                    materialToType.Clear();
                }
                disposedValue = true;
            }
        }

        ~Mesh()
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

        public override string ToString()
        {
            return name;
        }
    }
}