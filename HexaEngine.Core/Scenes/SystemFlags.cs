namespace HexaEngine.Core.Scenes
{
    using System;

    [Flags]
    public enum SystemFlags
    {
        None = 0,
        Awake = 1,
        Destory = 2,
        Update = 4,
        FixedUpdate = 8,
        PhysicsUpdate = 16,
        LateUpdate = 32,
    }
}