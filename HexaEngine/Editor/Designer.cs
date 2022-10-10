namespace HexaEngine.Editor
{
    using HexaEngine.Core.Input;
    using Silk.NET.SDL;

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
            if (!isShown) return;
            MainMenuBar.Draw();
            Inspector.Draw();
        }
    }
}