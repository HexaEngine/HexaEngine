namespace HexaEngine.Physics
{
    using MagicPhysX;
    using System.Runtime.InteropServices;

    public unsafe class SimulationEventCallbacks
    {
        private readonly PxSimulationEventCallback* callback;
        private readonly PxScene* pxScene;

        public SimulationEventCallbacks(PhysicsSystem system)
        {
            SimulationEventCallbackInfo info = new();
            info.collision_callback = (delegate* unmanaged[Cdecl]<void*, PxContactPairHeader*, PxContactPair*, uint, void>)Marshal.GetFunctionPointerForDelegate(CollisionCallback);
            info.trigger_callback = (delegate* unmanaged[Cdecl]<void*, PxTriggerPair*, uint, void>)Marshal.GetFunctionPointerForDelegate(TriggerCallback);
            info.constraint_break_callback = (delegate* unmanaged[Cdecl]<void*, PxConstraintInfo*, uint, void>)Marshal.GetFunctionPointerForDelegate(ConstraintBreakCallback);
            info.wake_sleep_callback = (delegate* unmanaged[Cdecl]<void*, PxActor**, uint, bool, void>)Marshal.GetFunctionPointerForDelegate(WakeSleepCallback);
            info.advance_callback = (delegate* unmanaged[Cdecl]<void*, PxRigidBody**, PxTransform*, uint, void>)Marshal.GetFunctionPointerForDelegate(AdvanceCallback);
            callback = NativeMethods.create_simulation_event_callbacks(&info);
            pxScene = system.PxScene;
            pxScene->SetSimulationEventCallbackMut(callback);
        }

        public void CollisionCallback(void* userdata, PxContactPairHeader* pairHeader, PxContactPair* pairs, uint count)
        {
            // TODO: Add event handler and forward to IColliderComponents.
        }

        public void TriggerCallback(void* userdata, PxTriggerPair* pairs, uint count)
        {
            // TODO: Add event handler and forward to IColliderComponents.
        }

        public void ConstraintBreakCallback(void* userdata, PxConstraintInfo* constraints, uint count)
        {
            // TODO: Add event handler and forward to IColliderComponents.
        }

        public void WakeSleepCallback(void* userdata, PxActor** actors, uint count, bool sleeping)
        {
            // TODO: Add event handler and forward to IColliderComponents.
        }

        public void AdvanceCallback(void* userdata, PxRigidBody** bodyBuffer, PxTransform* poseBuffer, uint count)
        {
            // TODO: Add event handler and forward to IColliderComponents.
        }

        public void Dispose()
        {
            pxScene->SetSimulationEventCallbackMut(null);
            callback->Delete();
        }
    }
}