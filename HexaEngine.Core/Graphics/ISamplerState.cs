namespace HexaEngine.Core.Graphics
{
    public interface ISamplerState : IDeviceChild
    {
        public SamplerStateDescription Description { get; }
    }
}