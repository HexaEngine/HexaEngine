namespace HexaEngine.Graphics.Graph
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;

    public sealed class Texture2DView : TextureView<Texture2D, Texture2DDescription>
    {
        public Texture2DView(GraphResourceBuilder builder, string name, in Texture2DDescription description) : base(builder, name, description)
        {
        }

        public override Viewport Viewport => new(description.Width, description.Height);
    }
}