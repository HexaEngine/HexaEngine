namespace HexaEngine.Plugins
{
    using System.Numerics;

    public unsafe struct Transform
    {
        public Vector3 Position;
        public Quaternion Orientation;
        public Vector3 Scale;

        public (Vector3, Quaternion, Vector3) POS { get => (Position, Orientation, Scale); set => (Position, Orientation, Scale) = value; }
    }
}