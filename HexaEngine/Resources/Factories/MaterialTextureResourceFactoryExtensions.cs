namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core;
    using HexaEngine.Resources;

    public static class MaterialTextureResourceFactoryExtensions
    {
        public static ResourceInstance<MaterialTexture>? LoadTexture(this ResourceManager manager, Core.IO.Materials.MaterialTexture desc)
        {
            string path = Paths.CurrentTexturePath + desc.File;
            return manager.CreateInstance<ResourceInstance<MaterialTexture>, Core.IO.Materials.MaterialTexture>(path, desc);
        }

        public static void UpdateTexture(this ResourceManager manager, ref ResourceInstance<MaterialTexture>? texture, Core.IO.Materials.MaterialTexture desc)
        {
            string fullname = Paths.CurrentTexturePath + desc.File;
            if (texture?.Name == fullname)
            {
                return;
            }

            texture?.Dispose();
            texture = manager.LoadTexture(desc);
        }

        public static Task<ResourceInstance<MaterialTexture>?> LoadTextureAsync(this ResourceManager manager, Core.IO.Materials.MaterialTexture desc)
        {
            return Task.Factory.StartNew(() => manager.LoadTexture(desc));
        }

        public static async Task<ResourceInstance<MaterialTexture>?> UpdateTextureAsync(this ResourceManager manager, ResourceInstance<MaterialTexture>? texture, Core.IO.Materials.MaterialTexture desc)
        {
            string fullname = Paths.CurrentTexturePath + desc.File;
            if (texture?.Name == fullname)
            {
                return texture;
            }

            texture?.Dispose();
            return await manager.LoadTextureAsync(desc);
        }
    }
}