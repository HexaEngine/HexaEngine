namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Materials;

    public static class TerrainMaterialResourceFactoryExtensions
    {
        public static TerrainMaterial LoadTerrainMaterial(this ResourceManager1 manager, MaterialData desc)
        {
            return manager.CreateInstance<TerrainMaterial, MaterialData>(desc.Name, desc) ?? throw new NotSupportedException();
        }

        public static async Task<TerrainMaterial> LoadTerrainMaterialAsync(this ResourceManager1 manager, MaterialData desc)
        {
            return await manager.CreateInstanceAsync<TerrainMaterial, MaterialData>(desc.Name, desc) ?? throw new NotSupportedException();
        }

        public static void UpdateTerrainMaterial(this ResourceManager1 manager, MaterialData desc)
        {
            if (!manager.TryGetInstance<TerrainMaterial>(desc.Name, out var terrainMaterial))
            {
                return;
            }
            terrainMaterial.Update(desc);
            terrainMaterial.BeginUpdate();

            for (int i = 0; i < terrainMaterial.TextureList.Count; i++)
            {
                manager.UnloadTexture(terrainMaterial.TextureList[i]);
            }
            terrainMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                terrainMaterial.TextureList.Add(manager.LoadTexture(desc.Textures[i]));
            }

            terrainMaterial.EndUpdate();
        }

        public static async Task UpdateTerrainMaterialAsync(this ResourceManager1 manager, MaterialData desc)
        {
            if (!manager.TryGetInstance<TerrainMaterial>(desc.Name, out var terrainMaterial))
            {
                return;
            }
            terrainMaterial.Update(desc);
            terrainMaterial.BeginUpdate();

            for (int i = 0; i < terrainMaterial.TextureList.Count; i++)
            {
                manager.UnloadTexture(terrainMaterial.TextureList[i]);
            }
            terrainMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                terrainMaterial.TextureList.Add(await manager.LoadTextureAsync(desc.Textures[i]));
            }

            terrainMaterial.EndUpdate();
        }

        public static void UnloadTerrainMaterial(this ResourceManager1 manager, TerrainMaterial terrainMaterial)
        {
            manager.DestroyInstance(terrainMaterial);
        }
    }
}