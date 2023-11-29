﻿namespace HexaEngine.Components
{
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes.Managers;

    [EditorComponent<SphereSelectionComponent>("Sphere Selection", true, true)]
    public class SphereSelectionComponent : ISelectableRayTest
    {
        private GameObject gameObject;
        private float radius = 0.5f;

        [JsonIgnore]
        public GameObject GameObject { get => gameObject; set => gameObject = value; }

        [JsonIgnore]
        public float Radius { get => radius; set => radius = value; }

        public void Awake()
        {
        }

        public void Destroy()
        {
        }

        public bool SelectRayTest(Ray ray, ref float depth)
        {
            BoundingSphere sphere = new(gameObject.Transform.GlobalPosition, radius);
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