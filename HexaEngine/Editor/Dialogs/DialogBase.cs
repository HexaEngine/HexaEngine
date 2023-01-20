namespace HexaEngine.Editor.Dialogs
{
    using ImGuiNET;
    using System.Runtime.CompilerServices;

    public abstract class DialogBase
    {
        private bool windowEnded;
        private bool shown;

        public abstract string Name { get; }

        protected abstract ImGuiWindowFlags Flags { get; }

        public bool Shown => shown;

        public void Draw()
        {
            if (!shown) return;
            windowEnded = false;
            if (ImGui.Begin(Name, Flags))
            {
                DrawContent();
            }

            EndDraw();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EndDraw()
        {
            ImGui.End();
            windowEnded = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void DrawContent();

        public virtual void Hide()
        {
            shown = false;
        }

        public abstract void Reset();

        public virtual void Show()
        {
            shown = true;
        }
    }
}