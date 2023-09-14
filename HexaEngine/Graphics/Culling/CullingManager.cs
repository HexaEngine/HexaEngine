namespace HexaEngine.Culling
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public struct TypeData
    {
        public uint IndexCountPerInstance;
        public uint StartIndexLocation;
        public int BaseVertexLocation;
        public uint StartInstanceLocation;
    }

    public class CullingContext
    {
        private readonly StructuredBuffer<TypeData> typeDataBuffer;
        private readonly StructuredBuffer<InstanceData> instanceDataBuffer;
        private uint currentType;

        public CullingContext(StructuredBuffer<TypeData> typeDataBuffer, StructuredBuffer<InstanceData> instanceDataBuffer)
        {
            this.typeDataBuffer = typeDataBuffer;
            this.instanceDataBuffer = instanceDataBuffer;
        }

        public void Reset()
        {
            typeDataBuffer.ResetCounter();
            instanceDataBuffer.ResetCounter();
            currentType = 0;
        }

        public void AppendType(TypeData type)
        {
            currentType = typeDataBuffer.Count;
            typeDataBuffer.Add(type);
        }

        public unsafe uint GetDrawArgsOffset()
        {
            return currentType * (uint)sizeof(DrawIndexedInstancedIndirectArgs);
        }

        public void AppendInstance(InstanceData instance)
        {
            instance.Type = currentType;
            instanceDataBuffer.Add(instance);
        }
    }

    public class CullingManager
    {
#nullable disable
        private readonly IGraphicsDevice device;
        private CullingFlags cullingFlags;

        private IComputePipeline occlusion;

        private ConstantBuffer<CBCamera> occlusionCameraBuffer;
        private StructuredUavBuffer<uint> instanceOffsets;
        private StructuredUavBuffer<Matrix4x4> instanceDataOutBuffer;
        private StructuredBuffer<TypeData> typeDataBuffer;
        private StructuredBuffer<InstanceData> instanceDataBuffer;
        private StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> swapBuffer;
        private DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;

        private StructuredBuffer<uint> instanceOffsetsNoCull;
        private StructuredBuffer<Matrix4x4> instanceDataNoCull;

        private ConstantBuffer<OcclusionParams> occlusionParamBuffer;
        private ISamplerState sampler;
        private unsafe void** occlusionSrvs;
        private unsafe void** occlusionUavs;
        private unsafe void** occlusionCbs;
        private CullingContext context;
#nullable enable

        public unsafe CullingManager(IGraphicsDevice device)
        {
            this.device = device;
            occlusion = device.CreateComputePipeline(new()
            {
                Path = "compute/occlusion/occlusion.hlsl",
            });

            instanceDataNoCull = new(device, CpuAccessFlags.Write);
            instanceOffsetsNoCull = new(device, CpuAccessFlags.Write);

            occlusionCameraBuffer = new(device, CpuAccessFlags.Write);
            occlusionParamBuffer = new(device, CpuAccessFlags.Write);
            instanceOffsets = new(device, CpuAccessFlags.None);
            instanceDataOutBuffer = new(device, CpuAccessFlags.Read);
            typeDataBuffer = new(device, CpuAccessFlags.Write);
            instanceDataBuffer = new(device, CpuAccessFlags.Write);
            swapBuffer = new(device, CpuAccessFlags.RW);
            drawIndirectArgs = new(device, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerStateDescription.PointClamp);
            occlusionCbs = AllocArray(2);
            occlusionCbs[0] = (void*)occlusionParamBuffer.Buffer.NativePointer;
            occlusionCbs[1] = (void*)occlusionCameraBuffer.Buffer.NativePointer;
            occlusionUavs = AllocArray(3);
            occlusionUavs[0] = (void*)instanceDataOutBuffer.UAV.NativePointer;
            occlusionUavs[1] = (void*)instanceOffsets.UAV.NativePointer;
            occlusionUavs[2] = (void*)swapBuffer.UAV.NativePointer;
            occlusionSrvs = AllocArray(2);
            occlusionSrvs[1] = (void*)instanceDataBuffer.SRV.NativePointer;
            context = new(typeDataBuffer, instanceDataBuffer);
        }

        public CullingContext Context { get => context; }

        public CullingFlags CullingFlags { get => cullingFlags; set => cullingFlags = value; }

        public StructuredBuffer<uint> InstanceOffsetsNoCull => instanceOffsetsNoCull;

        public StructuredBuffer<Matrix4x4> InstanceDataNoCull => instanceDataNoCull;

        public ConstantBuffer<CBCamera> OcclusionCameraBuffer => occlusionCameraBuffer;

        public StructuredUavBuffer<uint> InstanceOffsets => instanceOffsets;

        public StructuredUavBuffer<Matrix4x4> InstanceDataOutBuffer => instanceDataOutBuffer;

        public StructuredBuffer<InstanceData> InstanceDataBuffer => instanceDataBuffer;

        public StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> SwapBuffer => swapBuffer;

        public DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> DrawIndirectArgs => drawIndirectArgs;

        public void UpdateCamera(IGraphicsContext context, Viewport viewport)
        {
            if (CameraManager.Culling == null)
            {
                return;
            }

            occlusionCameraBuffer.Update(context, new(CameraManager.Culling, new(viewport.Width, viewport.Height)));
        }

        public void DoFrustumCulling(IGraphicsContext context, BoundingFrustum frustum, out int count)
        {
            count = 0;
            /*
            instanceDataBuffer.ResetCounter();
            instanceDataNoCull.ResetCounter();
            instanceOffsetsNoCull.ResetCounter();
            swapBuffer.Clear();

            uint offset = 0;
            for (int i = 0; i < manager.TypeCount; i++)
            {
                var type = manager.Types[i];
                type.UpdateInstanceBuffer((uint)i, instanceDataNoCull, instanceDataBuffer, swapBuffer, frustum, (cullingFlags & CullingFlags.Frustum) != 0);
                instanceOffsetsNoCull.ObjectAdded(offset);
                offset += (uint)type.Count;
            }

            swapBuffer.Update(context);

            drawIndirectArgs.Capacity = instanceOffsets.Capacity = swapBuffer.Capacity;
            instanceDataOutBuffer.Capacity = instanceDataBuffer.Capacity;
            instanceDataBuffer.Update(context);
            count = (int)instanceDataBuffer.Count;
            */
        }

        public unsafe void DoOcclusionCulling(IGraphicsContext context, Camera camera, int instanceCount, int typeCount, DepthMipChain mipChain)
        {
            if (instanceCount == 0)
            {
                return;
            }

            occlusionParamBuffer[0] = new()
            {
                ActivateCulling = (cullingFlags & CullingFlags.Occlusion) != 0 ? 1 : 0,
                NoofInstances = (uint)instanceCount,
                NoofPropTypes = (uint)typeCount,
                MaxMipLevel = (uint)mipChain.Mips,
                RTSize = new(mipChain.Width, mipChain.Height),
                P00 = camera.Transform.Projection.M11,
                P11 = camera.Transform.Projection.M22,
            };
            occlusionParamBuffer.Update(context);

            occlusionUavs[0] = (void*)instanceDataOutBuffer.UAV.NativePointer;
            occlusionUavs[1] = (void*)instanceOffsets.UAV.NativePointer;
            occlusionUavs[2] = (void*)swapBuffer.UAV.NativePointer;

            occlusionSrvs[1] = (void*)instanceDataBuffer.SRV.NativePointer;
            occlusionSrvs[0] = (void*)mipChain.SRV.NativePointer;

            uint* initialCount = stackalloc uint[] { unchecked((uint)-1), unchecked((uint)-1), unchecked((uint)-1) };
            context.CSSetShaderResources(0, 2, occlusionSrvs);
            context.CSSetUnorderedAccessViews(0, occlusionUavs, initialCount);
            context.CSSetSampler(0, sampler);
            context.CSSetConstantBuffers(0, 2, occlusionCbs);
            occlusion.Dispatch(context, (uint)instanceCount / 1024 + 1, 1, 1);
            context.ClearState();

            swapBuffer.CopyTo(context, drawIndirectArgs.Buffer);
            swapBuffer.Read(context);
            /*
            uint drawCalls = 0;
            uint vertexCount = 0;

            for (int i = 0; i < manager.TypeCount; i++)
            {
                vertexCount += swapBuffer[i].IndexCountPerInstance * swapBuffer[i].InstanceCount;
                drawCalls += swapBuffer[i].InstanceCount > 0 ? 1u : 0u;
            }

            uint fmt = 0;
            while (vertexCount > 1000)
            {
                vertexCount /= 1000;
                fmt++;
            }
            char suffix = fmt switch
            {
                0 => ' ',
                1 => 'K',
                _ => 'M',
            };*/
        }

        public void DoCulling(IGraphicsContext context, IShaderResourceView mipChain)
        {
            /*
            var camera = CameraManager.Culling;

            if (camera == null)
            {
                return;
            }

            DoFrustumCulling(context, camera.Transform.Frustum, out var count);
            DoOcclusionCulling(context, camera, count, mipChain);
            */
        }

        public unsafe void Release()
        {
            occlusionCameraBuffer.Dispose();
            occlusionCameraBuffer = null;
            instanceOffsets.Dispose();
            instanceOffsets = null;
            instanceDataOutBuffer.Dispose();
            instanceDataOutBuffer = null;
            typeDataBuffer.Dispose();
            typeDataBuffer = null;
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