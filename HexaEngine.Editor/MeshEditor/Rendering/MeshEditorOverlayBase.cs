namespace HexaEngine.Editor.MeshEditor.Rendering
{
    public abstract class MeshEditorOverlayBase : IMeshEditorOverlay
    {
        private bool disposedValue;

        public MeshEditorOverlayBase()
        {
        }

        public abstract string Name { get; }

        public abstract bool Enabled { get; set; }

        public abstract void DrawOverlay(MeshEditorMesh mesh, MeshEditorMaterial material);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposeCore();
                disposedValue = true;
            }
        }

        protected abstract void DisposeCore();

        ~MeshEditorOverlayBase()
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