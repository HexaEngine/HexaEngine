namespace HexaEngine.Animations
{
    using System.Collections.Generic;
    using System.Numerics;

    public class KeyframeChannel
    {
        public string Name { get; set; }

        public List<VectorKey> PositionKeys { get; set; }
        public List<QuaternionKey> RotationKeys { get; set; }
        public List<VectorKey> ScalingKeys { get; set; }
    }

    public struct VectorKey
    {
        public Vector3 Value { get; set; }
        public float Time { get; set; }
    }

    public struct QuaternionKey
    {
        public Quaternion Value { get; set; }
        public float Time { get; set; }
    }
}