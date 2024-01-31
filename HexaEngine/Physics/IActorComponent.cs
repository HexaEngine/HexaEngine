namespace HexaEngine.Physics
{
    using HexaEngine.Scenes;
    using MagicPhysX;

    public unsafe interface IActorComponent : IComponent
    {
        ActorType Type { get; set; }

        public event Action<IActorComponent>? OnRecreate;

        void BeginUpdate();

        void CreateActor(PxPhysics* physics, PxScene* scene);

        void DestroyActor();

        void EndUpdate();
    }
}