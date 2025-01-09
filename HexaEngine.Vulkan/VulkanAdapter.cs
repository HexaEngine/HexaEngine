namespace HexaEngine.Vulkan
{
    using Hexa.NET.SDL2;
    using Hexa.NET.Utilities;
    using Hexa.NET.Vulkan;
    using HexaEngine.Core.Debugging.Device;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VkInstance = Hexa.NET.Vulkan.VkInstance;
    using VkSurfaceKHR = Hexa.NET.Vulkan.VkSurfaceKHR;

    public unsafe class VulkanAdapter : IGraphicsAdapter, IDisposable
    {
        public VkInstance Instance;
        public VkDebugUtilsMessengerEXT DebugMessenger;
        public VkPhysicalDevice PhysicalDevice;
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
                VkDebugUtilsMessengerCreateInfoEXT createInfo = default;
                createInfo.SType = VkStructureType.DebugUtilsMessengerCreateInfoExt;
                createInfo.MessageSeverity = (uint)(VkDebugUtilsMessageSeverityFlagBitsEXT.VerboseBitExt | VkDebugUtilsMessageSeverityFlagBitsEXT.WarningBitExt | VkDebugUtilsMessageSeverityFlagBitsEXT.ErrorBitExt); //VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;
                createInfo.MessageType = (uint)(VkDebugUtilsMessageTypeFlagBitsEXT.GeneralBitExt | VkDebugUtilsMessageTypeFlagBitsEXT.ValidationBitExt | VkDebugUtilsMessageTypeFlagBitsEXT.PerformanceBitExt); //VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;
                createInfo.PfnUserCallback = (void*)Marshal.GetFunctionPointerForDelegate<PFNVkDebugUtilsMessengerCallbackEXT>(DebugCallback);
                createInfo.PUserData = null; // Optional

                VkDebugUtilsMessengerEXT debugMessenger;
                var result = Vulkan.VkCreateDebugUtilsMessengerEXT(Instance, &createInfo, null, &debugMessenger);
                if (result == VkResult.Success)
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
            SDL.VulkanGetInstanceExtensions(null, &rcount, (byte**)null);

            byte** extensions = (byte**)AllocArray(rcount);
            SDL.VulkanGetInstanceExtensions(null, &rcount, extensions);

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
            Vulkan.VkEnumerateInstanceLayerProperties(&count, null);

            VkLayerProperties[] layers = new VkLayerProperties[count];
            Vulkan.VkEnumerateInstanceLayerProperties(&count, ref layers[0]);

            fixed (VkLayerProperties* p = layers)
            {
                for (int i = 0; i < ValidationLayers.Length; i++)
                {
                    bool found = false;

                    for (int j = 0; j < count; j++)
                    {
                        string? str = Marshal.PtrToStringUTF8((nint)(&p[j].LayerName_0));
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

        public static bool CheckDeviceExtensionSupport(VkPhysicalDevice device)
        {
            uint extensionCount;
            Vulkan.VkEnumerateDeviceExtensionProperties(device, (byte*)null, &extensionCount, null);

            UnsafeList<VkExtensionProperties> availableExtensions = new((int)extensionCount);
            Vulkan.VkEnumerateDeviceExtensionProperties(device, (byte*)null, &extensionCount, availableExtensions.Data);

            SortedSet<string> requiredExtensions = new(DeviceExtensions);

            for (int i = 0; i < availableExtensions.Size; i++)
            {
                var extension = availableExtensions[i];
                requiredExtensions.Remove(ToStringFromUTF8(&extension.ExtensionName_0));
            }

            return requiredExtensions.Count == 0;
        }

        public static bool IsDeviceSuitable(VkPhysicalDevice device, VkSurfaceKHR surface)
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

        public static int RateDeviceSuitability(VkPhysicalDevice device, VkSurfaceKHR surface)
        {
            VkPhysicalDeviceProperties properties;
            Vulkan.VkGetPhysicalDeviceProperties(device, &properties);
            VkPhysicalDeviceFeatures features;
            Vulkan.VkGetPhysicalDeviceFeatures(device, &features);

            int score = 0;

            if (properties.DeviceType == VkPhysicalDeviceType.DiscreteGpu)
            {
                score += 1000;
            }

            score += (int)properties.Limits.MaxImageDimension2D;

            if (features.GeometryShader != 0 || !IsDeviceSuitable(device, surface))
            {
                return 0;
            }

            return score;
        }

        public static VkPhysicalDevice PickPhysicalDevice(VkPhysicalDevice[] devices, VkSurfaceKHR surface)
        {
            Dictionary<int, VkPhysicalDevice> candidates = new();

            for (int i = 0; i < devices.Length; i++)
            {
                candidates.Add(RateDeviceSuitability(devices[i], surface), devices[i]);
            }

            int scoreMax = candidates.Max(x => x.Key);
            return candidates[scoreMax];
        }

        internal static QueueFamilyIndices FindQueueFamilies(VkPhysicalDevice device, VkSurfaceKHR surface)
        {
            QueueFamilyIndices indices = default;

            uint queueFamilyCount = 0;
            Vulkan.VkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, null);

            VkQueueFamilyProperties[] properties = new VkQueueFamilyProperties[queueFamilyCount];
            Vulkan.VkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, ref properties[0]);

            for (uint i = 0; i < queueFamilyCount; i++)
            {
                var queueFamily = properties[i];
                if (((VkQueueFlagBits)queueFamily.QueueFlags & VkQueueFlagBits.GraphicsBit) != 0)
                {
                    indices.GraphicsFamily = i;
                }

                uint presentSupport;
                Vulkan.VkGetPhysicalDeviceSurfaceSupportKHR(device, i, surface, &presentSupport);

                if (presentSupport != 0)
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
            VkResult result;

            byte* pEngineName = "HexaEngine".ToUTF8Ptr();

            VkApplicationInfo info = new()
            {
                SType = VkStructureType.ApplicationInfo,
                ApiVersion = Vulkan.VkApiVersion13,
                ApplicationVersion = 0,                         // unknown.
                PApplicationName = null,                        // unknown.
                EngineVersion = 1 << 24 | 0 << 16 | 0 << 8 | 0, // HexaEngine version 1.0.0.0
                PEngineName = pEngineName,
                PNext = null
            };

            var extension_names = GetRequiredInstanceExtensions(out uint enabled_extension_count);

            if (debug && !CheckValidationLayerSupport())
            {
                debug = false;
                //throw new NotSupportedException();
            }

            VkInstanceCreateInfo createInfo = new()
            {
                SType = VkStructureType.InstanceCreateInfo,
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

            VkInstance instance;
            result = Vulkan.VkCreateInstance(&createInfo, null, &instance);
            if (result != VkResult.Success)
            {
                throw new VulkanException(result);
            }

            Free(pEngineName);

            this.Instance = instance;

            uint extensionCount = 0;
            Vulkan.VkEnumerateInstanceExtensionProperties((byte*)null, &extensionCount, (VkExtensionProperties*)null);

            VkExtensionProperties[] extensionProperties = new VkExtensionProperties[extensionCount];
            Vulkan.VkEnumerateInstanceExtensionProperties((byte*)null, &extensionCount, (VkExtensionProperties*)Unsafe.AsPointer(ref extensionProperties[0]));

            Trace.WriteLine("#### Instance Extensions ####");
            for (int i = 0; i < extensionCount; i++)
            {
                fixed (VkExtensionProperties* extensionProperties_ptr = extensionProperties)
                {
                    Trace.WriteLine(Marshal.PtrToStringUTF8(new IntPtr(&extensionProperties_ptr[i].ExtensionName_0)));
                }
            }
        }

        private static uint DebugCallback(VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity, uint messageTypes, VkDebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
        {
            if (messageSeverity >= VkDebugUtilsMessageSeverityFlagBitsEXT.WarningBitExt)
            {
                string message = ToStringFromUTF8(pCallbackData->PMessage);
                Trace.WriteLine(message);
            }

            return 0;
        }

        private void InitializePhysicalDevice(IWindow window)
        {
            VkResult result;

            uint count;
            result = Vulkan.VkEnumeratePhysicalDevices(Instance, &count, null);
            if (result != VkResult.Success)
            {
                throw new VulkanException(result);
            }

            VkPhysicalDevice[] physicalDevices = new VkPhysicalDevice[count];
            result = Vulkan.VkEnumeratePhysicalDevices(Instance, &count, (VkPhysicalDevice*)Unsafe.AsPointer(ref physicalDevices[0]));
            if (result != VkResult.Success)
            {
                throw new VulkanException(result);
            }

            window.Show();
            var surface = CreateSurface(window.GetWindow());

            PhysicalDevice = PickPhysicalDevice(physicalDevices, surface);

            Vulkan.VkDestroySurfaceKHR(Instance, surface, null);

            if (PhysicalDevice.Handle is 0)
            {
                throw new Exception();
            }

            uint devExtensionCount = 0;
            Vulkan.VkEnumerateDeviceExtensionProperties(PhysicalDevice, (byte*)null, &devExtensionCount, (VkExtensionProperties*)null);

            VkExtensionProperties[] devExtensionProperties = new VkExtensionProperties[devExtensionCount];
            Vulkan.VkEnumerateDeviceExtensionProperties(PhysicalDevice, (byte*)null, &devExtensionCount, ref devExtensionProperties[0]);
        }

        public IGraphicsDevice CreateGraphicsDevice(bool debug)
        {
            VkResult result;

            uint queue_count;
            Vulkan.VkGetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queue_count, null);

            VkQueueFamilyProperties[] queue_props = new VkQueueFamilyProperties[queue_count];
            Vulkan.VkGetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queue_count, ref queue_props[0]);

            VkPhysicalDeviceFeatures features;
            Vulkan.VkGetPhysicalDeviceFeatures(PhysicalDevice, &features);

            uint graphicsQueueNodeIndex = uint.MaxValue;
            for (uint i = 0; i < queue_count; i++)
            {
                if (((VkQueueFlagBits)queue_props[i].QueueFlags & VkQueueFlagBits.GraphicsBit) != 0)
                {
                    if (graphicsQueueNodeIndex == uint.MaxValue)
                    {
                        graphicsQueueNodeIndex = i;
                    }
                }
            }
            uint graphics_queue_node_index = graphicsQueueNodeIndex;

            float priority = 0;
            VkDeviceQueueCreateInfo queueInfo = new()
            {
                SType = VkStructureType.DeviceQueueCreateInfo,
                PNext = null,
                QueueFamilyIndex = graphics_queue_node_index,
                QueueCount = 1,
                PQueuePriorities = &priority,
            };

            var enabledExtensionNames = GetRequiredDeviceExtensions(out uint enabledExtensionCount);

            VkDeviceCreateInfo deviceInfo = new()
            {
                SType = VkStructureType.DeviceCreateInfo,
                PNext = null,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueInfo,
                EnabledLayerCount = 0,
                PpEnabledLayerNames = null,
                EnabledExtensionCount = enabledExtensionCount,
                PpEnabledExtensionNames = enabledExtensionNames,
                PEnabledFeatures = &features
            };

            VkDevice device = default;
            result = Vulkan.VkCreateDevice(PhysicalDevice, &deviceInfo, null, &device);
            if (result != VkResult.Success)
            {
                throw new VulkanException(result);
            }

            VkQueue queue;
            Vulkan.VkGetDeviceQueue(device, graphics_queue_node_index, 0, &queue);

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

        private VkSurfaceKHR CreateSurface(SDLWindow* window)
        {
            VkSurfaceKHR surface;
            SDL.VulkanCreateSurface(window, Instance.Handle, (Hexa.NET.SDL2.VkSurfaceKHR*)&surface);
            return surface;
        }

        internal VulkanSwapChain CreateSwapChain(VulkanGraphicsDevice device, SDLWindow* window)
        {
            return new VulkanSwapChain(device, window, CreateSurface(window), null, null);
        }

        internal VulkanSwapChain CreateSwapChain(VulkanGraphicsDevice device, SDLWindow* window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription)
        {
            return new VulkanSwapChain(device, window, CreateSurface(window), swapChainDescription, fullscreenDescription);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Instance.DestroyInstance(null);

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