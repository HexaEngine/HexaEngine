namespace HexaEngine.Components
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;

    [EditorComponent<SphereSelectionComponent>("Sphere Selection", true, true)]
    public class SphereSelectionComponent : Component, ISelectableHitTest
    {
        private float radius = 0.5f;

        [JsonIgnore]
        public float HitTestRadius { get => radius; set => radius = value; }

        public override void Awake()
        {
        }

        public override void Destroy()
        {
        }

        public bool SelectRayTest(Ray ray, ref float depth)
        {
            BoundingSphere sphere = new(GameObject.Transform.GlobalPosition, radius);
            var result = sphere.Intersects(ray);
            if (result == null)
            {
                return false;
            }
            depth = result.Value;
            return true;
        }
    }
}