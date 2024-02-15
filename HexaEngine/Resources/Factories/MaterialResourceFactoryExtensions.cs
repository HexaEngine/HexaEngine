﻿namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;

    public static class MaterialResourceFactoryExtensions
    {
        public static Material LoadMaterial(this ResourceManager manager, MeshData mesh, MaterialData desc, bool debone = true)
        {
            return manager.CreateInstance<Material, (MeshData, MaterialData, bool)>(desc.Guid, (mesh, desc, debone)) ?? throw new NotSupportedException();
        }

        public static async Task<Material> LoadMaterialAsync(this ResourceManager manager, MeshData mesh, MaterialData desc, bool debone = true)
        {
            return await manager.CreateInstanceAsync<Material, (MeshData, MaterialData, bool)>(desc.Guid, (mesh, desc, debone)) ?? throw new NotSupportedException();
        }

        public static void UpdateMaterial(this ResourceManager manager, MaterialData? desc)
        {
            if (desc == null)
            {
                return;
            }

            if (!manager.TryGetInstance(desc.Guid, out Material? modelMaterial))
            {
                return;
            }

            modelMaterial.Update(desc);
            modelMaterial.BeginUpdate();
            manager.UpdateMaterialShader(modelMaterial.Shader, desc);

            for (int i = 0; i < modelMaterial.TextureList.Count; i++)
            {
                modelMaterial.TextureList[i]?.Dispose();
            }
            modelMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Count; i++)
            {
                modelMaterial.TextureList.Add(manager.LoadTexture(desc.Textures[i]));
            }

            modelMaterial.EndUpdate();
        }

        public static void ReloadMaterial(this ResourceManager manager, Material modelMaterial)
        {
            var desc = modelMaterial.Data;
            modelMaterial.Update(desc);
            modelMaterial.BeginUpdate();
            manager.UpdateMaterialShader(modelMaterial.Shader, desc);

            for (int i = 0; i < modelMaterial.TextureList.Count; i++)
            {
                modelMaterial.TextureList[i]?.Dispose();
            }
            modelMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Count; i++)
            {
                modelMaterial.TextureList.Add(manager.LoadTexture(desc.Textures[i]));
            }

            modelMaterial.EndUpdate();
        }

        public static async Task UpdateMaterialAsync(this ResourceManager manager, MaterialData desc)
        {
            if (!manager.TryGetInstance(desc.Guid, out Material? modelMaterial))
            {
                return;
            }

            modelMaterial.Update(desc);
            modelMaterial.BeginUpdate();

            modelMaterial.Shader = await manager.UpdateMaterialShaderAsync(modelMaterial.Shader, desc);

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

        public static async Task ReloadMaterialAsync(this ResourceManager manager, Material modelMaterial)
        {
            var desc = modelMaterial.Data;
            modelMaterial.Update(desc);
            modelMaterial.BeginUpdate();

            modelMaterial.Shader = await manager.UpdateMaterialShaderAsync(modelMaterial.Shader, desc);

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
    }
}