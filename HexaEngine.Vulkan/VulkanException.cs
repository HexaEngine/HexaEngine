namespace HexaEngine.Vulkan
{
    using Hexa.NET.Vulkan;
    using System;

    public class VulkanException : Exception
    {
        public VulkanException(VkResult result) : base(result.ToString())
        {
        }
    }
}