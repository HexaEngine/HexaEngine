namespace HexaEngine.Physics
{
    using HexaEngine.Components.Physics.Collider;
    using HexaEngine.Core.Unsafes;
    using MagicPhysX;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate QueryHitType QueryFilterCallbackDelegate(RigidBody? rigidBody, Pointer<FilterData> filterData, IColliderComponent? collider, HitFlags hitFlags);

    public unsafe class QueryFilterCallback : IDisposable
    {
        internal PxQueryFilterCallback* queryFilterCallback;
        private bool disposedValue;
        private readonly QueryFilterCallbackDelegate callback;
        private readonly Func<Pointer<FilterData>, Pointer<QueryHit>, QueryHitType> postFilter;

        public QueryFilterCallback(RigidBody actorToIgnore)
        {
            queryFilterCallback = NativeMethods.create_raycast_filter_callback(actorToIgnore.Actor);
        }

        public QueryFilterCallback(QueryFilterCallbackDelegate callback)
        {
            this.callback = callback;
            var pUserCallback = Marshal.GetFunctionPointerForDelegate(callback);
            delegate* unmanaged[Cdecl]<PxRigidActor*, PxFilterData*, PxShape*, uint, void*, PxQueryHitType> pCallback = &InternalCallback;
            queryFilterCallback = NativeMethods.create_raycast_filter_callback_func(pCallback, (void*)pUserCallback);
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static PxQueryHitType InternalCallback(PxRigidActor* actor, PxFilterData* filterData, PxShape* shape, uint hitFlags, void* userdata)
        {
            var userCallback = Marshal.GetDelegateForFunctionPointer<QueryFilterCallbackDelegate>((nint)userdata);

            var act = Actor.mapper.GetManagedObject<RigidBody>(actor);
            var colliderShape = ColliderShape.mapper.GetManagedObject<ColliderShape>(shape);

            var result = userCallback(act, (FilterData*)filterData, colliderShape, Helper.Convert((PxHitFlags)hitFlags));

            return Helper.Convert(result);
        }

        public QueryFilterCallback(QueryFilterCallbackDelegate callback, Func<Pointer<FilterData>, Pointer<QueryHit>, QueryHitType> postFilter)
        {
            this.callback = callback;
            this.postFilter = postFilter;
            // TODO: Fork MagicPhysX and update to physx-sys-0.11.5.
            throw new NotSupportedException("Not available sadly, MagicPhysX needs to update to physx-sys-0.11.5");
        }

        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        private PxQueryHitType PostFilter(FilterData* filterData, QueryHit* hit, void* userdata)
        {
            return Helper.Convert(postFilter(filterData, hit));
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