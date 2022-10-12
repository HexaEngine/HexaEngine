namespace HexaEngine.Rendering
{
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class ModelInstance
    {
        public ModelMesh Mesh;
        public SceneNode Node;

        public Matrix4x4 Transform => Node.Transform.Global;

        public void GetBoundingBox(out BoundingBox box)
        {
            box = BoundingBox.Transform(Mesh.Box, Node.Transform.Global);
        }

        public ModelInstance(ModelMesh mesh, SceneNode node)
        {
            Mesh = mesh;
            Node = node;
        }
    }
}