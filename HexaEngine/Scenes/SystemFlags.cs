namespace HexaEngine.Scenes
{
    using System;

    [Flags]
    public enum SystemFlags
    {
        /// <summary>
        /// System will never be called.
        /// </summary>
        None = 0,

        /// <summary>
        /// System will be called on awake.
        /// </summary>
        Awake = 1,

        /// <summary>
        /// System will be called on graphics awake.
        /// </summary>
        GraphicsAwake = 2,

        /// <summary>
        /// System will be called on destroy.
        /// </summary>
        Destroy = 4,

        /// <summary>
        /// System will be called every frame.
        /// </summary>
        Update = 8,

        /// <summary>
        /// System will be called every fixed update.
        /// </summary>
        FixedUpdate = 16,

        /// <summary>
        /// System will be called every physics update.
        /// </summary>
        PhysicsUpdate = 32,

        /// <summary>
        /// System will be called after physics update.
        /// </summary>
        LateUpdate = 64,

        /// <summary>
        /// System will be called by the render thread.
        /// </summary>
        GraphicsUpdate = 128,

        /// <summary>
        /// System will be called on load.
        /// </summary>
        Load = 256,

        /// <summary>
        /// System will be called on unload.
        /// </summary>
        Unload = 512,
    }
}