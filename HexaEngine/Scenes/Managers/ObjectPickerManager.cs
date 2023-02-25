using HexaEngine.Core.Graphics;

namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Mathematics;
    using System.Numerics;
    using Texture = Texture;

    public struct SelectionData
    {
        public uint InstanceId;
        public uint TypeId;

        public override string ToString()
        {
            return $"Type:{TypeId}, Instance:{InstanceId}";
        }
    }

    public static class ObjectPickerManager
    {
#pragma warning disable CS8618 // Non-nullable field 'device' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private static IGraphicsDevice device;
#pragma warning restore CS8618 // Non-nullable field 'device' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'pipeline' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private static IGraphicsPipeline pipeline;
#pragma warning restore CS8618 // Non-nullable field 'pipeline' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'texture' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private static Texture texture;
#pragma warning restore CS8618 // Non-nullable field 'texture' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

#pragma warning disable CS8618 // Non-nullable field 'computePipeline' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private static IComputePipeline computePipeline;
#pragma warning restore CS8618 // Non-nullable field 'computePipeline' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'mouseBuffer' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private static ConstantBuffer<Vector4> mouseBuffer;
#pragma warning restore CS8618 // Non-nullable field 'mouseBuffer' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'outputBuffer' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private static StructuredUavBuffer<SelectionData> outputBuffer;
#pragma warning restore CS8618 // Non-nullable field 'outputBuffer' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

        public static void Initialize(IGraphicsDevice device, int width, int height)
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
            texture = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.RGBA32UInt), DepthStencilDesc.Default);

            computePipeline = device.CreateComputePipeline(new()
            {
                Path = "compute/selection/shader.hlsl",
            });
            mouseBuffer = new(device, CpuAccessFlags.Write);
            outputBuffer = new(device, 1, true, true);
        }

        public static void Resize(int width, int height)
        {
            texture.Dispose();
            texture = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.RGBA32UInt), DepthStencilDesc.Default);
        }

        public static unsafe GameObject? Select(IGraphicsContext context, Vector2 position, Mathematics.Viewport viewport)
        {
            InstanceManager? manager = InstanceManager.Current;
            if (manager == null)
                return null;

#pragma warning disable CS8604 // Possible null reference argument for parameter 'depthStencilView' in 'void IGraphicsContext.ClearDepthStencilView(IDepthStencilView depthStencilView, DepthStencilClearFlags flags, float depth, byte stencil)'.
            context.ClearDepthStencilView(texture.DepthStencilView, DepthStencilClearFlags.All, 1, 0);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'depthStencilView' in 'void IGraphicsContext.ClearDepthStencilView(IDepthStencilView depthStencilView, DepthStencilClearFlags flags, float depth, byte stencil)'.
#pragma warning disable CS8604 // Possible null reference argument for parameter 'renderTargetView' in 'void IGraphicsContext.ClearRenderTargetView(IRenderTargetView renderTargetView, Vector4 value)'.
            context.ClearRenderTargetView(texture.RenderTargetView, default);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'renderTargetView' in 'void IGraphicsContext.ClearRenderTargetView(IRenderTargetView renderTargetView, Vector4 value)'.
            context.SetRenderTarget(texture.RenderTargetView, texture.DepthStencilView);
            pipeline.BeginDraw(context, viewport);
            context.VSSetConstantBuffer(ResourceManager.GetConstantBuffer("CBCamera"), 1);
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

            SelectionData data = outputBuffer[0];

            if (data.InstanceId == 0 || data.TypeId == 0)
                return null;

            return manager.Types[(int)data.TypeId - 1].Instances[(int)data.InstanceId - 1].Parent;
        }

        public static void Release()
        {
            pipeline.Dispose();
            texture.Dispose();
            computePipeline.Dispose();
            mouseBuffer.Dispose();
            outputBuffer.Dispose();
        }
    }
}