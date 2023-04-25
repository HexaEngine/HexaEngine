namespace HexaEngine.Editor.Meshes
{
    using System.Numerics;

    public struct MeshEditorPerFrame
    {
        public Matrix4x4 ViewProjection;
        public Vector3 CameraPosWorld;
        public float TessellationFactor;
    }
}