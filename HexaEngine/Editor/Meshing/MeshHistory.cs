namespace HexaEngine.Editor.Meshes
{
    using HexaEngine.Core.IO.Meshes;

    public class MeshHistory
    {
        private Stack<MeshHistoryEntry> redoHistory = new();
        private Stack<MeshHistoryEntry> undoHistory = new();

        public void PushDo(MeshData data)
        {
        }

        public void Redo(MeshData data)
        {
        }

        public void Undo(MeshData data)
        {
        }
    }
}