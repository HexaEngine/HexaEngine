namespace HexaEngine.Lights
{
    public class ShadowAtlasHandle : IDisposable
    {
        private readonly ShadowAtlas atlas;
        private readonly SpatialAllocatorHandle handle;
        private bool isValid;

        public ShadowAtlasHandle(ShadowAtlas atlas, SpatialAllocatorHandle handle)
        {
            this.atlas = atlas;
            this.handle = handle;
            atlas.OnDisposing += OnDisposing;
            isValid = true;
        }

        private void OnDisposing(ShadowAtlas obj)
        {
            Dispose(false);
        }

        public ShadowAtlas Atlas => atlas;

        public SpatialAllocatorHandle Handle => handle;

        public bool IsValid { get => isValid; internal set => isValid = value; }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (isValid)
            {
                if (disposing)
                {
                    atlas.Free(this);
                }

                atlas.OnDisposing -= OnDisposing;
                isValid = false;
            }
        }
    }
}