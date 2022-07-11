namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public interface IRenderTargetView : IDeviceChild
    {
        RenderTargetViewDescription Description { get; }

        public Viewport Viewport { get; }
    }
}