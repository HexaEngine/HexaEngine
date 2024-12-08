namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Binary.Materials;

    public static class MaterialResourceFactoryExtensions
    {
        public static Material LoadMaterial<T>(this ResourceManager manager, MaterialShaderDesc shaderDesc, MaterialData desc)
        {
            return manager.CreateInstance<Material, (MaterialShaderDesc, MaterialData)>(ResourceTypeRegistry.GetGuid<T>(desc.Guid), (shaderDesc, desc)) ?? throw new NotSupportedException();
        }

        public static async Task<Material> LoadMaterialAsync<T>(this ResourceManager manager, MaterialShaderDesc shaderDesc, MaterialData desc)
        {
            return await manager.CreateInstanceAsync<Material, (MaterialShaderDesc, MaterialData)>(ResourceTypeRegistry.GetGuid<T>(desc.Guid), (shaderDesc, desc)) ?? throw new NotSupportedException();
        }

        public static void UpdateMaterial<T>(this ResourceManager manager, MaterialShaderDesc shaderDesc, MaterialData? desc)
        {
            if (desc == null)
            {
                return;
            }

            if (!manager.TryGetInstance(ResourceTypeRegistry.GetGuid<T>(desc.Guid), out Material? material))
            {
                return;
            }

            material.Update(desc);
            material.BeginUpdate();
            manager.UpdateMaterialShader(material.Shader, shaderDesc);

            for (int i = 0; i < material.TextureList.Count; i++)
            {
                material.TextureList[i]?.Dispose();
            }
            material.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Count; i++)
            {
                material.TextureList.Add(manager.LoadTexture(desc.Textures[i]));
            }

            material.EndUpdate();
        }

        public static async Task UpdateMaterialAsync<T>(this ResourceManager manager, MaterialShaderDesc shaderDesc, MaterialData desc)
        {
            if (!manager.TryGetInstance(ResourceTypeRegistry.GetGuid<T>(desc.Guid), out Material? modelMaterial))
            {
                return;
            }

            modelMaterial.Update(desc);
            modelMaterial.BeginUpdate();

            modelMaterial.Shader = (await manager.UpdateMaterialShaderAsync(modelMaterial.Shader, shaderDesc))!;

            for (int i = 0; i < modelMaterial.TextureList.Count; i++)
            {
                modelMaterial.TextureList[i]?.Dispose();
            }
            modelMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Count; i++)
            {
                modelMaterial.TextureList.Add(await manager.LoadTextureAsync(desc.Textures[i]));
            }

            modelMaterial.EndUpdate();
        }

        public static void UpdateMaterial<T>(this ResourceManager manager, MaterialData? desc)
        {
            if (desc == null)
            {
                return;
            }

            if (!manager.TryGetInstance(ResourceTypeRegistry.GetGuid<T>(desc.Guid), out Material? material))
            {
                return;
            }

            material.Update(desc);
            material.BeginUpdate();
            manager.UpdateMaterialShader(material.Shader, desc);

            for (int i = 0; i < material.TextureList.Count; i++)
            {
                material.TextureList[i]?.Dispose();
            }
            material.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Count; i++)
            {
                material.TextureList.Add(manager.LoadTexture(desc.Textures[i]));
            }

            material.EndUpdate();
        }

        public static async Task UpdateMaterialAsync<T>(this ResourceManager manager, MaterialData desc)
        {
            if (!manager.TryGetInstance(ResourceTypeRegistry.GetGuid<T>(desc.Guid), out Material? material))
            {
                return;
            }

            material.Update(desc);
            material.BeginUpdate();

            material.Shader = (await manager.UpdateMaterialShaderAsync(material.Shader, desc))!;

            for (int i = 0; i < material.TextureList.Count; i++)
            {
                material.TextureList[i]?.Dispose();
            }
            material.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Count; i++)
            {
                material.TextureList.Add(await manager.LoadTextureAsync(desc.Textures[i]));
            }

            material.EndUpdate();
        }
    }
}