namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core;
    using HexaEngine.Resources;

    public static class MaterialTextureResourceFactoryExtensions
    {
        public static ResourceInstance<MaterialTexture>? LoadTexture(this ResourceManager1 manager, Core.IO.Materials.MaterialTexture desc)
        {
            string path = Paths.CurrentTexturePath + desc.File;
            return manager.CreateInstance<ResourceInstance<MaterialTexture>, Core.IO.Materials.MaterialTexture>(path, desc);
        }

        public static void UnloadTexture(this ResourceManager1 manager, ResourceInstance<MaterialTexture>? instance)
        {
            manager.DestroyInstance(instance);
        }

        public static void UpdateTexture(this ResourceManager1 manager, ref ResourceInstance<MaterialTexture>? texture, Core.IO.Materials.MaterialTexture desc)
        {
            string fullname = Paths.CurrentTexturePath + desc.File;
            if (texture?.Name == fullname)
            {
                return;
            }

            manager.UnloadTexture(texture);
            texture = manager.LoadTexture(desc);
        }

        public static Task<ResourceInstance<MaterialTexture>?> LoadTextureAsync(this ResourceManager1 manager, Core.IO.Materials.MaterialTexture desc)
        {
            return Task.Factory.StartNew(() => manager.LoadTexture(desc));
        }

        public static async Task<ResourceInstance<MaterialTexture>?> UpdateTextureAsync(this ResourceManager1 manager, ResourceInstance<MaterialTexture>? texture, Core.IO.Materials.MaterialTexture desc)
        {
            string fullname = Paths.CurrentTexturePath + desc.File;
            if (texture?.Name == fullname)
            {
                return texture;
            }

            manager.UnloadTexture(texture);
            return await manager.LoadTextureAsync(desc);
        }
    }
}