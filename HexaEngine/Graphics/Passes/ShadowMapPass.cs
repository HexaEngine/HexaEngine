namespace HexaEngine.Graphics.Passes
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Configuration;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Effects;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Lights.Types;
    using HexaEngine.Profiling;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public class ShadowMapPass : DrawPass
    {
        private ResourceRef<ShadowAtlas> shadowAtlas;

        private ResourceRef<ConstantBuffer<PSMShadowParams>> psmBuffer;
        private ResourceRef<ConstantBuffer<CSMShadowParams>> csmBuffer;
        private ResourceRef<ConstantBuffer<DPSMShadowParams>> osmBuffer;

        private GaussianBlur blurFilter;
        private CopyEffect copyEffect;
        private ReprojectEffect reprojectEffect;
        private ClearSliceEffect clearSliceEffect;

        public ShadowMapPass() : base("ShadowMap")
        {
            AddWriteDependency(new("ShadowAtlas"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            shadowAtlas = creator.CreateShadowAtlas("ShadowAtlas", new(GraphicsSettings.ShadowMapFormat, GraphicsSettings.ShadowAtlasSize, 8, 1));
            psmBuffer = creator.CreateConstantBuffer<PSMShadowParams>("ShadowAtlas.CB.PSM", CpuAccessFlags.Write);
            csmBuffer = creator.CreateConstantBuffer<CSMShadowParams>("ShadowAtlas.CB.CSM", CpuAccessFlags.Write);
            osmBuffer = creator.CreateConstantBuffer<DPSMShadowParams>("ShadowAtlas.CB.OSM", CpuAccessFlags.Write);
            blurFilter = new(creator, "GaussianBlur", Format.R32G32Float, 1, 1);
            copyEffect = new(creator, "CopyPass", CopyFilter.None);
            clearSliceEffect = new(creator);
            reprojectEffect = new(creator);
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }

            var camera = CameraManager.Current;

            var drawables = scene.RenderManager.Drawables;
            var tree = scene.RenderManager.DrawablesTree;
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
                        DoDirectional(context, profiler, camera, tree, lights, light);
                        break;

                    case LightType.Point:
                        DoPoint(context, profiler, tree, lights, light);
                        break;

                    case LightType.Spot:
                        DoSpot(context, profiler, tree, lights, light);
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

        private void FilterArray(IGraphicsContext context, Texture2D source, uint cascadeMask, CSMShadowParams old, CSMShadowParams now, bool reproject)
        {
            if (source.Width != blurFilter.Width || source.Height != blurFilter.Height || source.Format != blurFilter.Format)
            {
                blurFilter.Resize(source.Format, source.Width, source.Height);
            }

            for (int i = 0; i < source.ArraySize; i++)
            {
                if ((cascadeMask & (1 << i)) != 0)
                {
                    blurFilter.Blur(context, source.SRVArraySlices[i], source.RTVArraySlices[i], source.Width, source.Height);
                }
                else if (reproject)
                {
                    Reproject(context, source, i, old, now);
                    blurFilter.Blur(context, source.SRVArraySlices[i], source.RTVArraySlices[i], source.Width, source.Height);
                }
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
                tex = new(GraphicsSettings.ShadowMapFormat, light.ShadowMapSize, light.ShadowMapSize, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
                depth = new(Format.D32Float, light.ShadowMapSize, light.ShadowMapSize, CpuAccessFlags.None, GpuAccessFlags.Write);
            }
            else if (tex.Format != GraphicsSettings.ShadowMapFormat || tex.Width != light.ShadowMapSize || tex.Height != light.ShadowMapSize)
            {
                tex.Resize(GraphicsSettings.ShadowMapFormat, light.ShadowMapSize, light.ShadowMapSize, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
                depth.Resize(light.ShadowMapSize, light.ShadowMapSize);
            }
        }

        private readonly Stack<int> stack = [];

        private void DoSpot(IGraphicsContext context, ICPUProfiler? profiler, BVHTree<IDrawable> tree, LightManager lights, LightSource light)
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
            foreach (var node in tree.Enumerate(SpotFilter, stack, spotlight.ShadowFrustum))
            {
                var drawable = node.Value;
                profiler?.Begin($"ShadowMap.UpdateSpot.{drawable.DebugName}");
                if ((drawable.Flags & RendererFlags.CastShadows) != 0)
                {
                    drawable.DrawShadowMap(context, psmBuffer.Value, ShadowType.Perspective);
                }
                profiler?.End($"ShadowMap.UpdateSpot.{drawable.DebugName}");
            }

            Filter(context, tex);
            Copy(context, tex, sourceViewport, destinationViewport);

            profiler?.End("ShadowMap.UpdateSpot");
        }

        private static BVHFilterResult SpotFilter(BVHNode<IDrawable> node, BoundingFrustum frustum)
        {
            if (node.Box.Intersects(frustum))
            {
                return BVHFilterResult.Keep;
            }
            return BVHFilterResult.Skip;
        }

        private void DoPoint(IGraphicsContext context, ICPUProfiler? profiler, BVHTree<IDrawable> tree, LightManager lights, LightSource light)
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

                foreach (var node in tree.Enumerate(PointFilter, stack, pointLight.ShadowBox))
                {
                    var drawable = node.Value;
                    profiler?.Begin($"ShadowMap.UpdatePoint.{drawable.DebugName}");
                    if ((drawable.Flags & RendererFlags.CastShadows) != 0)
                    {
                        drawable.DrawShadowMap(context, osmBuffer.Value, ShadowType.Omnidirectional);
                    }
                    profiler?.End($"ShadowMap.UpdatePoint.{drawable.DebugName}");
                }

                Filter(context, tex);
                Copy(context, tex, sourceViewport, destinationViewport);
            }
            profiler?.End("ShadowMap.UpdatePoint");
        }

        private static BVHFilterResult PointFilter(BVHNode<IDrawable> node, BoundingBox box)
        {
            if (node.Box.Intersects(box))
            {
                return BVHFilterResult.Keep;
            }
            return BVHFilterResult.Skip;
        }

        private void DoDirectional(IGraphicsContext context, ICPUProfiler? profiler, Camera? camera, BVHTree<IDrawable> tree, LightManager lights, LightSource light)
        {
            var old = csmBuffer.Value![0];
            profiler?.Begin("ShadowMap.UpdateDirectional");
            var directionalLight = (DirectionalLight)light;
            if (!directionalLight.UpdateShadowMap(context, lights.ShadowDataBuffer, csmBuffer.Value!, camera!, out uint cascadeMask, out var reproject))
            {
                return; // false return means nothing to update.
            }

            var map = directionalLight.GetMap()!;
            var csmDepthBuffer = directionalLight.GetDepthStencil()!;

            clearSliceEffect.Clear(context, map.UAV!, (uint)map.Width, (uint)map.Height, (uint)map.ArraySize, cascadeMask);
            context.ClearDepthStencilView(csmDepthBuffer.DSV, DepthStencilClearFlags.All, 1, 0);

            context.SetRenderTarget(map.RTV, csmDepthBuffer.DSV);
            context.SetViewport(map.Viewport);

            var now = csmBuffer.Value![0];

            foreach (var node in tree.Enumerate(CascadeFilter, stack, (directionalLight.ShadowFrustra, cascadeMask)))
            {
                var drawable = node.Value;
                profiler?.Begin($"ShadowMap.UpdateDirectional.{drawable.DebugName}");
                if ((drawable.Flags & RendererFlags.CastShadows) != 0)
                {
                    drawable.DrawShadowMap(context, csmBuffer.Value, ShadowType.Directional);
                }
                profiler?.End($"ShadowMap.UpdateDirectional.{drawable.DebugName}");
            }

            FilterArray(context, map, cascadeMask, old, now, reproject);

            profiler?.End("ShadowMap.UpdateDirectional");
        }

        private static BVHFilterResult CascadeFilter(BVHNode<IDrawable> node, (IReadOnlyList<BoundingFrustum> frusta, uint mask) args)
        {
            (IReadOnlyList<BoundingFrustum> frusta, uint mask) = args;
            for (int i = 0; i < frusta.Count; i++)
            {
                if ((mask & (1u << i)) != 0 && frusta[i].Intersects(node.Box))
                {
                    return BVHFilterResult.Keep;
                }
            }
            return BVHFilterResult.Skip;
        }

        private unsafe void Reproject(IGraphicsContext context, Texture2D texture, int slice, CSMShadowParams old, CSMShadowParams now)
        {
            var oldViewProj = Matrix4x4.Transpose(old[slice]);
            var newViewProj = now[slice];

            Matrix4x4.Invert(oldViewProj, out var oldViewProjInv);
            oldViewProjInv = Matrix4x4.Transpose(oldViewProjInv);

            reprojectEffect.Reproject(context, texture.UAVArraySlices![slice], (uint)texture.Width, (uint)texture.Height, oldViewProjInv, newViewProj, ReprojectFlags.VSM);
        }

        public override void Release()
        {
            clearSliceEffect.Dispose();
            reprojectEffect.Dispose();
            copyEffect.Dispose();
            blurFilter.Dispose();
            tex?.Dispose();
            depth?.Dispose();
        }
    }
}