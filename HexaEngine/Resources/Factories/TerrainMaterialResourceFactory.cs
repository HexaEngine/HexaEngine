namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Materials;

    public class TerrainMaterialResourceFactory : ResourceFactory<TerrainMaterial, MaterialData>
    {
        public TerrainMaterialResourceFactory(ResourceManager resourceManager) : base(resourceManager)
        {
        }

        protected override TerrainMaterial CreateInstance(ResourceManager manager, string name, MaterialData instanceData)
        {
            return new TerrainMaterial(this, instanceData);
        }

        protected override void LoadInstance(ResourceManager manager, TerrainMaterial instance, MaterialData instanceData)
        {
            for (int i = 0; i < instanceData.Textures.Length; i++)
            {
                instance.TextureList.Add(manager.LoadTexture(instanceData.Textures[i]));
            }

            instance.EndUpdate();
        }

        protected override async Task LoadInstanceAsync(ResourceManager manager, TerrainMaterial instance, MaterialData instanceData)
        {
            for (int i = 0; i < instanceData.Textures.Length; i++)
            {
                instance.TextureList.Add(await manager.LoadTextureAsync(instanceData.Textures[i]));
            }

            instance.EndUpdate();
        }

        protected override void UnloadInstance(ResourceManager manager, TerrainMaterial instance)
        {
            for (int i = 0; i < instance.TextureList.Count; i++)
            {
                instance.TextureList[i]?.Dispose();
            }
        }
    }
}