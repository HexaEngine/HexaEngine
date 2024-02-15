namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Resources;

    public static class MaterialTextureResourceFactoryExtensions
    {
        public static ResourceInstance<MaterialTexture>? LoadTexture(this ResourceManager manager, Core.IO.Binary.Materials.MaterialTexture desc)
        {
            return manager.CreateInstance<ResourceInstance<MaterialTexture>, Core.IO.Binary.Materials.MaterialTexture>(desc.File, desc);
        }

        public static void UpdateTexture(this ResourceManager manager, ref ResourceInstance<MaterialTexture>? texture, Core.IO.Binary.Materials.MaterialTexture desc)
        {
            if (desc.File == Guid.Empty)
            {
                return;
            }

            texture?.Dispose();
            texture = manager.LoadTexture(desc);
        }

        public static Task<ResourceInstance<MaterialTexture>?> LoadTextureAsync(this ResourceManager manager, Core.IO.Binary.Materials.MaterialTexture desc)
        {
            return Task.Factory.StartNew(() => manager.LoadTexture(desc));
        }

        public static async Task<ResourceInstance<MaterialTexture>?> UpdateTextureAsync(this ResourceManager manager, ResourceInstance<MaterialTexture>? texture, Core.IO.Binary.Materials.MaterialTexture desc)
        {
            if (desc.File == Guid.Empty)
            {
                return texture;
            }

            texture?.Dispose();
            return await manager.LoadTextureAsync(desc);
        }
    }
}