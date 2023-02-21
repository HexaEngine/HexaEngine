namespace HexaEngine.Core.Animations
{
    public struct MorphMeshChannel
    {
        public string MeshName;
        public List<MeshMorphKeyframe> Keyframes = new();

        public MorphMeshChannel(string meshName) : this()
        {
            MeshName = meshName;
        }
    }
}