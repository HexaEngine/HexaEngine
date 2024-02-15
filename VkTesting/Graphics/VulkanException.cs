namespace VkTesting.Graphics
{
    using Silk.NET.Vulkan;
    using System;

    public class VulkanException : Exception
    {
        public VulkanException(Result result) : base(result.ToString())
        {
        }
    }
}