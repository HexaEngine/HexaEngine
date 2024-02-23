namespace HexaEngine.Graphics
{
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using System.Numerics;

    public interface ILODRendererComponent : IComponent
    {
        BoundingBox BoundingBox { get; }

        public int LODLevel { get; }

        public int MaxLODLevel { get; }

        public int MinLODLevel { get; }

        float Distance(Vector3 position);

        void SetLODLevel(int level);
    }
}