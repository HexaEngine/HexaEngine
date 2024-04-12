namespace HexaEngine.Physics
{
    using MagicPhysX;

    public unsafe struct ControllerFilters(FilterData* filterData, QueryFilterCallback? queryFilterCallback, ControllerFilterCallbacks? controllerFilterCallbacks)
    {
        private QueryFilterCallback? queryFilterCallback = queryFilterCallback;
        private ControllerFilterCallbacks? controllerFilterCallbacks = controllerFilterCallbacks;
        internal PxControllerFilters filters = NativeMethods.PxControllerFilters_new((PxFilterData*)filterData, queryFilterCallback == null ? null : queryFilterCallback.queryFilterCallback, controllerFilterCallbacks == null ? null : controllerFilterCallbacks.controllerFilterCallback);

        public QueryFlags FilterFlags
        {
            readonly get => Helper.Convert(filters.mFilterFlags);
            set => filters.mFilterFlags = Helper.Convert(value);
        }

        public FilterData* FilterData
        {
            readonly get => (FilterData*)filters.mFilterData;
            set => filters.mFilterData = (PxFilterData*)value;
        }

        public QueryFilterCallback? FilterCallback
        {
            readonly get => queryFilterCallback;
            set
            {
                queryFilterCallback = value;
                filters.mFilterCallback = value == null ? null : value.queryFilterCallback;
            }
        }

        public ControllerFilterCallbacks? CCTFilterCallback
        {
            readonly get => controllerFilterCallbacks;
            set
            {
                controllerFilterCallbacks = value;
                filters.mCCTFilterCallback = value == null ? null : value.controllerFilterCallback;
            }
        }
    }
}