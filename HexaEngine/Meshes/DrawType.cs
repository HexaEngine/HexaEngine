namespace HexaEngine.Meshes
{
    public class DrawType
    {
        public uint MeshId;
        public uint TypeId;
        public uint DrawIndirectOffset;
        public List<DrawInstance> Instances;

        public DrawType(uint meshId, uint typeId, List<DrawInstance> instances)
        {
            MeshId = meshId;
            TypeId = typeId;
            Instances = instances;
        }
    }
}