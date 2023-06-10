#nullable disable

namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Lights;
    using System.Threading.Tasks;

    public class MaterialShader : IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly MeshData mesh;
        private readonly MaterialData material;
        private readonly bool debone;
        private IGraphicsPipeline pipeline;
        private IGraphicsPipeline depthOnly;
        private IGraphicsPipeline csm;
        private IGraphicsPipeline osm;
        private IGraphicsPipeline psm;
        private volatile bool initialized;
        private bool disposedValue;
        private MaterialShaderFlags flags;

        public MaterialShader(IGraphicsDevice device, MeshData mesh, MaterialData material, bool debone)
        {
            this.device = device;
            this.mesh = mesh;
            this.material = material;
            this.debone = debone;
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
            var elements = mesh.GetInputElements(debone);
            var macros = material.GetShaderMacros().Concat(mesh.GetShaderMacros(debone)).ToArray();
            var matflags = material.Flags;
            var custom = material.VertexShader != null && material.PixelShader != null;

            bool twoSided = false;
            if (material.TryGetProperty(MaterialPropertyType.TwoSided, out var twosidedProp))
            {
                twoSided = twosidedProp.AsBool();
            }

            bool blendFunc = false;
            if (material.TryGetProperty(MaterialPropertyType.BlendFunc, out var blendFuncProp))
            {
                blendFunc = blendFuncProp.AsBool();
            }

            var forward = (material.Flags & MaterialFlags.Transparent) != 0;
            if (forward)
            {
                flags |= MaterialShaderFlags.Forward;
                blendFunc = true;
            }
            else
            {
                flags |= MaterialShaderFlags.Deferred;
            }

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

            if (custom)
            {
                GraphicsPipelineDesc pipelineDesc = new()
                {
                    VertexShader = material.VertexShader,
                    HullShader = material.HullShader,
                    DomainShader = material.DomainShader,
                    GeometryShader = material.GeometryShader,
                    PixelShader = material.PixelShader,
                };

                GraphicsPipelineState pipelineState = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = blend,
                    Topology = PrimitiveTopology.TriangleList,
                };

                if ((matflags & MaterialFlags.Tessellation) != 0)
                {
                    Array.Resize(ref macros, macros.Length + 1);
                    macros[^1] = new("Tessellation", "1");
                    pipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                pipeline = device.CreateGraphicsPipeline(pipelineDesc, pipelineState, elements, macros);

                pipelineDesc.PixelShader = null;
                depthOnly = device.CreateGraphicsPipeline(pipelineDesc, pipelineState, elements, macros);
                flags |= MaterialShaderFlags.Custom | MaterialShaderFlags.Depth;
            }
            else
            {
                string basePath = forward ? "forward" : "deferred";
                GraphicsPipelineDesc pipelineDesc = new()
                {
                    VertexShader = $"{basePath}/geometry/vs.hlsl",
                    PixelShader = $"{basePath}/geometry/ps.hlsl"
                };

                GraphicsPipelineState pipelineState = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = blend,
                    Topology = PrimitiveTopology.TriangleList,
                };

                if ((matflags & MaterialFlags.Tessellation) != 0)
                {
                    Array.Resize(ref macros, macros.Length + 1);
                    macros[^1] = new("Tessellation", "1");
                    pipelineDesc.HullShader = $"{basePath}/geometry/hs.hlsl";
                    pipelineDesc.DomainShader = $"{basePath}/geometry/ds.hlsl";
                    pipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                pipeline = device.CreateGraphicsPipeline(pipelineDesc, pipelineState, elements, macros);

                pipelineDesc.PixelShader = null;
                depthOnly = device.CreateGraphicsPipeline(pipelineDesc, pipelineState, elements, macros);

                var csmPipelineDesc = new GraphicsPipelineDesc()
                {
                    VertexShader = "forward/geometry/csm/vs.hlsl",
                    GeometryShader = "forward/geometry/csm/gs.hlsl",
                };
                var csmPipelineState = new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullNone,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                };

                var osmPipelineDesc = new GraphicsPipelineDesc()
                {
                    VertexShader = "forward/geometry/osm/vs.hlsl",
                    GeometryShader = "forward/geometry/osm/gs.hlsl",
                    PixelShader = "forward/geometry/osm/ps.hlsl",
                };
                var osmPipelineState = new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                };

                var psmPipelineDesc = new GraphicsPipelineDesc()
                {
                    VertexShader = "forward/geometry/psm/vs.hlsl",
                    PixelShader = "forward/geometry/psm/ps.hlsl",
                };
                var psmPipelineState = new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullFront,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                };

                if ((matflags & MaterialFlags.Tessellation) != 0)
                {
                    csmPipelineDesc.HullShader = "forward/geometry/csm/hs.hlsl";
                    csmPipelineDesc.DomainShader = "forward/geometry/csm/ds.hlsl";
                    csmPipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    osmPipelineDesc.HullShader = "forward/geometry/osm/hs.hlsl";
                    osmPipelineDesc.DomainShader = "forward/geometry/osm/ds.hlsl";
                    osmPipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    psmPipelineDesc.HullShader = "forward/geometry/psm/hs.hlsl";
                    psmPipelineDesc.DomainShader = "forward/geometry/psm/ds.hlsl";
                    psmPipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                csm = device.CreateGraphicsPipeline(csmPipelineDesc, csmPipelineState, elements, macros);
                osm = device.CreateGraphicsPipeline(osmPipelineDesc, osmPipelineState, elements, macros);
                psm = device.CreateGraphicsPipeline(psmPipelineDesc, psmPipelineState, elements, macros);
                flags |= MaterialShaderFlags.Shadow | MaterialShaderFlags.Depth;
            }

            initialized = true;
        }

        private async Task CompileAsync()
        {
            flags = 0;
            var elements = mesh.GetInputElements(debone);
            var macros = material.GetShaderMacros().Concat(mesh.GetShaderMacros(debone)).ToArray();
            var matflags = material.Flags;
            var custom = material.VertexShader != null && material.PixelShader != null;

            bool twoSided = false;
            if (material.TryGetProperty(MaterialPropertyType.TwoSided, out var twosidedProp))
            {
                twoSided = twosidedProp.AsBool();
            }

            bool blendFunc = false;
            if (material.TryGetProperty(MaterialPropertyType.BlendFunc, out var blendFuncProp))
            {
                blendFunc = blendFuncProp.AsBool();
            }

            var forward = (material.Flags & MaterialFlags.Transparent) != 0;
            if (forward)
            {
                flags |= MaterialShaderFlags.Forward;
                blendFunc = true;
            }
            else
            {
                flags |= MaterialShaderFlags.Deferred;
            }

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

            if (custom)
            {
                GraphicsPipelineDesc pipelineDesc = new()
                {
                    VertexShader = material.VertexShader,
                    HullShader = material.HullShader,
                    DomainShader = material.DomainShader,
                    GeometryShader = material.GeometryShader,
                    PixelShader = material.PixelShader,
                };

                GraphicsPipelineState pipelineState = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = blend,
                    Topology = PrimitiveTopology.TriangleList,
                };

                if ((matflags & MaterialFlags.Tessellation) != 0)
                {
                    Array.Resize(ref macros, macros.Length + 1);
                    macros[^1] = new("Tessellation", "1");
                    pipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                pipeline = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, elements, macros);

                pipelineDesc.PixelShader = null;
                depthOnly = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, elements, macros);
                flags |= MaterialShaderFlags.Custom | MaterialShaderFlags.Depth;
            }
            else
            {
                string basePath = forward ? "forward" : "deferred";
                GraphicsPipelineDesc pipelineDesc = new()
                {
                    VertexShader = $"{basePath}/geometry/vs.hlsl",
                    PixelShader = $"{basePath}/geometry/ps.hlsl"
                };

                GraphicsPipelineState pipelineState = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = blend,
                    Topology = PrimitiveTopology.TriangleList,
                };

                if ((matflags & MaterialFlags.Tessellation) != 0)
                {
                    Array.Resize(ref macros, macros.Length + 1);
                    macros[^1] = new("Tessellation", "1");
                    pipelineDesc.HullShader = $"{basePath}/geometry/hs.hlsl";
                    pipelineDesc.DomainShader = $"{basePath}/geometry/ds.hlsl";
                    pipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                pipeline = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, elements, macros);

                pipelineDesc.PixelShader = null;
                depthOnly = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, elements, macros);

                var csmPipelineDesc = new GraphicsPipelineDesc()
                {
                    VertexShader = "forward/geometry/csm/vs.hlsl",
                    GeometryShader = "forward/geometry/csm/gs.hlsl",
                };
                var csmPipelineState = new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullNone,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                };

                var osmPipelineDesc = new GraphicsPipelineDesc()
                {
                    VertexShader = "forward/geometry/osm/vs.hlsl",
                    GeometryShader = "forward/geometry/osm/gs.hlsl",
                    PixelShader = "forward/geometry/osm/ps.hlsl",
                };
                var osmPipelineState = new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                };

                var psmPipelineDesc = new GraphicsPipelineDesc()
                {
                    VertexShader = "forward/geometry/psm/vs.hlsl",
                    PixelShader = "forward/geometry/psm/ps.hlsl",
                };
                var psmPipelineState = new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullFront,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                };

                if ((matflags & MaterialFlags.Tessellation) != 0)
                {
                    csmPipelineDesc.HullShader = "forward/geometry/csm/hs.hlsl";
                    csmPipelineDesc.DomainShader = "forward/geometry/csm/ds.hlsl";
                    csmPipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    osmPipelineDesc.HullShader = "forward/geometry/osm/hs.hlsl";
                    osmPipelineDesc.DomainShader = "forward/geometry/osm/ds.hlsl";
                    osmPipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    psmPipelineDesc.HullShader = "forward/geometry/psm/hs.hlsl";
                    psmPipelineDesc.DomainShader = "forward/geometry/psm/ds.hlsl";
                    psmPipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                csm = await device.CreateGraphicsPipelineAsync(csmPipelineDesc, csmPipelineState, elements, macros);
                osm = await device.CreateGraphicsPipelineAsync(osmPipelineDesc, osmPipelineState, elements, macros);
                psm = await device.CreateGraphicsPipelineAsync(psmPipelineDesc, psmPipelineState, elements, macros);
                flags |= MaterialShaderFlags.Shadow | MaterialShaderFlags.Depth;
            }

            initialized = true;
        }

        public void Recompile()
        {
            initialized = false;
            pipeline.Dispose();
            depthOnly.Dispose();
            csm.Dispose();
            osm.Dispose();
            psm.Dispose();
            Compile();
        }

        public async Task RecompileAsync()
        {
            initialized = false;
            pipeline.Dispose();
            depthOnly.Dispose();
            csm.Dispose();
            osm.Dispose();
            psm.Dispose();
            await CompileAsync();
        }

        public bool BeginDraw(IGraphicsContext context)
        {
            if (!initialized)
            {
                return false;
            }

            if (!pipeline.IsValid)
            {
                return false;
            }

            context.SetGraphicsPipeline(pipeline);
            return true;
        }

        public bool BeginDraw(IGraphicsContext context, IBuffer camera)
        {
            if (!initialized)
            {
                return false;
            }

            if (!pipeline.IsValid)
            {
                return false;
            }

            context.SetGraphicsPipeline(pipeline);
            context.DSSetConstantBuffer(camera, 1);
            context.VSSetConstantBuffer(camera, 1);
            return true;
        }

        public bool BeginDrawForward(IGraphicsContext context)
        {
            if (!initialized)
            {
                return false;
            }

            if (!pipeline.IsValid)
            {
                return false;
            }

            context.SetGraphicsPipeline(pipeline);
            return true;
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

            context.DSSetConstantBuffer(camera, 1);
            context.VSSetConstantBuffer(camera, 1);
            context.SetGraphicsPipeline(depthOnly);
            return true;
        }

        public bool BeginDrawShadow(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!initialized)
            {
                return false;
            }

            context.DSSetConstantBuffer(light, 1);
            context.VSSetConstantBuffer(light, 1);
            context.GSSetConstantBuffer(light, 1);
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline?.Dispose();
                depthOnly?.Dispose();
                csm?.Dispose();
                osm?.Dispose();
                psm?.Dispose();
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