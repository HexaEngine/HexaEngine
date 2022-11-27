namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Pipelines.Deferred;
    using HexaEngine.Pipelines.Deferred.PrePass;
    using HexaEngine.Rendering.ConstantBuffers;
    using HexaEngine.Resources;
    using HexaEngine.Scenes;
    using System;

    public class PrePass : IRenderPass
    {
        private readonly ResourceManager manager;
        private RenderTextureArray gbuffers;
        private IDepthStencilView dsv;
        private PrepassShader shader;

        public PrePass(ResourceManager manager)
        {
            this.manager = manager;
        }

        public string Name => "Pre Pass";

        public int Priority => 0;

        public event EventHandler? StateChanged;

        public void Initialize(IGraphicsDevice device, int width, int height, RenderPassCollection passes)
        {
            shader = new(device);
            shader.Constants.Add(new(passes.GetSharedBuffer<CBCamera>(), ShaderStage.Domain, 1));
            gbuffers = new(device, width, height, 8);
            passes.AddSharedShaderViews("GBUFFER", gbuffers.SRVs);
            dsv = passes.GetSharedDepthView("SWAPCHAIN");
        }

        public void Update(IGraphicsContext context, Scene scene)
        {
            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                if (manager.GetMesh(scene.Meshes[i], out var mesh))
                {
                    mesh.UpdateInstanceBuffer(context);
                }
            }
        }

        public unsafe void Draw(IGraphicsContext context, Scene scene, Viewport viewport)
        {
            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                if (manager.GetMesh(scene.Meshes[i], out var mesh))
                {
                    context.SetRenderTargets(gbuffers.RTVs, gbuffers.Count, dsv);
                    mesh.DrawAuto(context, shader, gbuffers.Viewport);
                }
            }
        }

        public void ResizeBegin()
        {
            gbuffers.Dispose();
        }

        public void ResizeEnd(IGraphicsDevice device, int width, int height, RenderPassCollection passes)
        {
            gbuffers = new(device, width, height, 8);
            passes.SetSharedShaderViews("GBUFFERS", gbuffers.SRVs);
        }
    }
}