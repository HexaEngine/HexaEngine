﻿namespace HexaEngine.Core.Renderers
{
    using HexaEngine.Core.Collections;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Core.Scenes;
    using System.Collections.Generic;

    public class RenderManager : ISystem
    {
        private readonly FlaggedList<RendererFlags, IRendererComponent> rendererComponents = new();
        private readonly List<IRendererComponent> renderers = new();
        private readonly List<IRendererComponent> backgroundQueue = new();
        private readonly List<IRendererComponent> geometryQueue = new();
        private readonly List<IRenderComponent> components = new();
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

        public void VisibilityTest(IGraphicsContext context, RenderQueueIndex index)
        {
            switch (index)
            {
                case RenderQueueIndex.Geometry:
                    VisibilityTestList(context, geometryQueue);
                    break;
            }
        }

        public void Draw(IGraphicsContext context, RenderQueueIndex index)
        {
            switch (index)
            {
                case RenderQueueIndex.Background:
                    DrawList(context, backgroundQueue);
                    break;

                case RenderQueueIndex.Geometry:
                    DrawList(context, geometryQueue);
                    break;
            }
        }

        private static void DrawList(IGraphicsContext context, List<IRendererComponent> renderers)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].Draw(context);
            }
        }

        private static void VisibilityTestList(IGraphicsContext context, List<IRendererComponent> renderers)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].VisibilityTest(context);
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

        public void UpdateShadows(IGraphicsContext context, Camera camera)
        {
            while (lights.UpdateShadowLightQueue.TryDequeue(out var light))
            {
                switch (light.LightType)
                {
                    case LightType.Directional:
                        var directionalLight = (DirectionalLight)light;
                        directionalLight.UpdateShadowMap(context, lights.ShadowDirectionalLights, camera);
                        for (int i = 0; i < renderers.Count; i++)
                        {
                            var renderer = renderers[i];
                            if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                            {
                                for (int j = 0; j < directionalLight.ShadowFrustra.Length; j++)
                                {
                                    if (directionalLight.ShadowFrustra[j].Intersects(renderer.BoundingBox))
                                    {
                                        renderer.DrawShadows(context, DirectionalLight.CSMBuffer, ShadowType.Cascaded);
                                        break;
                                    }
                                }
                            }
                        }
                        break;

                    case LightType.Point:
                        var pointLight = (PointLight)light;
                        pointLight.UpdateShadowMap(context, lights.ShadowPointLights);
                        for (int i = 0; i < renderers.Count; i++)
                        {
                            var renderer = renderers[i];
                            if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                            {
                                if (renderer.BoundingBox.Intersects(pointLight.ShadowBox))
                                {
                                    renderer.DrawShadows(context, PointLight.OSMBuffer, ShadowType.Omni);
                                }
                            }
                        }
                        break;

                    case LightType.Spot:
                        var spotlight = (Spotlight)light;
                        spotlight.UpdateShadowMap(context, lights.ShadowSpotlights);
                        for (int i = 0; i < renderers.Count; i++)
                        {
                            var renderer = renderers[i];
                            if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                            {
                                if (spotlight.ShadowFrustum.Intersects(renderer.BoundingBox))
                                {
                                    renderer.DrawShadows(context, Spotlight.PSMBuffer, ShadowType.Perspective);
                                }
                            }
                        }
                        break;
                }
                light.InUpdateQueue = false;
            }
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