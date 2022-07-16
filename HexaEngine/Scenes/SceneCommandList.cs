namespace HexaEngine.Scenes
{
    public enum CommandType
    {
        Load,
        Unload,
        Update
    }

    public struct SceneCommand
    {
        public CommandType Type;
        public object Sender;
    }
}