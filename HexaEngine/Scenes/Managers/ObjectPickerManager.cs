﻿namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using HexaEngine.Resources;
    using ImGuiNET;
    using System.Numerics;
    using Texture = Graphics.Texture;

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
        private static IGraphicsDevice device;
        private static IGraphicsPipeline pipeline;
        private static Texture texture;

        private static IComputePipeline computePipeline;
        private static ConstantBuffer<Vector4> mouseBuffer;
        private static StructuredUavBuffer<SelectionData> outputBuffer;

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

        public static unsafe GameObject? Select(IGraphicsContext context, Vector2 position, Viewport viewport)
        {
            InstanceManager? manager = InstanceManager.Current;
            if (manager == null)
                return null;

            context.ClearDepthStencilView(texture.DepthStencilView, DepthStencilClearFlags.All, 1, 0);
            context.ClearRenderTargetView(texture.RenderTargetView, default);
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