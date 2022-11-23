namespace HexaEngine.Editor
{
    using HexaEngine.Core.Input;
    using ImGuiNET;
    using ImGuizmoNET;
    using ImNodesNET;

    public static class Designer
    {
        private static bool inDesignMode = true;
        private static bool isShown = true;

        public static bool InDesignMode
        {
            get => inDesignMode;
            set
            {
                inDesignMode = value;
            }
        }

        public static bool IsShown
        {
            get => isShown;
            set => isShown = value;
        }

        public static History History { get; } = new();

        static Designer()
        {
            Keyboard.Released += (s, e) =>
            {
                if (Keyboard.IsDown(KeyCode.LCtrl))
                {
                    if (e.KeyCode == KeyCode.Z)
                    {
                        History.TryUndo();
                    }
                    if (e.KeyCode == KeyCode.Y)
                    {
                        History.TryRedo();
                    }
                }
            };
        }

        internal static void Draw()
        {
            if (!isShown) return;
            MainMenuBar.Draw();
            Inspector.Draw();
        }
    }
}