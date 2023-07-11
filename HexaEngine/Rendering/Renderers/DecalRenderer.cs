namespace HexaEngine.Rendering.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Meshes;
    using System.Numerics;

    public struct GPUDecal
    {
        public int Type;
        public Vector3 padd;

        public GPUDecal(Decal decal)
        {
            Type = (int)decal.Type;
        }
    }

    public class DecalRenderer : IDisposable
    {
        private readonly Cube cube;
        private readonly IGraphicsPipeline pipeline;
        private readonly IGraphicsPipeline pipelineModifyNormals;
        private readonly ISamplerState linearWrapSampler;
        private readonly ISamplerState pointClampSampler;
        private readonly ConstantBuffer<CBWorld> worldBuffer;
        private readonly ConstantBuffer<GPUDecal> decalBuffer;

        private readonly ResourceRef<IBuffer> camera;
        private readonly ResourceRef<IShaderResourceView> depth;

        public DecalRenderer(IGraphicsDevice device)
        {
            cube = new Cube(device);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "deferred/decal/vs.hlsl",
                PixelShader = "deferred/decal/ps.hlsl"
            }, new GraphicsPipelineState()
            {
                Rasterizer = RasterizerDescription.CullNone
            });
            pipelineModifyNormals = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "deferred/decal/vs.hlsl",
                PixelShader = "deferred/decal/ps.hlsl"
            }, new GraphicsPipelineState()
            {
                Rasterizer = RasterizerDescription.CullNone
            },
            new ShaderMacro[] { new("DECAL_MODIFY_NORMALS", 1) });
            linearWrapSampler = device.CreateSamplerState(SamplerDescription.LinearWrap);
            pointClampSampler = device.CreateSamplerState(SamplerDescription.PointClamp);
            worldBuffer = new(device, CpuAccessFlags.Write);
            decalBuffer = new(device, CpuAccessFlags.Write);

            camera = ResourceManager2.Shared.GetBuffer("CBCamera");
            depth = ResourceManager2.Shared.GetShaderResourceView("GBuffer.Depth");
        }

        public unsafe void Draw(IGraphicsContext context, Decal decal)
        {
            worldBuffer.Update(context, new(decal.Transform));
            decalBuffer.Update(context, new(decal));

            nint* cbs = stackalloc nint[] { worldBuffer.NativePointer, camera.Value.NativePointer };
            nint* srvs = stackalloc nint[] { decal.AlbedoDecalTexture?.SRV?.NativePointer ?? 0, decal.NormalDecalTexture?.SRV?.NativePointer ?? 0 };
            nint* sps = stackalloc nint[] { linearWrapSampler.NativePointer, pointClampSampler.NativePointer };
            context.VSSetConstantBuffers(0, 2, (void**)cbs);
            cbs[0] = decalBuffer.NativePointer;
            context.PSSetConstantBuffers(0, 2, (void**)cbs);
            context.PSSetShaderResources(0, 2, (void**)srvs);
            context.PSSetSamplers(0, 2, (void**)sps);

            context.SetGraphicsPipeline(decal.ModifyGBufferNormals ? pipelineModifyNormals : pipeline);
            cube.DrawAuto(context);
            context.SetGraphicsPipeline(null);

            ZeroMemory(srvs, sizeof(nint) * 2);
            ZeroMemory(cbs, sizeof(nint) * 2);
            ZeroMemory(sps, sizeof(nint) * 2);

            context.PSSetSamplers(0, 2, (void**)sps);
            context.PSSetShaderResources(0, 2, (void**)srvs);
            context.PSSetConstantBuffers(0, 2, (void**)cbs);
            context.VSSetConstantBuffers(0, 2, (void**)cbs);
        }

        public void Dispose()
        {
            cube.Dispose();
            pipeline.Dispose();
            linearWrapSampler.Dispose();
            pointClampSampler.Dispose();
            worldBuffer.Dispose();
            decalBuffer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}