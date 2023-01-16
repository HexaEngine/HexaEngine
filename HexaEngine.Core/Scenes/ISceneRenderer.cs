namespace HexaEngine.Objects
{
    using HexaEngine.Cameras;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using System;

    public interface ISceneRenderer : IDisposable
    {
        Task Initialize(IGraphicsDevice device, ISwapChain swapChain, IRenderWindow window);

        void Render(IGraphicsContext context, IRenderWindow window, Viewport viewport, Scene scene, Camera camera);

        void DrawSettings();
    }
}