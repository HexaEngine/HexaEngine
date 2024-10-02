﻿namespace HexaEngine.Editor.Dialogs
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using System.Runtime.CompilerServices;

    public abstract class Modal : IPopup
    {
        private bool windowEnded;
        private bool signalShow;
        protected bool signalClose;
        protected bool shown;

        public abstract string Name { get; }

        protected abstract ImGuiWindowFlags Flags { get; }

        public bool Shown { get => shown; protected set => shown = value; }

        public virtual void Draw()
        {
            if (!shown)
            {
                return;
            }

            if (signalShow)
            {
                shown = true;
                ImGui.OpenPopup(Name, ImGuiPopupFlags.None);
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
                ImGui.EndPopup();
                return;
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
        }

        public abstract void Reset();

        public virtual void Show()
        {
            signalShow = shown = true;
        }
    }
}