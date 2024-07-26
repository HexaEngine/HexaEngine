namespace HexaEngine.Vulkan
{
    using Hexa.NET.Utilities;
    using Silk.NET.Vulkan;

    public struct VkSwapChainSupportDetails
    {
        public SurfaceCapabilitiesKHR Capabilities;
        public UnsafeList<SurfaceFormatKHR> Formats;
        public UnsafeList<PresentModeKHR> PresentModes;
    }
}