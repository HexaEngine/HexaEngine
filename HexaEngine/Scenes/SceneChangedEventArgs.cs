namespace HexaEngine.Scenes
{
    using System;

    public class SceneChangedEventArgs : EventArgs
    {
        public SceneChangedEventArgs(SceneChangeType changeType, Scene? oldScene, Scene? scene)
        {
            ChangeType = changeType;
            OldScene = oldScene;
            Scene = scene;
        }

        public SceneChangeType ChangeType { get; }

        public Scene? OldScene { get; }

        public Scene? Scene { get; }
    }

    public enum SceneChangeType
    {
        Load,
        Unload,
        Reload,
    }

    public class SceneChangingEventArgs : EventArgs
    {
        public SceneChangingEventArgs(SceneChangeType changeType, Scene? oldScene, Scene? newScene)
        {
            ChangeType = changeType;
            OldScene = oldScene;
            NewScene = newScene;
        }

        public SceneChangeType ChangeType { get; }

        public Scene? OldScene { get; }

        public Scene? NewScene { get; }

        public bool Handled { get; set; }
    }
}