﻿namespace HexaEngine.Editor.ImagePainter.Tools
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Editor.ImagePainter;
    using ImGuiNET;
    using System.Numerics;

    public class Pencil : Tool
    {
        private Vector2 brushSize = Vector2.One;
        private Quad quad;
        private IGraphicsPipeline brushPipeline;
        private Vector2 oldPos;
        public override string Icon => "\xED63##PencilTool";

        public override string Name => "Pencil";

        public override void Init(IGraphicsDevice device)
        {
            quad = new(device);
            DepthStencilDescription depthStencil = new()
            {
                DepthEnable = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthFunc = ComparisonFunction.Less,
                StencilEnable = false,
                StencilReadMask = 255,
                StencilWriteMask = 255,
                FrontFace = DepthStencilOperationDescription.DefaultFront,
                BackFace = DepthStencilOperationDescription.DefaultBack
            };
            brushPipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/brush/vs.hlsl",
                PixelShader = "effects/brush/ps.hlsl",
            }, new GraphicsPipelineState()
            {
                Blend = BlendDescription.AlphaBlend,
                BlendFactor = Vector4.Zero,
                DepthStencil = depthStencil,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList,
                SampleMask = int.MaxValue,
                StencilRef = 1,
            });
        }

        public override void DrawSettings()
        {
            ImGui.InputFloat2("Size", ref brushSize);
        }

        public override void Draw(Vector2 position, Vector2 ratio, IGraphicsContext context)
        {
            if (position - oldPos == Vector2.Zero)
            {
                return;
            }

            var size = brushSize;
            var pos = position * ratio - size / 2f;

            context.SetViewport(new(pos, size));
            quad.DrawAuto(context, brushPipeline);
            oldPos = position;
        }

        public override void DrawPreview(Vector2 position, Vector2 ratio, IGraphicsContext context)
        {
            var size = brushSize;
            var pos = position * ratio - size / 2f;

            context.SetViewport(new(pos, size));
            quad.DrawAuto(context, brushPipeline);
        }

        public override void Dispose()
        {
            brushPipeline.Dispose();
            quad.Dispose();
        }
    }
}