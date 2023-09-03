namespace HexaEngine.Core.Graphics
{
    public interface IRenderTargetView : IDeviceChild
    {
        RenderTargetViewDescription Description { get; }
    }
}