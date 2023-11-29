namespace HexaEngine.Editor.MeshEditor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Meshes;
    using System.Numerics;

    public unsafe class MeshSource : IDisposable
    {
        public MeshData Data;
        public readonly VertexBuffer<MeshVertex> VertexBuffer;
        public readonly IndexBuffer<uint> IndexBuffer;
        public readonly uint Stride;
        public readonly uint VertexCount;
        public readonly uint IndexCount;
        public readonly IGraphicsPipeline Solid;
        public readonly IGraphicsPipeline Overlay;
        public readonly IGraphicsPipeline Normals;
        public readonly IGraphicsPipeline Points;
        public readonly uint TypeId;
        private bool disposedValue;

        public MeshSource(IGraphicsDevice device, MeshData data)
        {
            Data = data;
            VertexBuffer = data.CreateVertexBuffer(device, CpuAccessFlags.Write);
            IndexBuffer = data.CreateIndexBuffer(device, CpuAccessFlags.Write);
            Stride = (uint)sizeof(MeshVertex);

            VertexCount = data.VerticesCount;
            IndexCount = data.IndicesCount;

            InputElementDescription[] inputElements = MeshData.InputElements;

            Solid = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "forward/solid/vs.hlsl",
                PixelShader = "forward/solid/ps.hlsl",
                State = new GraphicsPipelineState()
                {
                    Blend = BlendDescription.Opaque,
                    BlendFactor = Vector4.Zero,
                    DepthStencil = DepthStencilDescription.DefaultLess,
                    Rasterizer = RasterizerDescription.CullNone,
                    Topology = PrimitiveTopology.TriangleList,
                    SampleMask = uint.MaxValue,
                    StencilRef = 0,
                },
                InputElements = inputElements
            });

            Overlay = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "forward/wireframe/vs.hlsl",
                PixelShader = "forward/wireframe/ps.hlsl",
                State = new GraphicsPipelineState()
                {
                    Blend = BlendDescription.Opaque,
                    BlendFactor = Vector4.Zero,
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = new RasterizerDescription(CullMode.None, FillMode.Wireframe, false, 0, 0, 0, true, false, false, false),
                    Topology = PrimitiveTopology.TriangleList,
                    SampleMask = uint.MaxValue,
                    StencilRef = 0,
                },
                InputElements = inputElements
            });

            Normals = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "debugging/normals/vs.hlsl",
                GeometryShader = "debugging/normals/gs.hlsl",
                PixelShader = "debugging/normals/ps.hlsl",
                State = new GraphicsPipelineState()
                {
                    Blend = BlendDescription.Opaque,
                    BlendFactor = Vector4.Zero,
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullBack,
                    Topology = PrimitiveTopology.TriangleList,
                    SampleMask = uint.MaxValue,
                    StencilRef = 0,
                },
                InputElements = inputElements
            });

            Points = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "forward/point/vs.hlsl",
                GeometryShader = "forward/point/gs.hlsl",
                PixelShader = "forward/point/ps.hlsl",
                State = new GraphicsPipelineState()
                {
                    Blend = BlendDescription.Opaque,
                    BlendFactor = Vector4.Zero,
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullBack,
                    Topology = PrimitiveTopology.PointList,
                    SampleMask = uint.MaxValue,
                    StencilRef = 0,
                },
                InputElements = inputElements
            });
        }

        public void Update(IGraphicsContext context, bool ib, bool vb)
        {
            if (ib)
            {
                Data.WriteIndexBuffer(context, IndexBuffer);
            }

            if (vb)
            {
                Data.WriteVertexBuffer(context, VertexBuffer);
            }
        }

        public void Bind(IGraphicsContext context)
        {
            context.SetIndexBuffer(IndexBuffer, Format.R32UInt, 0);
            context.SetVertexBuffer(VertexBuffer, Stride);
        }

        public void Draw(IGraphicsContext context)
        {
            context.SetIndexBuffer(IndexBuffer, Format.R32UInt, 0);
            context.SetVertexBuffer(VertexBuffer, Stride);
            context.DrawIndexedInstanced(IndexCount, 1, 0, 0, 0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                VertexBuffer.Dispose();
                IndexBuffer.Dispose();
                Solid.Dispose();
                Overlay.Dispose();
                Normals.Dispose();
                Points.Dispose();

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