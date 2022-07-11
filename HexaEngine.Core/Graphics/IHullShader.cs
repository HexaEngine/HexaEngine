namespace HexaEngine.Core.Graphics
{
    public interface IHullShader : IDeviceChild
    {
        public void Bind(IGraphicsContext context);
    }
}