namespace HexaEngine.Physics
{
    using HexaGen.Runtime;
    using MagicPhysX;
    using System.Runtime.InteropServices;

    public unsafe class SimulationEventCallbacks
    {
        private readonly PxSimulationEventCallback* callback;
        private readonly PxScene* pxScene;

        public delegate void CollisionDelegate(void* userdata, PxContactPairHeader* pairHeader, PxContactPair* pairs, uint count);

        public delegate void TriggerDelegate(void* userdata, PxTriggerPair* pairs, uint count);

        public delegate void ConstraintBreakDelegate(void* userdata, PxConstraintInfo* constraints, uint count);

        public delegate void WakeSleepDelegate(void* userdata, PxActor** actors, uint count, bool sleeping);

        public delegate void AdvanceDelegate(void* userdata, PxRigidBody** bodyBuffer, PxTransform* poseBuffer, uint count);

        private NativeCallback<CollisionDelegate> collisionCallback;
        private NativeCallback<TriggerDelegate> triggerCallback;
        private NativeCallback<ConstraintBreakDelegate> constraintBreakCallback;
        private NativeCallback<WakeSleepDelegate> wakeSleepCallback;
        private NativeCallback<AdvanceDelegate> advanceCallback;

        public SimulationEventCallbacks(PhysicsSystem system)
        {
            SimulationEventCallbackInfo info = new();
            collisionCallback = new(CollisionCallback);
            triggerCallback = new(TriggerCallback);
            constraintBreakCallback = new(ConstraintBreakCallback);
            wakeSleepCallback = new(WakeSleepCallback);
            advanceCallback = new(AdvanceCallback);
            info.collision_callback = (delegate* unmanaged[Cdecl]<void*, PxContactPairHeader*, PxContactPair*, uint, void>)Marshal.GetFunctionPointerForDelegate(collisionCallback.Callback!);
            info.trigger_callback = (delegate* unmanaged[Cdecl]<void*, PxTriggerPair*, uint, void>)Marshal.GetFunctionPointerForDelegate(triggerCallback.Callback!);
            info.constraint_break_callback = (delegate* unmanaged[Cdecl]<void*, PxConstraintInfo*, uint, void>)Marshal.GetFunctionPointerForDelegate(constraintBreakCallback.Callback!);
            info.wake_sleep_callback = (delegate* unmanaged[Cdecl]<void*, PxActor**, uint, bool, void>)Marshal.GetFunctionPointerForDelegate(wakeSleepCallback.Callback!);
            info.advance_callback = (delegate* unmanaged[Cdecl]<void*, PxRigidBody**, PxTransform*, uint, void>)Marshal.GetFunctionPointerForDelegate(advanceCallback.Callback!);
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
            if (callback != null)
            {
                pxScene->SetSimulationEventCallbackMut(null);
                callback->Delete();
                collisionCallback.Dispose();
                triggerCallback.Dispose();
                constraintBreakCallback.Dispose();
                wakeSleepCallback.Dispose();
                advanceCallback.Dispose();
            }
        }
    }
}