namespace VkTesting
{
    using System;
    using VkTesting.Events;
    using VkTesting.Graphics;

    public class VkApp : IDisposable
    {
        private VulkanGraphicsDevice device;

        public VkApp()
        {
            Application.MainWindow.Resized += MainWindow_Resized;
            device = new(Application.MainWindow, true);
        }

        private void MainWindow_Resized(object? sender, ResizedEventArgs e)
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