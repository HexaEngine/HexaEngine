using HexaEngine.Core;

namespace HexaEngine.Objects
{
    using HexaEngine.Cameras;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using HexaEngine.Windows;
    using System;

    public interface ISceneRenderer : IDisposable
    {
        Task Initialize(IGraphicsDevice device, Window window);

        void Render(IGraphicsContext context, SdlWindow window, Viewport viewport, Scene scene, Camera camera);

        void DrawSettings();

        Task Update(Scene scene);
    }
}