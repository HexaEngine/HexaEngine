namespace HexaEngine.Objects.Components
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes;
    using HexaEngine.Shaders;

    public class NormalVisualizerComponent : IForwardRendererComponent
    {
        private SceneNode node;
        private VertexBuffer<VertexPositionColor> _vertexBuffer;
        private LineShader shader;

        public string Model { get; set; }
        public bool IsVisible { get; set; }

        public void Initialize(IGraphicsDevice device, SceneNode node)
        {
            this.node = node;
            var path1 = Paths.CurrentModelPath + Model;
            using var fs = FileSystem.Open(path1);
            var mesh = MeshFactory.Instance.Load(fs);
            _vertexBuffer = new();
            foreach (MeshVertex vertex in mesh.Vertices)
            {
                _vertexBuffer.Append(new VertexPositionColor(vertex.Position, new(0, 0, 1, 1)), new VertexPositionColor(vertex.Position + vertex.Normal * 0.1f, new(0, 0, 1, 1)));
                _vertexBuffer.Append(new VertexPositionColor(vertex.Position, new(1, 0, 0, 1)), new VertexPositionColor(vertex.Position + vertex.Tangent * 0.1f, new(1, 0, 0, 1)));
            }

            shader = new(device);
        }

        public void Render(IGraphicsContext context, Viewport viewport, IView view)
        {
            _vertexBuffer.Bind(context);
            shader.Draw(context, viewport, view, node.Transform, _vertexBuffer.Count, 0);
        }

        public void Uninitialize()
        {
            shader.Dispose();
            _vertexBuffer.Dispose();
        }
    }
}