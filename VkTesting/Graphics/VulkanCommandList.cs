namespace VkTesting.Graphics
{
    using HexaEngine.Vulkan;
    using Silk.NET.Core.Native;
    using Silk.NET.Vulkan;
    using static HexaEngine.Vulkan.VulkanGraphicsDevice;

    public unsafe class VulkanCommandList : IDisposable
    {
        private readonly VulkanGraphicsDevice device;
        private readonly VulkanCommandAllocator commandAllocator;
        private CommandBuffer commandBuffer;
        private bool disposedValue;

        public VulkanCommandList(VulkanGraphicsDevice device, VulkanCommandAllocator commandAllocator)
        {
            this.device = device;
            this.commandAllocator = commandAllocator;
            CommandBufferAllocateInfo allocInfo = default;
            allocInfo.SType = StructureType.CommandBufferAllocateInfo;
            allocInfo.CommandPool = commandAllocator.CommandPool;
            allocInfo.Level = CommandBufferLevel.Primary;
            allocInfo.CommandBufferCount = 1;

            var result = vk.AllocateCommandBuffers(device.Device, allocInfo, out commandBuffer);

            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }
        }

        internal CommandBuffer CommandBuffer => commandBuffer;

        public void Reset()
        {
            vk.ResetCommandBuffer(commandBuffer, 0);
        }

        public void Begin()
        {
            CommandBufferBeginInfo beginInfo = default;
            beginInfo.SType = StructureType.CommandBufferBeginInfo;
            beginInfo.Flags = 0; // Optional
            beginInfo.PInheritanceInfo = null; // Optional

            var result = vk.BeginCommandBuffer(commandBuffer, beginInfo);
            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }
        }

        public void End()
        {
            var result = vk.EndCommandBuffer(commandBuffer);
            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }
        }

        public void SetViewport(uint firstViewport, uint viewportCount, Viewport* viewports)
        {
            vk.CmdSetViewport(commandBuffer, firstViewport, viewportCount, viewports);
        }

        public void SetScissor(uint firstScissor, uint scissorCount, Rect2D* scissors)
        {
            vk.CmdSetScissor(commandBuffer, firstScissor, scissorCount, scissors);
        }

        public void Draw(uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
        {
            vk.CmdDraw(commandBuffer, vertexCount, instanceCount, firstVertex, firstInstance);
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int vertexOffset, uint firstInstance)
        {
            vk.CmdDrawIndexed(commandBuffer, indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                var CommandBuffer = commandBuffer;
                vk.FreeCommandBuffers(device.Device, commandAllocator.CommandPool, 1, &CommandBuffer);
                disposedValue = true;
            }
        }

        ~VulkanCommandList()
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