namespace HexaEngine.Physics
{
    using Hexa.NET.Logging;
    using HexaEngine.Components.Physics.Collider;
    using HexaEngine.Core;
    using HexaEngine.Core.IO.Caching;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;
    using MagicPhysX;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public unsafe class PhysicsSystem : ISceneSystem
    {
        internal static readonly ILogger Logger = LoggerFactory.GetLogger("PhysX");
        private static readonly PersistentCache cookingCache = new("./cache/cookingcache.bin", "./cache/cookingcache.index");
        private static readonly PxFoundation* foundation;
        private static readonly PxTolerancesScale tolerancesScale;
        private static readonly PxPhysics* physics;
        private static readonly PxDefaultCpuDispatcher* dispatcher;

        private readonly ComponentTypeQuery<IActorComponent> actors = new();
        private readonly ComponentTypeQuery<IJointComponent> joints = new();
        private readonly ComponentTypeQuery<ICharacterControllerComponent> characterControllers = new();
        private PxScene* pxScene;

        private readonly SimulationEventCallbacks eventCallbacks;
        private readonly ControllerManager controllerManager;

        private static readonly object _lock = new();

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

            // create pvd
            var pvd = NativeMethods.phys_PxCreatePvd(foundation);

            fixed (byte* bytePointer = "127.0.0.1"u8)
            {
                var transport = NativeMethods.phys_PxDefaultPvdSocketTransportCreate(bytePointer, 5425, 10);
                pvd->ConnectMut(transport, PxPvdInstrumentationFlags.All);
            }

            if (foundation == null)
            {
                Logger.Error("Failed to create PxFoundation", true);
            }

            tolerancesScale = new PxTolerancesScale { length = 1, speed = 10 };
            var scale = tolerancesScale;
            physics = NativeMethods.phys_PxCreatePhysics(versionNumber, foundation, &scale, true, pvd, null);

            NativeMethods.phys_PxInitExtensions(physics, pvd);

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
            sceneDesc.flags |= PxSceneFlags.EnableCcd;
            sceneDesc.staticKineFilteringMode = PxPairFilteringMode.Keep;
            sceneDesc.EnableCustomFilterShader(&FilterShader, 0);

            pxScene = NativeMethods.PxPhysics_createScene_mut(physics, &sceneDesc);

            if (pxScene == null)
            {
                Logger.Error("Failed to create PxScene", true);
            }

            var pvdClient = pxScene->GetScenePvdClientMut();
            if (pvdClient != null)
            {
                pvdClient->SetScenePvdFlagMut(PxPvdSceneFlag.TransmitConstraints, true);
                pvdClient->SetScenePvdFlagMut(PxPvdSceneFlag.TransmitContacts, true);
                pvdClient->SetScenePvdFlagMut(PxPvdSceneFlag.TransmitScenequeries, true);
            }

            eventCallbacks = new(this);
            controllerManager = new(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization | MethodImplOptions.PreserveSig)]
        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static PxFilterFlags FilterShader(FilterShaderCallbackInfo* arg)
        {
            PxPairFlags flags = PxPairFlags.ContactDefault | PxPairFlags.DetectCcdContact | PxPairFlags.NotifyTouchCcd;
            *arg->pairFlags = flags;
            return 0;
        }

        public static object SyncObject => _lock;

        public static PersistentCache CookingCache => cookingCache;

        public static PxFoundation* PxFoundation => foundation;

        public static PxTolerancesScale TolerancesScale => tolerancesScale;

        public static PxPhysics* PxPhysics => physics;

        public PxScene* PxScene => pxScene;

        public PxSimulationStatistics Statistics => statistics;

        public SimulationEventCallbacks EventCallbacks => eventCallbacks;

        public ControllerManager ControllerManager => controllerManager;

        public static bool Debug { get; internal set; }

        public bool RaycastAny(Vector3 origin, Vector3 dir, float distance, QueryFlags flags)
        {
            return RaycastAny(pxScene, origin, dir, distance, flags, null);
        }

