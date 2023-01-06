namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using System.Collections.Concurrent;
    using System.Numerics;

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
        public BoundingSphere BoundingSphere;

        public unsafe Mesh(IGraphicsDevice device, string name, MeshVertex* vertices, uint verticesCount, int* indices, uint indicesCount, BoundingBox box, BoundingSphere boundingSphere)
        {
            this.name = name;
            BoundingBox = box;
            BoundingSphere = boundingSphere;
            if (verticesCount != 0)
            {
                VB = device.CreateBuffer(vertices, verticesCount, BindFlags.VertexBuffer, Usage.Immutable);
                VertexCount = (int)verticesCount;
            }
            if (indicesCount != 0)
            {
                IB = device.CreateBuffer(indices, indicesCount, BindFlags.IndexBuffer, Usage.Immutable);
                IndexCount = (int)indicesCount;
            }
        }

        public string Name => name;

        public bool IsUsed => instanceTypes.Count > 0;

        internal ModelInstanceType CreateInstanceType(IGraphicsDevice device, DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> argsBuffer, StructuredUavBuffer<Matrix4x4> instanceBuffer, StructuredUavBuffer<uint> instanceOffsets, StructuredBuffer<Matrix4x4> noCullInstanceBuffer, StructuredBuffer<uint> noCullInstanceOffsets, Material material)
        {
            semaphore.Wait();
            lock (materialToType)
            {
                if (!materialToType.TryGetValue(material.Name, out var type))
                {
                    type = new(device, argsBuffer, instanceBuffer, instanceOffsets, noCullInstanceBuffer, noCullInstanceOffsets, this, material);
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

        internal void DestroyInstanceType(ModelInstanceType type)
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

        public unsafe bool DrawAuto(IGraphicsContext context, IEffect effect)
        {
            if (VB == null) return false;
            if (IB == null) return false;
            if (semaphore.CurrentCount == 0) return false;

            effect.Draw(context);
            context.SetVertexBuffer(VB, (uint)sizeof(MeshVertex));
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

            context.SetVertexBuffer(VB, (uint)sizeof(MeshVertex));
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