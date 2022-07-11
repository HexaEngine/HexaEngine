namespace HexaEngine.Objects.Components
{
    using BepuPhysics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;

    public class ColliderStaticComponent : IComponent
    {
        private SceneNode node;
        private Scene scene;

        public StaticDescription StaticDescription { get; set; }

        public StaticHandle Handle { get; private set; }

        public StaticReference Static { get; private set; }

        public bool IsVisible { get; set; }

        public virtual void Initialize(IGraphicsDevice device, SceneNode node)
        {
            this.node = node;
            scene = node.GetScene();
            Handle = scene.Simulation.Statics.Add(StaticDescription);
            Static = scene.Simulation.Statics.GetStaticReference(Handle);
        }

        public virtual void Uninitialize()
        {
            scene.Simulation.Statics.Remove(Handle);
        }

        public virtual void Update()
        {
            Static = scene.Simulation.Statics.GetStaticReference(Handle);
            node.Transform.PositionRotation = (Static.Pose.Position, Static.Pose.Orientation);
        }
    }
}