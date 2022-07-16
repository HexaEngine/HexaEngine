namespace HexaEngine
{
    using HexaEngine.Windows;

    public abstract class Game
    {
        public abstract void Initialize();

        public abstract void InitializeWindow(GameWindow window);

        public abstract void Uninitialize();

        public GameSettings? Settings { get; set; }
    }
}