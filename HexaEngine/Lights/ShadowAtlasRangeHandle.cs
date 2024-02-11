namespace HexaEngine.Lights
{
    public class ShadowAtlasRangeHandle : IDisposable
    {
        private readonly ShadowAtlas atlas;
        private readonly SpatialAllocatorHandle[] handles;
        private bool isValid;

        public ShadowAtlasRangeHandle(ShadowAtlas atlas, SpatialAllocatorHandle[] handles)
        {
            this.atlas = atlas;
            this.handles = handles;
            atlas.OnDisposing += OnDisposing;
            isValid = true;
        }

        private void OnDisposing(ShadowAtlas obj)
        {
            Dispose(false);
        }

        public ShadowAtlas Atlas => atlas;

        public SpatialAllocatorHandle[] Handles => handles;

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
                    atlas.FreeRange(this);
                }

                atlas.OnDisposing -= OnDisposing;
                isValid = false;
            }
        }
    }
}