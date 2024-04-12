namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Graphics.Graph;
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
        private readonly IGraphicsPipelineState pipeline;
        private readonly IGraphicsPipelineState pipelineModifyNormals;
        private readonly ISamplerState linearWrapSampler;
        private readonly ISamplerState pointClampSampler;
        private readonly ConstantBuffer<CBWorld> worldBuffer;

        private readonly ConstantBuffer<GPUDecal> decalBuffer;

        private readonly ResourceRef<DepthStencil> depth;

        public DecalRenderer(IGraphicsDevice device)
        {
            cube = new Cube();
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "deferred/decal/vs.hlsl",
                PixelShader = "deferred/decal/ps.hlsl",
            }, new()
            {
                Rasterizer = RasterizerDescription.CullNone
            });
            pipelineModifyNormals = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "deferred/decal/vs.hlsl",
                PixelShader = "deferred/decal/ps.hlsl",
                Macros = [new("DECAL_MODIFY_NORMALS", 1)]
            }, new()
            {
                Rasterizer = RasterizerDescription.CullNone
            });
            linearWrapSampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);
            pointClampSampler = device.CreateSamplerState(SamplerStateDescription.PointClamp);
            worldBuffer = new(CpuAccessFlags.Write);
            decalBuffer = new(CpuAccessFlags.Write);

            depth = SceneRenderer.Current.ResourceBuilder.GetDepthStencilBuffer("#DepthStencil");
        }

        public unsafe void Draw(IGraphicsContext context, Decal decal)
        {
            worldBuffer.Update(context, new(decal.Transform));
            decalBuffer.Update(context, new(decal));

            nint* cbs = stackalloc nint[] { worldBuffer.NativePointer };
            nint* srvs = stackalloc nint[] { decal.AlbedoDecalTexture?.SRV?.NativePointer ?? 0, decal.NormalDecalTexture?.SRV?.NativePointer ?? 0 };
            nint* sps = stackalloc nint[] { linearWrapSampler.NativePointer, pointClampSampler.NativePointer };
            context.VSSetConstantBuffers(0, 1, (void**)cbs);
            cbs[0] = decalBuffer.NativePointer;
            context.PSSetConstantBuffers(0, 1, (void**)cbs);
            context.PSSetShaderResources(0, 2, (void**)srvs);
            context.PSSetSamplers(0, 2, (void**)sps);

            context.SetPipelineState(decal.ModifyGBufferNormals ? pipelineModifyNormals : pipeline);
            cube.DrawAuto(context);
            context.SetPipelineState(null);

            ZeroMemory(srvs, sizeof(nint) * 2);
            ZeroMemory(cbs, sizeof(nint) * 2);
            ZeroMemory(sps, sizeof(nint) * 2);

            context.PSSetSamplers(0, 2, (void**)sps);
            context.PSSetShaderResources(0, 2, (void**)srvs);
            context.PSSetConstantBuffers(0, 1, (void**)cbs);
            context.VSSetConstantBuffers(0, 1, (void**)cbs);
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