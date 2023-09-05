namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Materials;

    public class TerrainMaterialResourceFactory : ResourceFactory<TerrainMaterial, MaterialData>
    {
        protected override TerrainMaterial CreateInstance(ResourceManager1 manager, string name, MaterialData instanceData)
        {
            return new TerrainMaterial(instanceData);
        }

        protected override void LoadInstance(ResourceManager1 manager, TerrainMaterial instance, MaterialData instanceData)
        {
            for (int i = 0; i < instanceData.Textures.Length; i++)
            {
                instance.TextureList.Add(manager.LoadTexture(instanceData.Textures[i]));
            }

            instance.EndUpdate();
        }

        protected override async Task LoadInstanceAsync(ResourceManager1 manager, TerrainMaterial instance, MaterialData instanceData)
        {
            for (int i = 0; i < instanceData.Textures.Length; i++)
            {
                instance.TextureList.Add(await manager.LoadTextureAsync(instanceData.Textures[i]));
            }

            instance.EndUpdate();
        }

        protected override void UnloadInstance(ResourceManager1 manager, TerrainMaterial instance)
        {
            for (int i = 0; i < instance.TextureList.Count; i++)
            {
                manager.UnloadTexture(instance.TextureList[i]);
            }
            instance.Dispose();
        }
    }
}