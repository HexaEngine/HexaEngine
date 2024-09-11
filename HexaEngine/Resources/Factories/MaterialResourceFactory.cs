namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Binary.Materials;
    using System.Threading.Tasks;

    public class MaterialResourceFactory : ResourceFactory<Material, (MaterialShaderDesc, MaterialData)>
    {
        public MaterialResourceFactory(ResourceManager resourceManager) : base(resourceManager)
        {
        }

        protected override Material CreateInstance(ResourceManager manager, ResourceGuid id, (MaterialShaderDesc, MaterialData) instanceData)
        {
            return new(this, instanceData.Item2, id);
        }

        protected override void LoadInstance(ResourceManager manager, Material instance, (MaterialShaderDesc, MaterialData) instanceData)
        {
            (MaterialShaderDesc shaderDesc, MaterialData desc) = instanceData;
            instance.Shader = manager.LoadMaterialShader(instance.Id.UsageType, shaderDesc);
            for (int i = 0; i < desc.Textures.Count; i++)
            {
                var texture = manager.LoadTexture(desc.Textures[i]);
                instance.TextureList.Add(texture);
            }

            instance.EndUpdate();
        }

        protected override async Task LoadInstanceAsync(ResourceManager manager, Material instance, (MaterialShaderDesc, MaterialData) instanceData)
        {
            (MaterialShaderDesc shaderDesc, MaterialData desc) = instanceData;
            instance.Shader = await manager.LoadMaterialShaderAsync(instance.Id.UsageType, shaderDesc);
            for (int i = 0; i < desc.Textures.Count; i++)
            {
                var texture = await manager.LoadTextureAsync(desc.Textures[i]);
                instance.TextureList.Add(texture);
            }

            instance.EndUpdate();
        }

        protected override void UnloadInstance(ResourceManager manager, Material instance)
        {
            for (int i = 0; i < instance.TextureList.Count; i++)
            {
                instance.TextureList[i]?.Dispose();
            }
            instance.Shader?.Dispose();
        }
    }
}