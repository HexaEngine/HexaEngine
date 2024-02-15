namespace HexaEngine.Mathematics
{
    using System.Numerics;

    /// <summary>
    /// Represents the method that handles the event raised when a transform is updated.
    /// </summary>
    /// <param name="transform">The transform that was updated.</param>
    public delegate void TransformUpdatedEventHandler(Transform transform);

    /// <summary>
    /// Represents the method that handles the event raised when a transform is changed.
    /// </summary>
    /// <param name="transform">The transform that was changed.</param>
    public delegate void TransformChangedEventHandler(Transform transform);

    /// <summary>
    /// Represents the method that handles the event raised when transform flags are changed.
    /// </summary>
    /// <param name="transform">The transform for which flags were changed.</param>
    /// <param name="flags">The new flags value.</param>
    public delegate void TransformFlagsChangedEventHandler(Transform transform, TransformFlags flags);

    /// <summary>
    /// Represents a 3D transformation interface providing properties and methods
    /// for manipulating and querying transformation data.
    /// </summary>
    public interface ITransform
    {
        /// <summary>
        /// Gets the backward direction vector.
        /// </summary>
        Vector3 Backward { get; }

        /// <summary>
        /// Gets the downward direction vector.
        /// </summary>
        Vector3 Down { get; }

        /// <summary>
        /// Gets the forward direction vector.
        /// </summary>
        Vector3 Forward { get; }

        /// <summary>
        /// Gets the transformation matrix in global space.
        /// </summary>
        Matrix4x4 Global { get; }

        /// <summary>
        /// Gets the inverse of the transformation matrix in global space.
        /// </summary>
        Matrix4x4 GlobalInverse { get; }

        /// <summary>
        /// Gets or sets the global orientation of the transformation.
        /// </summary>
        Quaternion GlobalOrientation { get; set; }

        /// <summary>
        /// Gets or sets the global position of the transformation.
        /// </summary>
        Vector3 GlobalPosition { get; set; }

        /// <summary>
        /// Gets or sets the global scale of the transformation.
        /// </summary>
        Vector3 GlobalScale { get; set; }

        /// <summary>
        /// Gets the left direction vector.
        /// </summary>
        Vector3 Left { get; }

        /// <summary>
        /// Gets the transformation matrix in local space.
        /// </summary>
        Matrix4x4 Local { get; set; }

        /// <summary>
        /// Gets the inverse of the transformation matrix in local space.
        /// </summary>
        Matrix4x4 LocalInverse { get; }

        /// <summary>
        /// Gets or sets the orientation of the transformation in local space.
        /// </summary>
        Quaternion Orientation { get; set; }

        /// <summary>
        /// Gets or sets the parent transformation if part of a hierarchy.
        /// </summary>
        Transform? Parent { get; set; }

        /// <summary>
        /// Gets or sets the position of the transformation in local space.
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the combination of position, rotation, and scale.
        /// </summary>
        (Vector3, Quaternion) PositionRotation { get; set; }

        /// <summary>
        /// Gets or sets the combination of position, rotation, and scale.
        /// </summary>
        (Vector3, Quaternion, Vector3) PositionRotationScale { get; set; }

        /// <summary>
        /// Gets the right direction vector.
        /// </summary>
        Vector3 Right { get; }

        /// <summary>
        /// Gets or sets the rotation of the transformation in local space.
        /// </summary>
        Vector3 Rotation { get; set; }

        /// <summary>
        /// Gets or sets the scale of the transformation in local space.
        /// </summary>
        Vector3 Scale { get; set; }

        /// <summary>
        /// Gets the upward direction vector.
        /// </summary>
        Vector3 Up { get; }

        /// <summary>
        /// Gets the velocity vector of the transformation.
        /// </summary>
        Vector3 Velocity { get; }

        /// <summary>
        /// Gets the view transformation matrix.
        /// </summary>
        Matrix4x4 View { get; }

        /// <summary>
        /// Gets the inverse of the view transformation matrix.
        /// </summary>
        Matrix4x4 ViewInv { get; }

        /// <summary>
        /// Occurs when the transformation is updated.
        /// </summary>
        event TransformUpdatedEventHandler? Updated;

        /// <summary>
        /// Creates a clone of the current transformation instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ITransform"/> instance.</returns>
        object Clone();

        /// <summary>
        /// Creates a snapshot of the current state of the transformation.
        /// </summary>
        /// <returns>A <see cref="TransformSnapshot"/> representing the snapshot.</returns>
        TransformSnapshot CreateSnapshot();

        /// <summary>
        /// Recalculates the transformation state if necessary.
        /// </summary>
        /// <returns>True if the recalculation was successful; otherwise, false.</returns>
        bool Recalculate();

        /// <summary>
        /// Restores the state of the transformation to a previously saved state.
        /// </summary>
        void RestoreState();

        /// <summary>
        /// Saves the current state of the transformation for potential later restoration.
        /// </summary>
        void SaveState();
    }
}