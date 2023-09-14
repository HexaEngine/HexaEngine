namespace VkTesting.Graphics
{
    using Silk.NET.Vulkan;
    using System;
    using VkTesting.Unsafes;
    using VkTesting.Windows;
    using static VkTesting.Graphics.VulkanGraphicsDevice;

    public struct SwapChainSupportDetails
    {
        public SurfaceCapabilitiesKHR Capabilities;
        public UnsafeList<SurfaceFormatKHR> Formats;
        public UnsafeList<PresentModeKHR> PresentModes;
    }

    public unsafe class VulkanSwapChain : IDisposable
    {
        public VulkanGraphicsDevice device;
        public IWindow window;
        public SwapchainKHR SwapChain;
        public UnsafeList<Image> SwapChainImages = new();
        public Format format;
        public Extent2D extent;
        public UnsafeList<ImageView> SwapChainImageViews = new();

        private readonly UnsafeList<Semaphore> imageAvailableSemaphores = new();
        private readonly UnsafeList<Semaphore> renderFinishedSemaphores = new();
        private readonly UnsafeList<Fence> inFlightFences = new();

        private uint currentFrame = 0;
        private bool framebufferResized;
        private bool framebufferWaitResize;
        private bool framebufferCreated;
        private bool disposedValue;
        private const int MAX_FRAMES_IN_FLIGHT = 2;

        public VulkanSwapChain(VulkanGraphicsDevice device, IWindow window)
        {
            this.device = device;
            this.window = window;
            CreateSwapChain();
            CreateImageViews();
            CreateSyncObjects();
        }

        public uint ImageIndex { get; private set; }

        public uint CurrentFrame => currentFrame;

        public Format Format => format;

        public Extent2D Extent => extent;

        public int Count => SwapChainImageViews.Size;

        public UnsafeList<Semaphore> ImageAvailableSemaphores => imageAvailableSemaphores;

        public UnsafeList<Semaphore> RenderFinishedSemaphores => renderFinishedSemaphores;

        public UnsafeList<Fence> InFlightFences => inFlightFences;

        public event Action? Resized;

        public static bool IsDeviceSuitable(PhysicalDevice device, SurfaceKHR surface)
        {
            SwapChainSupportDetails swapChainSupport = QuerySwapChainSupport(device, surface);
            return !swapChainSupport.Formats.Empty && !swapChainSupport.PresentModes.Empty;
        }

        public static SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice device, SurfaceKHR surface)
        {
            SwapChainSupportDetails details;
            details.Formats = new();
            details.PresentModes = new();

            KhrSurface.GetPhysicalDeviceSurfaceCapabilities(device, surface, &details.Capabilities);

            uint formatCount;
            KhrSurface.GetPhysicalDeviceSurfaceFormats(device, surface, &formatCount, null);

            if (formatCount != 0)
            {
                details.Formats.Resize((int)formatCount);
                KhrSurface.GetPhysicalDeviceSurfaceFormats(device, surface, &formatCount, details.Formats.Data);
            }

            uint presentModeCount;
            KhrSurface.GetPhysicalDeviceSurfacePresentModes(device, surface, &presentModeCount, null);

            if (presentModeCount != 0)
            {
                details.PresentModes.Resize((int)presentModeCount);
                KhrSurface.GetPhysicalDeviceSurfacePresentModes(device, surface, &presentModeCount, details.PresentModes.Data);
            }

            return details;
        }

        internal static SurfaceFormatKHR ChooseSwapSurfaceFormat(UnsafeList<SurfaceFormatKHR> availableFormats)
        {
            for (int i = 0; i < availableFormats.Size; i++)
            {
                SurfaceFormatKHR availableFormat = availableFormats[i];
                if (availableFormat.Format == Format.B8G8R8A8Srgb && availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
                {
                    return availableFormat;
                }
            }

            return availableFormats[0];
        }

        internal static PresentModeKHR ChooseSwapPresentMode(UnsafeList<PresentModeKHR> availablePresentModes)
        {
            for (int i = 0; i < availablePresentModes.Size; i++)
            {
                PresentModeKHR availablePresentMode = availablePresentModes[i];
                if (availablePresentMode == PresentModeKHR.MailboxKhr)
                {
                    return availablePresentMode;
                }
            }

            return PresentModeKHR.FifoKhr;
        }

        internal Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
        {
            if (capabilities.CurrentExtent.Width != uint.MaxValue)
            {
                return capabilities.CurrentExtent;
            }
            else
            {
                Extent2D actualExtent = new((uint)window.Width, (uint)window.Height);

                actualExtent.Width = Math.Clamp(actualExtent.Width, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width);
                actualExtent.Height = Math.Clamp(actualExtent.Height, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height);

                return actualExtent;
            }
        }

        private void CreateSwapChain()
        {
            SwapChainSupportDetails swapChainSupport = QuerySwapChainSupport(device.PhysicalDevice, device.Surface);

            SurfaceFormatKHR surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
            PresentModeKHR presentMode = ChooseSwapPresentMode(swapChainSupport.PresentModes);
            Extent2D extent = ChooseSwapExtent(swapChainSupport.Capabilities);

            if (extent.Width == 0 || extent.Height == 0)
            {
                framebufferWaitResize = true;
                return;
            }

            uint imageCount = swapChainSupport.Capabilities.MinImageCount + 1;

            if (swapChainSupport.Capabilities.MaxImageCount > 0 && imageCount > swapChainSupport.Capabilities.MaxImageCount)
            {
                imageCount = swapChainSupport.Capabilities.MaxImageCount;
            }

            SwapchainCreateInfoKHR createInfo = default;
            createInfo.SType = StructureType.SwapchainCreateInfoKhr;
            createInfo.Surface = device.Surface;
            createInfo.MinImageCount = imageCount;
            createInfo.ImageFormat = surfaceFormat.Format;
            createInfo.ImageColorSpace = surfaceFormat.ColorSpace;
            createInfo.ImageExtent = extent;
            createInfo.ImageArrayLayers = 1;
            createInfo.ImageUsage = ImageUsageFlags.ColorAttachmentBit;

            QueueFamilyIndices indices = FindQueueFamilies(device.PhysicalDevice, device.Surface);
            uint* queueFamilyIndices = stackalloc uint[] { indices.GraphicsFamily.Value, indices.PresentFamily.Value };

            if (indices.GraphicsFamily != indices.PresentFamily)
            {
                createInfo.ImageSharingMode = SharingMode.Concurrent;
                createInfo.QueueFamilyIndexCount = 2;
                createInfo.PQueueFamilyIndices = queueFamilyIndices;
            }
            else
            {
                createInfo.ImageSharingMode = SharingMode.Exclusive;
                createInfo.QueueFamilyIndexCount = 0; // Optional
                createInfo.PQueueFamilyIndices = null; // Optional
            }

            createInfo.PreTransform = swapChainSupport.Capabilities.CurrentTransform;
            createInfo.CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr;
            createInfo.PresentMode = presentMode;
            createInfo.Clipped = true;
            createInfo.OldSwapchain = new(0);

            SwapchainKHR swapchainKHR;
            var result = KhrSwapchain.CreateSwapchain(device.Device, createInfo, null, &swapchainKHR);
            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            SwapChain = swapchainKHR;
            KhrSwapchain.GetSwapchainImages(device.Device, swapchainKHR, &imageCount, null);
            SwapChainImages.Resize((int)imageCount);
            KhrSwapchain.GetSwapchainImages(device.Device, swapchainKHR, &imageCount, SwapChainImages.Data);

            format = surfaceFormat.Format;
            this.extent = extent;

            framebufferCreated = true;
        }

        private void CreateImageViews()
        {
            SwapChainImageViews.Resize(SwapChainImages.Size);
            for (int i = 0; i < SwapChainImages.Size; i++)
            {
                ImageViewCreateInfo createInfo = default;
                createInfo.SType = StructureType.ImageViewCreateInfo;
                createInfo.Image = SwapChainImages[i];
                createInfo.ViewType = ImageViewType.Type2D;
                createInfo.Format = format;
                createInfo.Components.R = ComponentSwizzle.Identity;
                createInfo.Components.G = ComponentSwizzle.Identity;
                createInfo.Components.B = ComponentSwizzle.Identity;
                createInfo.Components.A = ComponentSwizzle.Identity;
                createInfo.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
                createInfo.SubresourceRange.BaseMipLevel = 0;
                createInfo.SubresourceRange.LevelCount = 1;
                createInfo.SubresourceRange.BaseArrayLayer = 0;
                createInfo.SubresourceRange.LayerCount = 1;

                var result = vk.CreateImageView(device.Device, createInfo, null, &SwapChainImageViews.Data[i]);
                if (result != Result.Success)
                {
                    throw new VulkanException(result);
                }
            }
        }

        public void CreateFramebuffers(VulkanRenderPass renderPass)
        {
            if (!renderPass.Framebuffers.Empty)
            {
                for (int i = 0; i < renderPass.Framebuffers.Size; i++)
                {
                    vk.DestroyFramebuffer(device.Device, renderPass.Framebuffers[i], null);
                }
            }

            renderPass.Framebuffers.Resize(SwapChainImageViews.Size);

            for (int i = 0; i < SwapChainImageViews.Size; i++)
            {
                FramebufferCreateInfo framebufferInfo = default;
                framebufferInfo.SType = StructureType.FramebufferCreateInfo;
                framebufferInfo.RenderPass = renderPass.RenderPass;
                framebufferInfo.AttachmentCount = 1;
                framebufferInfo.PAttachments = &SwapChainImageViews.Data[i];
                framebufferInfo.Width = extent.Width;
                framebufferInfo.Height = extent.Height;
                framebufferInfo.Layers = 1;

                var result = vk.CreateFramebuffer(device.Device, &framebufferInfo, null, &renderPass.Framebuffers.Data[i]);

                if (result != Result.Success)
                {
                    throw new VulkanException(result);
                }
            }
        }

        private void CreateSyncObjects()
        {
            SemaphoreCreateInfo semaphoreInfo = default;
            semaphoreInfo.SType = StructureType.SemaphoreCreateInfo;

            FenceCreateInfo fenceInfo = default;
            fenceInfo.SType = StructureType.FenceCreateInfo;
            fenceInfo.Flags = FenceCreateFlags.SignaledBit;

            imageAvailableSemaphores.Resize(MAX_FRAMES_IN_FLIGHT);
            renderFinishedSemaphores.Resize(MAX_FRAMES_IN_FLIGHT);
            inFlightFences.Resize(MAX_FRAMES_IN_FLIGHT);

            for (int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
            {
                var result = vk.CreateSemaphore(device.Device, semaphoreInfo, null, &imageAvailableSemaphores.Data[i]);
                if (result != Result.Success)
                {
                    throw new VulkanException(result);
                }

                result = vk.CreateSemaphore(device.Device, semaphoreInfo, null, &renderFinishedSemaphores.Data[i]);

                if (result != Result.Success)
                {
                    throw new VulkanException(result);
                }

                result = vk.CreateFence(device.Device, fenceInfo, null, &inFlightFences.Data[i]);

                if (result != Result.Success)
                {
                    throw new VulkanException(result);
                }
            }
        }

        public void RecreateSwapChain()
        {
            if (window.Width == 0 || window.Height == 0)
            {
                framebufferWaitResize = true;
                return;
            }

            framebufferWaitResize = false;

            vk.DeviceWaitIdle(device.Device);

            Cleanup();

            CreateSwapChain();
            if (framebufferWaitResize)
            {
                return;
            }
            CreateImageViews();
            Resized?.Invoke();
        }

        private void Cleanup()
        {
            if (!framebufferCreated)
            {
                return;
            }

            for (int i = 0; i < SwapChainImageViews.Size; i++)
            {
                vk.DestroyImageView(device.Device, SwapChainImageViews[i], null);
            }

            KhrSwapchain.DestroySwapchain(device.Device, SwapChain, null);

            framebufferCreated = false;
        }

        public void Resize()
        {
            framebufferResized = true;
        }

        public void BeginFrame()
        {
            if (framebufferWaitResize)
            {
                RecreateSwapChain();
                return;
            }

            vk.WaitForFences(device.Device, 1, &inFlightFences.Data[currentFrame], true, ulong.MaxValue);

            uint imageIndex;
            var result = KhrSwapchain.AcquireNextImage(device.Device, SwapChain, ulong.MaxValue, imageAvailableSemaphores.Data[currentFrame], new(0), &imageIndex);
            ImageIndex = imageIndex;

            if (result == Result.ErrorOutOfDateKhr)
            {
                RecreateSwapChain();
                return;
            }
            else if (result != Result.Success && result != Result.SuboptimalKhr)
            {
                throw new VulkanException(result);
            }

            vk.ResetFences(device.Device, 1, &inFlightFences.Data[currentFrame]);
        }

        public void Present()
        {
            Semaphore* signalSemaphores = stackalloc Semaphore[] { renderFinishedSemaphores[currentFrame] };
            SwapchainKHR* swapChains = stackalloc SwapchainKHR[] { SwapChain };
            PresentInfoKHR presentInfo = default;
            presentInfo.SType = StructureType.PresentInfoKhr;
            presentInfo.WaitSemaphoreCount = 1;
            presentInfo.PWaitSemaphores = signalSemaphores;

            uint imageIndex = ImageIndex;
            presentInfo.SwapchainCount = 1;
            presentInfo.PSwapchains = swapChains;
            presentInfo.PImageIndices = &imageIndex;
            presentInfo.PResults = null;

            var result = KhrSwapchain.QueuePresent(device.PresentQueue, &presentInfo);

            if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr || framebufferResized)
            {
                RecreateSwapChain();
            }
            else if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            currentFrame = (currentFrame + 1) % MAX_FRAMES_IN_FLIGHT;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Cleanup();
                for (int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
                {
                    vk.DestroySemaphore(device.Device, renderFinishedSemaphores[i], null);
                    vk.DestroySemaphore(device.Device, imageAvailableSemaphores[i], null);
                    vk.DestroyFence(device.Device, inFlightFences[i], null);
                }
                disposedValue = true;
            }
        }

        ~VulkanSwapChain()
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