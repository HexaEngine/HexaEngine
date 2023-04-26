namespace HexaEngine.Core.Instances
{
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;

    public class ModelInstance : IInstance
    {
        public readonly string Name;
        public readonly int Id;
        public readonly ModelInstanceType type;
        public readonly ResourceInstance<Mesh> Mesh;
        public readonly Transform transform;
        public readonly GameObject parent;
        private bool disposedValue;

        public event Action<ModelInstance>? Updated;

        public IInstanceType Type => type;

        public Transform Transform => transform;

        public GameObject Parent => parent;

        public void GetBoundingBox(out BoundingBox box)
        {
            if (Mesh.Value == null)
            {
                box = default;
                return;
            }
            box = BoundingBox.Transform(Mesh.Value.BoundingBox, transform);
        }

        public void GetBoundingSphere(out BoundingSphere sphere)
        {
            if (Mesh.Value == null)
            {
                sphere = default;
                return;
            }
            sphere = BoundingSphere.Transform(Mesh.Value.BoundingSphere, transform);
        }

        public bool VisibilityTest(BoundingFrustum frustum)
        {
            if (Mesh.Value == null)
            {
                return false;
            }

            return frustum.Intersects(BoundingBox.Transform(Mesh.Value.BoundingBox, transform));
        }

        public ModelInstance(int id, ModelInstanceType type, GameObject gameObject)
        {
            Name = $"{type}:{id}";
            Id = id;
            this.type = type;
            Mesh = type.Mesh;
            transform = gameObject.Transform;
            parent = gameObject;
            transform.Updated += TransformUpdated;
        }

        ~ModelInstance()
        {
            Dispose(disposing: false);
        }

        private void TransformUpdated(object? sender, EventArgs e)
        {
            Updated?.Invoke(this);
        }

        public override string ToString()
        {
            return Name;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                transform.Updated -= TransformUpdated;
                ResourceManager.UnloadMesh(Mesh);
                ResourceManager.UnloadMaterial(((ModelInstanceType)Type).Material);
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}