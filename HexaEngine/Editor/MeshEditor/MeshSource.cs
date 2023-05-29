namespace HexaEngine.Editor.MeshEditor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Meshes;
    using System.Numerics;

    public unsafe class MeshSource : IDisposable
    {
        public MeshData Data;
        public readonly IBuffer VB;
        public readonly IBuffer IB;
        public readonly IBuffer IDB;
        public readonly uint Stride;
        public readonly uint VertexCount;
        public readonly uint IndexCount;
        public readonly IGraphicsPipeline Solid;
        public readonly IGraphicsPipeline Overlay;
        public readonly IGraphicsPipeline Normals;
        public readonly uint TypeId;
        private bool disposedValue;

        public MeshSource(IGraphicsDevice device, MeshData data)
        {
            Data = data;
            VB = data.CreateVertexBuffer(device, Usage.Dynamic, CpuAccessFlags.Write);
            IB = data.CreateIndexBuffer(device, Usage.Dynamic, CpuAccessFlags.Write);
            var id = 0;
            IDB = device.CreateBuffer(&id, 4, BindFlags.ConstantBuffer, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.None);
            Stride = data.GetStride();

            VertexCount = data.VerticesCount;
            IndexCount = data.IndicesCount;

            ShaderMacro[] macros = data.GetShaderMacros();
            InputElementDescription[] inputElements = data.GetInputElements();

            Solid = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "forward/solid2/vs.hlsl",
                PixelShader = "forward/solid2/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                BlendFactor = Vector4.Zero,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList,
                SampleMask = uint.MaxValue,
                StencilRef = 0,
            }, inputElements, macros);

            Overlay = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "forward/wireframe2/vs.hlsl",
                PixelShader = "forward/wireframe2/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                BlendFactor = Vector4.Zero,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.Wireframe,
                Topology = PrimitiveTopology.TriangleList,
                SampleMask = uint.MaxValue,
                StencilRef = 0,
            }, inputElements, macros);

            Normals = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "debugging/normals2/vs.hlsl",
                GeometryShader = "debugging/normals2/gs.hlsl",
                PixelShader = "debugging/normals2/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                BlendFactor = Vector4.Zero,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList,
                SampleMask = uint.MaxValue,
                StencilRef = 0,
            }, inputElements, macros);
        }

        public void Update(IGraphicsContext context, bool ib, bool vb)
        {
            if (ib)
            {
                Data.WriteIndexBuffer(context, IB);
            }

            if (vb)
            {
                Data.WriteVertexBuffer(context, VB);
            }
        }

        public void Bind(IGraphicsContext context)
        {
            context.VSSetConstantBuffer(IDB, 0);
            context.SetIndexBuffer(IB, Format.R32UInt, 0);
            context.SetVertexBuffer(VB, Stride);
        }

        public void Draw(IGraphicsContext context)
        {
            context.SetIndexBuffer(IB, Format.R32UInt, 0);
            context.SetVertexBuffer(VB, Stride);
            context.VSSetConstantBuffer(IDB, 0);
            context.DrawIndexedInstanced(IndexCount, 1, 0, 0, 0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                VB.Dispose();
                IB.Dispose();
                Overlay.Dispose();
                disposedValue = true;
            }
        }

        ~MeshSource()
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