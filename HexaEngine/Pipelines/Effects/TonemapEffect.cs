namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using ImGuiNET;
    using System.Numerics;

    public class Tonemap : Effect
    {
        private readonly IBuffer buffer;
        private float bloomStrength = 0.04f;
        private bool dirty;

        public Tonemap(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/tonemap/vs.hlsl",
            PixelShader = "effects/tonemap/ps.hlsl",
        })
        {
            Mesh = new Quad(device);
            State = new()
            {
                Blend = BlendDescription.Opaque,
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList,
            };
            TargetType = PinType.Texture2D;
            ResourceSlots.Add((0, "Image", PinType.Texture2D));
            buffer = device.CreateBuffer(new Params(bloomStrength), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            Constants.Add(new(buffer, ShaderStage.Pixel, 0));
        }

        private struct Params
        {
            public float BloomStrength;
            public Vector3 padd;

            public Params(float bloomStrength)
            {
                BloomStrength = bloomStrength;
                padd = default;
            }
        }

        public float BloomStrength
        { get => bloomStrength; set { bloomStrength = value; dirty = true; } }

        public override void Draw(IGraphicsContext context)
        {
            if (dirty)
            {
                context.Write(buffer, new Params(BloomStrength));
                dirty = false;
            }
#nullable disable
            DrawAuto(context, Target.Viewport);
#nullable enable
        }

        public override void DrawSettings()
        {
            if (ImGui.CollapsingHeader("Tonemap settings"))
            {
                if (ImGui.InputFloat("Bloom strength", ref bloomStrength))
                {
                    dirty = true;
                }
            }
        }
    }
}