﻿namespace HexaEngine.Components.Collider
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Editor.Attributes;

    [EditorComponent<SphereCollider>("Sphere Collider")]
    public class SphereCollider : BaseCollider
    {
        private float radius = 1;

        [EditorProperty("FilterRadius")]
        public float Radius
        { get => radius; set { radius = value; update = true; } }

        public override void CreateShape()
        {
            if (Application.InDesignMode || parent == null || simulation == null || bufferPool == null || hasShape)
            {
                return;
            }

            Sphere sphere = new(radius);
            pose = new(parent.Transform.GlobalPosition, parent.Transform.GlobalOrientation);
            inertia = sphere.ComputeInertia(Mass);
            index = simulation.Shapes.Add(sphere);
            hasShape = true;
        }
    }
}