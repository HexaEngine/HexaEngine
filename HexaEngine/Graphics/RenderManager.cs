namespace HexaEngine.Graphics
{
    using HexaEngine.Collections;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;
    using HexaEngine.Mathematics;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class RenderManager : ISystem
    {
        private readonly IGraphicsDevice device;
        private readonly FlaggedList<RendererFlags, IRendererComponent> rendererComponents = new();
        private readonly ComponentTypeQuery<IRendererComponent> renderers = new();
        private readonly List<IRendererComponent> backgroundQueue = new();
        private readonly List<IRendererComponent> geometryQueue = new();
        private readonly List<IRendererComponent> alphaTestQueue = new();
        private readonly List<IRendererComponent> geometryLastQueue = new();
        private readonly List<IRendererComponent> transparencyQueue = new();
        private readonly List<IRendererComponent> overlayQueue = new();
        private readonly SortRendererAscending comparer = new();
        private readonly LightManager lights;
        private readonly CullingManager culling;

        private bool isLoaded;

        public string Name => "Renderers";

        public SystemFlags Flags => SystemFlags.Awake | SystemFlags.Destroy | SystemFlags.Load | SystemFlags.Unload;

        public RenderManager(IGraphicsDevice device, LightManager lights)
        {
            this.device = device;
            renderers.OnAdded += RendererOnAdded;
            renderers.OnRemoved += RendererOnRemoved;
            this.lights = lights;
            culling = new(device);
        }

        public IReadOnlyList<IRendererComponent> Renderers => renderers;

        public IReadOnlyList<IRendererComponent> BackgroundQueue => backgroundQueue;

        public IReadOnlyList<IRendererComponent> GeometryQueue => geometryQueue;

        public IReadOnlyList<IRendererComponent> AlphaTestQueue => alphaTestQueue;

        public IReadOnlyList<IRendererComponent> GeometryLastQueue => geometryLastQueue;

        public IReadOnlyList<IRendererComponent> TransparencyQueue => transparencyQueue;

        public IReadOnlyList<IRendererComponent> OverlayQueue => overlayQueue;

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(renderers);
        }

        public void Load(IGraphicsDevice device)
        {
            if (!isLoaded)
            {
                Parallel.For(0, renderers.Count, i =>
                {
                    renderers[i].Load(device);
                });

                isLoaded = true;
            }
        }

        public void Unload()
        {
            if (isLoaded)
            {
                for (int i = 0; i < renderers.Count; i++)
                {
                    renderers[i].Unload();
                }
                isLoaded = false;
            }
        }

        public void VisibilityTest(IGraphicsContext context, Viewport viewport, IShaderResourceView depthMip, RenderQueueIndex index)
        {
            culling.UpdateCamera(context, viewport);
            switch (index)
            {
                case RenderQueueIndex.Geometry:
                    VisibilityTestList(culling.Context, geometryQueue);
                    break;
            }
            culling.DoCulling(context, depthMip);
        }

        public void DrawDepth(IGraphicsContext context, RenderQueueIndex index)
        {
            if ((index & RenderQueueIndex.Background) != 0)
            {
                DrawDepthList(context, backgroundQueue);
            }
            if ((index & RenderQueueIndex.Geometry) != 0)
            {
                DrawDepthList(context, geometryQueue);
            }
            if ((index & RenderQueueIndex.AlphaTest) != 0)
            {
                DrawDepthList(context, alphaTestQueue);
            }
            if ((index & RenderQueueIndex.Transparency) != 0)
            {
                DrawDepthList(context, transparencyQueue);
            }
        }

        public void Draw(IGraphicsContext context, RenderQueueIndex index, RenderPath path)
        {
            switch (index)
            {
                case RenderQueueIndex.Background:
                    DrawList(context, backgroundQueue, path);
                    break;

                case RenderQueueIndex.Geometry:
                    DrawList(context, geometryQueue, path);
                    break;
            }
        }

        private static void DrawDepthList(IGraphicsContext context, List<IRendererComponent> renderers)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].DrawDepth(context);
            }
        }

        private static void DrawList(IGraphicsContext context, List<IRendererComponent> renderers, RenderPath path)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].Draw(context, path);
            }
        }

        private static void VisibilityTestList(CullingContext context, List<IRendererComponent> renderers)
        {
            var cam = CameraManager.Current;
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].VisibilityTest(context);
            }
        }

        private void GameObjectTransformed(GameObject obj)
        {
            lights.RendererUpdateQueue.EnqueueComponentIfIs(obj);
        }

        public void Update(IGraphicsContext context)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].Update(context);
            }
        }

        public void UpdateShadowMaps(IGraphicsContext context, Camera camera)
        {
            if (lights.UpdateShadowLightQueue.Count == 0)
                return;
            while (lights.UpdateShadowLightQueue.TryDequeue(out var light))
            {
                switch (light.LightType)
                {
                    case LightType.Directional:
                        var directionalLight = (DirectionalLight)light;
                        directionalLight.UpdateShadowMap(context, lights.ShadowDataBuffer, camera);
                        for (int i = 0; i < renderers.Count; i++)
                        {
                            var renderer = renderers[i];
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
                        }
                        break;

                    case LightType.Point:
                        var pointLight = (PointLight)light;
                        for (int i = 0; i < 6; i++)
                        {
                            pointLight.UpdateShadowMap(context, lights.ShadowDataBuffer, i);
                            for (int j = 0; j < renderers.Count; j++)
                            {
                                var renderer = renderers[j];
                                if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                                {
                                    if (renderer.BoundingBox.Intersects(pointLight.ShadowBox))
                                    {
                                        renderer.DrawShadowMap(context, PointLight.OSMBuffer, ShadowType.Omni);
                                    }
                                }
                            }
                        }

                        break;

                    case LightType.Spot:
                        var spotlight = (Spotlight)light;
                        spotlight.UpdateShadowMap(context, lights.ShadowDataBuffer);
                        for (int i = 0; i < renderers.Count; i++)
                        {
                            var renderer = renderers[i];
                            if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                            {
                                if (spotlight.ShadowFrustum.Intersects(renderer.BoundingBox))
                                {
                                    renderer.DrawShadowMap(context, Spotlight.PSMBuffer, ShadowType.Perspective);
                                }
                            }
                        }
                        break;
                }
                light.InUpdateQueue = false;
            }

            lights.ShadowDataBuffer.Update(context);
        }

        public void Destroy()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].Destroy();
            }

            backgroundQueue.Clear();
            geometryQueue.Clear();
            alphaTestQueue.Clear();
            geometryLastQueue.Clear();
            transparencyQueue.Clear();
            overlayQueue.Clear();

            culling.Release();
        }

        private void RendererOnRemoved(GameObject gameObject, IRendererComponent renderer)
        {
            if (isLoaded)
            {
                renderer.Unload();
            }

            gameObject.OnTransformed -= GameObjectTransformed;
            if (renderer.QueueIndex < (uint)RenderQueueIndex.Geometry)
            {
                backgroundQueue.Remove(renderer);
                return;
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.AlphaTest)
            {
                geometryQueue.Remove(renderer);
                return;
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.GeometryLast)
            {
                alphaTestQueue.Remove(renderer);
                return;
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.Transparency)
            {
                geometryLastQueue.Remove(renderer);
                return;
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.Overlay)
            {
                transparencyQueue.Remove(renderer);
                return;
            }

            overlayQueue.Remove(renderer);
        }

        private void RendererOnAdded(GameObject gameObject, IRendererComponent renderer)
        {
            if (isLoaded)
            {
                Task.Factory.StartNew(device =>
                {
                    renderer.Load((IGraphicsDevice)device);
                }, device);
            }

            gameObject.OnTransformed += GameObjectTransformed;
            if (renderer.QueueIndex < (uint)RenderQueueIndex.Geometry)
            {
                backgroundQueue.Add(renderer);
                backgroundQueue.Sort(comparer);
                return;
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.AlphaTest)
            {
                geometryQueue.Add(renderer);
                geometryQueue.Sort(comparer);
                return;
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.GeometryLast)
            {
                alphaTestQueue.Add(renderer);
                alphaTestQueue.Sort(comparer);
                return;
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.Transparency)
            {
                geometryLastQueue.Add(renderer);
                geometryLastQueue.Sort(comparer);
                return;
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.Overlay)
            {
                transparencyQueue.Add(renderer);
                transparencyQueue.Sort(comparer);
                return;
            }

            overlayQueue.Add(renderer);
            overlayQueue.Sort(comparer);
            return;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteGroup(IReadOnlyList<IRendererComponent> renderers, IGraphicsContext context, ICPUProfiler? profiler, string groupDebugName, RenderPath path)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                profiler?.Begin($"{groupDebugName}.{renderer.DebugName}");
                renderer.Draw(context, path);
                profiler?.End($"{groupDebugName}.{renderer.DebugName}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ExecuteGroup(IReadOnlyList<IRendererComponent> renderers, IGraphicsContext context, ICPUProfiler? profiler, string groupDebugName, RenderPath path, void** rtvs, uint rtvCount, IDepthStencilView dsv)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                if ((renderer.Flags & RendererFlags.NoDepthTest) != 0)
                {
                    context.SetRenderTargets(rtvCount, rtvs, null);
                }
                else
                {
                    context.SetRenderTargets(rtvCount, rtvs, dsv);
                }
                profiler?.Begin($"{groupDebugName}.{renderer.DebugName}");
                renderer.Draw(context, path);
                profiler?.End($"{groupDebugName}.{renderer.DebugName}");
            }
        }
    }
}