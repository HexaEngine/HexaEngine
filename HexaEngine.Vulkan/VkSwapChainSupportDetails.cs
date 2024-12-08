namespace HexaEngine.Vulkan
{
    using Hexa.NET.Vulkan;
    using Hexa.NET.Utilities;

    public struct VkSwapChainSupportDetails
    {
        public VkSurfaceCapabilitiesKHR Capabilities;
        public UnsafeList<VkSurfaceFormatKHR> Formats;
        public UnsafeList<VkPresentModeKHR> PresentModes;
    }
}