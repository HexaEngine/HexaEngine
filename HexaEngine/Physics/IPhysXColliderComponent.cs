namespace HexaEngine.Physics
{
    using HexaEngine.Scenes;
    using MagicPhysX;

    public unsafe interface IPhysXColliderComponent : IComponent
    {
        bool HasActor { get; }

        bool HasShape { get; }

        bool InCompound { get; }

        ColliderType Type { get; set; }

        void CreateActor();

        void CreateShape();

        void DestroyActor();

        void BeginUpdate();

        void EndUpdate();
    }
}