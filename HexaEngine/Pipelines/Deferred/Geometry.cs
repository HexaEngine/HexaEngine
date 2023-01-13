#nullable disable

namespace HexaEngine.Pipelines.Deferred
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using System.Threading.Tasks;

    public class Geometry : IEffect
    {
        private IGraphicsPipeline pipeline;
        private IGraphicsPipeline depthOnly;
        private IGraphicsPipeline depthFront;
        private IGraphicsPipeline depthBack;
        public IBuffer Camera;
        private bool disposedValue;

        public Task Initialize(IGraphicsDevice device, int width, int height)
        {
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "deferred/prepass/vs.hlsl",
                HullShader = "deferred/prepass/hs.hlsl",
                DomainShader = "deferred/prepass/ds.hlsl",
                PixelShader = "deferred/prepass/ps.hlsl"
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            },
            new InputElementDescription[]
            {
                new("POSITION", 0, Format.RGB32Float, 0),
                new("TEXCOORD", 0, Format.RG32Float, 0),
                new("NORMAL", 0, Format.RGB32Float, 0),
                new("TANGENT", 0, Format.RGB32Float, 0),
                       },
            new ShaderMacro[]
            {
                new("INSTANCED", 1)
            });

            depthOnly = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "deferred/prepass/vs.hlsl",
                HullShader = "deferred/prepass/hs.hlsl",
                DomainShader = "deferred/prepass/ds.hlsl",
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            },
            new InputElementDescription[]
            {
                new("POSITION", 0, Format.RGB32Float, 0),
                new("TEXCOORD", 0, Format.RG32Float, 0),
                new("NORMAL", 0, Format.RGB32Float, 0),
                new("TANGENT", 0, Format.RGB32Float, 0),
            },
            new ShaderMacro[]
            {
                new("INSTANCED", 1)
            });

            depthBack = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "deferred/prepass/vs.hlsl",
                HullShader = "deferred/prepass/hs.hlsl",
                DomainShader = "deferred/prepass/ds.hlsl",
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullFront,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            },
            new ShaderMacro[]
            {
                new("DEPTH", 1), new("INSTANCED", 1)
            });

            depthFront = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "deferred/prepass/vs.hlsl",
                HullShader = "deferred/prepass/hs.hlsl",
                DomainShader = "deferred/prepass/ds.hlsl",
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            },
            new ShaderMacro[]
            {
                new("DEPTH", 1), new("INSTANCED", 1)
            });

            return Task.CompletedTask;
        }

        public void BeginDraw(IGraphicsContext context, Viewport viewport)
        {
            pipeline.BeginDraw(context, viewport);
            context.DSSetConstantBuffer(Camera, 1);
        }

        public void BeginDrawDepth(IGraphicsContext context, IBuffer camera, Viewport viewport)
        {
            context.DSSetConstantBuffer(camera, 1);
            depthOnly.BeginDraw(context, viewport);
        }

        public void BeginDrawDepthFront(IGraphicsContext context, Viewport viewport)
        {
            depthFront.BeginDraw(context, viewport);
            context.DSSetConstantBuffer(Camera, 1);
        }

        public void BeginDrawDepthBack(IGraphicsContext context, Viewport viewport)
        {
            depthBack.BeginDraw(context, viewport);
            context.DSSetConstantBuffer(Camera, 1);
        }

        public void Draw(IGraphicsContext context)
        {
        }

        public void BeginResize()
        {
        }

        public void EndResize(int width, int height)
        {
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

        ~Geometry()
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