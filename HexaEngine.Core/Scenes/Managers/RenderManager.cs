namespace HexaEngine.Core.Scenes.Managers
{
    using BepuUtilities;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.Renderers;
    using System.Collections.Generic;

    public enum RenderQueueIndex
    {
        Background = 0,
        Geometry = 100,
        AlphaTest = 500,
        Transparency = 1000,
    }

    public class SortRendererAscending : IComparer<IRenderer>
    {
        int IComparer<IRenderer>.Compare(IRenderer? a, IRenderer? b)
        {
            if (a == null || b == null)
                return 0;
            if (a.QueueIndex < b.QueueIndex)
                return -1;
            if (a.QueueIndex > b.QueueIndex)
                return 1;
            else
                return 0;
        }
    }

    public class RenderManager : ISystem
    {
        private readonly List<IRenderer> renderers = new();
        private readonly List<IRenderer> backgroundQueue = new();
        private readonly List<IRenderer> geometryQueue = new();
        private readonly List<IRenderComponent> components = new();
        private readonly SortRendererAscending comparer = new();
        private readonly IGraphicsDevice device;
        private readonly InstanceManager instanceManager;

        public string Name => "Renderers";

        public RenderManager(IGraphicsDevice device, InstanceManager instanceManager)
        {
            this.device = device;
            this.instanceManager = instanceManager;
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

        private static void DrawList(IGraphicsContext context, List<IRenderer> renderers)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].Draw(context);
            }
        }

        private static void VisibilityTestList(IGraphicsContext context, List<IRenderer> renderers)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].VisibilityTest(context);
            }
        }

        public void Register(GameObject gameObject)
        {
            components.AddComponentIfIs(gameObject);
        }

        public void Unregister(GameObject gameObject)
        {
            components.RemoveComponentIfIs(gameObject);
        }

        public void Awake(ThreadDispatcher dispatcher)
        {
        }

        public void Update(ThreadDispatcher dispatcher)
        {
            Parallel.ForEach(components, c =>
            {
                c.Draw();
            });
        }

        public void FixedUpdate(ThreadDispatcher dispatcher)
        {
        }

        public void Destroy(ThreadDispatcher dispatcher)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].Uninitialize();
            }
            renderers.Clear();
            backgroundQueue.Clear();
            geometryQueue.Clear();
        }

        public T GetRenderer<T>() where T : class, IRenderer, new()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                if (renderer is T t)
                    return t;
            }

            T r = new();
            r.Initialize(device, instanceManager);
            AddRenderer(r);
            return r;
        }

        public void AddRenderer(IRenderer renderer)
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

        public void RemoveRenderer(IRenderer renderer)
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
    }
}