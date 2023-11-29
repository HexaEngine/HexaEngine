namespace HexaEngine.Editor.MeshEditor.Rendering
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Resources;
    using System.Numerics;

    public unsafe class MeshEditorMaterial : IDisposable
    {
        public MaterialData Data;
        public IGraphicsPipeline Basic;
        public IGraphicsPipeline Textured;
        public IGraphicsPipeline Shaded;

        public TextureList TextureList = new();

        private bool disposedValue;

        public MeshEditorMaterial(IGraphicsDevice device, MeshData mesh, MaterialData material)
        {
            Data = material;

            InputElementDescription[] inputElements = MeshData.InputElements;

            ShaderMacro[] macros = material.GetShaderMacros();

            Basic = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "tools/mesh/basic/vs.hlsl",
                PixelShader = "tools/mesh/basic/ps.hlsl",
                State = new GraphicsPipelineState()
                {
                    Blend = BlendDescription.Opaque,
                    BlendFactor = Vector4.Zero,
                    DepthStencil = DepthStencilDescription.DefaultLess,
                    Rasterizer = RasterizerDescription.CullNone,
                    Topology = PrimitiveTopology.TriangleList,
                    SampleMask = uint.MaxValue,
                    StencilRef = 0,
                },
                InputElements = inputElements,
                Macros = macros
            });

            Textured = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "tools/mesh/textured/vs.hlsl",
                PixelShader = "tools/mesh/textured/ps.hlsl",
                State = new GraphicsPipelineState()
                {
                    Blend = BlendDescription.Opaque,
                    BlendFactor = Vector4.Zero,
                    DepthStencil = DepthStencilDescription.DefaultLess,
                    Rasterizer = RasterizerDescription.CullNone,
                    Topology = PrimitiveTopology.TriangleList,
                    SampleMask = uint.MaxValue,
                    StencilRef = 0,
                },
                InputElements = inputElements,
                Macros = macros
            });

            Shaded = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "tools/mesh/shaded/vs.hlsl",
                PixelShader = "tools/mesh/shaded/ps.hlsl",
                State = new()
                {
                    Blend = BlendDescription.Opaque,
                    BlendFactor = Vector4.Zero,
                    DepthStencil = DepthStencilDescription.DefaultLess,
                    Rasterizer = RasterizerDescription.CullNone,
                    Topology = PrimitiveTopology.TriangleList,
                    SampleMask = uint.MaxValue,
                    StencilRef = 0,
                },
                InputElements = inputElements,
                Macros = macros
            });

            for (int i = 0; i < material.Textures.Count; i++)
            {
                var desc = material.Textures[i];
                string path = Paths.CurrentTexturePath + desc.File;
                var tex = new Texture2D(device, new TextureFileDescription(path));
                var sampler = device.CreateSamplerState(desc.GetSamplerDesc());
                TextureList.Add(new(tex, sampler, desc));
            }
        }

        public void DrawBasic(IGraphicsContext context, MeshEditorMesh mesh, uint instanceCount)
        {
            if (!Basic.IsReady)
                return;

            context.SetGraphicsPipeline(Basic);
            mesh.Draw(context, instanceCount);
            nint* temp = stackalloc nint[(int)TextureList.SlotCount];
            context.SetGraphicsPipeline(null);
        }

        public void DrawTextured(IGraphicsContext context, MeshEditorMesh mesh, uint instanceCount)
        {
            if (!Textured.IsReady)
                return;

            context.SetGraphicsPipeline(Textured);
            context.PSSetSamplers(0, TextureList.SlotCount, TextureList.Samplers);
            context.PSSetShaderResources(0, TextureList.SlotCount, TextureList.ShaderResourceViews);
            mesh.Draw(context, instanceCount);
            nint* temp = stackalloc nint[(int)TextureList.SlotCount];
            context.PSSetShaderResources(0, TextureList.SlotCount, (void**)temp);
            context.PSSetSamplers(0, TextureList.SlotCount, (void**)temp);
            context.SetGraphicsPipeline(null);
        }

        public void DrawShaded(IGraphicsContext context, MeshEditorMesh mesh, uint instanceCount)
        {
            if (!Shaded.IsReady)
                return;

            context.SetGraphicsPipeline(Shaded);
            context.PSSetSamplers(0, TextureList.SlotCount, TextureList.Samplers);
            context.PSSetShaderResources(0, TextureList.SlotCount, TextureList.ShaderResourceViews);
            mesh.Draw(context, instanceCount);
            nint* temp = stackalloc nint[(int)TextureList.SlotCount];
            context.PSSetShaderResources(0, TextureList.SlotCount, (void**)temp);
            context.PSSetSamplers(0, TextureList.SlotCount, (void**)temp);
            context.SetGraphicsPipeline(null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Basic.Dispose();
                Textured.Dispose();
                Shaded.Dispose();

                for (int i = 0; i < TextureList.Count; i++)
                {
                    TextureList[i].Dispose();
                }

                TextureList.Dispose();
                disposedValue = true;
            }
        }

        ~MeshEditorMaterial()
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
    }
}