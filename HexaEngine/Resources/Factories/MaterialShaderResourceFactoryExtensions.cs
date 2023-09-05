namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;

    public static class MaterialShaderResourceFactoryExtensions
    {
        public static ResourceInstance<Resources.MaterialShader>? LoadMaterialShader(this ResourceManager1 manager, MeshData mesh, MaterialData material, bool debone = false)
        {
            return manager.CreateInstance<ResourceInstance<Resources.MaterialShader>, (MeshData, MaterialData, bool)>(material.Name, (mesh, material, debone));
        }

        public static async Task<ResourceInstance<Resources.MaterialShader>?> LoadMaterialShaderAsync(this ResourceManager1 manager, MeshData mesh, MaterialData material, bool debone = false)
        {
            return await manager.CreateInstanceAsync<ResourceInstance<Resources.MaterialShader>, (MeshData, MaterialData, bool)>(material.Name, (mesh, material, debone));
        }

        public static void UnloadMaterialShader(this ResourceManager1 manager, ResourceInstance<Resources.MaterialShader>? shader)
        {
            manager.DestroyInstance(shader);
        }

        public static void UpdateMaterialShader(this ResourceManager1 manager, ref ResourceInstance<Resources.MaterialShader>? shader)
        {
            if (shader == null || shader.Value == null)
            {
                return;
            }

            shader.Value.Reload();
        }

        public static async Task<ResourceInstance<Resources.MaterialShader>?> UpdateMaterialShaderAsync(this ResourceManager1 manager, ResourceInstance<Resources.MaterialShader>? pipeline)
        {
            if (pipeline == null || pipeline.Value == null)
            {
                return null;
            }

            await pipeline.Value.ReloadAsync();
            return pipeline;
        }
    }
}