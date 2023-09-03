namespace HexaEngine.Scenes.Systems
{
    using BepuPhysics;
    using BepuUtilities;
    using BepuUtilities.Memory;
    using HexaEngine.Core;
    using HexaEngine.Physics;
    using HexaEngine.Physics.Characters;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class PhysicsSystem : ISystem
    {
        private readonly ComponentTypeQuery<IColliderComponent> colliders = new();

        private readonly ThreadDispatcher dispatcher;
        private readonly BufferPool bufferPool;
        private readonly Simulation simulation;

        private readonly CharacterControllers characterControllers;
        private readonly ContactEvents contactEvents;
        private readonly CollidableProperty<PhysicsMaterial> materials;

        public PhysicsSystem()
        {
            dispatcher = new(Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1));
            bufferPool = new BufferPool();
            characterControllers = new(bufferPool);
            contactEvents = new(dispatcher, bufferPool);
            materials = new(bufferPool);
            simulation = Simulation.Create(bufferPool, new NarrowphaseCallbacks(characterControllers, contactEvents, materials), new PoseIntegratorCallbacks(new Vector3(0, -9.81f, 0)), new SolveDescription(8, 1));
        }

        public string Name => "PhysicsUpdate";

        public ThreadDispatcher Dispatcher => dispatcher;

        public BufferPool BufferPool => bufferPool;

        public Simulation Simulation => simulation;

        public CharacterControllers CharacterControllers => characterControllers;

        public ContactEvents ContactEvents => contactEvents;

        public CollidableProperty<PhysicsMaterial> Materials => materials;

        public SystemFlags Flags { get; } = SystemFlags.PhysicsUpdate | SystemFlags.Awake | SystemFlags.Destroy;

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(colliders);
        }

        public void Update(float delta)
        {
            if (Application.InDesignMode)
            {
                return;
            }

            for (int i = 0; i < colliders.Count; i++)
            {
                colliders[i].BeginUpdate();
            }

            Simulation.Timestep(delta, dispatcher);

            for (int i = 0; i < colliders.Count; i++)
            {
                colliders[i].EndUpdate();
            }
        }

        public void Destroy()
        {
            contactEvents.Dispose();
            characterControllers.Dispose();
            materials.Dispose();
            simulation.Dispose();
            bufferPool.Clear();
            dispatcher.Dispose();
        }
    }
}