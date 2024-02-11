namespace HexaEngine.Physics
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO.Caching;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;
    using MagicPhysX;
    using System.Numerics;

    public unsafe class PhysicsSystem : ISystem
    {
        private static readonly PersistentCache cookingCache = new("./cache/cookingcache.bin", "./cache/cookingcache.index");
        private static readonly PxFoundation* foundation;
        private static readonly PxTolerancesScale tolerancesScale;
        private static readonly PxPhysics* physics;
        private static readonly PxDefaultCpuDispatcher* dispatcher;

        private readonly ComponentTypeQuery<IActorComponent> actors = new();
        private readonly ComponentTypeQuery<IJointComponent> joints = new();
        private PxScene* pxScene;

        private readonly SimulationEventCallbacks eventCallbacks;
        private readonly ControllerManager controllerManager;

        private readonly object _lock = new();

        private PxSimulationStatistics statistics;

        public string Name { get; } = "Physics";

        public SystemFlags Flags { get; } = SystemFlags.PhysicsUpdate | SystemFlags.Awake | SystemFlags.Destroy;

        static PhysicsSystem()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Application.OnApplicationClose += OnApplicationClose;

            uint PX_PHYSICS_VERSION_MAJOR = 5;
            uint PX_PHYSICS_VERSION_MINOR = 1;
            uint PX_PHYSICS_VERSION_BUGFIX = 3;
            uint versionNumber = (PX_PHYSICS_VERSION_MAJOR << 24) + (PX_PHYSICS_VERSION_MINOR << 16) + (PX_PHYSICS_VERSION_BUGFIX << 8);

            //nint fnPtr = Marshal.GetFunctionPointerForDelegate(ReportError);
            //var errorCallback = NativeMethods.create_error_callback((delegate* unmanaged[Cdecl]<PxErrorCode, sbyte*, sbyte*, uint, void*, void>)fnPtr, null);

            PxDefaultErrorCallback* errorCallback = NativeMethods.get_default_error_callback();
            PxDefaultAllocator* allocator = NativeMethods.get_default_allocator();

            foundation = NativeMethods.phys_PxCreateFoundation(versionNumber, (PxAllocatorCallback*)allocator, (PxErrorCallback*)errorCallback);

            if (foundation == null)
            {
                Logger.Error("Failed to create PxFoundation", true);
            }

            tolerancesScale = new PxTolerancesScale { length = 1, speed = 10 };
            var scale = tolerancesScale;
            physics = NativeMethods.phys_PxCreatePhysics(versionNumber, foundation, &scale, true, null, null);

            if (physics == null)
            {
                Logger.Error("Failed to create PxPhysics", true);
            }

            dispatcher = NativeMethods.phys_PxDefaultCpuDispatcherCreate((uint)Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1), null, PxDefaultCpuDispatcherWaitForWorkMode.WaitForWork, 0);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            cookingCache.Save();
        }

        private static void OnApplicationClose()
        {
            cookingCache.Dispose();

            dispatcher->ReleaseMut();
            physics->ReleaseMut();
            foundation->ReleaseMut();
        }

        public PhysicsSystem()
        {
            PxSceneDesc sceneDesc = NativeMethods.PxSceneDesc_new(NativeMethods.PxPhysics_getTolerancesScale(physics));
            sceneDesc.gravity = new() { x = 0, y = -9.81f, z = 0 };

            sceneDesc.cpuDispatcher = (PxCpuDispatcher*)dispatcher;
            sceneDesc.filterShader = NativeMethods.get_default_simulation_filter_shader();

            pxScene = NativeMethods.PxPhysics_createScene_mut(physics, &sceneDesc);

            if (pxScene == null)
            {
                Logger.Error("Failed to create PxScene", true);
            }

            eventCallbacks = new(this);
            controllerManager = new(this);
        }

        public object SyncObject => _lock;

        public static PersistentCache CookingCache => cookingCache;

        public static PxFoundation* PxFoundation => foundation;

        public static PxTolerancesScale TolerancesScale => tolerancesScale;

        public static PxPhysics* PxPhysics => physics;

        public PxScene* PxScene => pxScene;

        public PxSimulationStatistics Statistics => statistics;

        public SimulationEventCallbacks EventCallbacks => eventCallbacks;

        public ControllerManager ControllerManager => controllerManager;

        public static bool Debug { get; internal set; }

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(actors);
            scene.QueryManager.AddQuery(joints);

            actors.OnAdded += OnActorAdded;
            actors.OnRemoved += OnActorRemoved;

            joints.OnAdded += OnJointAdded;
            joints.OnRemoved += OnJointRemoved;

            for (int i = 0; i < actors.Count; i++)
            {
                actors[i].CreateActor(physics, pxScene);
            }

            for (int i = 0; i < joints.Count; i++)
            {
                joints[i].CreateJoint(physics, pxScene);
            }
        }

        /// <summary>
        /// Shift the scene origin by the specified vector. The poses of all objects in the
        /// scene and the corresponding data structures will get adjusted to reflect the
        /// new origin location (the shift vector will get subtracted from all object positions).
        /// It is the user's responsibility to keep track of the summed total origin shift
        /// and adjust all input/output to/from PhysX accordingly. Do not use this method
        /// while the simulation is running. Calls to this method while the simulation is
        /// running will be ignored. Make sure to propagate the origin shift to other dependent
        /// modules (for example, the character controller module etc.). This is an expensive
        /// operation and we recommend to use it only in the case where distance related
        /// precision issues may arise in areas far from the origin.
        /// </summary>
        /// <param name="shift"></param>
        public void ShiftOrigin(Vector3 shift)
        {
            lock (_lock)
            {
                pxScene->ShiftOriginMut((PxVec3*)&shift);
            }
        }

        private void OnJointAdded(GameObject gameObject, IJointComponent joint)
        {
            joint.OnRecreate += OnJointRecreate;
            lock (_lock)
            {
                joint.CreateJoint(physics, pxScene);
            }
        }

        private void OnJointRemoved(GameObject gameObject, IJointComponent joint)
        {
            joint.OnRecreate -= OnJointRecreate;
            lock (_lock)
            {
                joint.DestroyJoint();
            }
        }

        private void OnJointRecreate(IJointComponent joint)
        {
            lock (_lock)
            {
                joint.DestroyJoint();
                joint.CreateJoint(physics, pxScene);
            }
        }

        private void OnActorAdded(GameObject gameObject, IActorComponent actor)
        {
            actor.OnRecreate += OnActorRecreate;
            // lock, caller could be potentially a different thread.
            lock (_lock)
            {
                actor.CreateActor(physics, pxScene);
            }
        }

        private void OnActorRemoved(GameObject gameObject, IActorComponent actor)
        {
            actor.OnRecreate -= OnActorRecreate;
            // lock, caller could be potentially a different thread.
            lock (_lock)
            {
                actor.DestroyActor();
            }
        }

        private void OnActorRecreate(IActorComponent actor)
        {
            lock (_lock)
            {
                actor.DestroyActor();
                actor.CreateActor(physics, pxScene);
            }
        }

        public void Update(float delta)
        {
            if (Application.InEditMode)
            {
                return;
            }

            for (int i = 0; i < actors.Count; i++)
            {
                actors[i].BeginUpdate();
            }

            lock (_lock)
            {
                pxScene->SimulateMut(Time.FixedDelta, null, null, 0, true);

                uint error = 0;
                pxScene->FetchResultsMut(true, &error);

                if (error != 0)
                {
                    Logger.Error($"PhysX Error: {(PxErrorCode)error}");
                }
            }

            //PxSimulationStatistics stats;
            //pxScene->GetSimulationStatistics(&stats);
            //statistics = stats;

            for (int i = 0; i < actors.Count; i++)
            {
                actors[i].EndUpdate();
            }
        }

        public void Destroy()
        {
            for (int i = 0; i < joints.Count; i++)
            {
                joints[i].OnRecreate -= OnJointRecreate;
                joints[i].DestroyJoint();
            }

            joints.OnAdded -= OnJointAdded;
            joints.OnRemoved -= OnJointRemoved;

            joints.Dispose();

            for (int i = 0; i < actors.Count; i++)
            {
                actors[i].OnRecreate -= OnActorRecreate;
                actors[i].DestroyActor();
            }

            actors.OnAdded -= OnActorAdded;
            actors.OnRemoved -= OnActorRemoved;

            actors.Dispose();

            eventCallbacks.Dispose();
            controllerManager.Dispose();
            if (pxScene != null)
            {
                pxScene->ReleaseMut();
                pxScene = null;
            }
        }

        private static void ReportError(PxErrorCode code, sbyte* message, sbyte* file, uint line, void* userdata)
        {
            var msg = ToStringFromUTF8(((byte*)message));

            switch (code)
            {
                case PxErrorCode.DebugInfo:
                    Logger.Info(msg);
                    break;

                case PxErrorCode.PerfWarning:
                case PxErrorCode.DebugWarning:
                    Logger.Warn(msg);
                    break;

                case PxErrorCode.InvalidParameter:
                case PxErrorCode.InvalidOperation:
                case PxErrorCode.OutOfMemory:
                case PxErrorCode.InternalError:
                case PxErrorCode.Abort:
                    Logger.Error(msg);
                    break;
            }
        }
    }
}