namespace HexaEngine.Physics
{
    using HexaEngine.Components.Physics;
    using HexaEngine.Components.Physics.Collider;
    using HexaEngine.Core.Unsafes;
    using MagicPhysX;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public unsafe class QueryFilterCallback : IDisposable
    {
        internal PxQueryFilterCallback* queryFilterCallback;
        private bool disposedValue;
        private readonly Func<RigidBody, Pointer<FilterData>, IColliderComponent, PxHitFlags, PxQueryHitType> callback;
        private readonly Func<Pointer<FilterData>, Pointer<QueryHit>, PxQueryHitType> postFilter;

        public QueryFilterCallback(RigidBody actorToIgnore)
        {
            queryFilterCallback = NativeMethods.create_raycast_filter_callback(actorToIgnore.Actor);
        }

        public QueryFilterCallback(Func<RigidBody, Pointer<FilterData>, IColliderComponent, PxHitFlags, PxQueryHitType> callback)
        {
            this.callback = callback;
            delegate* unmanaged[Cdecl]<PxRigidActor*, FilterData*, PxShape*, uint, void*, PxQueryHitType> pCallback = (delegate* unmanaged[Cdecl]<PxRigidActor*, FilterData*, PxShape*, uint, void*, PxQueryHitType>)Marshal.GetFunctionPointerForDelegate(Callback);
            queryFilterCallback = NativeMethods.create_raycast_filter_callback_func((delegate* unmanaged[Cdecl]<PxRigidActor*, PxFilterData*, PxShape*, uint, void*, PxQueryHitType>)pCallback, null);
        }

        public QueryFilterCallback(Func<RigidBody, Pointer<FilterData>, IColliderComponent, PxHitFlags, PxQueryHitType> callback, Func<Pointer<FilterData>, Pointer<QueryHit>, PxQueryHitType> postFilter)
        {
            this.callback = callback;
            this.postFilter = postFilter;
            // TODO: Fork MagicPhysX and update to physx-sys-0.11.5.
            throw new NotSupportedException("Not available sadly, MagicPhysX needs to update to physx-sys-0.11.5");
        }

        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private PxQueryHitType Callback(PxRigidActor* actor, FilterData* filterData, PxShape* shape, PxHitFlags hitFlags, void* userdata)
        {
            var act = Actor.mapper.GetManagedObject<RigidBody>(actor);
            var colliderShape = ColliderShape.mapper.GetManagedObject<ColliderShape>(shape);

            return callback(act, filterData, colliderShape, hitFlags);
        }

        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private PxQueryHitType PostFilter(FilterData* filterData, QueryHit* hit, void* userdata)
        {
            return postFilter(filterData, hit);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                queryFilterCallback->Delete();
                disposedValue = true;
            }
        }

        ~QueryFilterCallback()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}