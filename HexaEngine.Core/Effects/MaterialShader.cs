#nullable disable

namespace HexaEngine.Pipelines.Deferred
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Meshes;
    using System.Threading.Tasks;

    public class MaterialShader : IDisposable
    {
        private IGraphicsPipeline pipeline;
        private IGraphicsPipeline depthOnly;

        private bool disposedValue;

        public async Task InitializeAsync(IGraphicsDevice device, MaterialData data)
        {
            disposedValue = false;

            var macros = data.GetShaderMacros();
            var flags = data.Flags;
            GraphicsPipelineDesc pipelineDesc = new()
            {
                VertexShader = "deferred/geometry/vs.hlsl",
                PixelShader = "deferred/geometry/ps.hlsl"
            };

            GraphicsPipelineState pipelineState = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            if ((flags & MaterialFlags.Tessellation) != 0)
            {
                Array.Resize(ref macros, macros.Length + 1);
                macros[^1] = new("Tessellation", "1");
                pipelineDesc.HullShader = "deferred/geometry/hs.hlsl";
                pipelineDesc.DomainShader = "deferred/geometry/ds.hlsl";
                pipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
            }

            pipeline = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState,
            new InputElementDescription[]
            {
                new("POSITION", 0, Format.RGB32Float, 0),
                new("TEXCOORD", 0, Format.RG32Float, 0),
                new("NORMAL", 0, Format.RGB32Float, 0),
                new("TANGENT", 0, Format.RGB32Float, 0),
            }, macros);

            pipelineDesc.PixelShader = null;
            depthOnly = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState,
            new InputElementDescription[]
            {
                new("POSITION", 0, Format.RGB32Float, 0),
                new("TEXCOORD", 0, Format.RG32Float, 0),
                new("NORMAL", 0, Format.RGB32Float, 0),
                new("TANGENT", 0, Format.RGB32Float, 0),
            }, macros);
        }

        public void Initialize(IGraphicsDevice device, MaterialData data)
        {
            disposedValue = false;
            var macros = data.GetShaderMacros();
            var flags = data.Flags;
            GraphicsPipelineDesc pipelineDesc = new()
            {
                VertexShader = "deferred/geometry/vs.hlsl",
                PixelShader = "deferred/geometry/ps.hlsl"
            };

            GraphicsPipelineState pipelineState = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            if ((flags & MaterialFlags.Tessellation) != 0)
            {
                Array.Resize(ref macros, macros.Length + 1);
                macros[^1] = new("Tessellation", "1");
                pipelineDesc.HullShader = "deferred/geometry/hs.hlsl";
                pipelineDesc.DomainShader = "deferred/geometry/ds.hlsl";
                pipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
            }

            pipeline = device.CreateGraphicsPipeline(pipelineDesc, pipelineState,
            new InputElementDescription[]
            {
                new("POSITION", 0, Format.RGB32Float, 0),
                new("TEXCOORD", 0, Format.RG32Float, 0),
                new("NORMAL", 0, Format.RGB32Float, 0),
                new("TANGENT", 0, Format.RGB32Float, 0),
            }, macros);

            pipelineDesc.PixelShader = null;
            depthOnly = device.CreateGraphicsPipeline(pipelineDesc, pipelineState,
            new InputElementDescription[]
            {
                new("POSITION", 0, Format.RGB32Float, 0),
                new("TEXCOORD", 0, Format.RG32Float, 0),
                new("NORMAL", 0, Format.RGB32Float, 0),
                new("TANGENT", 0, Format.RGB32Float, 0),
            }, macros);
        }

        public bool BeginDraw(IGraphicsContext context, IBuffer camera)
        {
            if (!pipeline.IsValid)
                return false;
            pipeline.BeginDraw(context);
            context.DSSetConstantBuffer(camera, 1);
            context.VSSetConstantBuffer(camera, 1);
            return true;
        }

        public bool BeginDrawDepth(IGraphicsContext context, IBuffer camera)
        {
            if (!depthOnly.IsValid)
                return false;
            context.DSSetConstantBuffer(camera, 1);
            context.VSSetConstantBuffer(camera, 1);
            depthOnly.BeginDraw(context);
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline.Dispose();
                depthOnly.Dispose();
                disposedValue = true;
            }
        }

        ~MaterialShader()
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