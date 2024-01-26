namespace HexaEngine.Physics
{
    using HexaEngine.Scenes;
    using MagicPhysX;

    public unsafe interface IPhysXColliderComponent : IComponent
    {
        bool HasActor { get; }

        bool HasShape { get; }

        bool InCompound { get; }

        PhysXColliderType Type { get; set; }

        void CreateActor();

        void CreateShape();

        void DestroyActor();

        void DestroyShape();

        void BeginUpdate();

        void EndUpdate();
    }
}