        public bool RaycastAny(Vector3 origin, Vector3 dir, float distance, QueryFlags flags, QueryFilterCallback? callback)
        {
            return RaycastAny(pxScene, origin, dir, distance, flags, callback);
        }

        internal bool RaycastAny(PxScene* pxScene, Vector3 origin, Vector3 dir, float distance, QueryFlags flags, QueryFilterCallback? callback)
        {
            lock (_lock)
            {
                PxQueryFilterData filterData;
                filterData.flags = Helper.Convert(flags);
                PxQueryHit queryHit;
                PxQueryFilterCallback* filterCallback = callback == null ? null : callback.queryFilterCallback;

                bool result = pxScene->QueryExtRaycastAny((PxVec3*)&origin, (PxVec3*)&dir, distance, &queryHit, &filterData, filterCallback, null);
                return result;
            }
        }

        public bool RaycastSingle(Vector3 origin, Vector3 dir, float distance, HitFlags hitFlags, out RaycastHit raycastHit, QueryFlags queryFlags)
        {
            return RaycastSingle(pxScene, origin, dir, distance, hitFlags, out raycastHit, queryFlags, null);
        }

        public bool RaycastSingle(Vector3 origin, Vector3 dir, float distance, HitFlags hitFlags, out RaycastHit raycastHit, QueryFlags queryFlags, QueryFilterCallback? callback)
        {
            return RaycastSingle(pxScene, origin, dir, distance, hitFlags, out raycastHit, queryFlags, callback);
        }

        internal static bool RaycastSingle(PxScene* pxScene, Vector3 origin, Vector3 dir, float distance, HitFlags hitFlags, out RaycastHit raycastHit, QueryFlags queryFlags, QueryFilterCallback? callback)
        {
            lock (_lock)
            {
                PxHitFlags pxHitFlags = Helper.Convert(hitFlags);
                PxRaycastHit pxRaycastHit;
                PxQueryFilterCallback* filterCallback = callback == null ? null : callback.queryFilterCallback;
                PxQueryFilterData filterData;
                filterData.flags = Helper.Convert(queryFlags);

                bool result = pxScene->QueryExtRaycastSingle((PxVec3*)&origin, (PxVec3*)&dir, distance, pxHitFlags, &pxRaycastHit, &filterData, filterCallback, null);

                if (result)
                {
                    var act = Actor.mapper.GetManagedObject<RigidBody>(pxRaycastHit.actor);
                    var colliderShape = ColliderShape.mapper.GetManagedObject<ColliderShape>(pxRaycastHit.shape);

                    raycastHit = new(pxRaycastHit.faceIndex, Helper.Convert(pxRaycastHit.flags), pxRaycastHit.position, pxRaycastHit.normal, pxRaycastHit.distance, pxRaycastHit.u, pxRaycastHit.v, act, colliderShape);

                    return true;
                }
                else
                {
                    raycastHit = default;
                    return false;
                }
            }
        }

        public int RaycastMultiple(Vector3 origin, Vector3 dir, float distance, HitFlags hitFlags, List<RaycastHit> hits, out bool blockingHit, uint maxHits, QueryFlags queryFlags, QueryFilterCallback? callback)
        {
            return RaycastMultiple(pxScene, origin, dir, distance, hitFlags, hits, out blockingHit, maxHits, queryFlags, callback);
        }

        public int RaycastMultiple(Vector3 origin, Vector3 dir, float distance, HitFlags hitFlags, List<RaycastHit> hits, out bool blockingHit, uint maxHits, QueryFlags queryFlags)
        {
            return RaycastMultiple(pxScene, origin, dir, distance, hitFlags, hits, out blockingHit, maxHits, queryFlags, null);
        }

