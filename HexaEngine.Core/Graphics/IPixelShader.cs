namespace HexaEngine.Core.Graphics
{
    public interface IPixelShader : IDeviceChild
    {
        public void Bind(IGraphicsContext context);
    }
}