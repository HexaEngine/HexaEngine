namespace HexaEngine.Rendering
{
    using HexaEngine.Collections;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public class RenderManager : ISystem
    {
        private readonly FlaggedList<RendererFlags, IRendererComponent> rendererComponents = new();
        private readonly List<IRendererComponent> renderers = new();
        private readonly List<IRendererComponent> backgroundQueue = new();
        private readonly List<IRendererComponent> geometryQueue = new();
        private readonly List<IRendererComponent> alphaTestQueue = new();
        private readonly List<IRendererComponent> geometryLastQueue = new();
        private readonly List<IRendererComponent> transparencyQueue = new();
        private readonly List<IRendererComponent> overlayQueue = new();
        private readonly SortRendererAscending comparer = new();
        private readonly IGraphicsDevice device;
        private readonly LightManager lights;

        public string Name => "Renderers";

        public SystemFlags Flags => SystemFlags.None;

        public RenderManager(IGraphicsDevice device, LightManager lights)
        {
            this.device = device;
            this.lights = lights;
        }

        public IReadOnlyList<IRendererComponent> BackgroundQueue => backgroundQueue;

        public IReadOnlyList<IRendererComponent> GeometryQueue => geometryQueue;

        public IReadOnlyList<IRendererComponent> AlphaTestQueue => alphaTestQueue;

        public IReadOnlyList<IRendererComponent> GeometryLastQueue => geometryLastQueue;

        public IReadOnlyList<IRendererComponent> TransparencyQueue => transparencyQueue;

        public IReadOnlyList<IRendererComponent> OverlayQueue => overlayQueue;

        public void VisibilityTest(IGraphicsContext context, RenderQueueIndex index)
        {
            switch (index)
            {
                case RenderQueueIndex.Geometry:
                    VisibilityTestList(context, geometryQueue);
                    break;
            }
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

        private static void VisibilityTestList(IGraphicsContext context, List<IRendererComponent> renderers)
        {
            var cam = CameraManager.Current;
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].VisibilityTest(context, cam);
            }
        }

        public void Register(GameObject gameObject)
        {
            if (gameObject.AddComponentIfIs<IRendererComponent>(AddRenderer))
            {
                gameObject.Transformed += GameObjectTransformed;
            }
        }

        public void Unregister(GameObject gameObject)
        {
            if (gameObject.RemoveComponentIfIs<IRendererComponent>(RemoveRenderer))
            {
                gameObject.Transformed -= GameObjectTransformed;
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
                renderers[i].Destory();
            }

            renderers.Clear();
            backgroundQueue.Clear();
            geometryQueue.Clear();
            alphaTestQueue.Clear();
            geometryLastQueue.Clear();
            transparencyQueue.Clear();
            overlayQueue.Clear();
        }

        public void AddRenderer(IRendererComponent renderer)
        {
            renderers.Add(renderer);
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

        public void RemoveRenderer(IRendererComponent renderer)
        {
            renderers.Remove(renderer);
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

        public void RemoveRenderer(IRendererComponent renderer, uint index)
        {
            renderers.Remove(renderer);
            if (index < (uint)RenderQueueIndex.Geometry)
            {
                backgroundQueue.Remove(renderer);
                return;
            }
            if (index < (uint)RenderQueueIndex.AlphaTest)
            {
                geometryQueue.Remove(renderer);
                return;
            }
            if (index < (uint)RenderQueueIndex.GeometryLast)
            {
                alphaTestQueue.Remove(renderer);
                return;
            }
            if (index < (uint)RenderQueueIndex.Transparency)
            {
                geometryLastQueue.Remove(renderer);
                return;
            }
            if (index < (uint)RenderQueueIndex.Overlay)
            {
                transparencyQueue.Remove(renderer);
                return;
            }

            overlayQueue.Remove(renderer);
        }

        public void UpdateQueueIndex(IRendererComponent renderer, uint oldQueueIndex)
        {
            RemoveRenderer(renderer, oldQueueIndex);
            AddRenderer(renderer);
        }

        public void Awake()
        {
            throw new NotImplementedException();
        }

        public void Update(float delta)
        {
            throw new NotImplementedException();
        }

        public void FixedUpdate()
        {
            throw new NotImplementedException();
        }
    }
}