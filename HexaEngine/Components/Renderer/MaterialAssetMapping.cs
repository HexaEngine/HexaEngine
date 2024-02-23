namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.IO.Binary.Meshes;

    public struct MaterialAssetMapping
    {
        public string Mesh;
        public AssetRef Material;

        public MaterialAssetMapping(MeshData mesh, AssetRef material)
        {
            Mesh = mesh.Name;
            Material = material;
        }
    }
}