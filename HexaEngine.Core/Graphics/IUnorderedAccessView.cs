namespace HexaEngine.Core.Graphics
{
    using Silk.NET.Core.Attributes;

    public interface IUnorderedAccessView : IDeviceChild
    {
        public UnorderedAccessViewDescription Description { get; }
    }
}