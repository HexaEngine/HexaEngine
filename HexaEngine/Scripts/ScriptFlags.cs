namespace HexaEngine.Scripts
{
    [Flags]
    public enum ScriptFlags
    {
        None = 0,
        Awake = 1,
        Destroy = 2,
        Update = 4,
        FixedUpdate = 8,
    }
}