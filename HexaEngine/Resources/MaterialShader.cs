namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Lights;
    using System.Threading.Tasks;

    public class MaterialShader : IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly MeshData mesh;
        private MaterialData material;
        private readonly bool debone;
        private IGraphicsPipelineState? forward;
        private IGraphicsPipelineState? deferred;
        private IGraphicsPipelineState? depthOnly;
        private IGraphicsPipelineState? csm;
        private IGraphicsPipelineState? osm;
        private IGraphicsPipelineState? psm;
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

        private void Setup(out InputElementDescription[] elements, out ShaderMacro[] macros, out ShaderMacro[] shadowMacros, out MaterialFlags matflags, out bool custom, out bool twoSided, out bool alphaTest, out bool blendFunc)
        {
            flags = MaterialShaderFlags.DepthTest | MaterialShaderFlags.Deferred | MaterialShaderFlags.Shadow | MaterialShaderFlags.Bake;

            elements = MeshData.InputElements;
            macros = material.GetShaderMacros();
            shadowMacros = [];

            if (debone! && (mesh.Flags & VertexFlags.Skinned) != 0)
            {
                elements = MeshData.SkinnedInputElements;
                flags &= ~MaterialShaderFlags.Bake;
                macros = [.. macros, new ShaderMacro("VtxSkinned", "1")];
                shadowMacros = [.. shadowMacros, new ShaderMacro("VtxSkinned", "1")];
            }

            matflags = material.Flags;
            custom = material.HasShader(MaterialShaderType.VertexShaderFile) && material.HasShader(MaterialShaderType.PixelShaderFile);
            twoSided = false;
            if (material.TryGetProperty(MaterialPropertyType.TwoSided, out var twosidedProp))
            {
                twoSided = twosidedProp.AsBool();
                if (twoSided)
                {
                    flags |= MaterialShaderFlags.TwoSided;
                }
            }

            alphaTest = false;
            if ((material.Flags & MaterialFlags.AlphaTest) != 0)
            {
                flags |= MaterialShaderFlags.AlphaTest;
                alphaTest = true;
            }

            blendFunc = false;
            if (material.TryGetProperty(MaterialPropertyType.BlendFunc, out var blendFuncProp))
            {
                blendFunc = blendFuncProp.AsBool();
                if (blendFunc)
                {
                    flags |= MaterialShaderFlags.AlphaTest;
                    flags |= MaterialShaderFlags.Transparent;
                    flags |= MaterialShaderFlags.Forward;
                    flags &= ~MaterialShaderFlags.Deferred;
                    flags &= ~MaterialShaderFlags.DepthTest;
                    flags &= ~MaterialShaderFlags.Shadow;
                    alphaTest = true;
                }
            }

            if ((material.Flags & MaterialFlags.Transparent) != 0)
            {
                flags |= MaterialShaderFlags.AlphaTest;
                flags |= MaterialShaderFlags.Transparent;
                flags |= MaterialShaderFlags.Forward;
                flags &= ~MaterialShaderFlags.Deferred;
                flags &= ~MaterialShaderFlags.DepthTest;
                flags &= ~MaterialShaderFlags.Shadow;
                blendFunc = true;
                alphaTest = true;
            }
        }

        private void Compile()
        {
            Setup(out InputElementDescription[] elements, out ShaderMacro[] macros, out ShaderMacro[] shadowMacros, out MaterialFlags matflags, out bool custom, out bool twoSided, out bool alphaTest, out bool blendFunc);

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
                VertexShader = $"forward/geometry/vs.hlsl",
                PixelShader = $"forward/geometry/ps.hlsl",
                Macros = [.. macros, new("CLUSTERED_FORWARD", 1)]
            };

            GraphicsPipelineStateDesc pipelineStateDescForward = new()
            {
                DepthStencil = DepthStencilDescription.DepthReadEquals,
                Rasterizer = rasterizer,
                Blend = blend,
                Topology = PrimitiveTopology.TriangleList,
                InputElements = elements,
            };

            GraphicsPipelineDesc pipelineDescDeferred = new()
            {
                VertexShader = $"deferred/geometry/vs.hlsl",
                PixelShader = $"deferred/geometry/ps.hlsl",
                Macros = macros
            };

            GraphicsPipelineStateDesc pipelineStateDescDeferred = new()
            {
                DepthStencil = DepthStencilDescription.DepthReadEquals,
                Rasterizer = rasterizer,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
                InputElements = elements
            };

            GraphicsPipelineDesc pipelineDescDepthOnly = new()
            {
                VertexShader = $"deferred/geometry/vs.hlsl",
                Macros = macros
            };

            GraphicsPipelineStateDesc pipelineStateDescDepthOnly = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = rasterizer,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
                InputElements = elements
            };

            GraphicsPipelineDesc pipelineDescBake = new()
            {
                VertexShader = $"forward/geometry/vs.hlsl",
                PixelShader = $"forward/geometry/ps.hlsl",
                Macros = [.. macros, new("CLUSTERED_FORWARD", 1), new("BAKE_PASS", 1)]
            };

            GraphicsPipelineStateDesc pipelineStateDescBake = new()
            {
                DepthStencil = DepthStencilDescription.DepthReadEquals,
                Rasterizer = rasterizer,
                Blend = blend,
                Topology = PrimitiveTopology.TriangleList,
                InputElements = elements
            };

            if ((matflags & MaterialFlags.Tessellation) != 0)
            {
                flags |= MaterialShaderFlags.Tessellation;
                Array.Resize(ref macros, macros.Length + 1);
                macros[^1] = new("Tessellation", "1");
                pipelineDescForward.HullShader = $"forward/geometry/hs.hlsl";
                pipelineDescForward.DomainShader = $"forward/geometry/ds.hlsl";
                pipelineDescDeferred.HullShader = $"deferred/geometry/hs.hlsl";
                pipelineDescDeferred.DomainShader = $"deferred/geometry/ds.hlsl";
                pipelineDescDepthOnly.HullShader = $"deferred/geometry/hs.hlsl";
                pipelineDescDepthOnly.DomainShader = $"deferred/geometry/ds.hlsl";

                pipelineStateDescForward.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                pipelineStateDescDeferred.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                pipelineStateDescDepthOnly.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
            }

            if (alphaTest)
            {
                pipelineDescDepthOnly.PixelShader = $"deferred/geometry/ps.hlsl";
                pipelineDescDepthOnly.Macros = [.. macros, new("DEPTH_TEST_ONLY")];
            }

            if ((flags & MaterialShaderFlags.Forward) != 0)
            {
                forward = device.CreateGraphicsPipelineState(pipelineDescForward, pipelineStateDescForward);
            }

            if ((flags & MaterialShaderFlags.Deferred) != 0)
            {
                deferred = device.CreateGraphicsPipelineState(pipelineDescDeferred, pipelineStateDescDeferred);
            }

            if ((flags & MaterialShaderFlags.DepthTest) != 0)
            {
                depthOnly = device.CreateGraphicsPipelineState(pipelineDescDepthOnly, pipelineStateDescDepthOnly);
            }

            if ((flags & MaterialShaderFlags.Shadow) != 0)
            {
                RasterizerDescription rasterizerShadow = RasterizerDescription.CullFront;
                if (twoSided)
                {
                    rasterizerShadow = RasterizerDescription.CullNone;
                }

                GraphicsPipelineDesc csmPipelineDesc = new()
                {
                    VertexShader = "forward/geometry/csm/vs.hlsl",
                    GeometryShader = "forward/geometry/csm/gs.hlsl",
                    Macros = shadowMacros
                };

                GraphicsPipelineStateDesc csmPipelineStateDesc = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                    InputElements = elements
                };

                GraphicsPipelineDesc osmPipelineDesc = new()
                {
                    VertexShader = "forward/geometry/dpsm/vs.hlsl",
                    PixelShader = "forward/geometry/dpsm/ps.hlsl",
                    Macros = shadowMacros
                };

                GraphicsPipelineStateDesc osmPipelineStateDesc = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullNone,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                    InputElements = elements
                };

                GraphicsPipelineDesc psmPipelineDesc = new()
                {
                    VertexShader = "forward/geometry/psm/vs.hlsl",
                    Macros = shadowMacros
                };

                GraphicsPipelineStateDesc psmPipelineStateDesc = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                    InputElements = elements
                };

                if ((matflags & MaterialFlags.Tessellation) != 0)
                {
                    csmPipelineDesc.HullShader = "forward/geometry/csm/hs.hlsl";
                    csmPipelineDesc.DomainShader = "forward/geometry/csm/ds.hlsl";
                    csmPipelineStateDesc.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    osmPipelineDesc.HullShader = "forward/geometry/dpsm/hs.hlsl";
                    osmPipelineDesc.DomainShader = "forward/geometry/dpsm/ds.hlsl";
                    osmPipelineStateDesc.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    psmPipelineDesc.HullShader = "forward/geometry/psm/hs.hlsl";
                    psmPipelineDesc.DomainShader = "forward/geometry/psm/ds.hlsl";
                    psmPipelineStateDesc.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                csm = device.CreateGraphicsPipelineState(csmPipelineDesc, csmPipelineStateDesc);
                osm = device.CreateGraphicsPipelineState(osmPipelineDesc, osmPipelineStateDesc);
                psm = device.CreateGraphicsPipelineState(psmPipelineDesc, psmPipelineStateDesc);
            }

            initialized = true;
        }

        public void Update(MaterialData material)
        {
            this.material = material;
        }

        public void Reload()
        {
            initialized = false;
            forward?.Dispose();
            deferred?.Dispose();
            depthOnly?.Dispose();
            csm?.Dispose();
            osm?.Dispose();
            psm?.Dispose();
            forward = null;
            deferred = null;
            depthOnly = null;
            csm = null;
            osm = null;
            psm = null;
            Compile();
        }

        public async Task ReloadAsync()
        {
            initialized = false;
            forward?.Dispose();
            deferred?.Dispose();
            depthOnly?.Dispose();
            csm?.Dispose();
            osm?.Dispose();
            psm?.Dispose();
            forward = null;
            deferred = null;
            depthOnly = null;
            csm = null;
            osm = null;
            psm = null;
            Compile();
        }

        public void Recompile()
        {
            initialized = false;
            forward?.Dispose();
            deferred?.Dispose();
            depthOnly?.Dispose();
            csm?.Dispose();
            osm?.Dispose();
            psm?.Dispose();
            forward = null;
            deferred = null;
            depthOnly = null;
            csm = null;
            osm = null;
            psm = null;
            Compile();
        }

        public bool BeginDrawForward(IGraphicsContext context)
        {
            if (!initialized)
            {
                return false;
            }

            if (forward == null || !forward.IsValid)
            {
                return false;
            }

            context.SetPipelineState(forward);
            return true;
        }

        public bool BeginDrawDeferred(IGraphicsContext context)
        {
            if (!initialized)
            {
                return false;
            }

            if (deferred == null || !deferred.IsValid)
            {
                return false;
            }

            context.SetPipelineState(deferred);
            return true;
        }

        public void EndDrawForward(IGraphicsContext context)
        {
            context.SetPipelineState(null);
        }

        public void EndDrawDeferred(IGraphicsContext context)
        {
            context.SetPipelineState(null);
        }

        public bool BeginDrawDepth(IGraphicsContext context)
        {
            if (!initialized)
            {
                return false;
            }

            if (depthOnly == null || !depthOnly.IsValid)
            {
                return false;
            }

            context.SetPipelineState(depthOnly);
            return true;
        }

        public void EndDrawDepth(IGraphicsContext context)
        {
            context.SetPipelineState(null);
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
                    if (psm == null || !psm.IsValid)
                    {
                        return false;
                    }

                    context.SetPipelineState(psm);
                    return true;

                case ShadowType.Cascaded:
                    if (csm == null || !csm.IsValid)
                    {
                        return false;
                    }

                    context.SetPipelineState(csm);
                    return true;

                case ShadowType.Omni:
                    if (osm == null || !osm.IsValid)
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
            context.DSSetConstantBuffer(1, null);
            context.VSSetConstantBuffer(1, null);
            context.GSSetConstantBuffer(1, null);
            context.SetPipelineState(null);
        }

        public bool BeginBake(IGraphicsContext context)
        {
            return false;
        }

        public void EndBake(IGraphicsContext context)
        {
            context.SetPipelineState(null);
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
                forward = null;
                deferred = null;
                depthOnly = null;
                csm = null;
                osm = null;
                psm = null;
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