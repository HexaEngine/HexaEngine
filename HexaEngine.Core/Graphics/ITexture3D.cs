namespace HexaEngine.Core.Graphics
{
    public interface ITexture3D : IResource
    {
        public Texture3DDescription Description { get; }
    }
}