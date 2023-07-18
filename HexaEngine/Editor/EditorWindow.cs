namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using ImGuiNET;

    public abstract class EditorWindow : IImGuiWindow
    {
        protected bool IsShown;
        protected bool IsDocked;
        private bool windowEnded;

        protected abstract string Name { get; }
        protected ImGuiWindowFlags Flags;

        public virtual void Init(IGraphicsDevice device)
        {
        }

        public virtual void DrawWindow(IGraphicsContext context)
        {
            if (!IsShown)
            {
                return;
            }

            if (!ImGui.Begin(Name, ref IsShown, Flags))
            {
                ImGui.End();
                return;
            }

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

        public virtual void Dispose()
        {
        }
    }
}