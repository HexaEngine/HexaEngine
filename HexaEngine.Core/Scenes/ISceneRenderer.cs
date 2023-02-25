namespace HexaEngine.Core.Scenes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Core.Windows;
    using HexaEngine.Mathematics;
    using System;

    public interface ISceneRenderer : IDisposable
    {
        RendererProfiler Profiler { get; }

        object Update { get; }

        object Culling { get; }

        object Geometry { get; }

        object SSAO { get; }

        object Lights { get; }

        PostProcessManager PostProcess { get; }

        object Debug { get; }

        Task Initialize(IGraphicsDevice device, ISwapChain swapChain, IRenderWindow window);

        void Render(IGraphicsContext context, IRenderWindow window, Viewport viewport, Scene scene, Camera camera);

        void DrawSettings();
    }
}