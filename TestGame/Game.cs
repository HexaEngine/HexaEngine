namespace TestGame
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor;
    using HexaEngine.Graphics;
    using HexaEngine.Scenes;
    using HexaEngine.Windows;

    public class Game : HexaEngine.Game
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
            ShaderCache.DisableCache = true;
            Settings = new();
            Settings.VSync = true;
            Settings.FPSLimit = false;
            Settings.Width = 1280;
            Settings.Height = 720;
            Settings.RenderWidth = 1920;
            Settings.RenderHeight = 1080;
        }

        /// <summary>
        /// Initializes the window.
        /// </summary>
        /// <param name="window">The window.</param>
        public override void InitializeWindow(GameWindow window)
        {
            Keyboard.OnKeyUp += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    window.Close();
                }
                if (e.KeyCode == Keys.F5)
                {
                    SceneManager.Current.Dispatcher.Invoke(() => Pipeline.ReloadShaders());
                }
                if (e.KeyCode == Keys.F10)
                {
                    window.LockCursor = !window.LockCursor;
                }
                if (e.KeyCode == Keys.F11)
                {
                    // TODO: Reimplement BorderlessFullscreen
                    //window.BorderlessFullscreen = !window.BorderlessFullscreen;
                }
                if (e.KeyCode == Keys.F1)
                {
                    Designer.IsShown = !Designer.IsShown;
                    if (Designer.IsShown)
                    {
                        window.LockCursor = false;
                    }
                    else
                    {
                        window.LockCursor = true;
                    }
                }
            };

            Designer.IsShown = true;
            MainScene scene = new();

            SceneManager.Load(scene);
        }

        /// <summary>
        /// Uninitializes this instance.
        /// </summary>
        public override void Uninitialize()
        {
        }
    }
}