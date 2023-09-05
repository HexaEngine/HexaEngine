namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using System.Threading.Tasks;

    public class MaterialResourceFactory : ResourceFactory<Material, (MeshData, MaterialData, bool)>
    {
        protected override Material CreateInstance(ResourceManager1 manager, string name, (MeshData, MaterialData, bool) instanceData)
        {
            return new(instanceData.Item2);
        }

        protected override void LoadInstance(ResourceManager1 manager, Material instance, (MeshData, MaterialData, bool) instanceData)
        {
            (MeshData mesh, MaterialData desc, bool debone) = instanceData;
            instance.Shader = manager.LoadMaterialShader(mesh, desc, debone);
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                instance.TextureList.Add(manager.LoadTexture(desc.Textures[i]));
            }

            instance.EndUpdate();
        }

        protected override async Task LoadInstanceAsync(ResourceManager1 manager, Material instance, (MeshData, MaterialData, bool) instanceData)
        {
            (MeshData mesh, MaterialData desc, bool debone) = instanceData;
            instance.Shader = await manager.LoadMaterialShaderAsync(mesh, desc, debone);
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                instance.TextureList.Add(await manager.LoadTextureAsync(desc.Textures[i]));
            }

            instance.EndUpdate();
        }

        protected override void UnloadInstance(ResourceManager1 manager, Material instance)
        {
            manager.UnloadMaterialShader(instance.Shader);
            for (int i = 0; i < instance.TextureList.Count; i++)
            {
                manager.UnloadTexture(instance.TextureList[i]);
            }
        }
    }
}