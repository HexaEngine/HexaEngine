namespace HexaEngine.Scenes
{
    public enum SceneFlags
    {
        None = 0,
        Initialized = 1,
        Valid = 2,
        Loaded = 4,
        Simulating = 8,
        UnsavedChanges = 16,
        PrefabScene = 32,
    }
}