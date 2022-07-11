namespace HexaEngine.Core.Graphics
{
    public interface IVertexShader : IDeviceChild
    {
        public void Bind(IGraphicsContext context);
    }
}