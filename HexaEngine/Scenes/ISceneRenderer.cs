namespace HexaEngine.Scenes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using System;
    using System.Numerics;

    public interface ISceneRenderer : IDisposable
    {
        CPUProfiler Profiler { get; }

        PostProcessingManager PostProcessing { get; }

        Vector2 Size { get; }

        Task Initialize(IGraphicsDevice device, ISwapChain swapChain, IRenderWindow window);

        void Render(IGraphicsContext context, IRenderWindow window, Viewport viewport, Scene scene, Camera camera);

        void DrawSettings();
    }
}