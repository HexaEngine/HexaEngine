#nullable disable

namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;

    public class ShadowMapPass : DrawPass
    {
        public ShadowMapPass() : base("ShadowMap")
        {
            AddWriteDependency(new("ShadowAtlas"));
        }

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device)
        {
            creator.CreateShadowAtlas("ShadowAtlas", new(Format.D32Float, 8192, 8));
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator)
        {
            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }

            var camera = CameraManager.Current;

            var renderers = scene.RenderManager;

            renderers.UpdateShadowMaps(context, camera);
        }
    }
}