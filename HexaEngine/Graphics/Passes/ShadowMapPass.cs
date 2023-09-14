namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;

    public class ShadowMapPass : DrawPass
    {
        private ResourceRef<ShadowAtlas> shadowAtlas;

        public ShadowMapPass() : base("ShadowMap")
        {
            AddWriteDependency(new("ShadowAtlas"));
        }

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device, ICPUProfiler? profiler)
        {
            shadowAtlas = creator.CreateShadowAtlas("ShadowAtlas", new(Format.D32Float, 8192, 8));
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }

            var camera = CameraManager.Current;

            var renderers = scene.RenderManager.Renderers;
            var lights = scene.LightManager;

            profiler?.Begin("ShadowMap.UpdateBuffer");
            lights.ShadowDataBuffer.Update(context);
            profiler?.End("ShadowMap.UpdateBuffer");

            if (lights.UpdateShadowLightQueue.Count == 0)
                return;

            profiler?.Begin("ShadowMap.UpdateAtlas");
            while (lights.UpdateShadowLightQueue.TryDequeue(out var light))
            {
                switch (light.LightType)
                {
                    case LightType.Directional:
                        profiler?.Begin("ShadowMap.UpdateDirectional");
                        var directionalLight = (DirectionalLight)light;
                        directionalLight.UpdateShadowMap(context, lights.ShadowDataBuffer, camera);
                        for (int i = 0; i < renderers.Count; i++)
                        {
                            var renderer = renderers[i];
                            profiler?.Begin($"ShadowMap.UpdateDirectional.{renderer.DebugName}");
                            if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                            {
                                for (int j = 0; j < directionalLight.ShadowFrustra.Length; j++)
                                {
                                    if (directionalLight.ShadowFrustra[j].Intersects(renderer.BoundingBox))
                                    {
                                        renderer.DrawShadowMap(context, DirectionalLight.CSMBuffer, ShadowType.Cascaded);
                                        break;
                                    }
                                }
                            }
                            profiler?.End($"ShadowMap.UpdateDirectional.{renderer.DebugName}");
                        }
                        profiler?.End("ShadowMap.UpdateDirectional");
                        break;

                    case LightType.Point:
                        profiler?.Begin("ShadowMap.UpdatePoint");
                        var pointLight = (PointLight)light;
                        for (int i = 0; i < 6; i++)
                        {
                            pointLight.UpdateShadowMap(context, lights.ShadowDataBuffer, i);
                            for (int j = 0; j < renderers.Count; j++)
                            {
                                var renderer = renderers[j];
                                profiler?.Begin($"ShadowMap.UpdatePoint.{renderer.DebugName}");
                                if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                                {
                                    if (renderer.BoundingBox.Intersects(pointLight.ShadowBox))
                                    {
                                        renderer.DrawShadowMap(context, PointLight.OSMBuffer, ShadowType.Omni);
                                    }
                                }
                                profiler?.End($"ShadowMap.UpdatePoint.{renderer.DebugName}");
                            }
                        }
                        profiler?.End("ShadowMap.UpdatePoint");
                        break;

                    case LightType.Spot:
                        profiler?.Begin("ShadowMap.UpdateSpot");
                        var spotlight = (Spotlight)light;
                        spotlight.UpdateShadowMap(context, lights.ShadowDataBuffer);
                        for (int i = 0; i < renderers.Count; i++)
                        {
                            var renderer = renderers[i];
                            profiler?.Begin($"ShadowMap.UpdateSpot.{renderer.DebugName}");
                            if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                            {
                                if (spotlight.ShadowFrustum.Intersects(renderer.BoundingBox))
                                {
                                    renderer.DrawShadowMap(context, Spotlight.PSMBuffer, ShadowType.Perspective);
                                }
                            }
                            profiler?.End($"ShadowMap.UpdateSpot.{renderer.DebugName}");
                        }
                        profiler?.End("ShadowMap.UpdateSpot");
                        break;
                }
                light.InUpdateQueue = false;
            }
            profiler?.End("ShadowMap.UpdateAtlas");
        }
    }
}