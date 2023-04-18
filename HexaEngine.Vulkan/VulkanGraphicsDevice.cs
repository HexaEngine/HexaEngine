﻿namespace HexaEngine.Vulkan
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using Silk.NET.Core.Native;
    using Silk.NET.SDL;
    using Silk.NET.Vulkan;
    using Silk.NET.Vulkan.Extensions.KHR;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Format = Core.Graphics.Format;
    using Viewport = Mathematics.Viewport;

    public unsafe class VulkanGraphicsDevice : IGraphicsDevice
    {
        public readonly Vk Vk;
        public readonly Sdl Sdl;

        private static readonly string[] DeviceExtensions = new string[]
        {
            "VK_KHR_swapchain"
        };

        private static readonly string[] ValidationLayers = new string[]
        {
            "VK_LAYER_KHRONOS_validation"
        };

        private readonly IWindow window;
        public KhrSwapchain KhrSwapchain;
        public KhrSurface KhrSurface;

        public Instance Instance;
        public PhysicalDevice PhysicalDevice;
        public Device Device;
        public SurfaceKHR Surface;
        public Queue Queue;
        public VulkanSwapChain swapChain;

        private bool validationLayersEnabled;

        public VulkanGraphicsDevice(IWindow window)
        {
            Sdl = Sdl.GetApi();
            Vk = Vk.GetApi();
            KhrSwapchain = new(Vk.Context);
            KhrSurface = new(Vk.Context);
            InitializeInstance();
            InitializePhysicalDevice();
            InitializeLogicalDevice();
            this.window = window;
        }

        public GraphicsBackend Backend => GraphicsBackend.Vulkan;

        public IGraphicsContext Context { get; }

        public ITextureLoader TextureLoader { get; }

        public string? DebugName { get; set; }

        public bool IsDisposed { get; }

        public nint NativePointer { get; }

        public event EventHandler? OnDisposed;

        #region Helpers

        private byte** GetRequiredInstanceExtensions(out uint count)
        {
            Sdl sdl = Sdl.GetApi();
            uint rcount = 0;
            sdl.VulkanGetInstanceExtensions(window.GetWindow(), &rcount, (byte**)null);

            byte** extensions = (byte**)AllocArray(rcount);
            sdl.VulkanGetInstanceExtensions(window.GetWindow(), &rcount, extensions);

            Trace.WriteLine("#### Required Extensions ####");
            for (int i = 0; i < rcount; i++)
            {
                Trace.WriteLine(Marshal.PtrToStringUTF8(new IntPtr(extensions[i])));
            }
            count = rcount;
            return extensions;
        }

        private byte** GetRequiredDeviceExtensions(out uint count)
        {
            byte** ptrs = (byte**)AllocArray((uint)DeviceExtensions.Length);
            for (int i = 0; i < DeviceExtensions.Length; i++)
            {
                ptrs[i] = DeviceExtensions[i].ToUTF8();
            }

            count = (uint)DeviceExtensions.Length;
            return ptrs;
        }

        private byte** GetValidationLayers(out uint count)
        {
            byte** ptrs = (byte**)AllocArray((uint)ValidationLayers.Length);
            for (int i = 0; i < ValidationLayers.Length; i++)
            {
                ptrs[i] = ValidationLayers[i].ToUTF8();
            }

            count = (uint)ValidationLayers.Length;
            return ptrs;
        }

        private bool CheckValidationLayerSupport()
        {
            uint count;
            Vk.EnumerateInstanceLayerProperties(&count, null);

            LayerProperties[] layers = new LayerProperties[count];
            Vk.EnumerateInstanceLayerProperties(&count, layers);

            fixed (LayerProperties* p = layers)
            {
                for (int i = 0; i < ValidationLayers.Length; i++)
                {
                    bool found = false;

                    for (int j = 0; j < count; j++)
                    {
                        string? str = Marshal.PtrToStringUTF8(new IntPtr(p[j].LayerName));
                        if (str?.Equals(ValidationLayers[i]) ?? false)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        return false;
                }
            }

            return true;
        }

        private int RateDeviceSuitability(PhysicalDevice device)
        {
            PhysicalDeviceProperties properties;
            Vk.GetPhysicalDeviceProperties(device, &properties);
            PhysicalDeviceFeatures features;
            Vk.GetPhysicalDeviceFeatures(device, &features);

            int score = 0;

            if (properties.DeviceType == PhysicalDeviceType.DiscreteGpu)
            {
                score += 1000;
            }

            score += (int)properties.Limits.MaxImageDimension2D;

            if (!features.GeometryShader)
            {
                return 0;
            }

            return score;
        }

        private PhysicalDevice PickPhysicalDevice(PhysicalDevice[] devices)
        {
            Dictionary<int, PhysicalDevice> candidates = new();

            for (int i = 0; i < devices.Length; i++)
            {
                candidates.Add(RateDeviceSuitability(devices[i]), devices[i]);
            }

            int scoreMax = candidates.Max(x => x.Key);
            return candidates[scoreMax];
        }

        public struct QueueFamilyIndices
        {
            public uint? GraphicsFamily;
            public uint? PresentFamily;

            public bool IsComplete => GraphicsFamily is not null && PresentFamily is not null;
        }

        internal QueueFamilyIndices FindQueueFamilies(PhysicalDevice device, SurfaceKHR surface)
        {
            QueueFamilyIndices indices = default;

            uint queueFamilyCount = 0;
            Vk.GetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, null);

            QueueFamilyProperties[] properties = new QueueFamilyProperties[queueFamilyCount];
            Vk.GetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, properties);

            for (uint i = 0; i < queueFamilyCount; i++)
            {
                var queueFamily = properties[i];
                if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                {
                    indices.GraphicsFamily = i;
                }

                bool presentSupport = false;
                KhrSurface.GetPhysicalDeviceSurfaceSupport(device, i, surface, (Silk.NET.Core.Bool32*)&presentSupport);

                if (presentSupport)
                {
                    indices.PresentFamily = i;
                }

                if (indices.IsComplete)
                {
                    break;
                }
            }

            return indices;
        }

        internal SurfaceFormatKHR ChooseSwapSurfaceFormat(SurfaceFormatKHR[] availableFormats)
        {
            foreach (var availableFormat in availableFormats)
            {
                if (availableFormat.Format == Silk.NET.Vulkan.Format.B8G8R8A8Srgb && availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
                {
                    return availableFormat;
                }
            }

            return availableFormats[0];
        }

        internal static PresentModeKHR ChooseSwapPresentMode(PresentModeKHR[] availablePresentModes)
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

        #endregion Helpers

        #region Instance

        private void InitializeInstance()
        {
            Result result;

            ApplicationInfo info = new()
            {
                SType = StructureType.ApplicationInfo,
                ApiVersion = Vk.Version10,
                ApplicationVersion = 0,
                EngineVersion = 0,
                PEngineName = null,
                PApplicationName = null,
                PNext = null
            };

            var extension_names = GetRequiredInstanceExtensions(out uint enabled_extension_count);

            if (validationLayersEnabled && !CheckValidationLayerSupport())
            {
                throw new NotSupportedException();
            }

            InstanceCreateInfo createInfo = new()
            {
                SType = StructureType.InstanceCreateInfo,
                PNext = null,
                PApplicationInfo = &info,
                EnabledExtensionCount = enabled_extension_count,
                PpEnabledExtensionNames = extension_names,
                PpEnabledLayerNames = null,
                EnabledLayerCount = 0,
            };

            if (validationLayersEnabled)
            {
                createInfo.PpEnabledLayerNames = GetValidationLayers(out uint enabled_layer_count);
                createInfo.EnabledLayerCount = enabled_layer_count;
            }

            Instance instance;
            result = Vk.CreateInstance(&createInfo, null, &instance);
            if (result != Result.Success)
                throw new VulkanException(result);
            this.Instance = instance;

            uint extensionCount = 0;
            Vk.EnumerateInstanceExtensionProperties((byte*)null, &extensionCount, (ExtensionProperties*)null);

            ExtensionProperties[] extensionProperties = new ExtensionProperties[extensionCount];
            Vk.EnumerateInstanceExtensionProperties((byte*)null, &extensionCount, (ExtensionProperties*)Unsafe.AsPointer(ref extensionProperties[0]));

            Trace.WriteLine("#### Instance Extensions ####");
            for (int i = 0; i < extensionCount; i++)
            {
                fixed (ExtensionProperties* extensionProperties_ptr = extensionProperties)
                {
                    Trace.WriteLine(Marshal.PtrToStringUTF8(new IntPtr(extensionProperties_ptr[i].ExtensionName)));
                }
            }
        }

        private void ShutdownInstance()
        {
            Vk.DestroyInstance(Instance, null);
        }

        #endregion Instance

        #region Physical Device

        private void InitializePhysicalDevice()
        {
            Result result;

            uint count;
            result = Vk.EnumeratePhysicalDevices(Instance, &count, null);
            if (result != Result.Success)
                throw new VulkanException(result);

            PhysicalDevice[] physicalDevices = new PhysicalDevice[count];
            result = Vk.EnumeratePhysicalDevices(Instance, &count, (PhysicalDevice*)Unsafe.AsPointer(ref physicalDevices[0]));
            if (result != Result.Success)
                throw new VulkanException(result);

            PhysicalDevice = PickPhysicalDevice(physicalDevices);

            if (PhysicalDevice.Handle is 0)
                throw new Exception();

            uint devExtensionCount = 0;
            Vk.EnumerateDeviceExtensionProperties(PhysicalDevice, (byte*)null, &devExtensionCount, (ExtensionProperties*)null);

            ExtensionProperties[] devExtensionProperties = new ExtensionProperties[devExtensionCount];
            Vk.EnumerateDeviceExtensionProperties(PhysicalDevice, (byte*)null, &devExtensionCount, devExtensionProperties);
        }

        #endregion Physical Device

        #region Logical Device

        private void InitializeLogicalDevice()
        {
            Result result;

            uint queue_count;
            Vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queue_count, null);

            QueueFamilyProperties[] queue_props = new QueueFamilyProperties[queue_count];
            Vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queue_count, queue_props);

            PhysicalDeviceFeatures features;
            Vk.GetPhysicalDeviceFeatures(PhysicalDevice, &features);

            uint graphicsQueueNodeIndex = uint.MaxValue;
            for (uint i = 0; i < queue_count; i++)
            {
                if ((queue_props[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
                {
                    if (graphicsQueueNodeIndex == uint.MaxValue)
                        graphicsQueueNodeIndex = i;
                }
            }
            uint graphics_queue_node_index = graphicsQueueNodeIndex;

            float priority = 0;
            DeviceQueueCreateInfo queueInfo = new()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                PNext = null,
                QueueFamilyIndex = graphics_queue_node_index,
                QueueCount = 1,
                PQueuePriorities = &priority,
            };

            var enabledExtensionNames = GetRequiredDeviceExtensions(out uint enabledExtensionCount);

            DeviceCreateInfo deviceInfo = new()
            {
                SType = StructureType.DeviceCreateInfo,
                PNext = null,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueInfo,
                EnabledLayerCount = 0,
                PpEnabledLayerNames = null,
                EnabledExtensionCount = enabledExtensionCount,
                PpEnabledExtensionNames = enabledExtensionNames,
                PEnabledFeatures = &features
            };

            Device device = default;
            result = Vk.CreateDevice(PhysicalDevice, &deviceInfo, null, &device);
            if (result != Result.Success)
                throw new VulkanException(result);
            Device = device;

            Queue queue;
            Vk.GetDeviceQueue(Device, graphics_queue_node_index, 0, &queue);
            Queue = queue;
        }

        private void ShutdownLogicalDevice()
        {
            Vk.DestroyDevice(Device, null);
        }

        private void CreateSurface()
        {
            SurfaceKHR surface;
            VkHandle handle = new(Instance.Handle);
            Sdl.VulkanCreateSurface(window.GetWindow(), handle, (VkNonDispatchableHandle*)&surface);
            Surface = surface;
        }

        #endregion Logical Device

        public IBuffer CreateBuffer(BufferDescription description)
        {
            BufferCreateInfo info = default;
            info.SharingMode = SharingMode.Concurrent;
            info.Size = (ulong)description.ByteWidth;

            if ((description.BindFlags & BindFlags.VertexBuffer) != 0)
                info.Usage |= BufferUsageFlags.VertexBufferBit;
            if ((description.BindFlags & BindFlags.IndexBuffer) != 0)
                info.Usage |= BufferUsageFlags.IndexBufferBit;
            if ((description.BindFlags & BindFlags.ConstantBuffer) != 0)
                info.Usage |= BufferUsageFlags.UniformBufferBit;
            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
                info.Usage |= BufferUsageFlags.ShaderBindingTableBitKhr;
            if ((description.MiscFlags & ResourceMiscFlag.DrawIndirectArguments) == 0)
                info.Usage |= BufferUsageFlags.IndirectBufferBit;
            Silk.NET.Vulkan.Buffer buffer;
            Vk.CreateBuffer(Device, &info, null, &buffer);

            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : struct
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer<T>(T value, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : struct
        {
            throw new NotImplementedException();
        }

        public IDepthStencilView CreateDepthStencilView(IResource resource, DepthStencilViewDescription description)
        {
            throw new NotImplementedException();
        }

        public IDepthStencilView CreateDepthStencilView(IResource resource)
        {
            throw new NotImplementedException();
        }

        public IQuery CreateQuery()
        {
            throw new NotImplementedException();
        }

        public IRenderTargetView CreateRenderTargetView(IResource resource, Viewport viewport)
        {
            throw new NotImplementedException();
        }

        public IRenderTargetView CreateRenderTargetView(IResource resource, RenderTargetViewDescription description, Viewport viewport)
        {
            throw new NotImplementedException();
        }

        public ISamplerState CreateSamplerState(SamplerDescription sampler)
        {
            throw new NotImplementedException();
        }

        public IShaderResourceView CreateShaderResourceView(IResource resource)
        {
            throw new NotImplementedException();
        }

        public IShaderResourceView CreateShaderResourceView(IResource texture, ShaderResourceViewDescription description)
        {
            throw new NotImplementedException();
        }

        public ITexture1D CreateTexture1D(Texture1DDescription description)
        {
            throw new NotImplementedException();
        }

        public ITexture1D CreateTexture1D(Texture1DDescription description, SubresourceData[]? subresources)
        {
            throw new NotImplementedException();
        }

        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            throw new NotImplementedException();
        }

        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag none)
        {
            throw new NotImplementedException();
        }

        public ITexture2D CreateTexture2D(Texture2DDescription description)
        {
            throw new NotImplementedException();
        }

        public ITexture2D CreateTexture2D(Texture2DDescription description, SubresourceData[]? subresources)
        {
            throw new NotImplementedException();
        }

        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, int sampleCount = 1, int sampleQuality = 0, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            throw new NotImplementedException();
        }

        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag none)
        {
            throw new NotImplementedException();
        }

        public ITexture3D CreateTexture3D(Texture3DDescription description)
        {
            throw new NotImplementedException();
        }

        public ITexture3D CreateTexture3D(Texture3DDescription description, SubresourceData[]? subresources)
        {
            throw new NotImplementedException();
        }

        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            throw new NotImplementedException();
        }

        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag none)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ITexture1D LoadTexture1D(string path)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTexture2D(string path)
        {
            throw new NotImplementedException();
        }

        public ITexture3D LoadTexture3D(string path)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTextureCube(string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTexture1D(ITexture1D texture, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTexture1D(ITexture1D texture, Format format, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTexture2D(ITexture2D texture, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTexture2D(ITexture2D texture, Format format, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTexture3D(ITexture3D texture, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTexture3D(ITexture3D texture, Format format, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTextureCube(ITexture2D texture, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTextureCube(ITexture2D texture, Format format, string path)
        {
            throw new NotImplementedException();
        }

        public IQuery CreateQuery(Query type)
        {
            throw new NotImplementedException();
        }

        public IGraphicsContext CreateDeferredContext()
        {
            throw new NotImplementedException();
        }

        public ITexture1D LoadTexture1D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTexture2D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            throw new NotImplementedException();
        }

        public ITexture3D LoadTexture3D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            throw new NotImplementedException();
        }

        public IUnorderedAccessView CreateUnorderedAccessView(IResource resource, UnorderedAccessViewDescription description)
        {
            throw new NotImplementedException();
        }

        public ISwapChain CreateSwapChain(SdlWindow window)
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer<T>(T* values, uint count, BufferDescription description) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer<T>(T* values, uint count, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public IShaderResourceView CreateShaderResourceView(IBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public IShaderResourceView CreateShaderResourceView(IBuffer buffer, ShaderResourceViewDescription description)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc)
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer(void* src, uint length, BufferDescription description)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTexture2D(string path, BindFlags flags)
        {
            throw new NotImplementedException();
        }
    }
}