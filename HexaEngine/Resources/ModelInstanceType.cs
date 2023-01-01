﻿namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using System.Collections.Concurrent;
    using System.Numerics;

    public class ModelInstanceType : IDisposable
    {
        private readonly SemaphoreSlim semaphore = new(1);
        private readonly List<ModelInstance> instances = new();
        private readonly ConcurrentQueue<ModelInstance> addQueue = new();
        private bool disposedValue;
        private readonly DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> argBuffer;
        private readonly StructuredUavBuffer<Matrix4x4> instanceBuffer;
        private readonly StructuredUavBuffer<uint> instanceOffsets;
        private readonly StructuredBuffer<Matrix4x4> frustumInstanceBuffer;
        private readonly ConstantBuffer<uint> frustumIdBuffer;
        private readonly ConstantBuffer<uint> idBuffer;
        private uint argBufferOffset;
        private int idCounter;

        public readonly string Name;
        public readonly Mesh Mesh;
        public readonly Material Material;

        public unsafe ModelInstanceType(IGraphicsDevice device, DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> argsBuffer, StructuredUavBuffer<Matrix4x4> instanceBuffer, StructuredUavBuffer<uint> instanceOffsets, Mesh mesh, Material material)
        {
            Name = $"{mesh},{material}";
            Mesh = mesh;
            Material = material;
            argBuffer = argsBuffer;
            this.instanceBuffer = instanceBuffer;
            this.instanceOffsets = instanceOffsets;
            frustumInstanceBuffer = new(device, CpuAccessFlags.Write);
            frustumIdBuffer = new(device, 4, CpuAccessFlags.None);
            idBuffer = new(device, 4, CpuAccessFlags.Write);
        }

        public int VertexCount => Mesh.VertexCount;

        public int IndexCount => Mesh.IndexCount;

        public int Visible => (int)frustumInstanceBuffer.Count;

        public int Count => instances.Count;

        public bool IsEmpty => instances.Count == 0;

        public IBuffer ArgBuffer => argBuffer.Buffer;

        public uint ArgBufferOffset => argBufferOffset;

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

            semaphore.Release();
        }

        public unsafe int UpdateInstanceBuffer(uint id, StructuredBuffer<InstanceData> buffer, StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> drawArgs, BoundingFrustum frustum)
        {
            int count = 0;

            while (addQueue.TryDequeue(out var instance))
            {
                instances.Add(instance);
            }

            idBuffer[0] = id;

            argBufferOffset = (uint)(id * sizeof(DrawIndexedInstancedIndirectArgs));
            frustumInstanceBuffer.ResetCounter();

            for (int i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];
                instance.GetBoundingBox(out var aabb);
                instance.GetBoundingSphere(out var sphere);
                if (frustum.Intersects(sphere))
                {
                    var world = Matrix4x4.Transpose(instance.Transform);
                    buffer.Add(new(id, world, aabb.Min, aabb.Max, sphere.Center, sphere.Radius));
                    frustumInstanceBuffer.Add(world);
                }
            }

            drawArgs.Add(new() { IndexCountPerInstance = (uint)Mesh.IndexCount });

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
            idBuffer.Update(context);
            context.SetVertexBuffer(0, Mesh.VB, (uint)sizeof(MeshVertex));
            context.SetIndexBuffer(Mesh.IB, Format.R32UInt, 0);
            context.VSSetShaderResource(frustumInstanceBuffer.SRV, 0);
            context.VSSetShaderResource(instanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(frustumIdBuffer.Buffer, 0);

            return true;
        }

        public unsafe bool BeginDraw(IGraphicsContext context)
        {
            if (Mesh.VB == null) return false;
            if (Mesh.IB == null) return false;
            if (Material == null) return false;
            if (semaphore.CurrentCount == 0) return false;
            if (!Material.Bind(context)) return false;

            frustumInstanceBuffer.Update(context);
            idBuffer.Update(context);
            context.SetVertexBuffer(0, Mesh.VB, (uint)sizeof(MeshVertex));
            context.SetIndexBuffer(Mesh.IB, Format.R32UInt, 0);
            context.VSSetShaderResource(instanceBuffer.SRV, 0);
            context.VSSetShaderResource(instanceOffsets.SRV, 1);
            context.VSSetConstantBuffer(idBuffer.Buffer, 0);

            return true;
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

        public unsafe void DrawAuto(IGraphicsContext context, GraphicsPipeline pipeline, Viewport viewport, int indexCount)
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