        internal static int RaycastMultiple(PxScene* pxScene, Vector3 origin, Vector3 dir, float distance, HitFlags hitFlags, List<RaycastHit> hits, out bool blockingHit, uint maxHits, QueryFlags queryFlags, QueryFilterCallback? callback)
        {
            lock (_lock)
            {
                PxHitFlags pxHitFlags = Helper.Convert(hitFlags);

                PxQueryFilterCallback* filterCallback = callback == null ? null : callback.queryFilterCallback;
                PxQueryFilterData filterData;
                filterData.flags = Helper.Convert(queryFlags);

                PxRaycastHit* raycastHitBuffer;

                bool ownsBuffer = maxHits > 128; // PxRaycastHit == 64 Bytes => 64 Bytes * 128 = 8192 Bytes.
                if (ownsBuffer)
                {
                    raycastHitBuffer = (PxRaycastHit*)Marshal.AllocHGlobal((nint)(sizeof(PxRaycastHit) * maxHits));
                }
                else
                {
                    PxRaycastHit* raycastHitBuffer1 = stackalloc PxRaycastHit[(int)maxHits];
                    raycastHitBuffer = raycastHitBuffer1;
                }

                uint bufferSize = 128;
                bool pxBlockingHit;
                int result = pxScene->QueryExtRaycastMultiple((PxVec3*)&origin, (PxVec3*)&dir, distance, pxHitFlags, raycastHitBuffer, bufferSize, &pxBlockingHit, &filterData, filterCallback, null);
                blockingHit = pxBlockingHit;

                if (result == -1)
                {
                    result = (int)bufferSize;
                }

                for (int i = 0; i < result; i++)
                {
                    var hit = raycastHitBuffer[i];

                    var act = Actor.mapper.GetManagedObject<RigidBody>(hit.actor);
                    var colliderShape = ColliderShape.mapper.GetManagedObject<ColliderShape>(hit.shape);

                    RaycastHit raycastHit = new(hit.faceIndex, Helper.Convert(hit.flags), hit.position, hit.normal, hit.distance, hit.u, hit.v, act, colliderShape);

                    hits.Add(raycastHit);
                }

                if (ownsBuffer)
                {
                    Marshal.FreeHGlobal((nint)raycastHitBuffer);
                }

                return result;
            }
        }

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(actors);
            scene.QueryManager.AddQuery(joints);
            scene.QueryManager.AddQuery(characterControllers);

            actors.OnAdded += OnActorAdded;
            actors.OnRemoved += OnActorRemoved;

            joints.OnAdded += OnJointAdded;
            joints.OnRemoved += OnJointRemoved;

            characterControllers.OnAdded += OnCharacterControllerAdded;
            characterControllers.OnRemoved += OnCharacterControllerRemoved;

            for (int i = 0; i < actors.Count; i++)
            {
                actors[i].CreateActor(physics, pxScene);
            }

            for (int i = 0; i < joints.Count; i++)
            {
                joints[i].CreateJoint(physics, pxScene);
            }

            for (int i = 0; i < characterControllers.Count; i++)
            {
                characterControllers[i].CreateController(physics, pxScene, controllerManager.manager);
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

        private void OnCharacterControllerRemoved(GameObject gameObject, ICharacterControllerComponent controller)
        {
            lock (_lock)
            {
                controller.DestroyController();
            }
        }

        private void OnCharacterControllerAdded(GameObject gameObject, ICharacterControllerComponent controller)
        {
            lock (_lock)
            {
                controller.CreateController(physics, pxScene, controllerManager.manager);
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

            controllerManager.manager->ComputeInteractionsMut(Time.FixedDelta, null);

            for (int i = 0; i < characterControllers.Count; i++)
            {
                characterControllers[i].Update();
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

            for (int i = 0; i < characterControllers.Count; i++)
            {
                characterControllers[i].DestroyController();
            }

            characterControllers.OnAdded -= OnCharacterControllerAdded;
            characterControllers.OnRemoved -= OnCharacterControllerRemoved;

            characterControllers.Dispose();

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