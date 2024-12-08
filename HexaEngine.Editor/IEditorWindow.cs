namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using System;

    public interface IEditorWindow
    {
        bool Initialized { get; }
        bool IsShown { get; }
        string Name { get; }

        event Action<IEditorWindow>? Shown;

        event Action<IEditorWindow>? Closed;

        void Dispose();

        void DrawContent(IGraphicsContext context);

        void DrawMenu();

        [Profiling.Profile]
        void DrawWindow(IGraphicsContext context);

        void Init(IGraphicsDevice device);

        void Focus();
    }
}