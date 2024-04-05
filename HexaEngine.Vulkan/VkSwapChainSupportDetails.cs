namespace HexaEngine.Vulkan
{
    using HexaEngine.Core.Unsafes;
    using Silk.NET.Vulkan;

    public struct VkSwapChainSupportDetails
    {
        public SurfaceCapabilitiesKHR Capabilities;
        public UnsafeList<SurfaceFormatKHR> Formats;
        public UnsafeList<PresentModeKHR> PresentModes;
    }
}