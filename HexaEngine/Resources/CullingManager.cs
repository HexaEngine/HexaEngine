namespace HexaEngine.Resources
{
    using HexaEngine.Cameras;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering.ConstantBuffers;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public static class CullingManager
    {
#nullable disable
        private static CullingFlags cullingFlags;
        private static IGraphicsDevice device;

        private static ComputePipeline occlusion;

        private static ConstantBuffer<CBCamera> occlusionCameraBuffer;
        private static StructuredUavBuffer<uint> instanceCounts;
        private static StructuredUavBuffer<uint> instanceOffsets;
        private static StructuredUavBuffer<Matrix4x4> instanceDataOutBuffer;
        private static StructuredBuffer<InstanceData> instanceDataBuffer;
        private static StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> swapBuffer;
        private static DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;

        private static ConstantBuffer<OcclusionParams> occlusionParamBuffer;
        private static ISamplerState sampler;
        private static unsafe void** occlusionSrvs;
        private static unsafe void** occlusionUavs;
        private static unsafe void** occlusionCbs;
#nullable enable

        public static CullingFlags CullingFlags { get => cullingFlags; set => cullingFlags = value; }

        public static ConstantBuffer<CBCamera> OcclusionCameraBuffer => occlusionCameraBuffer;

        public static StructuredUavBuffer<uint> InstanceCounts => instanceCounts;
        public static StructuredUavBuffer<uint> InstanceOffsets => instanceOffsets;
        public static StructuredUavBuffer<Matrix4x4> InstanceDataOutBuffer => instanceDataOutBuffer;
        public static StructuredBuffer<InstanceData> InstanceDataBuffer => instanceDataBuffer;
        public static StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> SwapBuffer => swapBuffer;
        public static DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> DrawIndirectArgs => drawIndirectArgs;

        internal static unsafe void Initialize(IGraphicsDevice device)
        {
            CullingManager.device = device;
            occlusion = new(device, new()
            {
                Path = "compute/occlusion/occlusion.hlsl",
            });

            occlusionCameraBuffer = new(device, CpuAccessFlags.Write);
            occlusionParamBuffer = new(device, CpuAccessFlags.Write);
            instanceCounts = new(device, false);
            instanceOffsets = new(device, false);
            instanceDataOutBuffer = new(device, false);
            instanceDataBuffer = new(device, CpuAccessFlags.Write);
            swapBuffer = new(device, true);
            drawIndirectArgs = new(device, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerDescription.PointClamp);
            occlusionCbs = AllocArray(2);
            occlusionCbs[0] = (void*)occlusionParamBuffer.Buffer.NativePointer;
            occlusionCbs[1] = (void*)occlusionCameraBuffer.Buffer.NativePointer;
            occlusionUavs = AllocArray(4);
            occlusionUavs[0] = (void*)instanceDataOutBuffer.UAV.NativePointer;
            occlusionUavs[1] = (void*)instanceCounts.UAV.NativePointer;
            occlusionUavs[2] = (void*)instanceOffsets.UAV.NativePointer;
            occlusionUavs[3] = (void*)swapBuffer.UAV.NativePointer;
            occlusionSrvs = AllocArray(2);
            occlusionSrvs[1] = (void*)instanceDataBuffer.SRV.NativePointer;
        }

        public static void UpdateCamera(IGraphicsContext context)
        {
            if (CameraManager.Culling == null) return;
            occlusionCameraBuffer[0] = new(CameraManager.Culling);
            occlusionCameraBuffer.Update(context);
        }

        public static void DoFrustumCulling(IGraphicsContext context, InstanceManager manager, BoundingFrustum frustum, out int count)
        {
            if (manager.InstanceCount == 0)
            {
                count = 0;
                return;
            }

            instanceDataBuffer.ResetCounter();

            swapBuffer.Clear();

            for (int i = 0; i < manager.TypeCount; i++)
            {
                var type = manager.Types[i];
                type.UpdateInstanceBuffer((uint)i, instanceDataBuffer, swapBuffer, frustum, cullingFlags.HasFlag(CullingFlags.Frustum));
            }

            swapBuffer.Update(context);

            drawIndirectArgs.Capacity = instanceOffsets.Capacity = instanceCounts.Capacity = swapBuffer.Capacity;
            instanceDataOutBuffer.Capacity = instanceDataBuffer.Capacity;
            instanceDataBuffer.Update(context);
            count = (int)instanceDataBuffer.Count;
        }

        public static unsafe void DoOcclusionCulling(IGraphicsContext context, InstanceManager manager, Camera camera, int instanceCount, DepthMipChain mipChain)
        {
            if (instanceCount == 0) return;

            occlusionParamBuffer[0] = new()
            {
                ActivateCulling = cullingFlags.HasFlag(CullingFlags.Occlusion) ? 1 : 0,
                NoofInstances = (uint)instanceCount,
                NoofPropTypes = (uint)manager.TypeCount,
                MaxMipLevel = (uint)mipChain.Mips,
                RTSize = new(mipChain.Width, mipChain.Height),
                P00 = camera.Transform.Projection.M11,
                P11 = camera.Transform.Projection.M22,
            };
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
            context.CSSetConstantBuffers(occlusionCbs, 2, 0);
            instanceCounts.Clear(context);
            occlusion.Dispatch(context, instanceCount / 1024 + 1, 1, 1);
            context.ClearState();

            swapBuffer.CopyTo(context, drawIndirectArgs.Buffer);
        }

        public static void DoCulling(IGraphicsContext context, DepthMipChain mipChain)
        {
            var camera = CameraManager.Culling;
            var instanceManager = InstanceManager.Current;
            if (camera == null) return;
            if (instanceManager == null) return;

            DoFrustumCulling(context, instanceManager, camera.Transform.Frustum, out var count);
            DoOcclusionCulling(context, instanceManager, camera, count, mipChain);
        }

        public static unsafe void Release()
        {
            occlusionCameraBuffer.Dispose();
            occlusionParamBuffer.Dispose();
            occlusion.Dispose();
            instanceCounts.Dispose();
            instanceOffsets.Dispose();
            instanceDataOutBuffer.Dispose();
            instanceDataBuffer.Dispose();
            swapBuffer.Dispose();
            drawIndirectArgs.Dispose();
            sampler.Dispose();
            Free(occlusionSrvs);
            Free(occlusionUavs);
            Free(occlusionCbs);
        }
    }
}