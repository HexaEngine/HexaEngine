namespace HexaEngine.Resources
{
    using HexaEngine.Mathematics;

    public class ModelInstance
    {
        public readonly int Id;
        public readonly ModelInstanceType Type;
        public readonly ModelMesh Mesh;
        public readonly Transform Transform;

        public void GetBoundingBox(out BoundingBox box)
        {
            box = BoundingBox.Transform(Mesh.AABB, Transform);
        }

        public bool VisibilityTest(BoundingFrustum frustum)
        {
            return frustum.Intersects(BoundingBox.Transform(Mesh.AABB, Transform));
        }

        public ModelInstance(int id, ModelInstanceType type, Transform node)
        {
            Id = id;
            Type = type;
            Mesh = type.Mesh;
            Transform = node;
        }

        public override string ToString()
        {
            return $"{Type}:{Id}";
        }
    }
}