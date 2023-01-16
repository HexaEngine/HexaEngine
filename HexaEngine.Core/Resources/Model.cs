namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Meshes;

    public class Model
    {
        public string Mesh;
        public MaterialDesc Material;

        public Model(string mesh, MaterialDesc material)
        {
            Mesh = mesh;
            Material = material;
        }
    }
}