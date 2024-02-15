namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Binary.Materials;

    public static class TerrainMaterialResourceFactoryExtensions
    {
        public static TerrainMaterial LoadTerrainMaterial(this ResourceManager manager, MaterialData desc)
        {
            return manager.CreateInstance<TerrainMaterial, MaterialData>(desc.Guid, desc) ?? throw new NotSupportedException();
        }

        public static async Task<TerrainMaterial> LoadTerrainMaterialAsync(this ResourceManager manager, MaterialData desc)
        {
            return await manager.CreateInstanceAsync<TerrainMaterial, MaterialData>(desc.Guid, desc) ?? throw new NotSupportedException();
        }

        public static void UpdateTerrainMaterial(this ResourceManager manager, MaterialData desc)
        {
            if (!manager.TryGetInstance<TerrainMaterial>(desc.Guid, out var terrainMaterial))
            {
                return;
            }
            terrainMaterial.Update(desc);
            terrainMaterial.BeginUpdate();

            for (int i = 0; i < terrainMaterial.TextureList.Count; i++)
            {
                terrainMaterial.TextureList[i]?.Dispose();
            }
            terrainMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Count; i++)
            {
                terrainMaterial.TextureList.Add(manager.LoadTexture(desc.Textures[i]));
            }

            terrainMaterial.EndUpdate();
        }

        public static async Task UpdateTerrainMaterialAsync(this ResourceManager manager, MaterialData desc)
        {
            if (!manager.TryGetInstance<TerrainMaterial>(desc.Guid, out var terrainMaterial))
            {
                return;
            }
            terrainMaterial.Update(desc);
            terrainMaterial.BeginUpdate();

            for (int i = 0; i < terrainMaterial.TextureList.Count; i++)
            {
                terrainMaterial.TextureList[i]?.Dispose();
            }
            terrainMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Count; i++)
            {
                terrainMaterial.TextureList.Add(await manager.LoadTextureAsync(desc.Textures[i]));
            }

            terrainMaterial.EndUpdate();
        }
    }
}