namespace HexaEngine.Core.Graphics
{
    public interface IDomainShader : IDeviceChild
    {
        public void Bind(IGraphicsContext context);
    }
}