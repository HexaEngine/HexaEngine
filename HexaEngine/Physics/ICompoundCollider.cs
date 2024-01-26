namespace HexaEngine.Physics
{
    using System.Collections.Generic;
    using System.Numerics;

    public interface ICompoundCollider : IBepuColliderComponent
    {
        Vector3 Center { get; }
        IReadOnlyList<IBepuColliderComponent>? Children { get; }
    }
}