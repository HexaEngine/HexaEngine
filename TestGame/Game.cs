namespace TestGame
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor;
    using HexaEngine.Graphics;
    using HexaEngine.Scenes;
    using HexaEngine.Windows;
    using Silk.NET.SDL;

    public class Game : HexaEngine.Game
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
            //ShaderCache.DisableCache = true;
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
            ImGuiConsole.RegisterCommand("recompile_shaders", _ =>
            {
                SceneManager.Current.Dispatcher.Invoke(() => { ShaderCache.Clear(); Pipeline.ReloadShaders(); });
            });
            Keyboard.OnKeyUp += (s, e) =>
            {
                if (e.KeyCode == KeyCode.KEscape)
                {
                    window.Close();
                }
                if (e.KeyCode == KeyCode.KF5)
                {
                    SceneManager.Current.Dispatcher.Invoke(() => Pipeline.ReloadShaders());
                }
                if (e.KeyCode == KeyCode.KF10)
                {
                    window.LockCursor = !window.LockCursor;
                }
                if (e.KeyCode == KeyCode.KF11)
                {
                    // TODO: Reimplement BorderlessFullscreen
                    //window.BorderlessFullscreen = !window.BorderlessFullscreen;
                }
                if (e.KeyCode == KeyCode.KF1)
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