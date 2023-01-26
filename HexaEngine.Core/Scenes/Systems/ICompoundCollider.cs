namespace HexaEngine.Core.Scenes.Systems
{
    using System.Collections.Generic;
    using System.Numerics;

    public interface ICompoundCollider : IBaseCollider
    {
        Vector3 Center { get; }
        IReadOnlyList<IBaseCollider>? Children { get; }
    }
}