namespace HexaEngine.Resources
{
    using HexaEngine.Objects;

    public class Model
    {
        public MeshData Mesh;
        public MaterialDesc Material;

        public Model(MeshData mesh, MaterialDesc material)
        {
            Mesh = mesh;
            Material = material;
        }
    }
}