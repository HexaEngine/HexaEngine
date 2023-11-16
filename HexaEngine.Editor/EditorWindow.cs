﻿namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using Hexa.NET.ImGui;

    public abstract class EditorWindow : IEditorWindow
    {
        protected bool IsDocked;
        protected ImGuiWindowFlags Flags;
        private bool windowEnded;
        private bool wasShown;
        private bool initialized;
        private bool isShown;

        protected abstract string Name { get; }

        string IEditorWindow.Name => Name;

        public bool IsShown { get => isShown; protected set => isShown = value; }

        public bool Initialized => initialized;

        public event Action<IEditorWindow>? Shown;

        public event Action<IEditorWindow>? Closed;

        public void Init(IGraphicsDevice device)
        {
            InitWindow(device);
            initialized = true;
        }

        protected virtual void InitWindow(IGraphicsDevice device)
        {
        }

        public virtual void DrawWindow(IGraphicsContext context)
        {
            if (!IsShown)
            {
                return;
            }

            if (!ImGui.Begin(Name, ref isShown, Flags))
            {
                if (wasShown)
                {
                    Closed?.Invoke(this);
                }
                wasShown = false;
                ImGui.End();
                return;
            }

            if (!wasShown)
            {
                Shown?.Invoke(this);
            }
            wasShown = true;

            windowEnded = false;

            DrawContent(context);

            if (!windowEnded)
            {
                ImGui.End();
            }
        }

        public abstract void DrawContent(IGraphicsContext context);

        protected void EndWindow()
        {
            if (!IsShown)
            {
                return;
            }

            ImGui.End();
            windowEnded = true;
        }

        public virtual void DrawMenu()
        {
            if (ImGui.MenuItem(Name))
            {
                IsShown = true;
            }
        }

        public virtual void Show()
        {
            IsShown = true;
        }

        public virtual void Close()
        {
            IsShown = false;
        }

        public void Dispose()
        {
            DisposeCore();
            initialized = false;
        }

        protected virtual void DisposeCore()
        {
        }

        public void Focus()
        {
            var window = ImGui.FindWindowByName(Name);
            ImGui.FocusWindow(window, ImGuiFocusRequestFlags.UnlessBelowModal);
        }
    }
}