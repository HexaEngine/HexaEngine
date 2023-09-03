namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Rendering.Graph;
    using System.Numerics;

    public struct SelectionResult
    {
        public uint InstanceId;
        public uint TypeId;
        public uint PrimitiveId;
        public uint VertexId;

        public override readonly string ToString()
        {
            return $"Type:{TypeId}, Instance:{InstanceId}, Primitive:{PrimitiveId}, Vertex:{VertexId}";
        }
    }

    public static class ObjectPickerManager
    {
        private static IGraphicsDevice device;
        private static IGraphicsPipeline pipeline;
        private static Texture2D texture;
        private static DepthStencil depthStencil;
        private static IComputePipeline computePipeline;

        private static ConstantBuffer<Vector4> mouseBuffer;
        private static StructuredUavBuffer<SelectionResult> outputBuffer;
        private static Graph.ResourceRef<ConstantBuffer<CBCamera>> camera;

        public static void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height)
        {
            ObjectPickerManager.device = device;
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "forward/selection/vs.hlsl",
                PixelShader = "forward/selection/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList,
            });
            texture = new(device, Format.R32G32B32A32UInt, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            depthStencil = new(device, width, height, Format.D32FloatS8X24UInt);

            computePipeline = device.CreateComputePipeline(new()
            {
                Path = "compute/selection/shader.hlsl",
            });
            mouseBuffer = new(device, CpuAccessFlags.Write);
            outputBuffer = new(device, 1, CpuAccessFlags.RW);
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
        }

        public static void Resize(int width, int height)
        {
            /*
            texture.Dispose();
            texture = new(device, Format.R32G32B32A32UInt, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            depthStencil = new(device, width, height, Format.D32FloatS8X24UInt);
            */
        }

        public static unsafe GameObject? SelectObject(IGraphicsContext context, Vector2 position, Mathematics.Viewport viewport)
        {
            /*
            InstanceManager? manager = InstanceManager.Current;
            if (manager == null)
            {
                return default;
            }

            var data = Select(context, position, viewport);

            if (data.InstanceId == 0 || data.TypeId == 0)
            {
                return null;
            }

            ImGuiConsole.Log(LogSeverity.Log, data.ToString());

            return manager.Types[(int)data.TypeId - 1].Instances[(int)data.InstanceId - 1].Parent;
            */
            return null;
        }

        public static unsafe SelectionResult Select(IGraphicsContext context, Vector2 position, Mathematics.Viewport viewport)
        {
            /*
            InstanceManager? manager = InstanceManager.Current;
            if (manager == null)
            {
                return default;
            }

            context.ClearDepthStencilView(texture.DepthStencilView, DepthStencilClearFlags.All, 1, 0);
            context.ClearRenderTargetView(texture.RenderTargetView, default);
            context.SetRenderTarget(texture.RenderTargetView, texture.DepthStencilView);
            context.SetViewport(viewport);
            pipeline.BeginDraw(context);
            context.VSSetConstantBuffer(camera.Value, 1);
            for (int i = 0; i < manager.TypeCount; i++)
            {
                var type = manager.Types[i];
                if (type.BeginDrawNoCulling(context))
                {
                    context.DrawIndexedInstanced((uint)type.IndexCount, (uint)type.Count, 0, 0, 0);
                }
            }

            mouseBuffer[0] = new(position, 0, 0);
            mouseBuffer.Update(context);

            context.ClearState();
            context.CSSetShaderResource(texture.ShaderResourceView, 0);
            context.CSSetConstantBuffer(mouseBuffer.Buffer, 0);
            void* uav = (void*)outputBuffer.UAV.NativePointer;
            context.CSSetUnorderedAccessViews(&uav, 1);
            computePipeline.Dispatch(context, 1, 1, 1);
            outputBuffer.Read(context);

            return outputBuffer[0];
            */
            return default;
        }

        public static void Release()
        {
            pipeline.Dispose();
            texture.Dispose();
            depthStencil.Dispose();
            computePipeline.Dispose();
            mouseBuffer.Dispose();
            outputBuffer.Dispose();
        }
    }
}