namespace HexaEngine.Mathematics
{
    using System;

    /// <summary>
    /// Enum representing various flags for transformation locking and status.
    /// </summary>
    [Flags]
    public enum TransformFlags : ushort
    {
        /// <summary>
        /// No flags set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Flag indicating the X-axis position is locked.
        /// </summary>
        LockPositionX = 1,

        /// <summary>
        /// Flag indicating the Y-axis position is locked.
        /// </summary>
        LockPositionY = 2,

        /// <summary>
        /// Flag indicating the Z-axis position is locked.
        /// </summary>
        LockPositionZ = 4,

        /// <summary>
        /// Flag indicating all position axes are locked.
        /// </summary>
        LockPosition = LockPositionX | LockPositionY | LockPositionZ,

        /// <summary>
        /// Flag indicating the X-axis rotation is locked.
        /// </summary>
        LockRotationX = 8,

        /// <summary>
        /// Flag indicating the Y-axis rotation is locked.
        /// </summary>
        LockRotationY = 16,

        /// <summary>
        /// Flag indicating the Z-axis rotation is locked.
        /// </summary>
        LockRotationZ = 32,

        /// <summary>
        /// Flag indicating all rotation axes are locked.
        /// </summary>
        LockRotation = LockRotationX | LockRotationY | LockRotationZ,

        /// <summary>
        /// Flag indicating the X-axis scale is locked.
        /// </summary>
        LockScaleX = 64,

        /// <summary>
        /// Flag indicating the Y-axis scale is locked.
        /// </summary>
        LockScaleY = 128,

        /// <summary>
        /// Flag indicating the Z-axis scale is locked.
        /// </summary>
        LockScaleZ = 256,

        /// <summary>
        /// Flag indicating all scale axes are locked.
        /// </summary>
        LockScale = LockScaleX | LockScaleY | LockScaleZ,

        /// <summary>
        /// Flag indicating the transformation is dirty and needs recalculation.
        /// </summary>
        IsDirty = 512,
    }
}