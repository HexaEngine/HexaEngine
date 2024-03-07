namespace HexaEngine.Physics
{
    using MagicPhysX;

    public unsafe class ControllerFilters
    {
        internal PxControllerFilters* filters;

        public ControllerFilters(QueryFilterCallback? queryFilterCallback)
        {
            PxFilterData* pFilterData = AllocT<PxFilterData>();

            NativeMethods.PxControllerFilters_new(pFilterData, queryFilterCallback == null ? null : queryFilterCallback.queryFilterCallback, null);
        }
    }
}