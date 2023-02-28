namespace HexaEngine.Core.Instances
{
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;

    public class ModelInstance
    {
        public readonly string Name;
        public readonly int Id;
        public readonly ModelInstanceType Type;
        public readonly Mesh Mesh;
        public readonly Transform Transform;
        public readonly GameObject Parent;

        public event Action<ModelInstance>? Updated;

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

        public ModelInstance(int id, ModelInstanceType type, GameObject gameObject)
        {
            Name = $"{type}:{id}";
            Id = id;
            Type = type;
            Mesh = type.Mesh;
            Transform = gameObject.Transform;
            Parent = gameObject;
            Transform.Updated += TransformUpdated;
        }

        ~ModelInstance()
        {
            Transform.Updated -= TransformUpdated;
        }

        private void TransformUpdated(object? sender, EventArgs e)
        {
            Updated?.Invoke(this);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}