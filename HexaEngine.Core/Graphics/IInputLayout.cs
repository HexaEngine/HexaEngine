namespace HexaEngine.Core.Graphics
{
    public interface IInputLayout : IDeviceChild
    {
        public void Bind(IGraphicsContext context);
    }
}