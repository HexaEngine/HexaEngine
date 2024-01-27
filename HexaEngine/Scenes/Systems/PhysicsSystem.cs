namespace HexaEngine.Scenes.Systems
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO.Caching;
    using HexaEngine.Physics;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;
    using MagicPhysX;

    public unsafe class PhysicsSystem : ISystem
    {
        private static readonly PersistentCache cookingCache = new("./cache/cookingcache.bin", "./cache/cookingcache.index");
        private static readonly PxFoundation* foundation;
        private static readonly PxPhysics* physics;

        private readonly ComponentTypeQuery<IPhysXColliderComponent> colliders = new();
        private readonly PxScene* pxScene;
        private readonly PxDefaultCpuDispatcher* dispatcher;

        private readonly object _lock = new();

        public string Name { get; } = "Physics";

        public SystemFlags Flags { get; } = SystemFlags.PhysicsUpdate | SystemFlags.Awake | SystemFlags.Destroy;

        static PhysicsSystem()
        {
            uint PX_PHYSICS_VERSION_MAJOR = 5;
            uint PX_PHYSICS_VERSION_MINOR = 1;
            uint PX_PHYSICS_VERSION_BUGFIX = 3;
            uint versionNumber = (PX_PHYSICS_VERSION_MAJOR << 24) + (PX_PHYSICS_VERSION_MINOR << 16) + (PX_PHYSICS_VERSION_BUGFIX << 8);

            PxDefaultErrorCallback* errorCallback = NativeMethods.get_default_error_callback();
            PxDefaultAllocator* allocator = NativeMethods.get_default_allocator();

            foundation = NativeMethods.phys_PxCreateFoundation(versionNumber, (PxAllocatorCallback*)allocator, (PxErrorCallback*)errorCallback);

            if (foundation == null)
            {
                Logger.Error("Failed to create PxFoundation", true);
            }

            var tolerancesScale = new PxTolerancesScale { length = 1, speed = 10 };
            physics = NativeMethods.phys_PxCreatePhysics(versionNumber, foundation, &tolerancesScale, true, null, null);

            if (physics == null)
            {
                Logger.Error("Failed to create PxPhysics", true);
            }

            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Application.OnApplicationClose += OnApplicationClose;
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            cookingCache.Save();
        }

        private static void OnApplicationClose()
        {
            cookingCache.Dispose();

            physics->ReleaseMut();
            foundation->ReleaseMut();
        }

        public PhysicsSystem()
        {
            PxSceneDesc sceneDesc = NativeMethods.PxSceneDesc_new(NativeMethods.PxPhysics_getTolerancesScale(physics));
            sceneDesc.gravity = new() { x = 0, y = -9.81f, z = 0 };

            dispatcher = NativeMethods.phys_PxDefaultCpuDispatcherCreate((uint)Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1), null, PxDefaultCpuDispatcherWaitForWorkMode.WaitForWork, 0);
            sceneDesc.cpuDispatcher = (PxCpuDispatcher*)dispatcher;
            sceneDesc.filterShader = NativeMethods.get_default_simulation_filter_shader();

            pxScene = NativeMethods.PxPhysics_createScene_mut(physics, &sceneDesc);

            if (pxScene == null)
            {
                Logger.Error("Failed to create PxScene", true);
            }
        }

        public const float TimestepDuration = 1 / 60f;
        private float accumulator;

        public object SyncObject => _lock;

        public static PersistentCache CookingCache => cookingCache;

        public static PxFoundation* PxFoundation => foundation;

        public static PxPhysics* PxPhysics => physics;

        public PxScene* PxScene => pxScene;

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(colliders);
            accumulator = 0;
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

            accumulator += delta;

            lock (_lock)
            {
                while (accumulator >= TimestepDuration)
                {
                    NativeMethods.PxScene_simulate_mut(pxScene, TimestepDuration, null, null, 0, true);
                    uint error = 0;
                    pxScene->FetchResultsMut(true, &error);

                    if (error != 0)
                    {
                        Logger.Error($"PhysX Error: {(PxErrorCode)error}");
                    }

                    accumulator -= TimestepDuration;
                }
            }

            for (int i = 0; i < colliders.Count; i++)
            {
                colliders[i].EndUpdate();
            }
        }

        public void Destroy()
        {
            NativeMethods.PxScene_release_mut(pxScene);
            NativeMethods.PxDefaultCpuDispatcher_release_mut(dispatcher);

            accumulator = 0;
        }
    }
}