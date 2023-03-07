namespace HexaEngine.OpenAL
{
    using System.Numerics;

    public struct AudioOrientation
    {
        /// <summary>
        /// the look at vector normalized also called forward vector.
        /// </summary>
        public Vector3 At;

        /// <summary>
        /// The up vector normalized.
        /// </summary>
        public Vector3 Up;

        public AudioOrientation(Vector3 at, Vector3 up)
        {
            At = at;
            Up = up;
        }
    }
}