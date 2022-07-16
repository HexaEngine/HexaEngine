using HexaEngine.Core;

namespace HexaEngine.Objects
{
    using HexaEngine.Cameras;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using System;

    public interface ISceneRenderer : IDisposable
    {
        void Initialize(IGraphicsDevice device, SdlWindow window);

        void Render(IGraphicsContext context, SdlWindow window, Viewport viewport, Scene scene, Camera camera);
        void DrawSettings();
    }
}