namespace HexaEngine.Core.Animations
{
    using System.Collections.Generic;

    public struct MeshChannel
    {
        public string MeshName;
        public List<MeshKeyframe> Keyframes = new();

        public MeshChannel(string meshName) : this()
        {
            MeshName = meshName;
        }
    }
}