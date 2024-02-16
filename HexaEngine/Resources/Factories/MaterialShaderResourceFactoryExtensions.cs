namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO.Binary.Materials;

    public static class MaterialShaderResourceFactoryExtensions
    {
        public static Resources.MaterialShader? LoadMaterialShader(this ResourceManager manager, MaterialShaderDesc desc)
        {
            return manager.CreateInstance<Resources.MaterialShader, MaterialShaderDesc>(desc.MaterialId, desc);
        }

        public static async Task<Resources.MaterialShader?> LoadMaterialShaderAsync(this ResourceManager manager, MaterialShaderDesc desc)
        {
            return await manager.CreateInstanceAsync<Resources.MaterialShader, MaterialShaderDesc>(desc.MaterialId, desc);
        }

        public static void UpdateMaterialShader(this ResourceManager manager, Resources.MaterialShader? shader, MaterialShaderDesc desc)
        {
            if (shader == null)
            {
                return;
            }

            shader.Update(desc);
            shader.Reload();
        }

        public static void UpdateMaterialShader(this ResourceManager manager, Resources.MaterialShader? shader, MaterialData desc)
        {
            if (shader == null)
            {
                return;
            }

            shader.Update(desc.GetShaderMacros());
            shader.Reload();
        }

        public static async Task<Resources.MaterialShader?> UpdateMaterialShaderAsync(this ResourceManager manager, Resources.MaterialShader? shader, MaterialShaderDesc desc)
        {
            if (shader == null)
            {
                return null;
            }

            shader.Update(desc);
            await shader.ReloadAsync();
            return shader;
        }

        public static async Task<Resources.MaterialShader?> UpdateMaterialShaderAsync(this ResourceManager manager, Resources.MaterialShader? shader, MaterialData material)
        {
            if (shader == null)
            {
                return null;
            }

            shader.Update(material.GetShaderMacros());
            await shader.ReloadAsync();
            return shader;
        }

        public static void RecompileShaders(this ResourceManager manager)
        {
            var factory = manager.GetFactoryByResourceType<Resources.MaterialShader, MaterialShaderDesc>();
            if (factory == null)
            {
                return;
            }

            Logger.Info("recompiling material shaders ...");
            foreach (var shader in factory.Instances)
            {
                shader.Value?.Recompile();
            }
            Logger.Info("recompiling material shaders ...  done!");
        }
    }
}