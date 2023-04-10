namespace HexaEngine.Core.Renderers
{
    using HexaEngine.Core.Culling;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;

    public class ModelRenderer : IRenderer
    {
        private readonly DrawQueue queue = new();
        private InstanceManager instanceManager;
        private ResourceRef<IBuffer> camera;

        public uint QueueIndex => (uint)RenderQueueIndex.Geometry;

        public IInstance CreateInstance(MeshData data, GameObject parent)
        {
            var material = ResourceManager.LoadMaterial(data, MaterialManager.GetMaterial(data.Material.Name) ?? data.Material);
            var mesh = ResourceManager.LoadMesh(data);
            mesh.Wait();
            return instanceManager.CreateInstance(mesh.Value.CreateInstanceType(mesh, material), parent);
        }

        public async Task<IInstance> CreateInstanceAsync(MeshData data, GameObject parent)
        {
            var material = await ResourceManager.LoadMaterialAsync(data, MaterialManager.GetMaterial(data.Material.Name) ?? data.Material);
            var mesh = await ResourceManager.LoadMeshAsync(data);
            mesh.Wait();
            return await instanceManager.CreateInstanceAsync(mesh.Value.CreateInstanceType(mesh, material), parent);
        }

        public void DestroyInstance(IInstance instance)
        {
            instanceManager.DestroyInstance(instance);
        }

        public void Initialize(IGraphicsDevice device, InstanceManager instanceManager)
        {
            this.instanceManager = instanceManager;
            camera = ResourceManager2.Shared.GetBuffer("CBCamera");
        }

        public void Uninitialize()
        {
        }

        public void Draw(IGraphicsContext context)
        {
            if (camera.Value == null)
                return;
            instanceManager.Draw(context, camera.Value);
            context.ClearState();
        }

        public void DrawDepth(IGraphicsContext context)
        {
        }

        public void DrawIndirect(IGraphicsContext context, IBuffer drawArgs, uint offset)
        {
        }

        public void VisibilityTest(IGraphicsContext context)
        {
            instanceManager.DrawDepth(context, CullingManager.OcclusionCameraBuffer.Buffer);
        }
    }
}