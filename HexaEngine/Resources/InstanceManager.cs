namespace HexaEngine.Resources
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public struct InstanceData
    {
        public uint Type;
        public Matrix4x4 World;
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 Center;
        public float Radius;

        public InstanceData(uint type, Matrix4x4 world, Vector3 min, Vector3 max, Vector3 center, float radius)
        {
            Type = type;
            World = world;
            Min = min;
            Max = max;
            Center = center;
            Radius = radius;
        }
    }

    public struct OcclusionParams
    {
        public uint NoofInstances;
        public uint NoofPropTypes;
        public int ActivateCulling;
        public uint MaxMipLevel;
        public Vector2 RTSize;
        public Vector2 Padding;
    }

    public class InstanceManager : IDisposable
    {
        private readonly IGraphicsDevice device;

        private readonly List<ModelInstanceType> types = new();
        private readonly List<ModelInstance> instances = new();
        private readonly SemaphoreSlim semaphore = new(1);
        private bool disposedValue;

        private readonly ComputePipeline occlusion;

        private readonly StructuredUavBuffer<uint> instanceCounts;
        private readonly StructuredUavBuffer<uint> instanceOffsets;
        private readonly StructuredUavBuffer<Matrix4x4> instanceDataOutBuffer;
        private readonly StructuredBuffer<InstanceData> instanceDataBuffer;
        private readonly StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> swapBuffer;
        private readonly DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;

        private readonly ConstantBuffer<OcclusionParams> occlusionParamBuffer;
        private readonly ISamplerState sampler;
        private readonly unsafe void** occlusionSrvs;
        private readonly unsafe void** occlusionUavs;

        public IReadOnlyList<ModelInstance> Instances => instances;

        public IReadOnlyList<ModelInstanceType> Types => types;

        public unsafe InstanceManager(IGraphicsDevice device)
        {
            this.device = device;
            occlusion = new(device, new()
            {
                Path = "compute/occlusion/occlusion.hlsl",
            });

            instanceCounts = new(device, false);
            instanceOffsets = new(device, false);
            instanceDataOutBuffer = new(device, false);
            instanceDataBuffer = new(device, CpuAccessFlags.Write);
            occlusionParamBuffer = new(device, CpuAccessFlags.Write);
            swapBuffer = new(device, true);
            drawIndirectArgs = new(device, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerDescription.PointClamp);
            occlusionUavs = AllocArray(4);
            occlusionUavs[0] = (void*)instanceDataOutBuffer.UAV.NativePointer;
            occlusionUavs[1] = (void*)instanceCounts.UAV.NativePointer;
            occlusionUavs[2] = (void*)instanceOffsets.UAV.NativePointer;
            occlusionUavs[3] = (void*)swapBuffer.UAV.NativePointer;
            occlusionSrvs = AllocArray(2);
            occlusionSrvs[1] = (void*)instanceDataBuffer.SRV.NativePointer;
        }

        public int DoFrustumCulling(IGraphicsContext context, BoundingFrustum frustum)
        {
            if (instances.Count == 0) return 0;
            instanceDataBuffer.ResetCounter();

            swapBuffer.Clear();

            for (int i = 0; i < types.Count; i++)
            {
                var type = types[i];
                type.UpdateInstanceBuffer((uint)i, instanceDataBuffer, swapBuffer, frustum);
            }

            swapBuffer.Update(context);

            drawIndirectArgs.Capacity = instanceDataOutBuffer.Capacity = instanceDataBuffer.Capacity;
            instanceDataBuffer.Update(context);

            return (int)instanceDataBuffer.Count;
        }

        public unsafe void DoOcclusionCulling(IGraphicsContext context, IBuffer camera, int instanceCount, DepthMipChain mipChain)
        {
            if (instanceCount == 0) return;
            occlusionParamBuffer[0] = new() { NoofInstances = (uint)instanceCount, NoofPropTypes = (uint)types.Count, ActivateCulling = 1, MaxMipLevel = (uint)mipChain.Mips, RTSize = new(mipChain.Width, mipChain.Height) };
            occlusionParamBuffer.Update(context);

            occlusionUavs[0] = (void*)instanceDataOutBuffer.UAV.NativePointer;
            occlusionUavs[1] = (void*)instanceCounts.UAV.NativePointer;
            occlusionUavs[2] = (void*)instanceOffsets.UAV.NativePointer;
            occlusionUavs[3] = (void*)swapBuffer.UAV.NativePointer;

            occlusionSrvs[1] = (void*)instanceDataBuffer.SRV.NativePointer;
            occlusionSrvs[0] = (void*)mipChain.SRV.NativePointer;

            context.CSSetShaderResources(occlusionSrvs, 2, 0);
            context.CSSetUnorderedAccessViews(occlusionUavs, 4, 0);
            context.CSSetSampler(sampler, 0);
            context.CSSetConstantBuffer(occlusionParamBuffer.Buffer, 0);
            context.CSSetConstantBuffer(camera, 1);
            instanceCounts.Clear(context);
            occlusion.Dispatch(context, (instanceCount / 1024) + 1, 1, 1);
            context.ClearState();

            swapBuffer.CopyTo(context, drawIndirectArgs.Buffer);
        }

        public void DoCulling(IGraphicsContext context, IBuffer camera, BoundingFrustum frustum, DepthMipChain mipChain)
        {
            var count = DoFrustumCulling(context, frustum);
            DoOcclusionCulling(context, camera, count, mipChain);
        }

        public ModelInstance CreateInstance(Model model, Transform transform)
        {
            var material = ResourceManager.LoadMaterial(model.Material);
            var mesh = ResourceManager.LoadMesh(model.Mesh);
            var instance = CreateInstance(mesh, material, transform);
            return instance;
        }

        public async Task<ModelInstance> CreateInstanceAsync(Model model, Transform transform)
        {
            var material = await ResourceManager.LoadMaterialAsync(model.Material);
            var mesh = await ResourceManager.LoadMeshAsync(model.Mesh);
            var instance = await CreateInstanceAsync(mesh, material, transform);
            return instance;
        }

        public ModelInstance CreateInstance(Mesh mesh, Material material, Transform transform)
        {
            semaphore.Wait();
            ModelInstanceType type;
            lock (types)
            {
                type = mesh.CreateInstanceType(device, drawIndirectArgs, instanceDataOutBuffer, instanceOffsets, material);
                if (!types.Contains(type))
                {
                    types.Add(type);
                }
            }

            ModelInstance instance;
            lock (instances)
            {
                instance = type.CreateInstance(device, transform);
                if (!instances.Contains(instance))
                {
                    instances.Add(instance);
                    instanceCounts.Add(new());
                    instanceCounts.Update(device.Context);
                }
            }

            ImGuiConsole.Log(instance.ToString());
            semaphore.Release();
            return instance;
        }

        public async Task<ModelInstance> CreateInstanceAsync(Mesh mesh, Material material, Transform transform)
        {
            await semaphore.WaitAsync();
            ModelInstanceType type;
            lock (types)
            {
                type = mesh.CreateInstanceType(device, drawIndirectArgs, instanceDataOutBuffer, instanceOffsets, material);
                if (!types.Contains(type))
                {
                    types.Add(type);
                }
            }

            ModelInstance instance;
            lock (instances)
            {
                instance = type.CreateInstance(device, transform);
                if (!instances.Contains(instance))
                {
                    instances.Add(instance);
                }
            }

            ImGuiConsole.Log(instance.ToString());
            semaphore.Release();
            return instance;
        }

        public void DestroyInstance(ModelInstance instance)
        {
            semaphore.Wait();

            var type = instance.Type;

            lock (types)
            {
                if (!types.Contains(type))
                {
                    throw new InvalidOperationException();
                }
            }

            lock (instances)
            {
                if (!instances.Contains(instance))
                {
                    throw new InvalidOperationException();
                }
                instances.Remove(instance);
            }

            type.DestroyInstance(device, instance);

            if (type.IsEmpty)
            {
                var mesh = type.Mesh;
                types.Remove(type);
                mesh.DestroyInstanceType(type);

                if (!mesh.IsUsed)
                {
                    ResourceManager.UnloadMesh(mesh);
                }
            }

            semaphore.Release();
        }

        public async Task DestroyInstanceAsync(ModelInstance instance)
        {
            await semaphore.WaitAsync();

            var type = instance.Type;

            lock (types)
            {
                if (!types.Contains(type))
                {
                    throw new InvalidOperationException();
                }
            }

            lock (instances)
            {
                if (!instances.Contains(instance))
                {
                    throw new InvalidOperationException();
                }
                instances.Remove(instance);
            }

            type.DestroyInstance(device, instance);

            if (type.IsEmpty)
            {
                var mesh = type.Mesh;
                types.Remove(type);
                mesh.DestroyInstanceType(type);

                if (!mesh.IsUsed)
                {
                    ResourceManager.UnloadMesh(mesh);
                }
            }

            semaphore.Release();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                occlusion.Dispose();
                instanceCounts.Dispose();
                instanceOffsets.Dispose();
                instanceDataOutBuffer.Dispose();
                instanceDataBuffer.Dispose();
                swapBuffer.Dispose();
                drawIndirectArgs.Dispose();
                occlusionParamBuffer.Dispose();
                sampler.Dispose();
                disposedValue = true;
            }
        }

        ~InstanceManager()
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