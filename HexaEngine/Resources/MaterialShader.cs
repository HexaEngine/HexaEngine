#nullable disable

namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Lights;
    using System.Threading.Tasks;

    public class MaterialShader : IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly MeshData mesh;
        private readonly MaterialData material;
        private readonly bool debone;
        private IGraphicsPipeline forward;
        private IGraphicsPipeline deferred;
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
            InputElementDescription[] elements = MeshData.InputElements;
            if (debone! && (mesh.Flags & VertexFlags.Skinned) != 0)
            {
                elements = MeshData.SkinnedInputElements;
            }
            var macros = material.GetShaderMacros();
            if (debone! && (mesh.Flags & VertexFlags.Skinned) != 0)
            {
                macros = macros.Append(new ShaderMacro("VtxSkinned", "1")).ToArray();
            }
            var matflags = material.Flags;
            var custom = material.HasShader(MaterialShaderType.VertexShaderFile) && material.HasShader(MaterialShaderType.PixelShaderFile);

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

            if ((material.Flags & MaterialFlags.Transparent) != 0)
            {
                flags |= MaterialShaderFlags.Transparent;
                blendFunc = true;
            }

            RasterizerDescription rasterizer = RasterizerDescription.CullBack;
            if (twoSided)
            {
                rasterizer = RasterizerDescription.CullNone;
            }

            BlendDescription blend = BlendDescription.Opaque;
            if (blendFunc)
            {
                blend = BlendDescription.Additive;
            }

            if (custom)
            {
                GraphicsPipelineDesc pipelineDesc = new()
                {
                    VertexShader = material.GetShader(MaterialShaderType.VertexShaderFile).Source,
                    HullShader = material.GetShader(MaterialShaderType.HullShaderFile).Source,
                    DomainShader = material.GetShader(MaterialShaderType.DomainShaderFile).Source,
                    GeometryShader = material.GetShader(MaterialShaderType.GeometryShaderFile).Source,
                    PixelShader = material.GetShader(MaterialShaderType.PixelShaderFile).Source,
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

                deferred = device.CreateGraphicsPipeline(pipelineDesc, pipelineState, elements, macros);

                pipelineDesc.PixelShader = null;
                depthOnly = device.CreateGraphicsPipeline(pipelineDesc, pipelineState, elements, macros);
                flags |= MaterialShaderFlags.Custom | MaterialShaderFlags.Depth;
            }
            else
            {
                GraphicsPipelineDesc pipelineDescForward = new()
                {
                    VertexShader = $"forward/geometry/vs.hlsl",
                    PixelShader = $"forward/geometry/ps.hlsl"
                };

                GraphicsPipelineDesc pipelineDescDeferred = new()
                {
                    VertexShader = $"deferred/geometry/vs.hlsl",
                    PixelShader = $"deferred/geometry/ps.hlsl"
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
                    pipelineDescForward.HullShader = $"forward/geometry/hs.hlsl";
                    pipelineDescForward.DomainShader = $"forward/geometry/ds.hlsl";
                    pipelineDescDeferred.HullShader = $"deferred/geometry/hs.hlsl";
                    pipelineDescDeferred.DomainShader = $"deferred/geometry/ds.hlsl";
                    pipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                forward = device.CreateGraphicsPipeline(pipelineDescForward, pipelineState, elements, macros.Append(new("CLUSTERED_FORWARD", 1)).ToArray());

                deferred = device.CreateGraphicsPipeline(pipelineDescDeferred, pipelineState, elements, macros);

                pipelineDescDeferred.PixelShader = null;
                depthOnly = device.CreateGraphicsPipeline(pipelineDescDeferred, pipelineState, elements, macros);

                var csmPipelineDesc = new GraphicsPipelineDesc()
                {
                    VertexShader = "forward/geometry/csm/vs.hlsl",
                    GeometryShader = "forward/geometry/csm/gs.hlsl",
                };
                var csmPipelineState = new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullFrontDepthBias,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                };

                var osmPipelineDesc = new GraphicsPipelineDesc()
                {
                    VertexShader = "forward/geometry/osm/vs.hlsl",
                    PixelShader = "forward/geometry/osm/ps.hlsl",
                };
                var osmPipelineState = new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullFrontDepthBias,
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
                    Rasterizer = RasterizerDescription.CullFrontDepthBias,
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
            InputElementDescription[] elements = MeshData.InputElements;
            if (debone! && (mesh.Flags & VertexFlags.Skinned) != 0)
            {
                elements = MeshData.SkinnedInputElements;
            }
            var macros = material.GetShaderMacros();
            if (debone! && (mesh.Flags & VertexFlags.Skinned) != 0)
            {
                macros = macros.Append(new ShaderMacro("VtxSkinned", "1")).ToArray();
            }
            var matflags = material.Flags;
            var custom = material.HasShader(MaterialShaderType.VertexShaderFile) && material.HasShader(MaterialShaderType.PixelShaderFile);

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

            if ((material.Flags & MaterialFlags.Transparent) != 0)
            {
                flags |= MaterialShaderFlags.Transparent;
                blendFunc = true;
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
                    VertexShader = material.GetShader(MaterialShaderType.VertexShaderFile).Source,
                    HullShader = material.GetShader(MaterialShaderType.HullShaderFile).Source,
                    DomainShader = material.GetShader(MaterialShaderType.DomainShaderFile).Source,
                    GeometryShader = material.GetShader(MaterialShaderType.GeometryShaderFile).Source,
                    PixelShader = material.GetShader(MaterialShaderType.PixelShaderFile).Source,
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

                deferred = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, elements, macros);

                pipelineDesc.PixelShader = null;
                depthOnly = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, elements, macros);
                flags |= MaterialShaderFlags.Custom | MaterialShaderFlags.Depth;
            }
            else
            {
                GraphicsPipelineDesc pipelineDescForward = new()
                {
                    VertexShader = $"forward/geometry/vs.hlsl",
                    PixelShader = $"forward/geometry/ps.hlsl"
                };

                GraphicsPipelineDesc pipelineDescDeferred = new()
                {
                    VertexShader = $"deferred/geometry/vs.hlsl",
                    PixelShader = $"deferred/geometry/ps.hlsl"
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
                    pipelineDescForward.HullShader = $"forward/geometry/hs.hlsl";
                    pipelineDescForward.DomainShader = $"forward/geometry/ds.hlsl";
                    pipelineDescDeferred.HullShader = $"deferred/geometry/hs.hlsl";
                    pipelineDescDeferred.DomainShader = $"deferred/geometry/ds.hlsl";

                    pipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                forward = await device.CreateGraphicsPipelineAsync(pipelineDescForward, pipelineState, elements, macros.Append(new("CLUSTERED_FORWARD", 1)).ToArray());

                deferred = await device.CreateGraphicsPipelineAsync(pipelineDescDeferred, pipelineState, elements, macros);

                pipelineDescDeferred.PixelShader = null;
                depthOnly = await device.CreateGraphicsPipelineAsync(pipelineDescDeferred, pipelineState, elements, macros);

                var csmPipelineDesc = new GraphicsPipelineDesc()
                {
                    VertexShader = "forward/geometry/csm/vs.hlsl",
                    GeometryShader = "forward/geometry/csm/gs.hlsl",
                };
                var csmPipelineState = new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullFrontDepthBias,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                };

                var osmPipelineDesc = new GraphicsPipelineDesc()
                {
                    VertexShader = "forward/geometry/osm/vs.hlsl",
                    PixelShader = "forward/geometry/osm/ps.hlsl",
                };
                var osmPipelineState = new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullFrontDepthBias,
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
                    Rasterizer = RasterizerDescription.CullFrontDepthBias,
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
            forward.Dispose();
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
            forward.Dispose();
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

        public void EndDrawForward(IGraphicsContext context)
        {
            context.SetGraphicsPipeline(null);
        }

        public void EndDrawDeferred(IGraphicsContext context)
        {
            context.SetGraphicsPipeline(null);
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
            context.DSSetConstantBuffer(1, null);
            context.VSSetConstantBuffer(1, null);
            context.GSSetConstantBuffer(1, null);
            context.SetGraphicsPipeline(null);
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