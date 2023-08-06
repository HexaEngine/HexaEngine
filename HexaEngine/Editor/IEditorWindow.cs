namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using System;

    public interface IEditorWindow
    {
        bool Initialized { get; }
        bool IsShown { get; }

        event Action<IEditorWindow>? Shown;

        event Action<IEditorWindow>? Closed;

        void Dispose();

        void DrawContent(IGraphicsContext context);

        void DrawMenu();

        void DrawWindow(IGraphicsContext context);

        void Init(IGraphicsDevice device);
    }
}