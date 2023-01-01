namespace HexaEngine.Resources
{
    using HexaEngine.Mathematics;

    public class ModelInstance
    {
        public readonly string Name;
        public readonly int Id;
        public readonly ModelInstanceType Type;
        public readonly Mesh Mesh;
        public readonly Transform Transform;

        public void GetBoundingBox(out BoundingBox box)
        {
            box = BoundingBox.Transform(Mesh.BoundingBox, Transform);
        }

        public void GetBoundingSphere(out BoundingSphere box)
        {
            box = BoundingSphere.Transform(Mesh.BoundingSphere, Transform);
        }

        public bool VisibilityTest(BoundingFrustum frustum)
        {
            return frustum.Intersects(BoundingBox.Transform(Mesh.BoundingBox, Transform));
        }

        public ModelInstance(int id, ModelInstanceType type, Transform node)
        {
            Name = $"{type}:{id}";
            Id = id;
            Type = type;
            Mesh = type.Mesh;
            Transform = node;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}