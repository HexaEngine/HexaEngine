namespace HexaEngine.Resources
{
    using HexaEngine.Core.IO.Binary.Meshes;

    public struct MeshDesc
    {
        public IMeshData MeshData;
        public ILODData LODData;
        public bool IndexDynamic;
        public bool VertexDynamic;

        public MeshDesc(IMeshData meshData, ILODData lODData, bool indexDynamic = false, bool vertexDynamic = false)
        {
            MeshData = meshData;
            LODData = lODData;
            IndexDynamic = indexDynamic;
            VertexDynamic = vertexDynamic;
        }
    }
}