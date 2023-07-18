namespace VkTesting.Graphics
{
    using HexaEngine.Vulkan;
    using Silk.NET.Vulkan;
    using System;
    using static HexaEngine.Vulkan.VulkanGraphicsDevice;

    public unsafe class VulkanCommandAllocator : IDisposable
    {
        private readonly VulkanGraphicsDevice device;
        private CommandPool commandPool;
        private bool disposedValue;

        public VulkanCommandAllocator(VulkanGraphicsDevice device)
        {
            this.device = device;
            QueueFamilyIndices queueFamilyIndices = FindQueueFamilies(device.PhysicalDevice, device.Surface);

            CommandPoolCreateInfo poolInfo = default;
            poolInfo.SType = StructureType.CommandPoolCreateInfo;
            poolInfo.Flags = CommandPoolCreateFlags.ResetCommandBufferBit;
            poolInfo.QueueFamilyIndex = queueFamilyIndices.GraphicsFamily.Value;

            var result = vk.CreateCommandPool(device.Device, poolInfo, null, out commandPool);

            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }
        }

        internal CommandPool CommandPool => commandPool;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                vk.DestroyCommandPool(device.Device, commandPool, null);
                disposedValue = true;
            }
        }

        ~VulkanCommandAllocator()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}