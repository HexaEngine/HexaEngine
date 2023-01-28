namespace HexaEngine.Core.Scenes.Systems
{
    using System.Collections.Generic;
    using System.Numerics;

    public interface ICompoundCollider : ICollider
    {
        Vector3 Center { get; }
        IReadOnlyList<ICollider>? Children { get; }
    }
}