﻿namespace HexaEngine.Components
{
    using HexaEngine.Components.Audio;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;

    [EditorComponent<SphereSelectionComponent>("Sphere Selection", true, true)]
    public class SphereSelectionComponent : ISelectableRayTest
    {
        private GameObject gameObject;
        private float radius = 0.5f;

        /// <summary>
        /// The GUID of the <see cref="SphereSelectionComponent"/>.
        /// </summary>
        /// <remarks>DO NOT CHANGE UNLESS YOU KNOW WHAT YOU ARE DOING. (THIS CAN BREAK REFERENCES)</remarks>
        public Guid Guid { get; set; } = Guid.NewGuid();

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