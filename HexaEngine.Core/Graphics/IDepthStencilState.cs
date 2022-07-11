namespace HexaEngine.Core.Graphics
{
    using System;

    public interface IDepthStencilState : IDeviceChild
    {
        DepthStencilDescription Description { get; }
    }
}