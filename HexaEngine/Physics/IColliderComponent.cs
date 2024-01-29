namespace HexaEngine.Physics
{
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using MagicPhysX;

    public unsafe interface IColliderComponent : IComponent
    {
        void BeginUpdate();

        void EndUpdate();

        unsafe void AddShapes(PxPhysics* physics, PxScene* scene, PxRigidActor* actor, Transform parent);

        void DestroyShapes();
    }

    public unsafe interface IActorComponent : IComponent
    {
        ActorType Type { get; set; }

        public event Action<IActorComponent>? OnRecreate;

        void BeginUpdate();

        void CreateActor(PxPhysics* physics, PxScene* scene);

        void DestroyActor();

        void EndUpdate();
    }

    public unsafe interface IRigidBodyComponent : IActorComponent
    {
    }

    [Flags]
    public enum ActorFlags
    {
        Visualization = 1,
        DisableGravity = 2,
        SendSleepNotifies = 4,
        DisableSimulation = 8
    }
}