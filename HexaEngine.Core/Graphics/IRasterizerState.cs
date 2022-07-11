namespace HexaEngine.Core.Graphics
{
    using System;

    public interface IRasterizerState : IDeviceChild
    {
        RasterizerDescription Description { get; }
    }
}