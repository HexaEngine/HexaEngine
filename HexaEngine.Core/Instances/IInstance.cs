namespace HexaEngine.Core.Instances
{
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using System;

    public interface IInstance : IDisposable
    {
        IInstanceType Type { get; }

        Transform Transform { get; }

        GameObject Parent { get; }

        event Action<ModelInstance>? Updated;

        void GetBoundingBox(out BoundingBox box);

        void GetBoundingSphere(out BoundingSphere box);

        string ToString();

        bool VisibilityTest(BoundingFrustum frustum);
    }
}