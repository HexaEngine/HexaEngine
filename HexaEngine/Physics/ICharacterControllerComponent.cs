namespace HexaEngine.Physics
{
    using HexaEngine.Scenes;
    using MagicPhysX;

    public unsafe interface ICharacterControllerComponent : IComponent
    {
        void CreateController(PxPhysics* physics, PxScene* scene, PxControllerManager* manager);

        void DestroyController();

        void Update();
    }
}