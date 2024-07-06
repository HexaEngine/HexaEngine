namespace HexaEngine.Vulkan
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging.Device;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Core.Windows;
    using Silk.NET.Core.Native;
    using Silk.NET.SDL;
    using Silk.NET.Vulkan;
    using Silk.NET.Vulkan.Extensions.KHR;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public unsafe class VulkanAdapter : IGraphicsAdapter, IDisposable
    {
        public static readonly Vk Vk = Vk.GetApi();
        public static readonly Sdl Sdl = Application.Sdl;
        public static readonly KhrSwapchain KhrSwapchain = new(Vk.Context);
        public static readonly KhrSurface KhrSurface = new(Vk.Context);
        public Instance Instance;
        public DebugUtilsMessengerEXT DebugMessenger;
        public PhysicalDevice PhysicalDevice;
        private bool disposedValue;

        public GraphicsBackend Backend { get; } = GraphicsBackend.Vulkan;

        public int PlatformScore { get; } = 10;

        public IReadOnlyList<GPU> GPUs { get; } = [];

        public int AdapterIndex { get; }

        public VulkanAdapter(IWindow window, bool debug)
        {
            InitializeInstance(ref debug);

            if (debug)
            {
                DebugUtilsMessengerCreateInfoEXT createInfo = default;
                createInfo.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
                createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt; //VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;
                createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt; //VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;
                createInfo.PfnUserCallback = new(DebugCallback);
                createInfo.PUserData = null; // Optional

                DebugUtilsMessengerEXT debugMessenger;
                var result = CreateDebugUtilsMessengerEXT(Instance, &createInfo, null, &debugMessenger);
                if (result == Result.Success)
                {
                    DebugMessenger = debugMessenger;
                }
            }

            InitializePhysicalDevice(window);
        }

        public static void Init(IWindow window, bool graphicsDebugging)
        {
            GraphicsAdapter.Adapters.Add(new VulkanAdapter(window, graphicsDebugging));
        }

        private static readonly string[] DeviceExtensions =
        [
            "VK_KHR_swapchain"
        ];

        private static readonly string[] ValidationLayers =
        [
            "VK_LAYER_KHRONOS_validation"
        ];

        #region Helper

        private byte** GetRequiredInstanceExtensions(out uint count)
        {
            uint rcount = 0;
            Sdl.VulkanGetInstanceExtensions(null, &rcount, (byte**)null);

            byte** extensions = (byte**)AllocArray(rcount);
            Sdl.VulkanGetInstanceExtensions(null, &rcount, extensions);

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
                ptrs[i] = DeviceExtensions[i].ToUTF8Ptr();
            }

            count = (uint)DeviceExtensions.Length;
            return ptrs;
        }

        private byte** GetValidationLayers(out uint count)
        {
            byte** ptrs = (byte**)AllocArray((uint)ValidationLayers.Length);
            for (int i = 0; i < ValidationLayers.Length; i++)
            {
                ptrs[i] = ValidationLayers[i].ToUTF8Ptr();
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
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool CheckDeviceExtensionSupport(PhysicalDevice device)
        {
            uint extensionCount;
            Vk.EnumerateDeviceExtensionProperties(device, (byte*)null, &extensionCount, null);

            UnsafeList<ExtensionProperties> availableExtensions = new(extensionCount);
            Vk.EnumerateDeviceExtensionProperties(device, (byte*)null, &extensionCount, availableExtensions.Data);

            SortedSet<string> requiredExtensions = new(DeviceExtensions);

            for (int i = 0; i < availableExtensions.Size; i++)
            {
                var extension = availableExtensions[i];
                requiredExtensions.Remove(ToStringFromUTF8(extension.ExtensionName));
            }

            return requiredExtensions.Count == 0;
        }

        public static bool IsDeviceSuitable(PhysicalDevice device, SurfaceKHR surface)
        {
            QueueFamilyIndices indices = FindQueueFamilies(device, surface);

            bool extensionsSupported = CheckDeviceExtensionSupport(device);

            bool swapChainAdequate = false;
            if (extensionsSupported)
            {
                swapChainAdequate = VulkanSwapChain.IsDeviceSuitable(device, surface);
            }

            return indices.IsComplete && extensionsSupported && swapChainAdequate;
        }

        public static int RateDeviceSuitability(PhysicalDevice device, SurfaceKHR surface)
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

            if (!features.GeometryShader || !IsDeviceSuitable(device, surface))
            {
                return 0;
            }

            return score;
        }

        public static PhysicalDevice PickPhysicalDevice(PhysicalDevice[] devices, SurfaceKHR surface)
        {
            Dictionary<int, PhysicalDevice> candidates = new();

            for (int i = 0; i < devices.Length; i++)
            {
                candidates.Add(RateDeviceSuitability(devices[i], surface), devices[i]);
            }

            int scoreMax = candidates.Max(x => x.Key);
            return candidates[scoreMax];
        }

        internal static QueueFamilyIndices FindQueueFamilies(PhysicalDevice device, SurfaceKHR surface)
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

        #endregion Helper

        private void InitializeInstance(ref bool debug)
        {
            Result result;

            byte* pEngineName = "HexaEngine".ToUTF8Ptr();

            ApplicationInfo info = new()
            {
                SType = StructureType.ApplicationInfo,
                ApiVersion = Vk.Version10,
                ApplicationVersion = 0,
                EngineVersion = 1,
                PEngineName = pEngineName,
                PApplicationName = null,
                PNext = null
            };

            var extension_names = GetRequiredInstanceExtensions(out uint enabled_extension_count);

            if (debug && !CheckValidationLayerSupport())
            {
                debug = false;
                //throw new NotSupportedException();
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

            if (debug)
            {
                createInfo.PpEnabledLayerNames = GetValidationLayers(out uint enabled_layer_count);
                createInfo.EnabledLayerCount = enabled_layer_count;
            }

            Instance instance;
            result = Vk.CreateInstance(&createInfo, null, &instance);
            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            Free(pEngineName);

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

        private Result CreateDebugUtilsMessengerEXT(Instance instance, DebugUtilsMessengerCreateInfoEXT* pCreateInfo, Silk.NET.Vulkan.AllocationCallbacks* pAllocator, DebugUtilsMessengerEXT* pDebugMessenger)
        {
            var func = (delegate*<Instance, DebugUtilsMessengerCreateInfoEXT*, Silk.NET.Vulkan.AllocationCallbacks*, DebugUtilsMessengerEXT*, Result>)(nint)Vk.GetInstanceProcAddr(instance, "vkCreateDebugUtilsMessengerEXT");
            if (func != null)
            {
                return func(instance, pCreateInfo, pAllocator, pDebugMessenger);
            }
            else
            {
                return Result.ErrorExtensionNotPresent;
            }
        }

        private void DestroyDebugUtilsMessengerEXT(Instance instance, DebugUtilsMessengerEXT debugMessenger, Silk.NET.Vulkan.AllocationCallbacks* pAllocator)
        {
            var func = (delegate*<Instance, DebugUtilsMessengerEXT, Silk.NET.Vulkan.AllocationCallbacks*, void>)(nint)Vk.GetInstanceProcAddr(instance, "vkDestroyDebugUtilsMessengerEXT");
            if (func != null)
            {
                func(instance, debugMessenger, pAllocator);
            }
        }

        private static uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageType, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
        {
            if (messageSeverity >= DebugUtilsMessageSeverityFlagsEXT.WarningBitExt)
            {
                string message = ToStringFromUTF8(pCallbackData->PMessage);
                Trace.WriteLine(message);
            }

            return 0;
        }

        private void InitializePhysicalDevice(IWindow window)
        {
            Result result;

            uint count;
            result = Vk.EnumeratePhysicalDevices(Instance, &count, null);
            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            PhysicalDevice[] physicalDevices = new PhysicalDevice[count];
            result = Vk.EnumeratePhysicalDevices(Instance, &count, (PhysicalDevice*)Unsafe.AsPointer(ref physicalDevices[0]));
            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            window.Show();
            var surface = CreateSurface(window.GetWindow());

            PhysicalDevice = PickPhysicalDevice(physicalDevices, surface);

            KhrSurface.DestroySurface(Instance, surface, null);

            if (PhysicalDevice.Handle is 0)
            {
                throw new Exception();
            }

            uint devExtensionCount = 0;
            Vk.EnumerateDeviceExtensionProperties(PhysicalDevice, (byte*)null, &devExtensionCount, (ExtensionProperties*)null);

            ExtensionProperties[] devExtensionProperties = new ExtensionProperties[devExtensionCount];
            Vk.EnumerateDeviceExtensionProperties(PhysicalDevice, (byte*)null, &devExtensionCount, devExtensionProperties);
        }

        public IGraphicsDevice CreateGraphicsDevice(bool debug)
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
                    {
                        graphicsQueueNodeIndex = i;
                    }
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
            {
                throw new VulkanException(result);
            }

            Queue queue;
            Vk.GetDeviceQueue(device, graphics_queue_node_index, 0, &queue);

            return new VulkanGraphicsDevice(this, device, queue);
        }

        public ulong GetMemoryAvailableForReservation()
        {
            throw new NotImplementedException();
        }

        public ulong GetMemoryBudget()
        {
            throw new NotImplementedException();
        }

        public ulong GetMemoryCurrentUsage()
        {
            throw new NotImplementedException();
        }

        public void PumpDebugMessages()
        {
            throw new NotImplementedException();
        }

        private SurfaceKHR CreateSurface(Window* window)
        {
            SurfaceKHR surface;
            VkHandle handle = new(Instance.Handle);
            Sdl.VulkanCreateSurface(window, handle, (VkNonDispatchableHandle*)&surface);
            return surface;
        }

        internal VulkanSwapChain CreateSwapChain(VulkanGraphicsDevice device, Window* window)
        {
            return new VulkanSwapChain(device, window, CreateSurface(window));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Vk.DestroyInstance(Instance, null);

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}