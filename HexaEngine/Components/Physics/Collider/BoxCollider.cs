namespace HexaEngine.Components.Physics.Collider
{
    using HexaEngine.Editor.Attributes;
    using MagicPhysX;
    using System.Numerics;

    [EditorCategory("Collider", "Physics")]
    [EditorComponent<BoxCollider>("Box Collider")]
    public unsafe class BoxCollider : ColliderShape
    {
        private float height = 1;
        private float depth = 1;
        private float width = 1;

        [EditorProperty("Width")]
        public float Width
        { get => width; set { width = value; } }

        [EditorProperty("Height")]
        public float Height
        { get => height; set { height = value; } }

        [EditorProperty("Depth")]
        public float Depth
        { get => depth; set { depth = value; } }

        public override unsafe void AddShapes(PxPhysics* physics, PxScene* scene, PxRigidActor* actor, PxTransform localPose, Vector3 scale)
        {
            var box = NativeMethods.PxBoxGeometry_new(width, height, depth);
            var shape = physics->CreateShapeMut((PxGeometry*)&box, material, true, PxShapeFlags.Visualization | PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);
            AttachShape(actor, shape);
        }
    }
}