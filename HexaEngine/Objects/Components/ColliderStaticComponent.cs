namespace HexaEngine.Objects.Components
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;

    [EditorComponent<ColliderStaticComponent>("Static Collider")]
    public class ColliderStaticComponent : IComponent
    {
        private bool canUpdate;
        private StaticHandle handle;
        private StaticReference staticReference;
#nullable disable
        private SceneNode node;
        private Scene scene;
        private StaticDescription? staticDescription;
        private Type shapeType;
#nullable enable

        public ColliderStaticComponent()
        {
            Editor = new PropertyEditor<ColliderStaticComponent>(this);
        }

        [EditorPropertyTypeSelector<IShape>("Type")]
        public Type? ShapeType
        {
            get => shapeType;
            set
            {
                shapeType = value;
            }
        }

        public StaticDescription? StaticDescription
        {
            get => staticDescription;
            set
            {
                staticDescription = value;
            }
        }

        public IPropertyEditor? Editor { get; }

        private void InitializeCollider()
        {/*
            RigidPose pose = new(node.Transform.Position, node.Transform.Orientation);
            StaticDescription = new(pose, )
            handle = scene.Simulation.Statics.Add(StaticDescription.Value);
            staticReference = scene.Simulation.Statics.GetStaticReference(handle);*/
        }

        public virtual void Initialize(IGraphicsDevice device, SceneNode node)
        {
            this.node = node;
            scene = node.GetScene();
            if (StaticDescription.HasValue)
            {
                handle = scene.Simulation.Statics.Add(StaticDescription.Value);
                staticReference = scene.Simulation.Statics.GetStaticReference(handle);
                canUpdate = true;
            }
        }

        public virtual void Uninitialize()
        {
            if (canUpdate)
                scene.Simulation.Statics.Remove(handle);
        }

        public virtual void Update()
        {
            if (canUpdate)
            {
                staticReference = scene.Simulation.Statics.GetStaticReference(handle);
                node.Transform.PositionRotation = (staticReference.Pose.Position, staticReference.Pose.Orientation);
            }
        }
    }
}