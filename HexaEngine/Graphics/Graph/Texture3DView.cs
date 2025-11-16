namespace HexaEngine.Graphics.Graph
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;

    public sealed class Texture3DView : TextureView<Texture3D, Texture3DDescription>
    {
        public Texture3DView(GraphResourceBuilder builder, string name, in Texture3DDescription description) : base(builder, name, description)
        {
        }

        public override Viewport Viewport => new(description.Width, description.Height);
    }
}