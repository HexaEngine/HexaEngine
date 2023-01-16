namespace HexaEngine.Resources
{
    using HexaEngine.IO.Meshes;
    using HexaEngine.Objects;

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