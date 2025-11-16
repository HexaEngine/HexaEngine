namespace HexaEngine.Graphics.Graph
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;

    public sealed class Texture1DView : TextureView<Texture1D, Texture1DDescription>
    {
        public Texture1DView(GraphResourceBuilder builder, string name, in Texture1DDescription description) : base(builder, name, description)
        {
        }

        public override Viewport Viewport => new(description.Width);
    }
}