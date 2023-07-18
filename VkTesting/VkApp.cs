namespace VkTesting
{
    using HexaEngine.Core;
    using HexaEngine.Vulkan;
    using System;

    public class VkApp : IDisposable
    {
        private VulkanGraphicsDevice device;

        public VkApp()
        {
            Application.MainWindow.Resized += MainWindow_Resized;
            device = new(Application.MainWindow, true);
        }

        private void MainWindow_Resized(object? sender, HexaEngine.Core.Windows.Events.ResizedEventArgs e)
        {
            device.SwapChain.Resize();
        }

        public void Render()
        {
            device.DrawFrame();
        }

        public void Dispose()
        {
            device.Cleanup();
            GC.SuppressFinalize(this);
        }
    }
}