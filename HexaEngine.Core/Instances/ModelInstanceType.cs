namespace HexaEngine.Core.Instances
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using System.Collections.Concurrent;
    using System.Numerics;

    public class ModelInstanceType : IInstanceType
    {
        private readonly SemaphoreSlim semaphore = new(1);
        private readonly List<IInstance> instances = new();
        private readonly ConcurrentQueue<IInstance> addQueue = new();
        private readonly ConcurrentQueue<IInstance> removeQueue = new();
        private bool disposedValue;
        private DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> argBuffer;
        private StructuredUavBuffer<Matrix4x4> instanceBuffer;
        private StructuredUavBuffer<uint> instanceOffsets;
        private StructuredBuffer<Matrix4x4> frustumInstanceBuffer;
        private StructuredBuffer<Matrix4x4> noCullInstanceBuffer;
        private StructuredBuffer<uint> noCullInstanceOffsets;
        private ConstantBuffer<uint> zeroIdBuffer;
        private ConstantBuffer<uint> idBuffer;
        private uint argBufferOffset;
        private int idCounter;
        private int instanceCount;

        public readonly string Name;
        public readonly ResourceInstance<Mesh> Mesh;
        public readonly Material Material;

        public unsafe ModelInstanceType(ResourceInstance<Mesh> mesh, Material material)
        {
            Name = $"{mesh},{material}";
            Mesh = mesh;
            Material = material;
        }

        public int VertexCount => Mesh.Value?.VertexCount ?? 0;

        public int IndexCount => Mesh.Value?.IndexCount ?? 0;

        public int Visible => (int)frustumInstanceBuffer.Count;

        public int Count => instanceCount;

        public bool IsEmpty => instanceCount == 0;

        public IBuffer ArgBuffer => argBuffer.Buffer;

        public uint ArgBufferOffset => argBufferOffset;

        public IReadOnlyList<IInstance> Instances => instances;

        public bool Forward => ((Material.Shader?.Value?.Flags ?? MaterialShaderFlags.None) & MaterialShaderFlags.Forward) != 0;

        public event Action<IInstanceType, IInstance>? Updated;

        public void Initialize(IGraphicsDevice device, DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> argsBuffer, StructuredUavBuffer<Matrix4x4> instanceBuffer, StructuredUavBuffer<uint> instanceOffsets, StructuredBuffer<Matrix4x4> noCullInstanceBuffer, StructuredBuffer<uint> noCullInstanceOffsets)
        {
            argBuffer = argsBuffer;
            this.instanceBuffer = instanceBuffer;
            this.instanceOffsets = instanceOffsets;
            this.noCullInstanceBuffer = noCullInstanceBuffer;
            this.noCullInstanceOffsets = noCullInstanceOffsets;
            frustumInstanceBuffer = new(device, CpuAccessFlags.Write);
            zeroIdBuffer = new(device, 4, CpuAccessFlags.None);
            idBuffer = new(device, 4, CpuAccessFlags.Write);
        }

        public IInstance CreateInstance(GameObject parent)
        {
            var value = Interlocked.Increment(ref idCounter);
            ModelInstance instance = new(value, this, parent);
            addQueue.Enqueue(instance);
            Interlocked.Increment(ref instanceCount);
            return instance;
        }

        public void DestroyInstance(IInstance instance)
        {
            Interlocked.Decrement(ref instanceCount);
            removeQueue.Enqueue(instance);
            instance.Dispose();
        }

        public unsafe int UpdateInstanceBuffer(uint id, StructuredBuffer<Matrix4x4> noCullBuffer, StructuredBuffer<InstanceData> buffer, StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> drawArgs, BoundingFrustum frustum, bool doCulling)
        {
            int count = 0;

            while (addQueue.TryDequeue(out var instance))
            {
                instance.Updated += InstanceUpdated;
                instances.Add(instance);
            }

            while (removeQueue.TryDequeue(out var instance))
            {
                instance.Updated -= InstanceUpdated;
                instances.Remove(instance);
            }

            idBuffer[0] = id;

            argBufferOffset = (uint)(id * sizeof(DrawIndexedInstancedIndirectArgs));
            frustumInstanceBuffer.ResetCounter();

            for (int i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];
                var transform = Matrix4x4.Transpose(instance.Transform);
                noCullBuffer.Add(transform);
                instance.GetBoundingBox(out var box);
                instance.GetBoundingSphere(out var sphere);
                if (!doCulling || frustum.Intersects(box))
                {
                    buffer.Add(new(id, transform, box.Min, box.Max, sphere.Center, sphere.Radius));
                    frustumInstanceBuffer.Add(transform);
                }
            }

            drawArgs.Add(new() { IndexCountPerInstance = (uint)(Mesh.Value?.IndexCount ?? 0) });

            return count;
        }

        private void InstanceUpdated(ModelInstance obj)
        {
            Updated?.Invoke(this, obj);
        }

        public unsafe int UpdateFrustumInstanceBuffer(BoundingFrustum frustum)
        {
            frustumInstanceBuffer.ResetCounter();

            int count = 0;

            for (int i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];
                instance.GetBoundingBox(out var box);
                if (frustum.Intersects(box))
                {
                    var world = Matrix4x4.Transpose(instance.Transform);
                    frustumInstanceBuffer.Add(world);
                }
            }

            return count;
        }

        public unsafe int UpdateFrustumInstanceBuffer(BoundingFrustum[] frusta)
        {
            frustumInstanceBuffer.ResetCounter();

            int count = 0;

            for (int i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];
                instance.GetBoundingBox(out var box);
                for (int j = 0; j < frusta.Length; j++)
                {
                    if (frusta[j].Intersects(box))
                    {
                        var world = Matrix4x4.Transpose(instance.Transform);
                        frustumInstanceBuffer.Add(world);
                        break;
                    }
                }
            }

            return count;
        }

        public unsafe int UpdateFrustumInstanceBuffer(BoundingBox viewBox)
        {
            frustumInstanceBuffer.ResetCounter();

            int count = 0;

            for (int i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];
                instance.GetBoundingBox(out var box);

                if (viewBox.Intersects(box))
                {
                    var world = Matrix4x4.Transpose(instance.Transform);
                    frustumInstanceBuffer.Add(world);
                }
            }

            return count;
        }

        public unsafe bool BeginDraw(IGraphicsContext context)
        {
            if (Mesh.Value == null) return false;
            if (Mesh.Value.VB == null) return false;
            if (Mesh.Value.IB == null) return false;
            if (Material == null) return false;
            if (semaphore.CurrentCount == 0) return false;
            if (!Material.Bind(context)) return false;

            idBuffer.Update(context);
            context.SetVertexBuffer(0, Mesh.Value.VB, Mesh.Value.Stride);
            context.SetIndexBuffer(Mesh.Value.IB, Format.R32UInt, 0);
            context.VSSetShaderResource(instanceBuffer.SRV, 0);
            context.VSSetShaderResource(instanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(idBuffer.Buffer, 0);

            return true;
        }

        public bool BeginDrawForward(IGraphicsContext context)
        {
            if (Mesh.Value == null) return false;
            if (Mesh.Value.VB == null) return false;
            if (Mesh.Value.IB == null) return false;
            if (Material == null) return false;
            if (semaphore.CurrentCount == 0) return false;
            if (!Material.Bind(context)) return false;

            idBuffer.Update(context);
            context.SetVertexBuffer(0, Mesh.Value.VB, Mesh.Value.Stride);
            context.SetIndexBuffer(Mesh.Value.IB, Format.R32UInt, 0);
            context.VSSetShaderResource(instanceBuffer.SRV, 0);
            context.VSSetShaderResource(instanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(idBuffer.Buffer, 0);
            Material.Shader.Value.BeginDrawForward(context);

            return true;
        }

        public unsafe bool BeginDrawNoOcculusion(IGraphicsContext context)
        {
            if (Mesh.Value == null) return false;
            if (Mesh.Value.VB == null) return false;
            if (Mesh.Value.IB == null) return false;
            if (Material == null) return false;
            if (semaphore.CurrentCount == 0) return false;
            if (!Material.Bind(context)) return false;

            frustumInstanceBuffer.Update(context);
            context.SetVertexBuffer(0, Mesh.Value.VB, Mesh.Value.Stride);
            context.SetIndexBuffer(Mesh.Value.IB, Format.R32UInt, 0);
            context.VSSetShaderResource(frustumInstanceBuffer.SRV, 0);
            context.VSSetShaderResource(instanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(zeroIdBuffer.Buffer, 0);

            return true;
        }

        public unsafe bool BeginDrawNoCulling(IGraphicsContext context)
        {
            if (Mesh.Value == null) return false;
            if (Mesh.Value.VB == null) return false;
            if (Mesh.Value.IB == null) return false;
            if (Material == null) return false;
            if (semaphore.CurrentCount == 0) return false;
            if (!Material.Bind(context)) return false;

            noCullInstanceBuffer.Update(context);
            noCullInstanceOffsets.Update(context);
            context.SetVertexBuffer(0, Mesh.Value.VB, Mesh.Value.Stride);
            context.SetIndexBuffer(Mesh.Value.IB, Format.R32UInt, 0);
            context.VSSetShaderResource(noCullInstanceBuffer.SRV, 0);
            context.VSSetShaderResource(noCullInstanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(idBuffer.Buffer, 0);

            return true;
        }

        public void DrawShadow(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (Mesh.Value == null) return;
            if (Mesh.Value.VB == null) return;
            if (Mesh.Value.IB == null) return;
            if (Material == null) return;
            if (semaphore.CurrentCount == 0) return;
            if (!Material.Bind(context)) return;

            frustumInstanceBuffer.Update(context);
            context.SetVertexBuffer(0, Mesh.Value.VB, Mesh.Value.Stride);
            context.SetIndexBuffer(Mesh.Value.IB, Format.R32UInt, 0);
            context.VSSetShaderResource(frustumInstanceBuffer.SRV, 0);
            context.VSSetShaderResource(instanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(zeroIdBuffer.Buffer, 0);
            Material.DrawShadow(context, light, type, (uint)IndexCount, (uint)Visible);
        }

        public unsafe void DrawDepth(IGraphicsContext context, IBuffer camera)
        {
            if (Mesh.Value == null) return;
            if (Mesh.Value.VB == null) return;
            if (Mesh.Value.IB == null) return;
            if (Material == null) return;
            if (semaphore.CurrentCount == 0) return;
            if (!Material.Bind(context)) return;

            idBuffer.Update(context);
            context.SetVertexBuffer(0, Mesh.Value.VB, Mesh.Value.Stride);
            context.SetIndexBuffer(Mesh.Value.IB, Format.R32UInt, 0);
            context.VSSetShaderResource(instanceBuffer.SRV, 0);
            context.VSSetShaderResource(instanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(idBuffer.Buffer, 0);
            Material.DrawDepth(context, camera, (uint)IndexCount, (uint)Visible);
        }

        public unsafe void Draw(IGraphicsContext context, IBuffer camera)
        {
            if (Mesh.Value == null) return;
            if (Mesh.Value.VB == null) return;
            if (Mesh.Value.IB == null) return;
            if (Material == null) return;
            if (semaphore.CurrentCount == 0) return;
            if (!Material.Bind(context)) return;

            idBuffer.Update(context);
            context.SetVertexBuffer(0, Mesh.Value.VB, Mesh.Value.Stride);
            context.SetIndexBuffer(Mesh.Value.IB, Format.R32UInt, 0);
            context.VSSetShaderResource(instanceBuffer.SRV, 0);
            context.VSSetShaderResource(instanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(idBuffer.Buffer, 0);
            Material.DrawIndirect(context, camera, ArgBuffer, ArgBufferOffset);
        }

        public void DrawNoOcclusion(IGraphicsContext context)
        {
            if (BeginDrawNoOcculusion(context))
                context.DrawIndexedInstanced((uint)IndexCount, (uint)Visible, 0, 0, 0);
        }

        public bool Equals(IInstanceType? other)
        {
            if (other == null) return false;
            if (other is not ModelInstanceType type) return false;
            return type.Mesh.Name == Mesh.Name && type.Material.Name == Material.Name;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                frustumInstanceBuffer.Dispose();
                zeroIdBuffer.Dispose();
                idBuffer.Dispose();
                Mesh.Value?.DestroyInstanceType(this);
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