namespace HexaEngine.Meshes
{
    public struct MeshDrawType
    {
        public uint MeshId;
        public uint TypeId;
        public uint DrawIndirectOffset;
        public MeshDrawInstance[] Instances;

        public MeshDrawType(uint meshId, uint typeId, MeshDrawInstance[] instances)
        {
            MeshId = meshId;
            TypeId = typeId;
            Instances = instances;
        }
    }
}