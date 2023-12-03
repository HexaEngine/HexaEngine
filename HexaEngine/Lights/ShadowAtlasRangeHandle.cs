namespace HexaEngine.Lights
{
    public struct ShadowAtlasRangeHandle
    {
        private ShadowAtlas atlas;
        private readonly SpatialAllocatorHandle[] handles;
        private bool valid;

        public ShadowAtlasRangeHandle(ShadowAtlas atlas, SpatialAllocatorHandle[] handles)
        {
            this.atlas = atlas;
            this.handles = handles;
            valid = true;
        }

        public readonly ShadowAtlas Atlas => atlas;

        public readonly SpatialAllocatorHandle[] Handles => handles;

        public bool IsValid { readonly get => valid; internal set => valid = value; }

        public void Release()
        {
            if (valid)
                atlas.FreeRange(ref this);
            valid = false;
            atlas = null;
        }
    }
}