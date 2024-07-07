namespace HexaEngine.Vulkan
{
    using Hexa.NET.Vulkan;
    using HexaEngine.Core.Unsafes;

    public struct VkSwapChainSupportDetails
    {
        public VkSurfaceCapabilitiesKHR Capabilities;
        public UnsafeList<VkSurfaceFormatKHR> Formats;
        public UnsafeList<VkPresentModeKHR> PresentModes;
    }
}