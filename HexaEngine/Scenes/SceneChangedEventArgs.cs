namespace HexaEngine.Scenes
{
    using System;

    public class SceneChangedEventArgs : EventArgs
    {
        public SceneChangedEventArgs(Scene? oldScene, Scene? scene)
        {
            OldScene = oldScene;
            Scene = scene;
        }

        public Scene? OldScene { get; }

        public Scene? Scene { get; }
    }
}