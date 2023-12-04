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
        private IGraphicsPipeline forward;
        private IGraphicsPipeline deferred;
        private IGraphicsPipeline depthOnly;
        private IGraphicsPipeline csm;
        private IGraphicsPipeline osm;
        private IGraphicsPipeline psm;
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
            await CompileAsync();
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
                State = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = blend,
                    Topology = PrimitiveTopology.TriangleList,
                    BlendFactor = Vector4.One
                },
                InputElements = elements,
                Macros = macros,
            };

            GraphicsPipelineDesc pipelineDescDeferred = new()
            {
                VertexShader = $"deferred/terrain/vs.hlsl",
                PixelShader = $"deferred/terrain/ps.hlsl",
                State = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                },
                InputElements = elements,
                Macros = macros,
            };

            forward = device.CreateGraphicsPipeline(pipelineDescForward);

            deferred = device.CreateGraphicsPipeline(pipelineDescDeferred);

            pipelineDescDeferred.VertexShader = "forward/terrain/depthVS.hlsl";
            pipelineDescDeferred.PixelShader = "forward/terrain/depthPS.hlsl";
            depthOnly = device.CreateGraphicsPipeline(pipelineDescDeferred);

            var csmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "forward/terrain/csm/vs.hlsl",
                GeometryShader = "forward/terrain/csm/gs.hlsl",
                State = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullNone,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                },
                InputElements = elements,
                Macros = macros,
            };

            var osmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "forward/terrain/osm/vs.hlsl",
                PixelShader = "forward/terrain/osm/ps.hlsl",
                State = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                },
                InputElements = elements,
                Macros = macros,
            };

            var psmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "forward/terrain/psm/vs.hlsl",
                State = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullFront,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                },
                InputElements = elements,
                Macros = macros,
            };

            csm = device.CreateGraphicsPipeline(csmPipelineDesc);
            osm = device.CreateGraphicsPipeline(osmPipelineDesc);
            psm = device.CreateGraphicsPipeline(psmPipelineDesc);
            flags |= MaterialShaderFlags.Shadow | MaterialShaderFlags.Depth;

            initialized = true;
        }

        private async Task CompileAsync()
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
                State = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = BlendDescription.AlphaBlend,
                    Topology = PrimitiveTopology.TriangleList,
                    BlendFactor = Vector4.One
                },
                InputElements = elements,
                Macros = macros,
            };

            GraphicsPipelineDesc pipelineDescDeferred = new()
            {
                VertexShader = $"deferred/terrain/vs.hlsl",
                PixelShader = $"deferred/terrain/ps.hlsl",
                State = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = blend,
                    Topology = PrimitiveTopology.TriangleList,
                },
                InputElements = elements,
                Macros = macros,
            };

            forward = await device.CreateGraphicsPipelineAsync(pipelineDescForward);

            deferred = await device.CreateGraphicsPipelineAsync(pipelineDescDeferred);

            pipelineDescDeferred.PixelShader = null;
            depthOnly = await device.CreateGraphicsPipelineAsync(pipelineDescDeferred);

            var csmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "forward/terrain/csm/vs.hlsl",
                GeometryShader = "forward/terrain/csm/gs.hlsl",
                State = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullNone,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                },
                InputElements = elements,
                Macros = macros,
            };

            var osmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "forward/terrain/osm/vs.hlsl",
                State = new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                },
                InputElements = elements,
                Macros = macros,
            };

            var psmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "forward/terrain/psm/vs.hlsl",
                State = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullFront,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                },
                InputElements = elements,
                Macros = macros,
            };

            csm = await device.CreateGraphicsPipelineAsync(csmPipelineDesc);
            osm = await device.CreateGraphicsPipelineAsync(osmPipelineDesc);
            psm = await device.CreateGraphicsPipelineAsync(psmPipelineDesc);
            flags |= MaterialShaderFlags.Shadow | MaterialShaderFlags.Depth;

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
            await CompileAsync();
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

            context.SetGraphicsPipeline(forward);
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

            context.SetGraphicsPipeline(forward);
            context.PSSetConstantBuffer(1, camera);
            context.DSSetConstantBuffer(1, camera);
            context.VSSetConstantBuffer(1, camera);
            return true;
        }

        public void EndDrawForward(IGraphicsContext context)
        {
            context.SetGraphicsPipeline(null);
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

            context.SetGraphicsPipeline(deferred);
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

            context.SetGraphicsPipeline(deferred);
            context.DSSetConstantBuffer(1, camera);
            context.VSSetConstantBuffer(1, camera);
            return true;
        }

        public void EndDrawDeferred(IGraphicsContext context)
        {
            context.SetGraphicsPipeline(null);
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
            context.SetGraphicsPipeline(depthOnly);
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

            context.SetGraphicsPipeline(depthOnly);
            return true;
        }

        public void EndDrawDepth(IGraphicsContext context)
        {
            context.SetGraphicsPipeline(null);
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

                    context.SetGraphicsPipeline(psm);
                    return true;

                case ShadowType.Cascaded:
                    if (!csm.IsValid)
                    {
                        return false;
                    }

                    context.SetGraphicsPipeline(csm);
                    return true;

                case ShadowType.Omni:
                    if (!osm.IsValid)
                    {
                        return false;
                    }

                    context.SetGraphicsPipeline(osm);
                    return true;
            }

            return false;
        }

        public void EndDrawShadow(IGraphicsContext context)
        {
            context.SetGraphicsPipeline(null);
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