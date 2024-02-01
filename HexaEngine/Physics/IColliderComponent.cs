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
}