namespace HexaEngine.Core.Culling
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public static class CullingManager
    {
        public struct CBCamera
        {
            public Matrix4x4 View;
            public Matrix4x4 Proj;
            public Matrix4x4 ViewInv;
            public Matrix4x4 ProjInv;
            public float Far;
            public float Near;
            public Vector2 Padd;

            public CBCamera(Camera camera)
            {
                Proj = Matrix4x4.Transpose(camera.Transform.Projection);
                View = Matrix4x4.Transpose(camera.Transform.View);
                ProjInv = Matrix4x4.Transpose(camera.Transform.ProjectionInv);
                ViewInv = Matrix4x4.Transpose(camera.Transform.ViewInv);
                Far = camera.Far;
                Near = camera.Near;
                Padd = default;
            }

            public CBCamera(CameraTransform camera)
            {
                Proj = Matrix4x4.Transpose(camera.Projection);
                View = Matrix4x4.Transpose(camera.View);
                ProjInv = Matrix4x4.Transpose(camera.ProjectionInv);
                ViewInv = Matrix4x4.Transpose(camera.ViewInv);
                Far = camera.Far;
                Near = camera.Near;
                Padd = default;
            }
        }

#nullable disable
        private static CullingFlags cullingFlags;
        private static IGraphicsDevice device;

        private static IComputePipeline occlusion;

        private static ConstantBuffer<CBCamera> occlusionCameraBuffer;
        private static StructuredUavBuffer<uint> instanceCounts;
        private static StructuredUavBuffer<uint> instanceOffsets;
        private static StructuredUavBuffer<Matrix4x4> instanceDataOutBuffer;
        private static StructuredBuffer<InstanceData> instanceDataBuffer;
        private static StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> swapBuffer;
        private static DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;

        private static StructuredBuffer<uint> instanceOffsetsNoCull;
        private static StructuredBuffer<Matrix4x4> instanceDataNoCull;

        private static ConstantBuffer<OcclusionParams> occlusionParamBuffer;
        private static ISamplerState sampler;
        private static unsafe void** occlusionSrvs;
        private static unsafe void** occlusionUavs;
        private static unsafe void** occlusionCbs;
#nullable enable

        public static CullingFlags CullingFlags { get => cullingFlags; set => cullingFlags = value; }

        public static StructuredBuffer<uint> InstanceOffsetsNoCull => instanceOffsetsNoCull;

        public static StructuredBuffer<Matrix4x4> InstanceDataNoCull => instanceDataNoCull;

        public static ConstantBuffer<CBCamera> OcclusionCameraBuffer => occlusionCameraBuffer;

        public static StructuredUavBuffer<uint> InstanceCounts => instanceCounts;

        public static StructuredUavBuffer<uint> InstanceOffsets => instanceOffsets;

        public static StructuredUavBuffer<Matrix4x4> InstanceDataOutBuffer => instanceDataOutBuffer;

        public static StructuredBuffer<InstanceData> InstanceDataBuffer => instanceDataBuffer;

        public static StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> SwapBuffer => swapBuffer;

        public static DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> DrawIndirectArgs => drawIndirectArgs;

        public static unsafe void Initialize(IGraphicsDevice device)
        {
            CullingManager.device = device;
            occlusion = device.CreateComputePipeline(new()
            {
                Path = "compute/occlusion/occlusion.hlsl",
            });

            instanceDataNoCull = new(device, CpuAccessFlags.Write);
            instanceOffsetsNoCull = new(device, CpuAccessFlags.Write);

            occlusionCameraBuffer = new(device, CpuAccessFlags.Write);
            occlusionParamBuffer = new(device, CpuAccessFlags.Write);
            instanceCounts = new(device, false, false);
            instanceOffsets = new(device, false, false);
            instanceDataOutBuffer = new(device, false, false);
            instanceDataBuffer = new(device, CpuAccessFlags.Write);
            swapBuffer = new(device, true, false);
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
            instanceDataNoCull.ResetCounter();
            instanceOffsetsNoCull.ResetCounter();
            swapBuffer.Clear();

            uint offset = 0;
            for (int i = 0; i < manager.TypeCount; i++)
            {
                var type = manager.Types[i];
                type.UpdateInstanceBuffer((uint)i, instanceDataNoCull, instanceDataBuffer, swapBuffer, frustum, (cullingFlags & CullingFlags.Frustum) != 0);
                instanceOffsetsNoCull.Add(offset);
                offset += (uint)type.Count;
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
                ActivateCulling = (cullingFlags & CullingFlags.Occlusion) != 0 ? 1 : 0,
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
            occlusionCameraBuffer = null;
            instanceCounts.Dispose();
            instanceCounts = null;
            instanceOffsets.Dispose();
            instanceOffsets = null;
            instanceDataOutBuffer.Dispose();
            instanceDataOutBuffer = null;
            instanceDataBuffer.Dispose();
            instanceDataBuffer = null;
            swapBuffer.Dispose();
            swapBuffer = null;
            drawIndirectArgs.Dispose();
            drawIndirectArgs = null;

            instanceOffsetsNoCull.Dispose();
            instanceOffsetsNoCull = null;
            instanceDataNoCull.Dispose();
            instanceDataNoCull = null;

            occlusionParamBuffer.Dispose();
            occlusionParamBuffer = null;
            occlusion.Dispose();
            occlusion = null;

            sampler.Dispose();
            sampler = null;
            Free(occlusionSrvs);
            occlusionSrvs = null;
            Free(occlusionUavs);
            occlusionUavs = null;
            Free(occlusionCbs);
            occlusionCbs = null;
        }
    }
}