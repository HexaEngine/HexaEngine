namespace HexaEngine.Scenes
{
    using HexaEngine.Mathematics;
    using System.Collections.Generic;

    public interface IReadOnlyPrefab
    {
        Guid Guid { get; }

        string Name { get; }

        string FullName { get; }

        IReadOnlyTransform Transform { get; }

        IReadOnlyList<IComponent> Components { get; }

        IReadOnlyList<GameObject> Children { get; }
    }
}