namespace HexaEngine.Editor.MeshEditor
{
    using System.Numerics;

    public struct ModelEditorPerFrame
    {
        public Matrix4x4 ViewProjection;
        public Vector3 CameraPosWorld;
        public float TessellationFactor;
    }
}