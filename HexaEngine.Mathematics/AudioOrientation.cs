namespace HexaEngine.Mathematics
{
    using System.Numerics;

    /// <summary>
    /// Represents a 3D orientation for 3D Audio.
    /// </summary>
    public struct AudioOrientation
    {
        /// <summary>
        /// The look at vector normalized also called forward vector.
        /// </summary>
        public Vector3 At;

        /// <summary>
        /// The up vector normalized.
        /// </summary>
        public Vector3 Up;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioOrientation"/> struct.
        /// </summary>
        /// <param name="at">The look at vector normalized also called forward vector.</param>
        /// <param name="up">The up vector normalized.</param>
        public AudioOrientation(Vector3 at, Vector3 up)
        {
            At = at;
            Up = up;
        }
    }
}