namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Culling;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System.Collections.Concurrent;

    public class Mesh : IDisposable
    {
        private readonly ConcurrentDictionary<string, ModelInstanceType> materialToType = new();
        private readonly List<ModelInstanceType> instanceTypes = new();
        private readonly IGraphicsDevice device;
        private readonly string name;
        private bool disposedValue;
        public IBuffer? VB;
        public IBuffer? IB;
        public int VertexCount;
        public int IndexCount;
        public BoundingBox BoundingBox;
        public BoundingSphere BoundingSphere;

        public unsafe Mesh(IGraphicsDevice device, string name, MeshVertex[] vertices, uint[] indices, BoundingBox box, BoundingSphere boundingSphere)
        {
            this.device = device;
            this.name = name;
            BoundingBox = box;
            BoundingSphere = boundingSphere;
            if (vertices.Length != 0)
            {
                fixed (MeshVertex* ptr = vertices)
                {
                    VB = device.CreateBuffer(ptr, (uint)vertices.Length, BindFlags.VertexBuffer, Usage.Immutable);
                }

                VertexCount = vertices.Length;
            }
            if (indices.Length != 0)
            {
                fixed (uint* ptr = indices)
                {
                    IB = device.CreateBuffer(ptr, (uint)indices.Length, BindFlags.IndexBuffer, Usage.Immutable);
                }
                IndexCount = indices.Length;
            }
        }

        public unsafe Mesh(IGraphicsDevice device, MeshData data)
        {
            this.device = device;
            name = data.Name;
            var vertices = data.Vertices;
            var indices = data.Indices;
            BoundingBox = data.Box;
            BoundingSphere = data.Sphere;
            if (vertices.Length != 0)
            {
                fixed (MeshVertex* ptr = vertices)
                {
                    VB = device.CreateBuffer(ptr, (uint)vertices.Length, BindFlags.VertexBuffer, Usage.Immutable);
                }

                VertexCount = vertices.Length;
            }
            if (indices.Length != 0)
            {
                fixed (uint* ptr = indices)
                {
                    IB = device.CreateBuffer(ptr, (uint)indices.Length, BindFlags.IndexBuffer, Usage.Immutable);
                }
                IndexCount = indices.Length;
            }
        }

        public string Name => name;

        public bool IsUsed => instanceTypes.Count > 0;

        internal ModelInstanceType CreateInstanceType(ResourceInstance<Mesh> mesh, Material material)
        {
            lock (materialToType)
            {
                lock (instanceTypes)
                {
                    if (!materialToType.TryGetValue(material.Name, out var type))
                    {
                        type = new(mesh, material);
                        type.Initialize(device, CullingManager.DrawIndirectArgs, CullingManager.InstanceDataOutBuffer, CullingManager.InstanceOffsets, CullingManager.InstanceDataNoCull, CullingManager.InstanceOffsetsNoCull);
                        if (!materialToType.TryAdd(material.Name, type))
                            throw new InvalidOperationException();

                        instanceTypes.Add(type);
                    }

                    return type;
                }
            }
        }

        internal void DestroyInstanceType(ModelInstanceType type)
        {
            lock (instanceTypes)
            {
                if (instanceTypes.Contains(type))
                {
                    instanceTypes.Remove(type);
                    materialToType.Remove(type.Material.Name, out _);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
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
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return name;
        }
    }
}