namespace HexaEngine.Physics
{
    using HexaEngine.Components.Physics;
    using System.Numerics;

    public struct RaycastHit(uint faceIndex, HitFlags flags, Vector3 position, Vector3 normal, float distance, float u, float v, RigidBody? actor, IColliderComponent? shape)
    {
        public uint FaceIndex = faceIndex;
        public HitFlags Flags = flags;
        public Vector3 Position = position;
        public Vector3 Normal = normal;
        public float Distance = distance;
        public float U = u;
        public float V = v;
        public RigidBody? Actor = actor;
        public IColliderComponent? Shape = shape;
    }
}