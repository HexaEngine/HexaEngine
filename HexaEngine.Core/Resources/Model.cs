namespace HexaEngine.Core.Resources
{
    public class Model
    {
        public string Mesh;
        public string Material;

        public Model(string mesh, string material)
        {
            Mesh = mesh;
            Material = material;
        }
    }
}