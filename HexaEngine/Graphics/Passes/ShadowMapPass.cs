namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Configuration;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Effects;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Lights.Types;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public class ShadowMapPass : DrawPass
    {
        private ResourceRef<ShadowAtlas> shadowAtlas;

        private ResourceRefNotNull<ConstantBuffer<PSMShadowParams>> psmBuffer;
        private ResourceRefNotNull<ConstantBuffer<CSMShadowParams>> csmBuffer;
        private ResourceRefNotNull<ConstantBuffer<DPSMShadowParams>> osmBuffer;

        private GaussianBlur blurFilter;
        private CopyEffect copyEffect;

        public ShadowMapPass() : base("ShadowMap")
        {
            AddWriteDependency(new("ShadowAtlas"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            shadowAtlas = creator.CreateShadowAtlas("ShadowAtlas", new(GraphicsSettings.ShadowMapFormat, GraphicsSettings.ShadowAtlasSize, 8, 1));
            psmBuffer = new(creator.CreateConstantBuffer<PSMShadowParams>("ShadowAtlas.CB.PSM", CpuAccessFlags.Write));
            csmBuffer = new(creator.CreateConstantBuffer<CSMShadowParams>("ShadowAtlas.CB.CSM", CpuAccessFlags.Write));
            osmBuffer = new(creator.CreateConstantBuffer<DPSMShadowParams>("ShadowAtlas.CB.OSM", CpuAccessFlags.Write));
            blurFilter = new(creator, "GaussianBlur", Format.R32G32Float, 1, 1);
            copyEffect = new(creator, "CopyPass", CopyFilter.None);
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
            {
                return;
            }

            profiler?.Begin("ShadowMap.UpdateAtlas");
            while (lights.UpdateShadowLightQueue.TryDequeue(out var light))
            {
                switch (light.LightType)
                {
                    case LightType.Directional:
                        DoDirectional(context, profiler, camera, renderers, lights, light);
                        break;

                    case LightType.Point:
                        DoPoint(context, profiler, renderers, lights, light);
                        break;

                    case LightType.Spot:
                        DoSpot(context, profiler, renderers, lights, light);
                        break;
                }
                light.InUpdateQueue = false;
            }
            profiler?.End("ShadowMap.UpdateAtlas");
        }

        private Texture2D tex;
        private DepthStencil depth;

        private void Filter(IGraphicsContext context, Texture2D source)
        {
            if (GraphicsSettings.SoftShadowMode < SoftShadowMode.ESM)
            {
                return;
            }

            if (source.Width != blurFilter.Width || source.Height != blurFilter.Height || source.Format != blurFilter.Format)
            {
                blurFilter.Resize(source.Format, source.Width, source.Height);
            }

            blurFilter.Blur(context, source.SRV, source.RTV, source.Width, source.Height);
        }

        private void FilterArray(IGraphicsContext context, Texture2D source)
        {
            if (source.Width != blurFilter.Width || source.Height != blurFilter.Height || source.Format != blurFilter.Format)
            {
                blurFilter.Resize(source.Format, source.Width, source.Height);
            }

            for (int i = 0; i < source.ArraySize; i++)
            {
                blurFilter.Blur(context, source.SRVArraySlices[i], source.RTVArraySlices[i], source.Width, source.Height);
            }
        }

        private void Copy(IGraphicsContext context, Texture2D source, Viewport sourceViewport, Viewport destinationRect)
        {
            copyEffect.Copy(context, source.SRV, shadowAtlas.Value.RTV, sourceViewport, destinationRect);
        }

        private void PrepareBuffers(IGraphicsContext context, Light light)
        {
            if (tex == null)
            {
                tex = new(context.Device, GraphicsSettings.ShadowMapFormat, light.ShadowMapSize, light.ShadowMapSize, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
                depth = new(context.Device, Format.D32Float, light.ShadowMapSize, light.ShadowMapSize, CpuAccessFlags.None, GpuAccessFlags.Write);
            }
            else if (tex.Format != GraphicsSettings.ShadowMapFormat || tex.Width != light.ShadowMapSize || tex.Height != light.ShadowMapSize)
            {
                tex.Resize(context.Device, GraphicsSettings.ShadowMapFormat, light.ShadowMapSize, light.ShadowMapSize, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
                depth.Resize(context.Device, light.ShadowMapSize, light.ShadowMapSize);
            }
        }

        private void DoSpot(IGraphicsContext context, ICPUProfiler? profiler, IReadOnlyList<IRendererComponent> renderers, LightManager lights, LightSource light)
        {
            profiler?.Begin("ShadowMap.UpdateSpot");
            var spotlight = (Spotlight)light;

            PrepareBuffers(context, spotlight);

            context.ClearRenderTargetView(tex.RTV, Vector4.One);
            context.ClearDepthStencilView(depth, DepthStencilClearFlags.All, 1, 0);

            var destinationViewport = spotlight.UpdateShadowMap(context, lights.ShadowDataBuffer, psmBuffer.Value);
            Viewport sourceViewport = tex.Viewport;

            context.SetRenderTarget(tex.RTV, depth.DSV);
            context.SetViewport(sourceViewport);

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

            Filter(context, tex);
            Copy(context, tex, sourceViewport, destinationViewport);

            profiler?.End("ShadowMap.UpdateSpot");
        }

        private void DoPoint(IGraphicsContext context, ICPUProfiler? profiler, IReadOnlyList<IRendererComponent> renderers, LightManager lights, LightSource light)
        {
            profiler?.Begin("ShadowMap.UpdatePoint");
            var pointLight = (PointLight)light;
            for (int i = 0; i < 2; i++)
            {
                PrepareBuffers(context, pointLight);

                context.ClearRenderTargetView(tex.RTV, Vector4.One);
                context.ClearDepthStencilView(depth, DepthStencilClearFlags.All, 1, 0);

                var destinationViewport = pointLight.UpdateShadowMap(context, lights.ShadowDataBuffer, osmBuffer.Value, i);
                Viewport sourceViewport = tex.Viewport;

                context.SetRenderTarget(tex.RTV, depth.DSV);
                context.SetViewport(sourceViewport);

                for (int j = 0; j < renderers.Count; j++)
                {
                    var renderer = renderers[j];
                    profiler?.Begin($"ShadowMap.UpdatePoint.{renderer.DebugName}");
                    if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                    {
                        if (renderer.BoundingBox.Intersects(pointLight.ShadowBox))
                        {
                            renderer.DrawShadowMap(context, osmBuffer.Value, ShadowType.Omnidirectional);
                        }
                    }
                    profiler?.End($"ShadowMap.UpdatePoint.{renderer.DebugName}");
                }

                Filter(context, tex);
                Copy(context, tex, sourceViewport, destinationViewport);
            }
            profiler?.End("ShadowMap.UpdatePoint");
        }

        private void DoDirectional(IGraphicsContext context, ICPUProfiler? profiler, Camera? camera, IReadOnlyList<IRendererComponent> renderers, LightManager lights, LightSource light)
        {
            profiler?.Begin("ShadowMap.UpdateDirectional");
            var directionalLight = (DirectionalLight)light;
            directionalLight.UpdateShadowMap(context, lights.ShadowDataBuffer, csmBuffer.Value, camera);
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                profiler?.Begin($"ShadowMap.UpdateDirectional.{renderer.DebugName}");
                if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                {
                    for (int j = 0; j < directionalLight.ShadowFrustra.Count; j++)
                    {
                        if (directionalLight.ShadowFrustra[j].Intersects(renderer.BoundingBox))
                        {
                            renderer.DrawShadowMap(context, csmBuffer.Value, ShadowType.Directional);
                            break;
                        }
                    }
                }
                profiler?.End($"ShadowMap.UpdateDirectional.{renderer.DebugName}");
            }
            var map = directionalLight.GetMap();
            if (map != null)
            {
                FilterArray(context, map);
            }

            profiler?.End("ShadowMap.UpdateDirectional");
        }

        public override void Release()
        {
            copyEffect.Dispose();
            blurFilter.Dispose();
            tex?.Dispose();
            depth?.Dispose();
        }
    }
}