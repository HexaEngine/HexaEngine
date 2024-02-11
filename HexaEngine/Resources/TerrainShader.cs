#nullable disable

namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Terrains;
    using HexaEngine.Lights;
    using System.Numerics;
    using System.Threading.Tasks;

    public class TerrainShader : IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly bool alphaBlend;
        private readonly ShaderMacro[] macros;
        private IGraphicsPipelineState forward;
        private IGraphicsPipelineState deferred;
        private IGraphicsPipelineState depthOnly;
        private IGraphicsPipelineState csm;
        private IGraphicsPipelineState osm;
        private IGraphicsPipelineState psm;
        private volatile bool initialized;
        private bool disposedValue;
        private MaterialShaderFlags flags;

        public TerrainShader(IGraphicsDevice device, ShaderMacro[] macros, bool alphaBlend)
        {
            this.device = device;
            this.alphaBlend = alphaBlend;
            this.macros = [.. macros, new("CLUSTERED_FORWARD", "1")];
        }

        public MaterialShaderFlags Flags => flags;

        public void Initialize()
        {
            Compile();
        }

        public async Task InitializeAsync()
        {
            Compile();
        }

        private void Compile()
        {
            flags = 0;
            var elements = TerrainCellData.InputElements;

            bool twoSided = false;

            bool blendFunc = alphaBlend;

            RasterizerDescription rasterizer = RasterizerDescription.CullBack;
            if (twoSided)
            {
                rasterizer = RasterizerDescription.CullNone;
            }

            BlendDescription blend = BlendDescription.Opaque;
            if (blendFunc)
            {
                blend = BlendDescription.AlphaBlend;
            }

            GraphicsPipelineDesc pipelineDescForward = new()
            {
                VertexShader = $"forward/terrain/vs.hlsl",
                PixelShader = $"forward/terrain/ps.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc pipelineStateDescForward = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = rasterizer,
                Blend = blend,
                Topology = PrimitiveTopology.TriangleList,
                BlendFactor = Vector4.One,
                InputElements = elements
            };

            GraphicsPipelineDesc pipelineDescDeferred = new()
            {
                VertexShader = $"deferred/terrain/vs.hlsl",
                PixelShader = $"deferred/terrain/ps.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc pipelineStateDescDeferred = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = rasterizer,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
                InputElements = elements,
            };

            forward = device.CreateGraphicsPipelineState(pipelineDescForward, pipelineStateDescForward);

            deferred = device.CreateGraphicsPipelineState(pipelineDescDeferred, pipelineStateDescDeferred);

            pipelineDescDeferred.VertexShader = "forward/terrain/depthVS.hlsl";
            pipelineDescDeferred.PixelShader = "forward/terrain/depthPS.hlsl";

            depthOnly = device.CreateGraphicsPipelineState(pipelineDescDeferred, pipelineStateDescDeferred);

            GraphicsPipelineDesc csmPipelineDesc = new()
            {
                VertexShader = "forward/terrain/csm/vs.hlsl",
                GeometryShader = "forward/terrain/csm/gs.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc csmPipelineStateDesc = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
                InputElements = elements
            };

            GraphicsPipelineDesc osmPipelineDesc = new()
            {
                VertexShader = "forward/terrain/osm/vs.hlsl",
                PixelShader = "forward/terrain/osm/ps.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc osmPipelineStateDesc = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
                InputElements = elements
            };

            GraphicsPipelineDesc psmPipelineDesc = new()
            {
                VertexShader = "forward/terrain/psm/vs.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc psmPipelineStateDesc = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullFront,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
                InputElements = elements,
            };

            csm = device.CreateGraphicsPipelineState(csmPipelineDesc, csmPipelineStateDesc);
            osm = device.CreateGraphicsPipelineState(osmPipelineDesc, osmPipelineStateDesc);
            psm = device.CreateGraphicsPipelineState(psmPipelineDesc, psmPipelineStateDesc);
            flags |= MaterialShaderFlags.Shadow | MaterialShaderFlags.DepthTest;

            initialized = true;
        }

        public void Recompile()
        {
            initialized = false;
            deferred.Dispose();
            depthOnly.Dispose();
            csm.Dispose();
            osm.Dispose();
            psm.Dispose();
            Compile();
        }

        public async Task RecompileAsync()
        {
            initialized = false;
            deferred.Dispose();
            depthOnly.Dispose();
            csm.Dispose();
            osm.Dispose();
            psm.Dispose();
            Compile();
        }

        public bool BeginDrawForward(IGraphicsContext context)
        {
            if (!initialized)
            {
                return false;
            }

            if (!forward.IsValid)
            {
                return false;
            }

            context.SetPipelineState(forward);
            return true;
        }

        public bool BeginDrawForward(IGraphicsContext context, IBuffer camera)
        {
            if (!initialized)
            {
                return false;
            }

            if (!forward.IsValid)
            {
                return false;
            }

            context.SetPipelineState(forward);
            context.PSSetConstantBuffer(1, camera);
            context.DSSetConstantBuffer(1, camera);
            context.VSSetConstantBuffer(1, camera);
            return true;
        }

        public void EndDrawForward(IGraphicsContext context)
        {
            context.SetPipelineState(null);
            context.PSSetConstantBuffer(1, null);
            context.DSSetConstantBuffer(1, null);
            context.VSSetConstantBuffer(1, null);
        }

        public bool BeginDrawDeferred(IGraphicsContext context)
        {
            if (!initialized)
            {
                return false;
            }

            if (!deferred.IsValid)
            {
                return false;
            }

            context.SetPipelineState(deferred);
            return true;
        }

        public bool BeginDrawDeferred(IGraphicsContext context, IBuffer camera)
        {
            if (!initialized)
            {
                return false;
            }

            if (!deferred.IsValid)
            {
                return false;
            }

            context.SetPipelineState(deferred);
            context.DSSetConstantBuffer(1, camera);
            context.VSSetConstantBuffer(1, camera);
            return true;
        }

        public void EndDrawDeferred(IGraphicsContext context)
        {
            context.SetPipelineState(null);
            context.DSSetConstantBuffer(1, null);
            context.VSSetConstantBuffer(1, null);
        }

        public bool BeginDrawDepth(IGraphicsContext context, IBuffer camera)
        {
            if (!initialized)
            {
                return false;
            }

            if (!depthOnly.IsValid)
            {
                return false;
            }

            context.DSSetConstantBuffer(1, camera);
            context.VSSetConstantBuffer(1, camera);
            context.SetPipelineState(depthOnly);
            return true;
        }

        public bool BeginDrawDepth(IGraphicsContext context)
        {
            if (!initialized)
            {
                return false;
            }

            if (!depthOnly.IsValid)
            {
                return false;
            }

            context.SetPipelineState(depthOnly);
            return true;
        }

        public void EndDrawDepth(IGraphicsContext context)
        {
            context.SetPipelineState(null);
            context.DSSetConstantBuffer(1, null);
            context.VSSetConstantBuffer(1, null);
        }

        public bool BeginDrawShadow(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!initialized)
            {
                return false;
            }

            context.DSSetConstantBuffer(1, light);
            context.VSSetConstantBuffer(1, light);
            context.GSSetConstantBuffer(1, light);
            switch (type)
            {
                case ShadowType.Perspective:
                    if (!psm.IsValid)
                    {
                        return false;
                    }

                    context.SetPipelineState(psm);
                    return true;

                case ShadowType.Cascaded:
                    if (!csm.IsValid)
                    {
                        return false;
                    }

                    context.SetPipelineState(csm);
                    return true;

                case ShadowType.Omni:
                    if (!osm.IsValid)
                    {
                        return false;
                    }

                    context.SetPipelineState(osm);
                    return true;
            }

            return false;
        }

        public void EndDrawShadow(IGraphicsContext context)
        {
            context.SetPipelineState(null);
            context.DSSetConstantBuffer(1, null);
            context.VSSetConstantBuffer(1, null);
            context.GSSetConstantBuffer(1, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                forward?.Dispose();
                deferred?.Dispose();
                depthOnly?.Dispose();
                csm?.Dispose();
                osm?.Dispose();
                psm?.Dispose();
                disposedValue = true;
            }
        }

        ~TerrainShader()
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