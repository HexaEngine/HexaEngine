namespace HexaEngine.Graphics.Culling
{
    using Hexa.NET.Mathematics;
    using Hexa.NET.Utilities.Threading;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Graphics;
    using HexaEngine.Profiling;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class CullingManager
    {
#nullable disable
        private CullingFlags cullingFlags = CullingFlags.All;
        private float depthBias = 0.0f;

        private IComputePipelineState culling;

        private ConstantBuffer<CBCamera> occlusionCameraBuffer;
        private StructuredUavBuffer<uint> instanceOffsets;
        private StructuredUavBuffer<Matrix4x4> instanceDataOutBuffer;
        private StructuredBuffer<TypeData> typeDataBuffer;
        private StructuredBuffer<GPUInstance> instanceDataBuffer;
        private StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> swapBuffer;
        private DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;
        private StructuredUavBuffer<uint> visibleListBuffer;

        private StructuredBuffer<uint> instanceOffsetsNoCull;
        private StructuredBuffer<Matrix4x4> instanceDataNoCull;

        private readonly EventHandlers<CapacityChangedEventArgs> resizedEventHandlers = new();

        private ConstantBuffer<OcclusionParams> occlusionParamBuffer;
        private SamplerState sampler;
        private readonly CullingContext context;
        private CullingStats stats;
#nullable enable

        public CullingManager(IGraphicsDevice device)
        {
            culling = device.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Path = "compute/culling/culling.hlsl",
            });

            instanceDataNoCull = new(CpuAccessFlags.Write);
            instanceOffsetsNoCull = new(CpuAccessFlags.Write);

            occlusionCameraBuffer = new(CpuAccessFlags.Write);
            occlusionParamBuffer = new(CpuAccessFlags.Write);
            instanceOffsets = new(CpuAccessFlags.None);
            instanceDataOutBuffer = new(CpuAccessFlags.Read);
            typeDataBuffer = new(CpuAccessFlags.Write);
            instanceDataBuffer = new(CpuAccessFlags.Write);
            swapBuffer = new(CpuAccessFlags.RW);
            drawIndirectArgs = new(CpuAccessFlags.Write);
            visibleListBuffer = new(CpuAccessFlags.Read);
            sampler = new(new(Filter.MinMagLinearMipPoint, TextureAddressMode.Clamp));

            var bindings = culling.Bindings;
            bindings.SetCBV("CullingParams", occlusionParamBuffer);
            bindings.SetCBV("CameraBuffer", occlusionCameraBuffer);

            bindings.SetSRV("instanceDataIn", instanceDataBuffer.SRV);

            bindings.SetUAV("instanceDataOut", instanceDataOutBuffer.UAV);
            bindings.SetUAV("instanceOffsets", instanceOffsets.UAV);
            bindings.SetUAV("drawArgs", swapBuffer.UAV);
            bindings.SetUAV("visibleListOut", visibleListBuffer.UAV);

            bindings.SetSampler("samplerPoint", sampler);

            context = new(instanceOffsetsNoCull, instanceDataNoCull, instanceOffsets, instanceDataOutBuffer, typeDataBuffer, instanceDataBuffer, swapBuffer, drawIndirectArgs, visibleListBuffer);

            instanceDataBuffer.Resize += BufferResize;
            instanceDataOutBuffer.Resize += BufferResize;
            instanceOffsets.Resize += BufferResize;
            swapBuffer.Resize += BufferResize;
            visibleListBuffer.Resize += BufferResize;
        }

        private bool resized;

        private void BufferResize(object? sender, CapacityChangedEventArgs e)
        {
            resized = true;
            resizedEventHandlers.Invoke(this, e);
        }

        public static CullingManager Current { get; internal set; } = new(Application.GraphicsDevice);

        public CullingContext Context => context;

        public CullingFlags CullingFlags { get => cullingFlags; set => cullingFlags = value; }

        public float DepthBias { get => depthBias; set => depthBias = value; }

        public CullingStats Stats { get => stats; }

        public event EventHandler<CapacityChangedEventArgs> BuffersResized
        {
            add => resizedEventHandlers.AddHandler(value);
            remove => resizedEventHandlers.RemoveHandler(value);
        }

        public void UpdateCamera(IGraphicsContext context, Camera camera, Viewport viewport)
        {
            occlusionCameraBuffer.Update(context, new(camera, new(viewport.Width, viewport.Height)));
        }

        private Vector4 Frustum;
        private float CamNear;
        private float CamFar;

        public void ComputePlanes(Camera camera)
        {
            Matrix4x4 projection = camera.Transform.Projection;

            Matrix4x4 projectionT = Matrix4x4.Transpose(projection);

            Vector4 frustumX = MathUtil.NormalizePlane(projectionT.GetRow(3) + projectionT.GetRow(0)); // x + w < 0
            Vector4 frustumY = MathUtil.NormalizePlane(projectionT.GetRow(3) + projectionT.GetRow(1)); // y + w < 0

            Frustum = new(frustumX.X, frustumX.Z, frustumY.Y, frustumY.Z);
            CamNear = camera.Near;
            CamFar = camera.Far;
        }

        public bool IsVisible(Vector3 center, float radius)
        {
            bool visible = true;

            // the left/top/right/bottom plane culling utilizes frustum symmetry to cull against two planes at the same time
            visible = visible && center.Z * Frustum[1] - Math.Abs(center.X) * Frustum[0] > -radius;
            visible = visible && center.Z * Frustum[3] - Math.Abs(center.Y) * Frustum[2] > -radius;
            // the near/far plane culling uses camera space Z directly
            visible = visible && center.Z + radius > CamNear && center.Z - radius < CamFar;

            return visible;
        }

        public unsafe void ExecuteCulling(IGraphicsContext context, Camera camera, int instanceCount, int typeCount, DepthMipChain mipChain, ICPUProfiler? profiler)
        {
            if (instanceCount == 0)
            {
                return;
            }

            Matrix4x4 projection = camera.Transform.Projection;

            Matrix4x4 projectionT = Matrix4x4.Transpose(projection);

            Vector4 frustumX = MathUtil.NormalizePlane(projectionT.GetRow(3) + projectionT.GetRow(0)); // x + w < 0
            Vector4 frustumY = MathUtil.NormalizePlane(projectionT.GetRow(3) + projectionT.GetRow(1)); // y + w < 0

            occlusionParamBuffer[0] = new()
            {
                OcclusionCulling = (cullingFlags & CullingFlags.Occlusion) != 0 ? 1 : 0,
                FrustumCulling = (cullingFlags & CullingFlags.Frustum) != 0 ? 1 : 0,
                NumberOfInstances = (uint)instanceCount,
                NumberOfPropTypes = (uint)typeCount,
                MaxMipLevel = (uint)mipChain.MipLevels,

                Frustum = new(frustumX.X, frustumX.Z, frustumY.Y, frustumY.Z),
                P00 = projectionT.M11,
                P11 = projectionT.M22,
                DepthBias = depthBias
            };
            occlusionParamBuffer.Update(context);

            var bindings = culling.Bindings;

            bindings.SetSRV("instanceDataIn", instanceDataBuffer.SRV);

            bindings.SetUAV("instanceDataOut", instanceDataOutBuffer.UAV);
            bindings.SetUAV("instanceOffsets", instanceOffsets.UAV);
            bindings.SetUAV("drawArgs", swapBuffer.UAV);
            bindings.SetUAV("visibleListOut", visibleListBuffer.UAV);

            culling.Bindings.SetSRV("inputRT", mipChain.SRV);
            context.SetComputePipelineState(culling);
            context.Dispatch((uint)instanceCount / 1024 + 1, 1, 1);
            context.SetComputePipelineState(null);

            swapBuffer.CopyTo(context, drawIndirectArgs.Buffer);

            if ((cullingFlags & CullingFlags.Debug) == 0)
            {
                return;
            }

            swapBuffer.Read(context);

            uint drawCalls = 0;
            uint drawInstanceCount = 0;
            uint vertexCount = 0;

            for (int i = 0; i < this.context.TypeCount; i++)
            {
                vertexCount += swapBuffer[i].IndexCountPerInstance * swapBuffer[i].InstanceCount;
                drawInstanceCount += swapBuffer[i].InstanceCount;
                drawCalls += swapBuffer[i].InstanceCount > 0 ? 1u : 0u;
            }

            stats.DrawCalls = (uint)typeCount;
            stats.DrawInstanceCount = (uint)instanceCount;

            stats.ActualDrawCalls = drawCalls;
            stats.ActualDrawInstanceCount = drawInstanceCount;

            stats.VertexCount = vertexCount;

            stats.Instances = instanceDataBuffer.Items;
            stats.InstanceCount = instanceDataBuffer.Count;

            stats.Args = swapBuffer.Items;
            stats.ArgsCount = swapBuffer.Count;
        }

        public void Release()
        {
            resizedEventHandlers.Clear();

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
            visibleListBuffer.Dispose();
            visibleListBuffer = null;

            instanceOffsetsNoCull.Dispose();
            instanceOffsetsNoCull = null;
            instanceDataNoCull.Dispose();
            instanceDataNoCull = null;

            occlusionParamBuffer.Dispose();
            occlusionParamBuffer = null;
            culling.Dispose();
            culling = null;

            sampler.Dispose();
            sampler = null;
        }
    }
}