namespace HexaEngine.Objects.Components
{
    using BepuPhysics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;

    public class ColliderDynamicComponent : IComponent
    {
        private SceneNode node;
        private Scene scene;
        public BodyDescription BodyDescription { get; set; }

        public BodyHandle Handle { get; private set; }

        public BodyReference Body { get; private set; }

        public bool IsVisible { get; set; }

        public virtual void Initialize(IGraphicsDevice device, SceneNode node)
        {
            this.node = node;
            scene = node.GetScene();
            Handle = scene.Simulation.Bodies.Add(BodyDescription);
            Body = scene.Simulation.Bodies.GetBodyReference(Handle);
        }

        public virtual void Uninitialize()
        {
            scene.Simulation.Bodies.Remove(Handle);
        }

        public virtual void Update()
        {
            node.Transform.PositionRotation = (Body.Pose.Position, Body.Pose.Orientation);
        }
    }
}