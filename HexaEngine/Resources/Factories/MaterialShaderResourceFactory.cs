﻿namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using System.Threading.Tasks;

    public class MaterialShaderResourceFactory : ResourceFactory<ResourceInstance<Resources.MaterialShader>, (MeshData, MaterialData, bool)>
    {
        private readonly IGraphicsDevice device;

        public MaterialShaderResourceFactory(ResourceManager resourceManager, IGraphicsDevice device) : base(resourceManager)
        {
            this.device = device;
        }

        protected override ResourceInstance<Resources.MaterialShader> CreateInstance(ResourceManager manager, Guid id, (MeshData, MaterialData, bool) instanceData)
        {
            return new ResourceInstance<Resources.MaterialShader>(this, id);
        }

        protected override void LoadInstance(ResourceManager manager, ResourceInstance<Resources.MaterialShader> instance, (MeshData, MaterialData, bool) instanceData)
        {
            (MeshData mesh, MaterialData material, bool debone) = instanceData;
            var shader = new Resources.MaterialShader(device, mesh, material, debone);
            shader.Initialize();
            instance.EndLoad(shader);
        }

        protected override Task LoadInstanceAsync(ResourceManager manager, ResourceInstance<Resources.MaterialShader> instance, (MeshData, MaterialData, bool) instanceData)
        {
            (MeshData mesh, MaterialData material, bool debone) = instanceData;
            var shader = new Resources.MaterialShader(device, mesh, material, debone);
            shader.Initialize();
            instance.EndLoad(shader);
            return Task.CompletedTask;
        }

        protected override void UnloadInstance(ResourceManager manager, ResourceInstance<Resources.MaterialShader> instance)
        {
        }
    }
}