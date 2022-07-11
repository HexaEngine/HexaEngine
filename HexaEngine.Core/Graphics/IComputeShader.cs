namespace HexaEngine.Core.Graphics
{
    public interface IComputeShader : IDeviceChild
    {
        public void Bind(IGraphicsContext context);
    }
}