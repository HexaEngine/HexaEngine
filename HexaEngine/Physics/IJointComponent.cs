namespace HexaEngine.Physics
{
    using HexaEngine.Scenes;
    using MagicPhysX;

    public interface IJointComponent : IComponent
    {
        public event Action<IJointComponent>? OnRecreate;

        unsafe void CreateJoint(PxPhysics* physics, PxScene* scene);

        void DestroyJoint();
    }
}