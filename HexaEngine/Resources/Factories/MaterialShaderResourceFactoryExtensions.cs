namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;

    public static class MaterialShaderResourceFactoryExtensions
    {
        public static ResourceInstance<Resources.MaterialShader>? LoadMaterialShader(this ResourceManager manager, MeshData mesh, MaterialData material, bool debone = false)
        {
            return manager.CreateInstance<ResourceInstance<Resources.MaterialShader>, (MeshData, MaterialData, bool)>(material.Name, (mesh, material, debone));
        }

        public static async Task<ResourceInstance<Resources.MaterialShader>?> LoadMaterialShaderAsync(this ResourceManager manager, MeshData mesh, MaterialData material, bool debone = false)
        {
            return await manager.CreateInstanceAsync<ResourceInstance<Resources.MaterialShader>, (MeshData, MaterialData, bool)>(material.Name, (mesh, material, debone));
        }

        public static void UpdateMaterialShader(this ResourceManager manager, ref ResourceInstance<Resources.MaterialShader>? shader)
        {
            if (shader == null || shader.Value == null)
            {
                return;
            }

            shader.Value.Reload();
        }

        public static async Task<ResourceInstance<Resources.MaterialShader>?> UpdateMaterialShaderAsync(this ResourceManager manager, ResourceInstance<Resources.MaterialShader>? pipeline)
        {
            if (pipeline == null || pipeline.Value == null)
            {
                return null;
            }

            await pipeline.Value.ReloadAsync();
            return pipeline;
        }

        public static void RecompileShaders(this ResourceManager manager)
        {
            var factory = manager.GetFactoryByResourceType<ResourceInstance<Resources.MaterialShader>, (MeshData, MaterialData, bool)>();
            if (factory == null)
            {
                return;
            }

            Logger.Info("recompiling material shaders ...");
            foreach (var shader in factory.Instances)
            {
                shader.Value?.Value?.Recompile();
            }
            Logger.Info("recompiling material shaders ...  done!");
        }
    }
}