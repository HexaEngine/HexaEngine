namespace HexaEngine.Core.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Core.Renderers;
    using System.Collections.Generic;

    public enum RenderQueueIndex
    {
        Background = 0,
        Geometry = 100,
        AlphaTest = 500,
        Transparency = 1000,
    }

    public class SortRendererAscending : IComparer<IRendererComponent>
    {
        int IComparer<IRendererComponent>.Compare(IRendererComponent? a, IRendererComponent? b)
        {
            if (a == null || b == null)
            {
                return 0;
            }

            if (a.QueueIndex < b.QueueIndex)
            {
                return -1;
            }

            if (a.QueueIndex > b.QueueIndex)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public class RenderManager : ISystem
    {
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
            gameObject.AddComponentIfIs<IRendererComponent>(AddRenderer);
        }

        public void Unregister(GameObject gameObject)
        {
            gameObject.RemoveComponentIfIs<IRendererComponent>(RemoveRenderer);
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
                        var directionalLight = ((DirectionalLight)light);
                        directionalLight.UpdateShadowMap(context, lights.ShadowDirectionalLights, camera);
                        for (int i = 0; i < renderers.Count; i++)
                        {
                            var renderer = renderers[i];
                            if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                                renderer.DrawShadows(context, DirectionalLight.CSMBuffer, ShadowType.Cascaded);
                        }
                        break;

                    case LightType.Point:
                        var pointLight = ((PointLight)light);
                        pointLight.UpdateShadowMap(context, lights.ShadowPointLights);
                        for (int i = 0; i < renderers.Count; i++)
                        {
                            var renderer = renderers[i];
                            if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                                renderer.DrawShadows(context, PointLight.OSMBuffer, ShadowType.Omni);
                        }
                        break;

                    case LightType.Spot:
                        var spotlight = ((Spotlight)light);
                        spotlight.UpdateShadowMap(context, lights.ShadowSpotlights);
                        for (int i = 0; i < renderers.Count; i++)
                        {
                            var renderer = renderers[i];
                            if ((renderer.Flags & RendererFlags.CastShadows) != 0)
                                renderer.DrawShadows(context, Spotlight.PSMBuffer, ShadowType.Perspective);
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