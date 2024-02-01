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
                macros = [.. macros, new ShaderMacro("VtxSkinned", "1")];
            }
            var matflags = material.Flags;
            var custom = material.HasShader(MaterialShaderType.VertexShaderFile) && material.HasShader(MaterialShaderType.PixelShaderFile);

            bool twoSided = false;
            if (material.TryGetProperty(MaterialPropertyType.TwoSided, out var twosidedProp))
            {
                twoSided = twosidedProp.AsBool();
                if (twoSided)
                {
                    flags |= MaterialShaderFlags.TwoSided;
                }
            }

            bool alphaTest = false;
            if ((material.Flags & MaterialFlags.AlphaTest) != 0)
            {
                flags |= MaterialShaderFlags.AlphaTest;
                alphaTest = true;
            }

            bool blendFunc = false;
            if (material.TryGetProperty(MaterialPropertyType.BlendFunc, out var blendFuncProp))
            {
                blendFunc = blendFuncProp.AsBool();
                if (blendFunc)
                {
                    flags |= MaterialShaderFlags.AlphaTest;
                    flags |= MaterialShaderFlags.Transparent;
                    alphaTest = true;
                }
            }

            if ((material.Flags & MaterialFlags.Transparent) != 0)
            {
                flags |= MaterialShaderFlags.AlphaTest;
                flags |= MaterialShaderFlags.Transparent;
                blendFunc = true;
                alphaTest = true;
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
                flags |= MaterialShaderFlags.Custom | MaterialShaderFlags.Depth;
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

                forward = device.CreateGraphicsPipeline(pipelineDescForward);

                deferred = device.CreateGraphicsPipeline(pipelineDescDeferred);

                depthOnly = device.CreateGraphicsPipeline(pipelineDescDepthOnly);

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
                if (twoSided)
                {
                    flags |= MaterialShaderFlags.TwoSided;
                }
            }

            bool alphaTest = false;
            if ((material.Flags & MaterialFlags.AlphaTest) != 0)
            {
                flags |= MaterialShaderFlags.AlphaTest;
                alphaTest = true;
            }

            bool blendFunc = false;
            if (material.TryGetProperty(MaterialPropertyType.BlendFunc, out var blendFuncProp))
            {
                blendFunc = blendFuncProp.AsBool();
                if (blendFunc)
                {
                    flags |= MaterialShaderFlags.AlphaTest;
                    flags |= MaterialShaderFlags.Transparent;
                    alphaTest = true;
                }
            }

            if ((material.Flags & MaterialFlags.Transparent) != 0)
            {
                flags |= MaterialShaderFlags.AlphaTest;
                flags |= MaterialShaderFlags.Transparent;
                blendFunc = true;
                alphaTest = true;
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
                flags |= MaterialShaderFlags.Custom | MaterialShaderFlags.Depth;
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

                forward = await device.CreateGraphicsPipelineAsync(pipelineDescForward);

                deferred = await device.CreateGraphicsPipelineAsync(pipelineDescDeferred);

                depthOnly = await device.CreateGraphicsPipelineAsync(pipelineDescDepthOnly);

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

                csm = await device.CreateGraphicsPipelineAsync(csmPipelineDesc);
                osm = await device.CreateGraphicsPipelineAsync(osmPipelineDesc);
                psm = await device.CreateGraphicsPipelineAsync(psmPipelineDesc);
                flags |= MaterialShaderFlags.Shadow | MaterialShaderFlags.Depth;
            }

            initialized = true;
        }

        public void Reload()
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

        public async Task ReloadAsync()
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

        public void Recompile()
        {
            initialized = false;
            forward.Recompile();
            deferred.Recompile();
            depthOnly.Recompile();
            csm.Recompile();
            osm.Recompile();
            psm.Recompile();
            initialized = true;
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