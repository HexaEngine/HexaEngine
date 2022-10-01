namespace HexaEngine.Core.Graphics
{
    using Silk.NET.Core.Attributes;

    public interface IUnorderedAccessView : IDeviceChild
    {
    }

    public struct UnorderedAccessViewDesciption
    {
        public Format Format;
        public UAVDimension Dimension;
    }

    public enum UAVDimension
    {
        Unknown = 0x0,
        Buffer = 0x1,
        Texture1D = 0x2,
        Texture1Darray = 0x3,
        Texture2D = 0x4,
        Texture2Darray = 0x5,
        Texture3D = 0x8
    }
}