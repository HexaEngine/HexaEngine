﻿namespace HexaEngine.Components.Collider
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Editor.Attributes;

    [EditorComponent<CapsuleCollider>("Capsule Collider")]
    public class CapsuleCollider : BaseCollider
    {
        private float radius = 1;
        private float length = 2;

        [EditorProperty("FilterRadius")]
        public float Radius
        { get => radius; set { radius = value; update = true; } }

        [EditorProperty("Length")]
        public float Length
        { get => length; set { length = value; update = true; } }

        public override void CreateShape()
        {
            if (Application.InDesignMode || GameObject == null || simulation == null || hasShape)
            {
                return;
            }

            Capsule capsule = new(radius, length);
            pose = new(GameObject.Transform.GlobalPosition, GameObject.Transform.GlobalOrientation);
            inertia = capsule.ComputeInertia(Mass);
            index = simulation.Shapes.Add(capsule);
            hasShape = true;
        }
    }
}