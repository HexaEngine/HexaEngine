namespace HexaEngine.Core.Physics
{
    using System.Collections.Generic;
    using System.Numerics;

    public interface ICompoundCollider : IColliderComponent
    {
        Vector3 Center { get; }
        IReadOnlyList<IColliderComponent>? Children { get; }
    }
}