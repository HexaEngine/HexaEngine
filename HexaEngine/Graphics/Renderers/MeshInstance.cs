namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class MeshInstance : IRendererInstance
    {
        public uint QueueIndex { get; }

        public Matrix4x4 Transform { get; }

        public BoundingBox BoundingBox { get; }

        public event QueueIndexChangedEventHandler1? QueueIndexChanged;
    }
}