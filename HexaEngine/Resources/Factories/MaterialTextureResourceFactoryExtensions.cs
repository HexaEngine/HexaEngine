namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Resources;

    public static class MaterialTextureResourceFactoryExtensions
    {
        private static readonly int typeId = ResourceTypeRegistry.GetId(typeof(MaterialTextureResourceFactory));

        public static MaterialTexture? LoadTexture(this ResourceManager manager, Core.IO.Binary.Materials.MaterialTexture desc)
        {
            return manager.CreateInstance<MaterialTexture, Core.IO.Binary.Materials.MaterialTexture>(new(desc.File, typeId), desc);
        }

        public static Task<MaterialTexture?> LoadTextureAsync(this ResourceManager manager, Core.IO.Binary.Materials.MaterialTexture desc)
        {
            return Task.Factory.StartNew(() => manager.LoadTexture(desc));
        }
    }
}