namespace HexaEngine.Scenes
{
    using System;

    [Flags]
    public enum SystemFlags
    {
        None = 0,
        Awake = 1,
        GraphicsAwake = 2,
        Destroy = 4,
        Update = 8,
        FixedUpdate = 16,
        PhysicsUpdate = 32,
        LateUpdate = 64,
        GraphicsUpdate = 128,
    }
}