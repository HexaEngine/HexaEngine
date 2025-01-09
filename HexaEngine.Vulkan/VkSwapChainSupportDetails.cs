namespace HexaEngine.Vulkan
{
    using Hexa.NET.Utilities;
    using Hexa.NET.Vulkan;

    public struct VkSwapChainSupportDetails
    {
        public VkSurfaceCapabilitiesKHR Capabilities;
        public UnsafeList<VkSurfaceFormatKHR> Formats;
        public UnsafeList<VkPresentModeKHR> PresentModes;
    }
}