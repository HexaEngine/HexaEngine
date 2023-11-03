namespace HexaEngine.Mathematics
{
    using System.Numerics;

    /// <summary>
    /// Represents a snapshot of a transformation within a 3D space.
    /// </summary>
    public struct TransformSnapshot
    {
        /// <summary>
        /// The parent transform of this snapshot, or null if there is no parent.
        /// </summary>
        public Transform? Parent;

        /// <summary>
        /// The local position of the transform.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The local rotation of the transform.
        /// </summary>
        public Vector3 Rotation;

        /// <summary>
        /// The local scale of the transform.
        /// </summary>
        public Vector3 Scale;

        /// <summary>
        /// The local orientation of the transform as a quaternion.
        /// </summary>
        public Quaternion Orientation;

        /// <summary>
        /// The global position of the transform.
        /// </summary>
        public Vector3 GlobalPosition;

        /// <summary>
        /// The global orientation of the transform as a quaternion.
        /// </summary>
        public Quaternion GlobalOrientation;

        /// <summary>
        /// The global scale of the transform.
        /// </summary>
        public Vector3 GlobalScale;

        /// <summary>
        /// The global forward direction.
        /// </summary>
        public Vector3 Forward;

        /// <summary>
        /// The global backward direction.
        /// </summary>
        public Vector3 Backward;

        /// <summary>
        /// The global left direction.
        /// </summary>
        public Vector3 Left;

        /// <summary>
        /// The global right direction.
        /// </summary>
        public Vector3 Right;

        /// <summary>
        /// The global up direction.
        /// </summary>
        public Vector3 Up;

        /// <summary>
        /// The global down direction.
        /// </summary>
        public Vector3 Down;

        /// <summary>
        /// The global transformation matrix.
        /// </summary>
        public Matrix4x4 Global;

        /// <summary>
        /// The inverse of the global transformation matrix.
        /// </summary>
        public Matrix4x4 GlobalInverse;

        /// <summary>
        /// The local transformation matrix.
        /// </summary>
        public Matrix4x4 Local;

        /// <summary>
        /// The inverse of the local transformation matrix.
        /// </summary>
        public Matrix4x4 LocalInverse;

        /// <summary>
        /// The view transformation matrix.
        /// </summary>
        public Matrix4x4 View;

        /// <summary>
        /// The inverse of the view transformation matrix.
        /// </summary>
        public Matrix4x4 ViewInv;

        /// <summary>
        /// The velocity of the transformation.
        /// </summary>
        public Vector3 Velocity;

        /// <summary>
        /// The previous position of the transformation.
        /// </summary>
        public Vector3 OldPos;
    }
}