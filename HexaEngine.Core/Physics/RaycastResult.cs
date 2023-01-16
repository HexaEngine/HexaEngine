namespace HexaEngine.Core.Physics
{
    using BepuPhysics.Collidables;
    using System.Numerics;

    public struct RaycastResult
    {
        public bool Hit;
        public CollidableReference Collidable;
        public float T;
        public int ChildIndex;
        public Vector3 Normal;
    }
}