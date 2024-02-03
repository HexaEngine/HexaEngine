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
        private IGraphicsPipeline? forward;
        private IGraphicsPipeline? deferred;
        private IGraphicsPipeline? depthOnly;
        private IGraphicsPipeline? csm;
        private IGraphicsPipeline? osm;
        private IGraphicsPipeline? psm;
        private IGraphicsPipeline? bake;
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
            Setup(out InputElementDescription[] elements, out ShaderMacro[] macros, out MaterialFlags matflags, out bool custom, out bool twoSided, out bool alphaTest, out bool blendFunc);

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
                    State = new()
                    {
                        DepthStencil = DepthStencilDescription.Default,
                        Rasterizer = rasterizer,
                        Blend = blend,
                        Topology = PrimitiveTopology.TriangleList,
                    },
                    InputElements = elements,
                    Macros = [.. macros, new("CLUSTERED_FORWARD", 1)]
                };

                if ((matflags & MaterialFlags.Tessellation) != 0)
                {
                    pipelineDesc.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    pipelineDesc.Macros = [.. macros, new("Tessellation", 1)];
                }

                deferred = device.CreateGraphicsPipeline(pipelineDesc);

                pipelineDesc.PixelShader = null;
                depthOnly = device.CreateGraphicsPipeline(pipelineDesc);
                flags |= MaterialShaderFlags.Custom | MaterialShaderFlags.DepthTest;
            }
            else
            {
                GraphicsPipelineDesc pipelineDescForward = new()
                {
                    VertexShader = $"forward/geometry/vs.hlsl",
                    PixelShader = $"forward/geometry/ps.hlsl",
                    State = new()
                    {
                        DepthStencil = DepthStencilDescription.DepthReadEquals,
                        Rasterizer = rasterizer,
                        Blend = blend,
                        Topology = PrimitiveTopology.TriangleList,
                    },
                    InputElements = elements,
                    Macros = [.. macros, new("CLUSTERED_FORWARD", 1)]
                };

                GraphicsPipelineDesc pipelineDescDeferred = new()
                {
                    VertexShader = $"deferred/geometry/vs.hlsl",
                    PixelShader = $"deferred/geometry/ps.hlsl",
                    State = new()
                    {
                        DepthStencil = DepthStencilDescription.DepthReadEquals,
                        Rasterizer = rasterizer,
                        Blend = BlendDescription.Opaque,
                        Topology = PrimitiveTopology.TriangleList,
                    },
                    InputElements = elements,
                    Macros = macros
                };

                GraphicsPipelineDesc pipelineDescDepthOnly = new()
                {
                    VertexShader = $"deferred/geometry/vs.hlsl",
                    State = new()
                    {
                        DepthStencil = DepthStencilDescription.Default,
                        Rasterizer = rasterizer,
                        Blend = BlendDescription.Opaque,
                        Topology = PrimitiveTopology.TriangleList,
                    },
                    InputElements = elements,
                    Macros = macros
                };

                GraphicsPipelineDesc pipelineBake = new()
                {
                    VertexShader = $"forward/geometry/vs.hlsl",
                    PixelShader = $"forward/geometry/ps.hlsl",
                    State = new()
                    {
                        DepthStencil = DepthStencilDescription.DepthReadEquals,
                        Rasterizer = rasterizer,
                        Blend = blend,
                        Topology = PrimitiveTopology.TriangleList,
                    },
                    InputElements = elements,
                    Macros = [.. macros, new("CLUSTERED_FORWARD", 1), new("BAKE_PASS", 1)]
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
                    pipelineDescForward.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    pipelineDescDeferred.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    pipelineDescDepthOnly.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                if (alphaTest)
                {
                    pipelineDescDepthOnly.PixelShader = $"deferred/geometry/ps.hlsl";
                    pipelineDescDepthOnly.Macros = [.. macros, new("DEPTH_TEST_ONLY")];
                }

                if ((flags & MaterialShaderFlags.Forward) != 0)
                {
                    forward = device.CreateGraphicsPipeline(pipelineDescForward);
                }

                if ((flags & MaterialShaderFlags.Deferred) != 0)
                {
                    deferred = device.CreateGraphicsPipeline(pipelineDescDeferred);
                }

                if ((flags & MaterialShaderFlags.DepthTest) != 0)
                {
                    depthOnly = device.CreateGraphicsPipeline(pipelineDescDepthOnly);
                }

                if ((flags & MaterialShaderFlags.Bake) != 0)
                {
                    bake = device.CreateGraphicsPipeline(pipelineBake);
                }

                if ((flags & MaterialShaderFlags.Shadow) != 0)
                {
                    RasterizerDescription rasterizerShadow = RasterizerDescription.CullFront;
                    if (twoSided)
                    {
                        rasterizerShadow = RasterizerDescription.CullNone;
                    }

                    var csmPipelineDesc = new GraphicsPipelineDesc()
                    {
                        VertexShader = "forward/geometry/csm/vs.hlsl",
                        GeometryShader = "forward/geometry/csm/gs.hlsl",
                        State = new()
                        {
                            DepthStencil = DepthStencilDescription.Default,
                            Rasterizer = rasterizer,
                            Blend = BlendDescription.Opaque,
                            Topology = PrimitiveTopology.TriangleList,
                        },
                        InputElements = elements,
                        Macros = macros
                    };

                    var osmPipelineDesc = new GraphicsPipelineDesc()
                    {
                        VertexShader = "forward/geometry/dpsm/vs.hlsl",
                        PixelShader = "forward/geometry/dpsm/ps.hlsl",
                        State = new()
                        {
                            DepthStencil = DepthStencilDescription.Default,
                            Rasterizer = rasterizer,
                            Blend = BlendDescription.Opaque,
                            Topology = PrimitiveTopology.TriangleList,
                        },
                        InputElements = elements,
                        Macros = macros
                    };

                    var psmPipelineDesc = new GraphicsPipelineDesc()
                    {
                        VertexShader = "forward/geometry/psm/vs.hlsl",
                        State = new()
                        {
                            DepthStencil = DepthStencilDescription.Default,
                            Rasterizer = rasterizer,
                            Blend = BlendDescription.Opaque,
                            Topology = PrimitiveTopology.TriangleList,
                        },
                        InputElements = elements,
                        Macros = macros
                    };

                    if ((matflags & MaterialFlags.Tessellation) != 0)
                    {
                        csmPipelineDesc.HullShader = "forward/geometry/csm/hs.hlsl";
                        csmPipelineDesc.DomainShader = "forward/geometry/csm/ds.hlsl";
                        csmPipelineDesc.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                        osmPipelineDesc.HullShader = "forward/geometry/dpsm/hs.hlsl";
                        osmPipelineDesc.DomainShader = "forward/geometry/dpsm/ds.hlsl";
                        osmPipelineDesc.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                        psmPipelineDesc.HullShader = "forward/geometry/psm/hs.hlsl";
                        psmPipelineDesc.DomainShader = "forward/geometry/psm/ds.hlsl";
                        psmPipelineDesc.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    }

                    csm = device.CreateGraphicsPipeline(csmPipelineDesc);
                    osm = device.CreateGraphicsPipeline(osmPipelineDesc);
                    psm = device.CreateGraphicsPipeline(psmPipelineDesc);
                }
            }

            initialized = true;
        }

        private void Setup(out InputElementDescription[] elements, out ShaderMacro[] macros, out MaterialFlags matflags, out bool custom, out bool twoSided, out bool alphaTest, out bool blendFunc)
        {
            flags = MaterialShaderFlags.DepthTest | MaterialShaderFlags.Deferred | MaterialShaderFlags.Shadow | MaterialShaderFlags.Bake;

            elements = MeshData.InputElements;
            macros = material.GetShaderMacros();

            if (debone! && (mesh.Flags & VertexFlags.Skinned) != 0)
            {
                elements = MeshData.SkinnedInputElements;
                flags &= ~MaterialShaderFlags.Bake;
                macros = [.. macros, new ShaderMacro("VtxSkinned", "1")];
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

        private async Task CompileAsync()
        {
            Setup(out InputElementDescription[] elements, out ShaderMacro[] macros, out MaterialFlags matflags, out bool custom, out bool twoSided, out bool alphaTest, out bool blendFunc);

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
                    State = new()
                    {
                        DepthStencil = DepthStencilDescription.Default,
                        Rasterizer = rasterizer,
                        Blend = blend,
                        Topology = PrimitiveTopology.TriangleList,
                    },
                    InputElements = elements,
                    Macros = [.. macros, new("CLUSTERED_FORWARD", 1)]
                };

                if ((matflags & MaterialFlags.Tessellation) != 0)
                {
                    pipelineDesc.Macros = [.. macros, new("Tessellation", 1)];
                    pipelineDesc.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                deferred = await device.CreateGraphicsPipelineAsync(pipelineDesc);

                pipelineDesc.PixelShader = null;
                depthOnly = await device.CreateGraphicsPipelineAsync(pipelineDesc);
                flags |= MaterialShaderFlags.Custom | MaterialShaderFlags.DepthTest;
            }
            else
            {
                GraphicsPipelineDesc pipelineDescForward = new()
                {
                    VertexShader = $"forward/geometry/vs.hlsl",
                    PixelShader = $"forward/geometry/ps.hlsl",
                    State = new()
                    {
                        DepthStencil = DepthStencilDescription.DepthReadEquals,
                        Rasterizer = rasterizer,
                        Blend = blend,
                        Topology = PrimitiveTopology.TriangleList,
                    },
                    InputElements = elements,
                    Macros = [.. macros, new("CLUSTERED_FORWARD", 1)]
                };

                GraphicsPipelineDesc pipelineDescDeferred = new()
                {
                    VertexShader = $"deferred/geometry/vs.hlsl",
                    PixelShader = $"deferred/geometry/ps.hlsl",
                    State = new()
                    {
                        DepthStencil = DepthStencilDescription.DepthReadEquals,
                        Rasterizer = rasterizer,
                        Blend = BlendDescription.Opaque,
                        Topology = PrimitiveTopology.TriangleList,
                    },
                    InputElements = elements,
                    Macros = macros
                };

                GraphicsPipelineDesc pipelineDescDepthOnly = new()
                {
                    VertexShader = $"deferred/geometry/vs.hlsl",
                    State = new()
                    {
                        DepthStencil = DepthStencilDescription.Default,
                        Rasterizer = rasterizer,
                        Blend = BlendDescription.Opaque,
                        Topology = PrimitiveTopology.TriangleList,
                    },
                    InputElements = elements,
                    Macros = macros
                };

                GraphicsPipelineDesc pipelineBake = new()
                {
                    VertexShader = $"forward/geometry/vs.hlsl",
                    PixelShader = $"forward/geometry/ps.hlsl",
                    State = new()
                    {
                        DepthStencil = DepthStencilDescription.DepthReadEquals,
                        Rasterizer = rasterizer,
                        Blend = blend,
                        Topology = PrimitiveTopology.TriangleList,
                    },
                    InputElements = elements,
                    Macros = [.. macros, new("CLUSTERED_FORWARD", 1), new("BAKE_PASS", 1)]
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

                    pipelineDescForward.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    pipelineDescDeferred.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    pipelineDescDepthOnly.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                if (alphaTest)
                {
                    pipelineDescDepthOnly.PixelShader = $"deferred/geometry/ps.hlsl";
                    pipelineDescDepthOnly.Macros = [.. macros, new("DEPTH_TEST_ONLY")];
                }

                if ((flags & MaterialShaderFlags.Forward) != 0)
                {
                    forward = await device.CreateGraphicsPipelineAsync(pipelineDescForward);
                }

                if ((flags & MaterialShaderFlags.Deferred) != 0)
                {
                    deferred = await device.CreateGraphicsPipelineAsync(pipelineDescDeferred);
                }

                if ((flags & MaterialShaderFlags.DepthTest) != 0)
                {
                    depthOnly = await device.CreateGraphicsPipelineAsync(pipelineDescDepthOnly);
                }

                if ((flags & MaterialShaderFlags.Bake) != 0)
                {
                    bake = await device.CreateGraphicsPipelineAsync(pipelineBake);
                }

                if ((flags & MaterialShaderFlags.Shadow) != 0)
                {
                    RasterizerDescription rasterizerShadow = RasterizerDescription.CullFront;
                    if (twoSided)
                    {
                        rasterizerShadow = RasterizerDescription.CullNone;
                    }

                    var csmPipelineDesc = new GraphicsPipelineDesc()
                    {
                        VertexShader = "forward/geometry/csm/vs.hlsl",
                        GeometryShader = "forward/geometry/csm/gs.hlsl",
                        State = new()
                        {
                            DepthStencil = DepthStencilDescription.Default,
                            Rasterizer = rasterizer,
                            Blend = BlendDescription.Opaque,
                            Topology = PrimitiveTopology.TriangleList,
                        },
                        InputElements = elements,
                        Macros = macros
                    };

                    var osmPipelineDesc = new GraphicsPipelineDesc()
                    {
                        VertexShader = "forward/geometry/dpsm/vs.hlsl",
                        PixelShader = "forward/geometry/dpsm/ps.hlsl",
                        State = new()
                        {
                            DepthStencil = DepthStencilDescription.Default,
                            Rasterizer = RasterizerDescription.CullNone,
                            Blend = BlendDescription.Opaque,
                            Topology = PrimitiveTopology.TriangleList,
                        },
                        InputElements = elements,
                        Macros = macros
                    };

                    var psmPipelineDesc = new GraphicsPipelineDesc()
                    {
                        VertexShader = "forward/geometry/psm/vs.hlsl",
                        State = new()
                        {
                            DepthStencil = DepthStencilDescription.Default,
                            Rasterizer = rasterizer,
                            Blend = BlendDescription.Opaque,
                            Topology = PrimitiveTopology.TriangleList,
                        },
                        InputElements = elements,
                        Macros = macros
                    };

                    if ((matflags & MaterialFlags.Tessellation) != 0)
                    {
                        csmPipelineDesc.HullShader = "forward/geometry/csm/hs.hlsl";
                        csmPipelineDesc.DomainShader = "forward/geometry/csm/ds.hlsl";
                        csmPipelineDesc.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                        osmPipelineDesc.HullShader = "forward/geometry/dpsm/hs.hlsl";
                        osmPipelineDesc.DomainShader = "forward/geometry/dpsm/ds.hlsl";
                        osmPipelineDesc.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                        psmPipelineDesc.HullShader = "forward/geometry/psm/hs.hlsl";
                        psmPipelineDesc.DomainShader = "forward/geometry/psm/ds.hlsl";
                        psmPipelineDesc.State.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    }

                    csm = await device.CreateGraphicsPipelineAsync(csmPipelineDesc);
                    osm = await device.CreateGraphicsPipelineAsync(osmPipelineDesc);
                    psm = await device.CreateGraphicsPipelineAsync(psmPipelineDesc);
                }
            }

            initialized = true;
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
            bake?.Dispose();
            forward = null;
            deferred = null;
            depthOnly = null;
            csm = null;
            osm = null;
            psm = null;
            bake = null;
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
            bake?.Dispose();
            forward = null;
            deferred = null;
            depthOnly = null;
            csm = null;
            osm = null;
            psm = null;
            bake = null;
            await CompileAsync();
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
            bake?.Dispose();
            forward = null;
            deferred = null;
            depthOnly = null;
            csm = null;
            osm = null;
            psm = null;
            bake = null;
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

            context.SetGraphicsPipeline(forward);
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

            if (depthOnly == null || !depthOnly.IsValid)
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
                    if (psm == null || !psm.IsValid)
                    {
                        return false;
                    }

                    context.SetGraphicsPipeline(psm);
                    return true;

                case ShadowType.Cascaded:
                    if (csm == null || !csm.IsValid)
                    {
                        return false;
                    }

                    context.SetGraphicsPipeline(csm);
                    return true;

                case ShadowType.Omni:
                    if (osm == null || !osm.IsValid)
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

        public bool BeginBake(IGraphicsContext context)
        {
            if (!initialized)
            {
                return false;
            }

            if (bake == null || !bake.IsValid)
            {
                return false;
            }

            context.SetGraphicsPipeline(bake);
            return true;
        }

        public void EndBake(IGraphicsContext context)
        {
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
                bake?.Dispose();
                forward = null;
                deferred = null;
                depthOnly = null;
                csm = null;
                osm = null;
                psm = null;
                bake = null;
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