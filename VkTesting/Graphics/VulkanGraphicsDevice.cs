namespace HexaEngine.Vulkan
{
    using HexaEngine.Core;
    using HexaEngine.Core.Unsafes;
    using Silk.NET.Core.Native;
    using Silk.NET.SDL;
    using Silk.NET.Vulkan;
    using Silk.NET.Vulkan.Extensions.KHR;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VkTesting.Graphics;
    using VkTesting.Windows;
    using Semaphore = Silk.NET.Vulkan.Semaphore;
    using Viewport = Silk.NET.Vulkan.Viewport;

    public unsafe class VulkanGraphicsDevice
    {
        public static readonly Vk vk;
        public static readonly Sdl Sdl;

        private static readonly string[] DeviceExtensions = new string[]
        {
            "VK_KHR_swapchain"
        };

        private static readonly string[] ValidationLayers = new string[]
        {
            "VK_LAYER_KHRONOS_validation"
        };

        private readonly IWindow window;
        public static KhrSwapchain KhrSwapchain;
        public static KhrSurface KhrSurface;

        public Instance Instance;
        public DebugUtilsMessengerEXT DebugMessenger;

        public PhysicalDevice PhysicalDevice;
        public Device Device;

        public SurfaceKHR Surface;

        public Queue GraphicsQueue;
        public Queue PresentQueue;

        public VulkanCommandAllocator CommandAllocator;
        public VulkanCommandList[] CommandLists;
        public VulkanSwapChain SwapChain;

        private VulkanRenderPass RenderPass;
        private VulkanGraphicsPipeline graphicsPipeline;

        private const int MAX_FRAMES_IN_FLIGHT = 2;

        private readonly bool enableValidationLayers;

        static VulkanGraphicsDevice()
        {
            Sdl = Sdl.GetApi();
            vk = Vk.GetApi();
            KhrSwapchain = new(vk.Context);
            KhrSurface = new(vk.Context);
        }

        public VulkanGraphicsDevice(IWindow window, bool debug)
        {
            enableValidationLayers = debug;
            this.window = window;

            CreateInstance();
            SetupDebugMessenger();
            CreateSurface();
            PickPhysicalDevice();
            CreateLogicalDevice();
            CommandAllocator = new(this);
            CommandLists = new VulkanCommandList[MAX_FRAMES_IN_FLIGHT];
            for (int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
            {
                CommandLists[i] = new(this, CommandAllocator);
            }
            SwapChain = new(this, window);
            SwapChain.Resized += CreateFramebuffers;
            CreateRenderPass();
            CreateGraphicsPipeline();
            CreateFramebuffers();
        }

        public string? DebugName { get; set; }

        public bool IsDisposed { get; }

        public nint NativePointer { get; }

        public event EventHandler? OnDisposed;

        #region Helpers

        private UnsafeList<Pointer<byte>> GetRequiredInstanceExtensions()
        {
            Sdl sdl = Sdl.GetApi();
            uint count = 0;
            sdl.VulkanGetInstanceExtensions(window.GetWindow(), &count, (byte**)null);

            UnsafeList<Pointer<byte>> extensions = new(null, (int)count);
            sdl.VulkanGetInstanceExtensions(window.GetWindow(), &count, (byte**)extensions.Data);

            if (enableValidationLayers)
            {
                extensions.Add("VK_EXT_debug_utils".ToUTF8());
            }

            Trace.WriteLine("#### Required Extensions ####");
            for (int i = 0; i < extensions.Size; i++)
            {
                Trace.WriteLine(Marshal.PtrToStringUTF8(extensions[i]));
            }

            return extensions;
        }

        public byte** GetRequiredDeviceExtensions(out uint count)
        {
            byte** ptrs = (byte**)AllocArray((uint)DeviceExtensions.Length);
            for (int i = 0; i < DeviceExtensions.Length; i++)
            {
                ptrs[i] = DeviceExtensions[i].ToUTF8();
            }

            count = (uint)DeviceExtensions.Length;
            return ptrs;
        }

        public byte** GetValidationLayers(out uint count)
        {
            byte** ptrs = (byte**)AllocArray((uint)ValidationLayers.Length);
            for (int i = 0; i < ValidationLayers.Length; i++)
            {
                ptrs[i] = ValidationLayers[i].ToUTF8();
            }

            count = (uint)ValidationLayers.Length;
            return ptrs;
        }

        public bool CheckValidationLayerSupport()
        {
            uint count;
            vk.EnumerateInstanceLayerProperties(&count, null);

            LayerProperties[] layers = new LayerProperties[count];
            vk.EnumerateInstanceLayerProperties(&count, layers);

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

        public static bool CheckDeviceExtensionSupport(PhysicalDevice device)
        {
            uint extensionCount;
            vk.EnumerateDeviceExtensionProperties(device, (byte*)null, &extensionCount, null);

            UnsafeList<ExtensionProperties> availableExtensions = new((int)extensionCount);
            vk.EnumerateDeviceExtensionProperties(device, (byte*)null, &extensionCount, availableExtensions.Data);

            SortedSet<string> requiredExtensions = new(DeviceExtensions);

            for (int i = 0; i < availableExtensions.Size; i++)
            {
                var extension = availableExtensions[i];
                requiredExtensions.Remove(ToStringFromUTF8(extension.ExtensionName));
            }

            return requiredExtensions.Count == 0;
        }

        public static int RateDeviceSuitability(PhysicalDevice device, SurfaceKHR surface)
        {
            PhysicalDeviceProperties properties;
            vk.GetPhysicalDeviceProperties(device, &properties);
            PhysicalDeviceFeatures features;
            vk.GetPhysicalDeviceFeatures(device, &features);

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

        public static QueueFamilyIndices FindQueueFamilies(PhysicalDevice device, SurfaceKHR surface)
        {
            QueueFamilyIndices indices = default;

            uint queueFamilyCount = 0;
            vk.GetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, null);

            QueueFamilyProperties[] properties = new QueueFamilyProperties[queueFamilyCount];
            vk.GetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, properties);

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

        #endregion Helpers

        #region Instance

        private static uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageType, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
        {
            if (messageSeverity >= DebugUtilsMessageSeverityFlagsEXT.WarningBitExt)
            {
                string message = ToStringFromUTF8(pCallbackData->PMessage);
                Trace.WriteLine(message);
            }

            return 0;
        }

        private void CreateInstance()
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

            var extension_names = GetRequiredInstanceExtensions();

            if (enableValidationLayers && !CheckValidationLayerSupport())
            {
                throw new NotSupportedException("validation layers requested, but not available!");
            }

            InstanceCreateInfo createInfo = new()
            {
                SType = StructureType.InstanceCreateInfo,
                PNext = null,
                PApplicationInfo = &info,
                EnabledExtensionCount = (uint)extension_names.Size,
                PpEnabledExtensionNames = (byte**)extension_names.Data,
                PpEnabledLayerNames = null,
                EnabledLayerCount = 0,
            };

            if (enableValidationLayers)
            {
                createInfo.PpEnabledLayerNames = GetValidationLayers(out uint enabled_layer_count);
                createInfo.EnabledLayerCount = enabled_layer_count;
            }

            Instance instance;
            result = vk.CreateInstance(&createInfo, null, &instance);
            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            this.Instance = instance;

            uint extensionCount = 0;
            vk.EnumerateInstanceExtensionProperties((byte*)null, &extensionCount, (ExtensionProperties*)null);

            ExtensionProperties[] extensionProperties = new ExtensionProperties[extensionCount];
            vk.EnumerateInstanceExtensionProperties((byte*)null, &extensionCount, (ExtensionProperties*)Unsafe.AsPointer(ref extensionProperties[0]));

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
            vk.DestroyInstance(Instance, null);
        }

        #endregion Instance

        #region DebugMessenger

        private void SetupDebugMessenger()
        {
            if (!enableValidationLayers) return;
            DebugUtilsMessengerCreateInfoEXT createInfo = default;
            createInfo.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
            createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt; //VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT;
            createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt; //VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;
            createInfo.PfnUserCallback = new(DebugCallback);
            createInfo.PUserData = null; // Optional

            DebugUtilsMessengerEXT debugMessenger;
            var result = CreateDebugUtilsMessengerEXT(Instance, &createInfo, null, &debugMessenger);
            if (result != Result.Success)
            {
                throw new Exception("failed to set up debug messenger!");
            }
            DebugMessenger = debugMessenger;
        }

        private Result CreateDebugUtilsMessengerEXT(Instance instance, DebugUtilsMessengerCreateInfoEXT* pCreateInfo, AllocationCallbacks* pAllocator, DebugUtilsMessengerEXT* pDebugMessenger)
        {
            var func = (delegate*<Instance, DebugUtilsMessengerCreateInfoEXT*, AllocationCallbacks*, DebugUtilsMessengerEXT*, Result>)(nint)vk.GetInstanceProcAddr(instance, "vkCreateDebugUtilsMessengerEXT");
            if (func != null)
            {
                return func(instance, pCreateInfo, pAllocator, pDebugMessenger);
            }
            else
            {
                return Result.ErrorExtensionNotPresent;
            }
        }

        private void DestroyDebugUtilsMessengerEXT(Instance instance, DebugUtilsMessengerEXT debugMessenger, AllocationCallbacks* pAllocator)
        {
            var func = (delegate*<Instance, DebugUtilsMessengerEXT, AllocationCallbacks*, void>)(nint)vk.GetInstanceProcAddr(instance, "vkDestroyDebugUtilsMessengerEXT");
            if (func != null)
            {
                func(instance, debugMessenger, pAllocator);
            }
        }

        #endregion DebugMessenger

        #region Surface

        private void CreateSurface()
        {
            SurfaceKHR surface;
            VkHandle handle = new(Instance.Handle);
            Sdl.VulkanCreateSurface(window.GetWindow(), handle, (VkNonDispatchableHandle*)&surface);
            Surface = surface;
        }

        #endregion Surface

        #region Physical Device

        private void PickPhysicalDevice()
        {
            Result result;

            uint count;
            result = vk.EnumeratePhysicalDevices(Instance, &count, null);
            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            PhysicalDevice[] physicalDevices = new PhysicalDevice[count];
            result = vk.EnumeratePhysicalDevices(Instance, &count, (PhysicalDevice*)Unsafe.AsPointer(ref physicalDevices[0]));
            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            PhysicalDevice = PickPhysicalDevice(physicalDevices, Surface);

            if (PhysicalDevice.Handle is 0)
            {
                throw new Exception();
            }

            uint devExtensionCount = 0;
            vk.EnumerateDeviceExtensionProperties(PhysicalDevice, (byte*)null, &devExtensionCount, (ExtensionProperties*)null);

            ExtensionProperties[] devExtensionProperties = new ExtensionProperties[devExtensionCount];
            vk.EnumerateDeviceExtensionProperties(PhysicalDevice, (byte*)null, &devExtensionCount, devExtensionProperties);
        }

        #endregion Physical Device

        #region Logical Device

        private void CreateLogicalDevice()
        {
            Result result;

            uint queue_count;
            vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queue_count, null);

            QueueFamilyProperties[] queue_props = new QueueFamilyProperties[queue_count];
            vk.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, &queue_count, queue_props);

            PhysicalDeviceFeatures features;
            vk.GetPhysicalDeviceFeatures(PhysicalDevice, &features);

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
                PEnabledFeatures = &features,
            };

            QueueFamilyIndices indices = FindQueueFamilies(PhysicalDevice, Surface);
            UnsafeList<DeviceQueueCreateInfo> queueCreateInfos = new();
            SortedSet<uint> uniqueQueueFamilies = new() { indices.GraphicsFamily.Value, indices.PresentFamily.Value };
            float queuePriority = 1.0f;
            foreach (var queueFamily in uniqueQueueFamilies)
            {
                DeviceQueueCreateInfo queueCreateInfo = default;
                queueCreateInfo.SType = StructureType.DeviceQueueCreateInfo;
                queueCreateInfo.QueueFamilyIndex = queueFamily;
                queueCreateInfo.QueueCount = 1;
                queueCreateInfo.PQueuePriorities = &queuePriority;
                queueCreateInfos.Add(queueCreateInfo);
            }

            deviceInfo.QueueCreateInfoCount = (uint)queueCreateInfos.Size;
            deviceInfo.PQueueCreateInfos = queueCreateInfos.Data;

            Device device = default;
            result = vk.CreateDevice(PhysicalDevice, &deviceInfo, null, &device);
            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            Device = device;

            Queue graphicsQueue;
            vk.GetDeviceQueue(Device, graphics_queue_node_index, 0, &graphicsQueue);
            GraphicsQueue = graphicsQueue;

            Queue presentQueue;
            vk.GetDeviceQueue(Device, indices.PresentFamily.Value, 0, &presentQueue);
            PresentQueue = presentQueue;
        }

        private void ShutdownLogicalDevice()
        {
            vk.DestroyDevice(Device, null);
        }

        #endregion Logical Device

        #region RenderPass

        private void CreateRenderPass()
        {
            RenderPass = new(this, SwapChain.Format);
        }

        #endregion RenderPass

        #region GraphicsPipeline

        public void CreateGraphicsPipeline()
        {
            graphicsPipeline = new(this, RenderPass, new()
            {
                VertexShader = "assets/shaders/vs.hlsl",
                FragmentShader = "assets/shaders/ps.hlsl"
            });
        }

        private ShaderModule CreateShaderModule(ShaderBlob blob)
        {
            ShaderModuleCreateInfo createInfo = default;
            createInfo.SType = StructureType.ShaderModuleCreateInfo;
            createInfo.CodeSize = blob.Length;
            createInfo.PCode = (uint*)blob.Data;

            var result = vk.CreateShaderModule(Device, createInfo, null, out var shaderModule);

            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            return shaderModule;
        }

        #endregion GraphicsPipeline

        #region Framebuffers

        private void CreateFramebuffers()
        {
            SwapChain.CreateFramebuffers(RenderPass);
        }

        #endregion Framebuffers

        #region CommandBuffer

        private void RecordCommandBuffer(VulkanCommandList commandList, uint imageIndex)
        {
            commandList.Begin();

            RenderPass.BeginRenderPass(commandList, imageIndex, SwapChain.Extent);
            graphicsPipeline.Bind(commandList);

            Viewport viewport;
            viewport.X = 0.0f;
            viewport.Y = 0.0f;
            viewport.Width = SwapChain.Extent.Width;
            viewport.Height = SwapChain.Extent.Height;
            viewport.MinDepth = 0.0f;
            viewport.MaxDepth = 1.0f;
            commandList.SetViewport(0, 1, &viewport);

            Rect2D scissor;
            scissor.Offset = default;
            scissor.Extent = SwapChain.Extent;
            commandList.SetScissor(0, 1, &scissor);

            commandList.Draw(3, 1, 0, 0);

            RenderPass.EndRenderPass(commandList);

            commandList.End();
        }

        #endregion CommandBuffer

        #region DrawFrame

        public void DrawFrame()
        {
            SwapChain.BeginFrame();

            var currentFrame = SwapChain.CurrentFrame;
            var imageIndex = SwapChain.ImageIndex;
            var commandList = CommandLists[currentFrame];
            commandList.Reset();

            RecordCommandBuffer(commandList, imageIndex);

            SubmitInfo submitInfo = default;
            submitInfo.SType = StructureType.SubmitInfo;

            var commandBuffer = commandList.CommandBuffer;
            Semaphore* waitSemaphores = stackalloc Semaphore[] { SwapChain.ImageAvailableSemaphores[currentFrame] };
            PipelineStageFlags* waitStages = stackalloc PipelineStageFlags[] { PipelineStageFlags.ColorAttachmentOutputBit };
            submitInfo.WaitSemaphoreCount = 1;
            submitInfo.PWaitSemaphores = waitSemaphores;
            submitInfo.PWaitDstStageMask = waitStages;
            submitInfo.CommandBufferCount = 1;
            submitInfo.PCommandBuffers = &commandBuffer;

            Semaphore* signalSemaphores = stackalloc Semaphore[] { SwapChain.RenderFinishedSemaphores[currentFrame] };
            submitInfo.SignalSemaphoreCount = 1;
            submitInfo.PSignalSemaphores = signalSemaphores;

            var result = vk.QueueSubmit(GraphicsQueue, 1, &submitInfo, SwapChain.InFlightFences[currentFrame]);

            if (result != Result.Success)
            {
                throw new VulkanException(result);
            }

            SwapChain.Present();
        }

        #endregion DrawFrame

        #region Cleanup

        public void Cleanup()
        {
            SwapChain.Dispose();

            graphicsPipeline.Dispose();

            RenderPass.Dispose();

            CommandAllocator.Dispose();

            vk.DestroyDevice(Device, null);

            if (enableValidationLayers)
            {
                DestroyDebugUtilsMessengerEXT(Instance, DebugMessenger, null);
            }

            KhrSurface.DestroySurface(Instance, Surface, null);
            vk.DestroyInstance(Instance, null);
        }

        #endregion Cleanup
    }
}