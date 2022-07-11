namespace HexaEngine.Core.Graphics
{
    public interface IGeometryShader : IDeviceChild
    {
        public void Bind(IGraphicsContext context);
    }
}