namespace HexaEngine.Core.Graphics
{
    public interface IBlendState : IDeviceChild
    {
        BlendDescription Description { get; }
    }
}