namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;

    public static class MaterialResourceFactoryExtensions
    {
        public static Material LoadMaterial(this ResourceManager1 manager, MeshData mesh, MaterialData desc, bool debone = true)
        {
            return manager.CreateInstance<Material, (MeshData, MaterialData, bool)>(desc.Name, (mesh, desc, debone)) ?? throw new NotSupportedException();
        }

        public static void UpdateMaterial(this ResourceManager1 manager, MaterialData desc)
        {
            if (!manager.TryGetInstance(desc.Name, out Material? modelMaterial))
            {
                return;
            }

            modelMaterial.Update(desc);
            modelMaterial.BeginUpdate();
            manager.UpdateMaterialShader(ref modelMaterial.Shader);

            for (int i = 0; i < modelMaterial.TextureList.Count; i++)
            {
                manager.UnloadTexture(modelMaterial.TextureList[i]);
            }
            modelMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                modelMaterial.TextureList.Add(manager.LoadTexture(desc.Textures[i]));
            }

            modelMaterial.EndUpdate();
        }

        public static void ReloadMaterial(this ResourceManager1 manager, Material modelMaterial)
        {
            var desc = modelMaterial.Desc;
            modelMaterial.Update(desc);
            modelMaterial.BeginUpdate();
            manager.UpdateMaterialShader(ref modelMaterial.Shader);

            for (int i = 0; i < modelMaterial.TextureList.Count; i++)
            {
                manager.UnloadTexture(modelMaterial.TextureList[i]);
            }
            modelMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                modelMaterial.TextureList.Add(manager.LoadTexture(desc.Textures[i]));
            }

            modelMaterial.EndUpdate();
        }

        public static void UnloadMaterial(this ResourceManager1 manager, Material modelMaterial)
        {
            manager.DestroyInstance(modelMaterial);
        }

        public static async Task UpdateMaterialAsync(this ResourceManager1 manager, MaterialData desc)
        {
            if (!manager.TryGetInstance(desc.Name, out Material? modelMaterial))
            {
                return;
            }

            modelMaterial.Update(desc);
            modelMaterial.BeginUpdate();

            modelMaterial.Shader = await manager.UpdateMaterialShaderAsync(modelMaterial.Shader);

            for (int i = 0; i < modelMaterial.TextureList.Count; i++)
            {
                manager.UnloadTexture(modelMaterial.TextureList[i]);
            }
            modelMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                modelMaterial.TextureList.Add(await manager.LoadTextureAsync(desc.Textures[i]));
            }

            modelMaterial.EndUpdate();
        }

        public static async Task ReloadMaterialAsync(this ResourceManager1 manager, Material modelMaterial)
        {
            var desc = modelMaterial.Desc;
            modelMaterial.Update(desc);
            modelMaterial.BeginUpdate();

            modelMaterial.Shader = await manager.UpdateMaterialShaderAsync(modelMaterial.Shader);

            for (int i = 0; i < modelMaterial.TextureList.Count; i++)
            {
                manager.UnloadTexture(modelMaterial.TextureList[i]);
            }
            modelMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                modelMaterial.TextureList.Add(await manager.LoadTextureAsync(desc.Textures[i]));
            }

            modelMaterial.EndUpdate();
        }
    }
}