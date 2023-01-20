namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Meshes;

    public class Model
    {
        public string Mesh;

        // TODO: Do not store material as instance in model
        public MaterialDesc Material;

        public Model(string mesh, MaterialDesc material)
        {
            Mesh = mesh;
            Material = material;
        }
    }
}