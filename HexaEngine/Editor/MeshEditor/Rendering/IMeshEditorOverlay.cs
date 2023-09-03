namespace HexaEngine.Editor.MeshEditor.Rendering
{
    public interface IMeshEditorOverlay : IDisposable
    {
        public string Name { get; }

        public bool Enabled { get; set; }

        public void DrawOverlay(MeshEditorMesh mesh, MeshEditorMaterial material);
    }
}