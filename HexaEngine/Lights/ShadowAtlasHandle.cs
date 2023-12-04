namespace HexaEngine.Lights
{
    public struct ShadowAtlasHandle : IDisposable
    {
        private ShadowAtlas atlas;
        private SpatialAllocatorHandle handle;
        private bool valid;

        public ShadowAtlasHandle(ShadowAtlas atlas, SpatialAllocatorHandle handle)
        {
            this.atlas = atlas;
            this.handle = handle;
            valid = true;
        }

        public readonly ShadowAtlas Atlas => atlas;

        public readonly SpatialAllocatorHandle Handle => handle;

        public bool IsValid { readonly get => valid; internal set => valid = value; }

        public void Dispose()
        {
            if (valid)
                atlas.Free(ref this);
            this = default;
        }
    }
}