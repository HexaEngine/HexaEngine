namespace HexaEngine.Scenes
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
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

        Vector2 Size { get; set; }

        Task Initialize(IGraphicsDevice device, ISwapChain swapChain, ICoreWindow window);

        void Render(IGraphicsContext context, IScene scene, Camera camera);

        void RenderTo(IGraphicsContext context, IRenderTargetView target, Viewport viewport, IScene scene, Camera camera);

        void TakeScreenshot(IGraphicsContext context, string path);

        void DrawSettings();
    }
}