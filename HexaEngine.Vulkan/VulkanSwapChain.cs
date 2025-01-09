namespace HexaEngine.Vulkan
{
    using Hexa.NET.SDL2;
    using Hexa.NET.Utilities;
    using Hexa.NET.Vulkan;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows.Events;
    using System;
    using VkSurfaceKHR = Hexa.NET.Vulkan.VkSurfaceKHR;

    public unsafe class VulkanSwapChain : ISwapChain
    {
        public VulkanGraphicsDevice device;
        private SDLWindow* window;
        private readonly VkSurfaceKHR surface;
        private readonly SwapChainDescription? swapChainDescription;
        private readonly SwapChainFullscreenDescription? fullscreenDescription;
        public VkSwapchainKHR SwapChain;
        public UnsafeList<VkImage> SwapChainImages = new();
        public VkFormat format;
        public VkExtent2D extent;
        public UnsafeList<VkImageView> SwapChainImageViews = new();

        private readonly UnsafeList<VkSemaphore> imageAvailableSemaphores = new();
        private readonly UnsafeList<VkSemaphore> renderFinishedSemaphores = new();
        private readonly UnsafeList<VkFence> inFlightFences = new();

        private uint currentFrame = 0;
        private bool framebufferResized;
        private bool framebufferWaitResize;
        private bool framebufferCreated;
        private bool disposedValue;
        private const int MAX_FRAMES_IN_FLIGHT = 2;

        public VulkanSwapChain(VulkanGraphicsDevice device, SDLWindow* window, VkSurfaceKHR surface, SwapChainDescription? swapChainDescription, SwapChainFullscreenDescription? fullscreenDescription)
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

            VkSurfaceFormatKHR surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
            VkPresentModeKHR presentMode = ChooseSwapPresentMode(swapChainSupport.PresentModes);
            VkExtent2D extent = ChooseSwapExtent(swapChainSupport.Capabilities);

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

            VkSwapchainCreateInfoKHR createInfo = default;
            createInfo.SType = VkStructureType.SwapchainCreateInfoKhr;
            createInfo.Surface = surface;
            createInfo.MinImageCount = imageCount;
            createInfo.ImageFormat = surfaceFormat.Format;
            createInfo.ImageColorSpace = surfaceFormat.ColorSpace;
            createInfo.ImageExtent = extent;
            createInfo.ImageArrayLayers = 1;
            createInfo.ImageUsage = (uint)VkImageUsageFlagBits.ColorAttachmentBit;

            QueueFamilyIndices indices = VulkanAdapter.FindQueueFamilies(device.PhysicalDevice, surface);
            uint* queueFamilyIndices = stackalloc uint[] { indices.GraphicsFamily.Value, indices.PresentFamily.Value };

            if (indices.GraphicsFamily != indices.PresentFamily)
            {
                createInfo.ImageSharingMode = VkSharingMode.Concurrent;
                createInfo.QueueFamilyIndexCount = 2;
                createInfo.PQueueFamilyIndices = queueFamilyIndices;
            }
            else
            {
                createInfo.ImageSharingMode = VkSharingMode.Exclusive;
                createInfo.QueueFamilyIndexCount = 0; // Optional
                createInfo.PQueueFamilyIndices = null; // Optional
            }

            createInfo.PreTransform = swapChainSupport.Capabilities.CurrentTransform;
            createInfo.CompositeAlpha = VkCompositeAlphaFlagBitsKHR.OpaqueBitKhr;
            createInfo.PresentMode = presentMode;
            createInfo.Clipped = 1;
            createInfo.OldSwapchain = new(0);

            VkSwapchainKHR swapchainKHR;
            var result = Vulkan.VkCreateSwapchainKHR(device.Device, &createInfo, null, &swapchainKHR);
            if (result != VkResult.Success)
            {
                throw new VulkanException(result);
            }

            SwapChain = swapchainKHR;
            Vulkan.VkGetSwapchainImagesKHR(device.Device, swapchainKHR, &imageCount, null);
            SwapChainImages.Resize((int)imageCount);
            Vulkan.VkGetSwapchainImagesKHR(device.Device, swapchainKHR, &imageCount, SwapChainImages.Data);

            format = surfaceFormat.Format;
            this.extent = extent;

            framebufferCreated = true;
        }

        private void CreateImageViews()
        {
            SwapChainImageViews.Resize(SwapChainImages.Size);
            for (int i = 0; i < SwapChainImages.Size; i++)
            {
                VkImageViewCreateInfo createInfo = default;
                createInfo.SType = VkStructureType.ImageViewCreateInfo;
                createInfo.Image = SwapChainImages[i];
                createInfo.ViewType = VkImageViewType.Type2D;
                createInfo.Format = format;
                createInfo.Components.R = VkComponentSwizzle.Identity;
                createInfo.Components.G = VkComponentSwizzle.Identity;
                createInfo.Components.B = VkComponentSwizzle.Identity;
                createInfo.Components.A = VkComponentSwizzle.Identity;
                createInfo.SubresourceRange.AspectMask = (uint)VkImageAspectFlagBits.ColorBit;
                createInfo.SubresourceRange.BaseMipLevel = 0;
                createInfo.SubresourceRange.LevelCount = 1;
                createInfo.SubresourceRange.BaseArrayLayer = 0;
                createInfo.SubresourceRange.LayerCount = 1;

                var result = Vulkan.VkCreateImageView(device.Device, &createInfo, null, &SwapChainImageViews.Data[i]);
                if (result != VkResult.Success)
                {
                    throw new VulkanException(result);
                }
            }
        }

        private void CreateSyncObjects()
        {
            VkSemaphoreCreateInfo semaphoreInfo = default;
            semaphoreInfo.SType = VkStructureType.SemaphoreCreateInfo;

            VkFenceCreateInfo fenceInfo = default;
            fenceInfo.SType = VkStructureType.FenceCreateInfo;
            fenceInfo.Flags = (uint)VkFenceCreateFlagBits.SignaledBit;

            imageAvailableSemaphores.Resize(MAX_FRAMES_IN_FLIGHT);
            renderFinishedSemaphores.Resize(MAX_FRAMES_IN_FLIGHT);
            inFlightFences.Resize(MAX_FRAMES_IN_FLIGHT);

            for (int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
            {
                var result = Vulkan.VkCreateSemaphore(device.Device, &semaphoreInfo, null, &imageAvailableSemaphores.Data[i]);
                if (result != VkResult.Success)
                {
                    throw new VulkanException(result);
                }

                result = Vulkan.VkCreateSemaphore(device.Device, &semaphoreInfo, null, &renderFinishedSemaphores.Data[i]);

                if (result != VkResult.Success)
                {
                    throw new VulkanException(result);
                }

                result = Vulkan.VkCreateFence(device.Device, &fenceInfo, null, &inFlightFences.Data[i]);

                if (result != VkResult.Success)
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

        public static bool IsDeviceSuitable(VkPhysicalDevice device, VkSurfaceKHR surface)
        {
            VkSwapChainSupportDetails swapChainSupport = QuerySwapChainSupport(device, surface);
            return !swapChainSupport.Formats.Empty && !swapChainSupport.PresentModes.Empty;
        }

        public static unsafe VkSwapChainSupportDetails QuerySwapChainSupport(VkPhysicalDevice device, VkSurfaceKHR surface)
        {
            VkSwapChainSupportDetails details;
            details.Formats = new();
            details.PresentModes = new();

            Vulkan.VkGetPhysicalDeviceSurfaceCapabilitiesKHR(device, surface, &details.Capabilities);

            uint formatCount;
            Vulkan.VkGetPhysicalDeviceSurfaceFormatsKHR(device, surface, &formatCount, null);

            if (formatCount != 0)
            {
                details.Formats.Resize((int)formatCount);
                Vulkan.VkGetPhysicalDeviceSurfaceFormatsKHR(device, surface, &formatCount, details.Formats.Data);
            }

            uint presentModeCount;
            Vulkan.VkGetPhysicalDeviceSurfacePresentModesKHR(device, surface, &presentModeCount, null);

            if (presentModeCount != 0)
            {
                details.PresentModes.Resize((int)presentModeCount);
                Vulkan.VkGetPhysicalDeviceSurfacePresentModesKHR(device, surface, &presentModeCount, details.PresentModes.Data);
            }

            return details;
        }

        internal VkSurfaceFormatKHR ChooseSwapSurfaceFormat(UnsafeList<VkSurfaceFormatKHR> availableFormats)
        {
            VkSurfaceFormatKHR chosenFormat = availableFormats[0];
            foreach (var availableFormat in availableFormats)
            {
                if (availableFormat.Format == VkFormat.B8G8R8A8_UNORM && availableFormat.ColorSpace == VkColorSpaceKHR.SrgbNonlinearKhr)
                {
                    chosenFormat = availableFormat;
                }

                if (availableFormat.Format == VkFormat.A2B10G10R10_UNORM_PACK32 && availableFormat.ColorSpace == VkColorSpaceKHR.Hdr10St2084Ext)
                {
                    chosenFormat = availableFormat;
                }
            }

            return chosenFormat;
        }

        internal static VkPresentModeKHR ChooseSwapPresentMode(UnsafeList<VkPresentModeKHR> availablePresentModes)
        {
            foreach (VkPresentModeKHR availablePresentMode in availablePresentModes)
            {
                if (availablePresentMode == VkPresentModeKHR.MailboxKhr)
                {
                    return availablePresentMode;
                }
            }

            return VkPresentModeKHR.FifoKhr;
        }

        internal VkExtent2D ChooseSwapExtent(VkSurfaceCapabilitiesKHR capabilities)
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