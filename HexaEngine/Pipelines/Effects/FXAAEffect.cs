namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Nodes;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects.Primitives;
    using System.Collections.Generic;

    public class FXAA : Effect
    {
        public FXAA(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/fxaa/vs.hlsl",
            PixelShader = "effects/fxaa/ps.hlsl"
        })
        {
            AutoClear = true;
            Mesh = new Quad(device);
            TargetType = PinType.Texture2D;
            ResourceSlots.Add((0, "Image", PinType.Texture2D));
        }

        public override void Draw(IGraphicsContext context)
        {
#nullable disable
            DrawAuto(context, Target.Viewport);
#nullable enable
        }

        public void Draw(IGraphicsContext context, Viewport viewport)
        {
            DrawAuto(context, viewport);
        }

        public override void DrawSettings()
        {
            throw new NotImplementedException();
        }
    }
}