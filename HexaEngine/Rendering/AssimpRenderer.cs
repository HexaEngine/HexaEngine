namespace HexaEngine.Rendering
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using Silk.NET.Assimp;
    using System.Collections.Generic;

    public unsafe class AssimpRenderer
    {
        private readonly Dictionary<Pointer<Mesh>, RenderMesh> meshes = new();
        private readonly Dictionary<Pointer<Material>, RenderMaterial> materials = new();
        private readonly Dictionary<Pointer<Texture>, RenderTexture> textures = new();

        private struct RenderMesh
        {
            public IBuffer VB;
            public IBuffer IB;
            public IBuffer ISB;
            public RenderMaterial Material;
        }

        private struct RenderMaterial
        {
            public RenderTexture[] Textures;
            public string Name;
        }

        private struct RenderTexture
        {
            public IShaderResourceView SRV;
            public ISamplerState Sampler;
        }

        public void Load(Scene* scene)
        {
        }

        public void UnloadLoad(Scene* oldScene, Scene* newScene)
        {
        }

        public void Unload(Scene* scene)
        {
        }
    }
}