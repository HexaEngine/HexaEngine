namespace HexaEngine.Editor
{
    using HexaEngine.Core.Input;
    using HexaEngine.Editor.Widgets;
    using ImGuiNET;
    using Silk.NET.SDL;

    public static class Designer
    {
        private static bool inDesignMode = true;
        private static uint dockid;

        public static bool InDesignMode
        {
            get => inDesignMode;
            set
            {
                inDesignMode = value;
                MainMenuBar.IsShown = value;
            }
        }

        public static History History { get; } = new();

        public static uint DockId { get => dockid; set => dockid = value; }

        static Designer()
        {
            Keyboard.OnKeyUp += (s, e) =>
            {
                if (Keyboard.IsDown(KeyCode.KLctrl))
                {
                    if (e.KeyCode == KeyCode.KZ)
                    {
                        History.TryUndo();
                    }
                    if (e.KeyCode == KeyCode.KY)
                    {
                        History.TryRedo();
                    }
                }
            };
        }

        internal static void Draw()
        {
            if (!inDesignMode) return;
            MainMenuBar.Draw();
            ImGui.Begin("Editor", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.DockNodeHost);
            dockid = ImGui.GetWindowDockID();
            ImGui.SetWindowPos(new(0, MainMenuBar.Height));
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize - new System.Numerics.Vector2(0, MainMenuBar.Height));
            ImGui.End();

            Inspector.Draw();
        }
    }
}