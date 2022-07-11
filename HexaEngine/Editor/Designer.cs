namespace HexaEngine.Editor
{
    using ImGuiNET;

    public static class Designer
    {
        private static bool isShown;
        private static bool inDesignMode = true;
        private static uint dockid;

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

            ImGui.Begin("MainWindow", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoFocusOnAppearing);
            dockid = ImGui.GetWindowDockID();
            ImGui.SetWindowPos(new());
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);
            ImGui.End();
            MainMenuBar.Draw();
            Inspector.Draw();
            FramebufferDebugger.Draw();
            SceneLayout.Draw();
            SceneElementProperties.Draw();
            SceneMaterials.Draw();
        }
    }
}