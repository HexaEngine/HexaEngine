namespace HexaEngine.Resources
{
    using HexaEngine.Objects;

    public class Model
    {
        public Mesh Mesh;
        public Material? Material;

        public Model(Mesh mesh, Material? material)
        {
            Mesh = mesh;
            Material = material;
        }
    }
}