namespace HexaEngine.Scenes
{
    public enum CommandType
    {
        Load,
        Unload,
        Update
    }

    public enum ChildCommandType
    {
        None,
        Added,
        Removed,
        Updated,
    }

    public struct SceneCommand
    {
        public CommandType Type;
        public object Sender;
        public ChildCommandType ChildCommand;
        public object? Child;

        public SceneCommand(CommandType type, object sender, ChildCommandType childCommand = ChildCommandType.None, object? child = null)
        {
            Type = type;
            Sender = sender;
            ChildCommand = childCommand;
            Child = child;
        }
    }
}