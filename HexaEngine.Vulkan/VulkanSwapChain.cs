namespace HexaEngine.Vulkan
{
    using Hexa.NET.SDL2;
    using HexaEngine.Core.Graphics;
    using Hexa.NET.Utilities;
    using HexaEngine.Core.Windows.Events;
    using Silk.NET.Vulkan;
    using System;
    using Semaphore = Silk.NET.Vulkan.Semaphore;

    public unsafe class VulkanSwapChain : ISwapChain
    {
        public VulkanGraphicsDevice device;
        private SDLWindow* window;
        private readonly SurfaceKHR surface;
        private readonly SwapChainDescription? swapChainDescription;
        private readonly SwapChainFullscreenDescription? fullscreenDescription;
        public SwapchainKHR SwapChain;
        public UnsafeList<Image> SwapChainImages = new();
        public Silk.NET.Vulkan.Format format;
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
        private const uint MAX_FRAMES_IN_FLIGHT = 2;

        public VulkanSwapChain(VulkanGraphicsDevice device, SDLWindow* window, SurfaceKHR surface, SwapChainDescription? swapChainDescription, SwapChainFullscreenDescription? fullscreenDescription)
        {
            this.device = device;
            this.window = window;
            this.surface = surface;
            this.swapChainDescription = swapChainDescription;
            this.fullscreenDescription = fullscreenDescription;
            CreateSwapChain();
            CreateImageViews();
            CreateSyncObjects();
        }

        private void CreateSwapChain()
        {
            VkSwapChainSupportDetails swapChainSupport = QuerySwapChainSupport(device.PhysicalDevice, surface);

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
            createInfo.Surface = surface;
            createInfo.MinImageCount = imageCount;
            createInfo.ImageFormat = surfaceFormat.Format;
            createInfo.ImageColorSpace = surfaceFormat.ColorSpace;
            createInfo.ImageExtent = extent;
            createInfo.ImageArrayLayers = 1;
            createInfo.ImageUsage = ImageUsageFlags.ColorAttachmentBit;

            QueueFamilyIndices indices = VulkanAdapter.FindQueueFamilies(device.PhysicalDevice, surface);
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
            var result = VulkanAdapter.KhrSwapchain.CreateSwapchain(device.Device, createInfo, null, &swapchainKHR);
            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            SwapChain = swapchainKHR;
            VulkanAdapter.KhrSwapchain.GetSwapchainImages(device.Device, swapchainKHR, &imageCount, null);
            SwapChainImages.Resize(imageCount);
            VulkanAdapter.KhrSwapchain.GetSwapchainImages(device.Device, swapchainKHR, &imageCount, SwapChainImages.Data);

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

                var result = VulkanAdapter.Vk.CreateImageView(device.Device, createInfo, null, &SwapChainImageViews.Data[i]);
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
                var result = VulkanAdapter.Vk.CreateSemaphore(device.Device, semaphoreInfo, null, &imageAvailableSemaphores.Data[i]);
                if (result != Result.Success)
                {
                    throw new VulkanException(result);
                }

                result = VulkanAdapter.Vk.CreateSemaphore(device.Device, semaphoreInfo, null, &renderFinishedSemaphores.Data[i]);

                if (result != Result.Success)
                {
                    throw new VulkanException(result);
                }

                result = VulkanAdapter.Vk.CreateFence(device.Device, fenceInfo, null, &inFlightFences.Data[i]);

                if (result != Result.Success)
                {
                    throw new VulkanException(result);
                }
            }
        }

        public ITexture2D Backbuffer { get; }

        public IRenderTargetView BackbufferRTV { get; }

        public IDepthStencilView BackbufferDSV { get; }

        public int Width { get; }

        public int Height { get; }

        public Hexa.NET.Mathematics.Viewport Viewport { get; }

        public bool VSync { get; set; }

        public int TargetFPS { get; set; }

        public bool LimitFPS { get; set; }

        public bool Active { get; set; }

        public event EventHandler? Resizing;

        public event EventHandler<ResizedEventArgs>? Resized;

        public event EventHandler<DeviceRemovedEventArgs>? DeviceRemoved;

        public static bool IsDeviceSuitable(PhysicalDevice device, SurfaceKHR surface)
        {
            VkSwapChainSupportDetails swapChainSupport = QuerySwapChainSupport(device, surface);
            return !swapChainSupport.Formats.Empty && !swapChainSupport.PresentModes.Empty;
        }

        public static unsafe VkSwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice device, SurfaceKHR surface)
        {
            VkSwapChainSupportDetails details;
            details.Formats = new();
            details.PresentModes = new();

            VulkanAdapter.KhrSurface.GetPhysicalDeviceSurfaceCapabilities(device, surface, &details.Capabilities);

            uint formatCount;
            VulkanAdapter.KhrSurface.GetPhysicalDeviceSurfaceFormats(device, surface, &formatCount, null);

            if (formatCount != 0)
            {
                details.Formats.Resize(formatCount);
                VulkanAdapter.KhrSurface.GetPhysicalDeviceSurfaceFormats(device, surface, &formatCount, details.Formats.Data);
            }

            uint presentModeCount;
            VulkanAdapter.KhrSurface.GetPhysicalDeviceSurfacePresentModes(device, surface, &presentModeCount, null);

            if (presentModeCount != 0)
            {
                details.PresentModes.Resize(presentModeCount);
                VulkanAdapter.KhrSurface.GetPhysicalDeviceSurfacePresentModes(device, surface, &presentModeCount, details.PresentModes.Data);
            }

            return details;
        }

        internal SurfaceFormatKHR ChooseSwapSurfaceFormat(UnsafeList<SurfaceFormatKHR> availableFormats)
        {
            SurfaceFormatKHR chosenFormat = availableFormats[0];
            foreach (var availableFormat in availableFormats)
            {
                if (availableFormat.Format == Silk.NET.Vulkan.Format.B8G8R8A8Unorm && availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
                {
                    chosenFormat = availableFormat;
                }

                if (availableFormat.Format == Silk.NET.Vulkan.Format.A2B10G10R10UnormPack32 && availableFormat.ColorSpace == ColorSpaceKHR.SpaceHdr10ST2084Ext)
                {
                    chosenFormat = availableFormat;
                }
            }

            return chosenFormat;
        }

        internal static PresentModeKHR ChooseSwapPresentMode(UnsafeList<PresentModeKHR> availablePresentModes)
        {
            foreach (PresentModeKHR availablePresentMode in availablePresentModes)
            {
                if (availablePresentMode == PresentModeKHR.MailboxKhr)
                {
                    return availablePresentMode;
                }
            }

            return PresentModeKHR.FifoKhr;
        }

        internal Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
        {
            return new()
            {
                // TODO: Window size instead of 0
                Width = Math.Clamp(0, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width),
                Height = Math.Clamp(0, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height)
            };
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Present(uint syncInt)
        {
            throw new NotImplementedException();
        }

        public void Present(bool sync)
        {
            throw new NotImplementedException();
        }

        public void Present()
        {
            throw new NotImplementedException();
        }

        public void Resize(int width, int height)
        {
            throw new NotImplementedException();
        }

        public void Wait()
        {
            throw new NotImplementedException();
        }

        public void WaitForPresent()
        {
            throw new NotImplementedException();
        }
    }
}