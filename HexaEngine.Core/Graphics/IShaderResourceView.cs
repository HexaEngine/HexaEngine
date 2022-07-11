namespace HexaEngine.Core.Graphics
{
    public interface IShaderResourceView : IDeviceChild
    {
        ShaderResourceViewDescription Description { get; }
        bool IsDisposed { get; }
    }
}