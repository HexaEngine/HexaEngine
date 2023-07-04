namespace HexaEngine.Vulkan
{
    using System;
    using Silk.NET.Vulkan;

    public class VulkanException : Exception
    {
        public VulkanException(Result result) : base(result.ToString())
        {
        }
    }
}