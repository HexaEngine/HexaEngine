namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Mathematics;
    using System.Collections.Concurrent;
    using System.Numerics;

    public class ModelInstanceType : IDisposable
    {
        private readonly SemaphoreSlim semaphore = new(1);
        private readonly List<ModelInstance> instances = new();
        private readonly ConcurrentQueue<ModelInstance> addQueue = new();
        private readonly ConcurrentQueue<ModelInstance> removeQueue = new();
        private bool disposedValue;
        private readonly DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> argBuffer;
        private readonly StructuredUavBuffer<Matrix4x4> instanceBuffer;
        private readonly StructuredUavBuffer<uint> instanceOffsets;
        private readonly StructuredBuffer<Matrix4x4> frustumInstanceBuffer;
        private readonly StructuredBuffer<Matrix4x4> noCullInstanceBuffer;
        private readonly StructuredBuffer<uint> noCullInstanceOffsets;
        private readonly ConstantBuffer<uint> zeroIdBuffer;
        private readonly ConstantBuffer<uint> idBuffer;
        private uint argBufferOffset;
        private int idCounter;

        public readonly string Name;
        public readonly Mesh Mesh;
        public readonly Material Material;

        public unsafe ModelInstanceType(IGraphicsDevice device, DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> argsBuffer, StructuredUavBuffer<Matrix4x4> instanceBuffer, StructuredUavBuffer<uint> instanceOffsets, StructuredBuffer<Matrix4x4> noCullInstanceBuffer, StructuredBuffer<uint> noCullInstanceOffsets, Mesh mesh, Material material)
        {
            Name = $"{mesh},{material}";
            Mesh = mesh;
            Material = material;
            argBuffer = argsBuffer;
            this.instanceBuffer = instanceBuffer;
            this.instanceOffsets = instanceOffsets;
            this.noCullInstanceBuffer = noCullInstanceBuffer;
            this.noCullInstanceOffsets = noCullInstanceOffsets;
            frustumInstanceBuffer = new(device, CpuAccessFlags.Write);
            zeroIdBuffer = new(device, 4, CpuAccessFlags.None);
            idBuffer = new(device, 4, CpuAccessFlags.Write);
        }

        public int VertexCount => Mesh.VertexCount;

        public int IndexCount => Mesh.IndexCount;

        public int Visible => (int)frustumInstanceBuffer.Count;

        public int Count => instances.Count;

        public bool IsEmpty => instances.Count == 0;

        public IBuffer ArgBuffer => argBuffer.Buffer;

        public uint ArgBufferOffset => argBufferOffset;

        public IReadOnlyList<ModelInstance> Instances => instances;

        public event Action<ModelInstanceType, ModelInstance> Updated;

        public ModelInstance CreateInstance(GameObject parent)
        {
            var value = Interlocked.Increment(ref idCounter);
            ModelInstance instance = new(value, this, parent);
            addQueue.Enqueue(instance);
            return instance;
        }

        public void DestroyInstance(ModelInstance instance)
        {
            removeQueue.Enqueue(instance);
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

            drawArgs.Add(new() { IndexCountPerInstance = (uint)Mesh.IndexCount });

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
                    break;
                }
            }

            return count;
        }

        public unsafe bool BeginDrawNoOcculusion(IGraphicsContext context)
        {
            if (Mesh.VB == null) return false;
            if (Mesh.IB == null) return false;
            if (Material == null) return false;
            if (semaphore.CurrentCount == 0) return false;
            if (!Material.Bind(context)) return false;

            frustumInstanceBuffer.Update(context);
            context.SetVertexBuffer(0, Mesh.VB, (uint)sizeof(MeshVertex));
            context.SetIndexBuffer(Mesh.IB, Format.R32UInt, 0);
            context.VSSetShaderResource(frustumInstanceBuffer.SRV, 0);
            context.VSSetShaderResource(instanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(zeroIdBuffer.Buffer, 0);

            return true;
        }

        public unsafe bool BeginDrawNoCulling(IGraphicsContext context)
        {
            if (Mesh.VB == null) return false;
            if (Mesh.IB == null) return false;
            if (Material == null) return false;
            if (semaphore.CurrentCount == 0) return false;
            if (!Material.Bind(context)) return false;

            noCullInstanceBuffer.Update(context);
            noCullInstanceOffsets.Update(context);
            context.SetVertexBuffer(0, Mesh.VB, (uint)sizeof(MeshVertex));
            context.SetIndexBuffer(Mesh.IB, Format.R32UInt, 0);
            context.VSSetShaderResource(noCullInstanceBuffer.SRV, 0);
            context.VSSetShaderResource(noCullInstanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(idBuffer.Buffer, 0);

            return true;
        }

        public unsafe bool BeginDraw(IGraphicsContext context)
        {
            if (Mesh.VB == null) return false;
            if (Mesh.IB == null) return false;
            if (Material == null) return false;
            if (semaphore.CurrentCount == 0) return false;
            if (!Material.Bind(context)) return false;

            idBuffer.Update(context);
            context.SetVertexBuffer(0, Mesh.VB, (uint)sizeof(MeshVertex));
            context.SetIndexBuffer(Mesh.IB, Format.R32UInt, 0);
            context.VSSetShaderResource(instanceBuffer.SRV, 0);
            context.VSSetShaderResource(instanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(idBuffer.Buffer, 0);

            return true;
        }

        public void Draw(IGraphicsContext context)
        {
            if (BeginDraw(context))
                context.DrawIndexedInstancedIndirect(ArgBuffer, ArgBufferOffset);
        }

        public void DrawNoOcclusion(IGraphicsContext context)
        {
            if (BeginDrawNoOcculusion(context))
                context.DrawIndexedInstanced((uint)IndexCount, (uint)Visible, 0, 0, 0);
        }

        public unsafe bool DrawAuto(IGraphicsContext context, int indexCount)
        {
            if (Material == null) return false;
            if (semaphore.CurrentCount == 0) return false;
            if (!Material.Bind(context)) return false;

            frustumInstanceBuffer.Update(context);
            idBuffer.Update(context);
            context.VSSetShaderResource(instanceBuffer.SRV, 0);
            context.VSSetShaderResource(instanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(idBuffer.Buffer, 0);
            context.DrawIndexedInstancedIndirect(argBuffer.Buffer, argBufferOffset);
            return true;
        }

        public unsafe void DrawAuto(IGraphicsContext context, IGraphicsPipeline pipeline, Viewport viewport, int indexCount)
        {
            if (Material == null) return;
            if (semaphore.CurrentCount == 0) return;
            if (!Material.Bind(context)) return;

            frustumInstanceBuffer.Update(context);
            idBuffer.Update(context);
            context.VSSetShaderResource(instanceBuffer.SRV, 0);
            context.VSSetShaderResource(instanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(idBuffer.Buffer, 0);
            pipeline.DrawIndexedInstancedIndirect(context, viewport, argBuffer.Buffer, argBufferOffset);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                frustumInstanceBuffer.Dispose();
                zeroIdBuffer.Dispose();
                idBuffer.Dispose();
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