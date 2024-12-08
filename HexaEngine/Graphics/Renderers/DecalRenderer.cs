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

            pipeline.Bindings.SetCBV("DecalBuffer", decalBuffer);
            pipeline.Bindings.SetCBV("WorldBuffer", worldBuffer);
            pipeline.Bindings.SetSampler("linearWrapSampler", linearWrapSampler);
            pipeline.Bindings.SetSampler("pointClampSampler", pointClampSampler);

            pipelineModifyNormals.Bindings.SetCBV("DecalBuffer", decalBuffer);
            pipelineModifyNormals.Bindings.SetCBV("WorldBuffer", worldBuffer);
            pipelineModifyNormals.Bindings.SetSampler("linearWrapSampler", linearWrapSampler);
            pipelineModifyNormals.Bindings.SetSampler("pointClampSampler", pointClampSampler);

            depth = SceneRenderer.Current.ResourceBuilder.GetDepthStencilBuffer("#DepthStencil");
        }

        public unsafe void Draw(IGraphicsContext context, Decal decal)
        {
            worldBuffer.Update(context, new(decal.Transform));
            decalBuffer.Update(context, new(decal));

            IGraphicsPipelineState pso = decal.ModifyGBufferNormals ? pipelineModifyNormals : pipeline;

            pso.Bindings.SetSRV("baseColorTex", decal.AlbedoDecalTexture?.SRV);
            pso.Bindings.SetSRV("normalTex", decal.NormalDecalTexture?.SRV);

            context.SetGraphicsPipelineState(pso);
            cube.DrawAuto(context);
            context.SetGraphicsPipelineState(null);
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