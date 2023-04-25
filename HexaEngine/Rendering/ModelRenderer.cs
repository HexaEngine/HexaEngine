using HexaEngine.Core.Renderers;

namespace HexaEngine.Rendering
{
    using HexaEngine.Core.Culling;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;

    public class ModelRenderer : IRenderer
    {
        private InstanceManager instanceManager;
        private ResourceRef<IBuffer> camera;
        private readonly SemaphoreSlim semaphore = new(1);

        public uint QueueIndex => (uint)RenderQueueIndex.Geometry;

        public IInstance CreateInstance(MeshData data, MaterialLibrary library, GameObject parent)
        {
            semaphore.Wait();
            var material = ResourceManager.LoadMaterial(data, library.GetMaterial(data.MaterialName));
            var mesh = ResourceManager.LoadMesh(data);
            mesh.Wait();
            var instance = instanceManager.CreateInstance(mesh.Value.CreateInstanceType(mesh, material), parent);
            semaphore.Release();
            return instance;
        }

        public async Task<IInstance> CreateInstanceAsync(MeshData data, MaterialLibrary library, GameObject parent)
        {
            await semaphore.WaitAsync();
            var material = await ResourceManager.LoadMaterialAsync(data, library.GetMaterial(data.MaterialName));
            var mesh = await ResourceManager.LoadMeshAsync(data);
            mesh.Wait();
            var instance = await instanceManager.CreateInstanceAsync(mesh.Value.CreateInstanceType(mesh, material), parent);
            semaphore.Release();
            return instance;
        }

        public void DestroyInstance(IInstance instance)
        {
            semaphore.Wait();
            instanceManager.DestroyInstance(instance);
            semaphore.Release();
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