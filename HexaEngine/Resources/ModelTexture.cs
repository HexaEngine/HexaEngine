namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;

    public class ModelTexture
    {
        public string Name;
        public IShaderResourceView SRV;
        public int InstanceCount;

        public ModelTexture(string name, IShaderResourceView sRV, int instances)
        {
            Name = name;
            SRV = sRV;
            InstanceCount = instances;
        }
    }
}