namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using System.Threading.Tasks;

    public class MaterialResourceFactory : ResourceFactory<Material, (MeshData, MaterialData, bool)>
    {
        public MaterialResourceFactory(ResourceManager resourceManager) : base(resourceManager)
        {
        }

        protected override Material CreateInstance(ResourceManager manager, string name, (MeshData, MaterialData, bool) instanceData)
        {
            return new(this, instanceData.Item2);
        }

        protected override void LoadInstance(ResourceManager manager, Material instance, (MeshData, MaterialData, bool) instanceData)
        {
            (MeshData mesh, MaterialData desc, bool debone) = instanceData;
            instance.Shader = manager.LoadMaterialShader(mesh, desc, debone);
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                instance.TextureList.Add(manager.LoadTexture(desc.Textures[i]));
            }

            instance.EndUpdate();
        }

        protected override async Task LoadInstanceAsync(ResourceManager manager, Material instance, (MeshData, MaterialData, bool) instanceData)
        {
            (MeshData mesh, MaterialData desc, bool debone) = instanceData;
            instance.Shader = await manager.LoadMaterialShaderAsync(mesh, desc, debone);
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                instance.TextureList.Add(await manager.LoadTextureAsync(desc.Textures[i]));
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