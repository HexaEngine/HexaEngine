namespace HexaEngine.Core.Scenes
{
    using System;

    public class SceneChangedEventArgs : EventArgs
    {
        public SceneChangedEventArgs(Scene? old, Scene @new)
        {
            Old = old;
            New = @new;
        }

        public Scene? Old { get; }
        public Scene New { get; }
    }
}