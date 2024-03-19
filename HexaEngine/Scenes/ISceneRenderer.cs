namespace HexaEngine.Scenes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public enum SceneDrawFlags
    {
        None,
        NoPostProcessing,
        NoOverlay,
    }

    public interface ISceneRenderer : IDisposable
    {
        SceneDrawFlags DrawFlags { get; set; }

        ICPUFlameProfiler Profiler { get; }

        Vector2 Size { get; set; }

        Task Initialize(IGraphicsDevice device, ISwapChain swapChain, ICoreWindow window);

        void Render(IGraphicsContext context, Viewport viewport, Scene scene, Camera camera);

        void RenderTo(IGraphicsContext context, IRenderTargetView target, Viewport viewport, Scene scene, Camera camera);

        void TakeScreenshot(IGraphicsContext context, string path);

        void DrawSettings();
    }
}