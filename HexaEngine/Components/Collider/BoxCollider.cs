﻿namespace HexaEngine.Components.Collider
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core;
    using HexaEngine.Editor.Attributes;

    [EditorCategory("Collider")]
    [EditorComponent(typeof(BoxCollider), "Box Collider")]
    public class BoxCollider : BaseCollider
    {
        private float height = 1;
        private float depth = 1;
        private float width = 1;

        [EditorProperty("Width")]
        public float Width
        { get => width; set { width = value; update = true; } }

        [EditorProperty("Height")]
        public float Height
        { get => height; set { height = value; update = true; } }

        [EditorProperty("Depth")]
        public float Depth
        { get => depth; set { depth = value; update = true; } }

        public override void CreateShape()
        {
            if (Application.InDesignMode || GameObject == null || simulation == null || hasShape)
            {
                return;
            }

            Box box = new(width * 2, height * 2, depth * 2);
            pose = new(GameObject.Transform.GlobalPosition, GameObject.Transform.GlobalOrientation);
            lock (simulation)
            {
                index = simulation.Shapes.Add(box);
            }
            inertia = box.ComputeInertia(Mass);
            hasShape = true;
        }
    }
}