namespace HexaEngine.Editor.TextEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using System;

    [EditorWindowCategory("Tools")]
    public class TextEditorWindow : Hexa.NET.ImGui.Widgets.Extras.TextEditor.TextEditorWindow, IEditorWindow
    {
        private bool wasShown;

        public TextEditorWindow()
        {
        }

        public bool Initialized { get; }

        protected override string Name => "Text Editor";

        bool IEditorWindow.IsShown => IsShown;

        string IEditorWindow.Name => Name;

        public event Action<IEditorWindow>? Shown;

        public event Action<IEditorWindow>? Closed;

        public void Init(IGraphicsDevice device)
        {
        }

        public void DrawContent(IGraphicsContext context)
        {
            DrawContent();
        }

        public void DrawWindow(IGraphicsContext context)
        {
            if (!IsShown)
            {
                return;
            }

            if (!ImGui.Begin(Name, ref IsShown, Flags))
            {
                if (wasShown)
                {
                    OnClosed();
                }
                wasShown = false;
                ImGui.End();
                return;
            }

            if (!wasShown)
            {
                OnShown();
            }
            wasShown = true;

            windowEnded = false;

            DrawContent(context);

            if (!windowEnded)
            {
                ImGui.End();
            }
        }

        public void Focus()
        {
            ImGuiWindowPtr window = ImGuiP.FindWindowByName(Name);
            ImGuiP.FocusWindow(window, ImGuiFocusRequestFlags.UnlessBelowModal);
        }

        protected virtual void OnShown()
        {
            Shown?.Invoke(this);
        }

        protected virtual void OnClosed()
        {
            Closed?.Invoke(this);
        }
    }
}