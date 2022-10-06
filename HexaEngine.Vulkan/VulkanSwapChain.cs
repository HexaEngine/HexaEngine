namespace HexaEngine.Vulkan
{
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;

    public class VulkanSwapChain : ISwapChain
    {
        public VulkanSwapChain()
        {
            throw new NotImplementedException();
        }

        public ITexture2D Backbuffer { get; }
        public IRenderTargetView BackbufferRTV { get; }
        public IDepthStencilView BackbufferDSV { get; }
        public int Width { get; }
        public int Height { get; }
        public Viewport Viewport { get; }

        public event EventHandler? Resizing;

        public event EventHandler<ResizedEventArgs>? Resized;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Present(uint syncInt)
        {
            throw new NotImplementedException();
        }

        public void Resize(int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}