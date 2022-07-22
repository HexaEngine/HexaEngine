﻿namespace HexaEngine.Editor
{
    using HexaEngine.Core.IO;
    using HexaEngine.Editor.Widgets;
    using ImGuiNET;

    public static class Designer
    {
        private static bool isShown;
        private static bool inDesignMode = true;
        private static uint dockid;
        private static readonly float[] data = new float[10];

        public static bool IsShown
        {
            get => isShown;
            set
            {
                isShown = value;
                MainMenuBar.IsShown = value;
            }
        }

        public static bool InDesignMode { get => inDesignMode; set => inDesignMode = value; }

        public static uint DockId { get => dockid; set => dockid = value; }

        static Designer()
        {
        }

        internal static void Draw()
        {
            if (!isShown || !inDesignMode) return;
            MainMenuBar.Draw();
            ImGui.Begin("Editor", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.DockNodeHost);
            dockid = ImGui.GetWindowDockID();
            ImGui.SetWindowPos(new(0, MainMenuBar.Height));
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize - new System.Numerics.Vector2(0, MainMenuBar.Height));
            ImGui.End();

            Inspector.Draw();
            FramebufferDebugger.Draw();
            SceneLayout.Draw();
            SceneElementProperties.Draw();
            SceneMaterials.Draw();
            AssetExplorer.Draw();
        }
    }
}