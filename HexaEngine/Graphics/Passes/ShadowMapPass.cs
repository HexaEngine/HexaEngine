﻿namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Configuration;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Lights.Types;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;

    public class ShadowMapPass : DrawPass
    {
        private ResourceRef<ShadowAtlas> shadowAtlas;

        private ResourceRefNotNull<ConstantBuffer<PSMShadowParams>> psmBuffer;
        private ResourceRefNotNull<ConstantBuffer<CSMShadowParams>> csmBuffer;
        private ResourceRefNotNull<ConstantBuffer<DPSMShadowParams>> osmBuffer;

        public ShadowMapPass() : base("ShadowMap")
        {
            AddWriteDependency(new("ShadowAtlas"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            shadowAtlas = creator.CreateShadowAtlas("ShadowAtlas", new(Format.D32Float, GraphicsSettings.ShadowAtlasSize, 8));
            psmBuffer = new(creator.CreateConstantBuffer<PSMShadowParams>("ShadowAtlas.CB.PSM", CpuAccessFlags.Write));
            csmBuffer = new(creator.CreateConstantBuffer<CSMShadowParams>("ShadowAtlas.CB.CSM", CpuAccessFlags.Write));
            osmBuffer = new(creator.CreateConstantBuffer<DPSMShadowParams>("ShadowAtlas.CB.OSM", CpuAccessFlags.Write));
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
                        directionalLight.UpdateShadowMap(context, lights.ShadowDataBuffer, csmBuffer.Value, camera);
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
                                        renderer.DrawShadowMap(context, csmBuffer.Value, ShadowType.Cascaded);
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
                        for (int i = 0; i < 2; i++)
                        {
                            pointLight.UpdateShadowMap(context, lights.ShadowDataBuffer, osmBuffer.Value, i);
                            for (int j = 0; j < renderers.Count; j++)
                            {
                                var renderer = renderers[j];
                                profiler?.Begin($"ShadowMap.UpdatePoint.{renderer.DebugName}");
                                if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                                {
                                    if (renderer.BoundingBox.Intersects(pointLight.ShadowBox))
                                    {
                                        renderer.DrawShadowMap(context, osmBuffer.Value, ShadowType.Omni);
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
                        spotlight.UpdateShadowMap(context, lights.ShadowDataBuffer, psmBuffer.Value);
                        for (int i = 0; i < renderers.Count; i++)
                        {
                            var renderer = renderers[i];
                            profiler?.Begin($"ShadowMap.UpdateSpot.{renderer.DebugName}");
                            if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                            {
                                if (spotlight.IntersectFrustum(renderer.BoundingBox))
                                {
                                    renderer.DrawShadowMap(context, psmBuffer.Value, ShadowType.Perspective);
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