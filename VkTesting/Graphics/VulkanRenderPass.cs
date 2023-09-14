namespace VkTesting.Graphics
{
    using Silk.NET.Vulkan;
    using VkTesting.Unsafes;
    using static VulkanGraphicsDevice;

    public unsafe class VulkanRenderPass : IDisposable
    {
        private readonly VulkanGraphicsDevice device;
        private readonly RenderPass renderPass;
        private bool disposedValue;
        private readonly UnsafeList<Framebuffer> framebuffers = new();

        public VulkanRenderPass(VulkanGraphicsDevice device, Format format)
        {
            AttachmentDescription colorAttachment = default;
            colorAttachment.Format = format;
            colorAttachment.Samples = SampleCountFlags.Count1Bit;
            colorAttachment.LoadOp = AttachmentLoadOp.Clear;
            colorAttachment.StoreOp = AttachmentStoreOp.Store;
            colorAttachment.InitialLayout = ImageLayout.Undefined;
            colorAttachment.FinalLayout = ImageLayout.PresentSrcKhr;

            AttachmentReference colorAttachmentRef = default;
            colorAttachmentRef.Attachment = 0;
            colorAttachmentRef.Layout = ImageLayout.ColorAttachmentOptimal;

            SubpassDescription subpass = default;
            subpass.PipelineBindPoint = PipelineBindPoint.Graphics;
            subpass.ColorAttachmentCount = 1;
            subpass.PColorAttachments = &colorAttachmentRef;

            SubpassDependency dependency = default;
            dependency.SrcSubpass = Vk.SubpassExternal;
            dependency.DstSubpass = 0;
            dependency.SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit;
            dependency.SrcAccessMask = 0;
            dependency.DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit;
            dependency.DstAccessMask = AccessFlags.ColorAttachmentWriteBit;

            RenderPassCreateInfo renderPassInfo = default;
            renderPassInfo.SType = StructureType.RenderPassCreateInfo;
            renderPassInfo.AttachmentCount = 1;
            renderPassInfo.PAttachments = &colorAttachment;
            renderPassInfo.SubpassCount = 1;
            renderPassInfo.PSubpasses = &subpass;
            renderPassInfo.DependencyCount = 1;
            renderPassInfo.PDependencies = &dependency;

            var result = vk.CreateRenderPass(device.Device, renderPassInfo, null, out renderPass);

            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            this.device = device;
        }

        internal RenderPass RenderPass => renderPass;

        public UnsafeList<Framebuffer> Framebuffers => framebuffers;

        public void ReleaseFramebuffers()
        {
            for (int i = 0; i < framebuffers.Size; i++)
            {
                vk.DestroyFramebuffer(device.Device, framebuffers[i], null);
            }
        }

        public void BeginRenderPass(VulkanCommandList commandList, uint imageIndex, Extent2D extent)
        {
            RenderPassBeginInfo renderPassInfo = default;
            renderPassInfo.SType = StructureType.RenderPassBeginInfo;
            renderPassInfo.RenderPass = renderPass;
            renderPassInfo.Framebuffer = framebuffers[imageIndex];
            renderPassInfo.RenderArea.Offset = default;
            renderPassInfo.RenderArea.Extent = extent;
            ClearValue clearColor = new(new(0, 0, 0, 1));
            renderPassInfo.ClearValueCount = 1;
            renderPassInfo.PClearValues = &clearColor;

            vk.CmdBeginRenderPass(commandList.CommandBuffer, renderPassInfo, SubpassContents.Inline);
        }

        public void EndRenderPass(VulkanCommandList commandList)
        {
            vk.CmdEndRenderPass(commandList.CommandBuffer);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < framebuffers.Size; i++)
                {
                    vk.DestroyFramebuffer(device.Device, framebuffers[i], null);
                }
                vk.DestroyRenderPass(device.Device, renderPass, null);
                disposedValue = true;
            }
        }

        ~VulkanRenderPass()
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