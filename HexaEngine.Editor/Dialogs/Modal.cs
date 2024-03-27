namespace HexaEngine.Editor.Dialogs
{
    using Hexa.NET.ImGui;
    using System.Runtime.CompilerServices;

    public abstract class Modal : IPopup
    {
        private bool windowEnded;
        private bool signalShow;
        private bool signalClose;
        private bool shown;

        public abstract string Name { get; }

        protected abstract ImGuiWindowFlags Flags { get; }

        public bool Shown => shown;

        public virtual void Draw()
        {
            if (signalShow)
            {
                shown = true;
                ImGui.OpenPopup(Name);
                signalShow = false;
            }

            if (!ImGui.BeginPopupModal(Name, ref shown, Flags))
            {
                return;
            }

            if (signalClose)
            {
                ImGui.CloseCurrentPopup();
                signalClose = false;
                shown = false;
            }
            windowEnded = false;

            DrawContent();

            if (!windowEnded)
            {
                ImGui.EndPopup();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EndDraw()
        {
            ImGui.EndPopup();
            windowEnded = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void DrawContent();

        public virtual void Close()
        {
            signalClose = true;
            shown = false;
        }

        public abstract void Reset();

        public virtual void Show()
        {
            signalShow = shown = true;
        }
    }
}