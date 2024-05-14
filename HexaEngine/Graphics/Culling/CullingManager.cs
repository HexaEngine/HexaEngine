namespace HexaEngine.Graphics.Culling
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class CullingManager
    {
#nullable disable
        private readonly IGraphicsDevice device;
        private CullingFlags cullingFlags = CullingFlags.All;
        private float depthBias = 0.0f;

        private IComputePipeline culling;

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

        private ConstantBuffer<OcclusionParams> occlusionParamBuffer;
        private SamplerState sampler;
        private unsafe void** cullingSRVs;
        private unsafe void** cullingUAVs;
        private unsafe void** cullingCBVs;
        private readonly CullingContext context;
#nullable enable

        public unsafe CullingManager(IGraphicsDevice device)
        {
            this.device = device;
            culling = device.CreateComputePipeline(new()
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
            sampler = new(new(Filter.MaximumMinMagLinearMipPoint, TextureAddressMode.Clamp));
            cullingCBVs = AllocArray(2);
            cullingCBVs[0] = (void*)occlusionParamBuffer.Buffer.NativePointer;
            cullingCBVs[1] = (void*)occlusionCameraBuffer.Buffer.NativePointer;
            cullingUAVs = AllocArray(4);
            cullingUAVs[0] = (void*)instanceDataOutBuffer.UAV.NativePointer;
            cullingUAVs[1] = (void*)instanceOffsets.UAV.NativePointer;
            cullingUAVs[2] = (void*)swapBuffer.UAV.NativePointer;
            cullingUAVs[3] = (void*)visibleListBuffer.UAV.NativePointer;
            cullingSRVs = AllocArray(2);
            cullingSRVs[1] = (void*)instanceDataBuffer.SRV.NativePointer;
            context = new(instanceOffsetsNoCull, instanceDataNoCull, instanceOffsets, instanceDataOutBuffer, typeDataBuffer, instanceDataBuffer, swapBuffer, drawIndirectArgs, visibleListBuffer);
        }

        public static CullingManager Current { get; internal set; } = new(Application.GraphicsDevice);

        public CullingContext Context => context;

        public CullingFlags CullingFlags { get => cullingFlags; set => cullingFlags = value; }

        public float DepthBias { get => depthBias; set => depthBias = value; }

        public StructuredBuffer<uint> InstanceOffsetsNoCull => instanceOffsetsNoCull;

        public StructuredBuffer<Matrix4x4> InstanceDataNoCull => instanceDataNoCull;

        public ConstantBuffer<CBCamera> OcclusionCameraBuffer => occlusionCameraBuffer;

        public StructuredUavBuffer<uint> InstanceOffsets => instanceOffsets;

        public StructuredUavBuffer<Matrix4x4> InstanceDataOutBuffer => instanceDataOutBuffer;

        public StructuredBuffer<GPUInstance> InstanceDataBuffer => instanceDataBuffer;

        public StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> SwapBuffer => swapBuffer;

        public DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> DrawIndirectArgs => drawIndirectArgs;

        public void UpdateCamera(IGraphicsContext context, Camera camera, Viewport viewport)
        {
            occlusionCameraBuffer.Update(context, new(camera, new(viewport.Width, viewport.Height)));
        }

        private static Vector3 ExtractScale(Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.X = new Vector3(matrix.M11, matrix.M12, matrix.M13).Length();
            scale.Y = new Vector3(matrix.M21, matrix.M22, matrix.M23).Length();
            scale.Z = new Vector3(matrix.M31, matrix.M32, matrix.M33).Length();

            float det = matrix.GetDeterminant();
            if (det < 0)
            {
                scale.X = -scale.X;
            }

            return scale;
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

            cullingUAVs[0] = (void*)instanceDataOutBuffer.UAV.NativePointer;
            cullingUAVs[1] = (void*)instanceOffsets.UAV.NativePointer;
            cullingUAVs[2] = (void*)swapBuffer.UAV.NativePointer;
            cullingUAVs[3] = (void*)visibleListBuffer.UAV.NativePointer;

            cullingSRVs[1] = (void*)instanceDataBuffer.SRV.NativePointer;
            cullingSRVs[0] = (void*)mipChain.SRV.NativePointer;

            uint* initialCount = stackalloc uint[] { unchecked((uint)-1), unchecked((uint)-1), unchecked((uint)-1) };
            context.CSSetShaderResources(0, 2, cullingSRVs);
            context.CSSetUnorderedAccessViews(4, cullingUAVs, initialCount);
            context.CSSetSampler(0, sampler);
            context.CSSetConstantBuffers(0, 2, cullingCBVs);
            culling.Dispatch(context, (uint)instanceCount / 1024 + 1, 1, 1);
            context.ClearState();

            swapBuffer.CopyTo(context, drawIndirectArgs.Buffer);

            if ((cullingFlags & CullingFlags.Debug) == 0)
            {
                return;
            }

            swapBuffer.Read(context);

            uint drawCalls = 0;
            uint drawInstanceCount = 0;
            uint vertexCount = 0;

            ImGui.InputFloat("Depth Bias", ref depthBias);

            ImGui.Checkbox("Draw Bounding Spheres", ref drawBoundingSpheres);

            if (drawBoundingSpheres)
            {
                for (int i = 0; i < instanceCount; i++)
                {
                    var instance = instanceDataBuffer[i];
                    var world = Matrix4x4.Transpose(instance.World);

                    var center = Vector3.Transform(instance.BoundingSphere.Center, world);
                    var radius = instance.BoundingSphere.Radius * ExtractScale(world).Length();
                    DebugDraw.DrawSphere(center, default, radius, Colors.White);
                }
            }

            for (int i = 0; i < this.context.TypeCount; i++)
            {
                vertexCount += swapBuffer[i].IndexCountPerInstance * swapBuffer[i].InstanceCount;
                drawInstanceCount += swapBuffer[i].InstanceCount;
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
            };

            ImGui.Text($"Draw Calls: {typeCount}/{drawCalls}, Instances: {instanceCount}/{drawInstanceCount}, Vertices: {vertexCount}{suffix}");

            var flags = (int)cullingFlags;
            ImGui.CheckboxFlags("Frustum Culling", ref flags, (int)CullingFlags.Frustum);
            ImGui.CheckboxFlags("Occlusion Culling", ref flags, (int)CullingFlags.Occlusion);
            cullingFlags = (CullingFlags)flags;
        }

        private bool drawBoundingSpheres = false;

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
            Free(cullingSRVs);
            cullingSRVs = null;
            Free(cullingUAVs);
            cullingUAVs = null;
            Free(cullingCBVs);
            cullingCBVs = null;
        }
    }
}