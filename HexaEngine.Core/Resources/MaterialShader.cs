#nullable disable

namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using System.Threading.Tasks;

    public class MaterialShader : IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly MeshData mesh;
        private readonly MaterialData material;
        private IGraphicsPipeline pipeline;
        private IGraphicsPipeline depthOnly;
        private volatile bool initialized;
        private bool disposedValue;

        public MaterialShader(IGraphicsDevice device, MeshData mesh, MaterialData material)
        {
            this.device = device;
            this.mesh = mesh;
            this.material = material;
        }

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
            var elements = mesh.GetInputElements();
            var macros = material.GetShaderMacros().Concat(mesh.GetShaderMacros()).ToArray();
            var flags = material.Flags;
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

                if ((flags & MaterialFlags.Tessellation) != 0)
                {
                    Array.Resize(ref macros, macros.Length + 1);
                    macros[^1] = new("Tessellation", "1");
                    pipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                pipeline = device.CreateGraphicsPipeline(pipelineDesc, pipelineState, elements, macros);

                pipelineDesc.PixelShader = null;
                depthOnly = device.CreateGraphicsPipeline(pipelineDesc, pipelineState, elements, macros);
            }
            else
            {
                GraphicsPipelineDesc pipelineDesc = new()
                {
                    VertexShader = "deferred/geometry/vs.hlsl",
                    PixelShader = "deferred/geometry/ps.hlsl"
                };

                GraphicsPipelineState pipelineState = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = blend,
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

                pipeline = device.CreateGraphicsPipeline(pipelineDesc, pipelineState, elements, macros);

                pipelineDesc.PixelShader = null;
                depthOnly = device.CreateGraphicsPipeline(pipelineDesc, pipelineState, elements, macros);
            }

            initialized = true;
        }

        private async Task CompileAsync()
        {
            var elements = mesh.GetInputElements();
            var macros = material.GetShaderMacros().Concat(mesh.GetShaderMacros()).ToArray();
            var flags = material.Flags;
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

                if ((flags & MaterialFlags.Tessellation) != 0)
                {
                    Array.Resize(ref macros, macros.Length + 1);
                    macros[^1] = new("Tessellation", "1");
                    pipelineState.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                pipeline = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, elements, macros);

                pipelineDesc.PixelShader = null;
                depthOnly = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, elements, macros);
            }
            else
            {
                GraphicsPipelineDesc pipelineDesc = new()
                {
                    VertexShader = "deferred/geometry/vs.hlsl",
                    PixelShader = "deferred/geometry/ps.hlsl"
                };

                GraphicsPipelineState pipelineState = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = blend,
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

                pipeline = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, elements, macros);

                pipelineDesc.PixelShader = null;
                depthOnly = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, elements, macros);
            }

            initialized = true;
        }

        public void Recompile()
        {
            initialized = false;
            pipeline.Dispose();
            depthOnly.Dispose();
            Compile();
        }

        public async Task RecompileAsync()
        {
            initialized = false;
            pipeline.Dispose();
            depthOnly.Dispose();
            await CompileAsync();
        }

        public bool BeginDraw(IGraphicsContext context, IBuffer camera)
        {
            if (!initialized)
                return false;
            if (!pipeline.IsValid)
                return false;
            pipeline.BeginDraw(context);
            context.DSSetConstantBuffer(camera, 1);
            context.VSSetConstantBuffer(camera, 1);
            return true;
        }

        public bool BeginDrawDepth(IGraphicsContext context, IBuffer camera)
        {
            if (!initialized)
                return false;
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