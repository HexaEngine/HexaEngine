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
        Awake = 1 << 0,

        /// <summary>
        /// System will be called on graphics awake.
        /// </summary>
        GraphicsAwake = 1 << 1,

        /// <summary>
        /// System will be called on destroy.
        /// </summary>
        Destroy = 1 << 2,

        /// <summary>
        /// System will be called every frame before the actual update.
        /// </summary>
        EarlyUpdate = 1 << 3,

        /// <summary>
        /// System will be called every frame.
        /// </summary>
        Update = 1 << 4,

        /// <summary>
        /// System will be called after update.
        /// </summary>
        LateUpdate = 1 << 5,

        /// <summary>
        /// System will be called every fixed update.
        /// </summary>
        FixedUpdate = 1 << 6,

        /// <summary>
        /// System will be called every physics update.
        /// </summary>
        PhysicsUpdate = 1 << 7,

        /// <summary>
        /// System will be called by the render thread.
        /// </summary>
        GraphicsUpdate = 1 << 8,

        /// <summary>
        /// System <see cref="ISceneSystem.Update(float)"/> will be called every the camera moved.
        /// </summary>
        CameraUpdate = 1 << 9,

        /// <summary>
        /// System will be called on load.
        /// </summary>
        Load = 1 << 10,

        /// <summary>
        /// System will be called on unload.
        /// </summary>
        Unload = 1 << 11,
    }
}