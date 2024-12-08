namespace HexaEngine.Graphics
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Profiling;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class RenderSystem : ISceneSystem
    {
        private readonly IGraphicsDevice device;
        private readonly ComponentTypeQuery<IDrawable> drawables = new();
        private readonly List<IDrawable> backgroundQueue = [];
        private readonly List<IDrawable> geometryQueue = [];
        private readonly List<IDrawable> alphaTestQueue = [];
        private readonly List<IDrawable> geometryLastQueue = [];
        private readonly List<IDrawable> transparencyQueue = [];
        private readonly List<IDrawable> overlayQueue = [];

        private readonly LightManager lights;
        private readonly CullingManager culling;

        private readonly BVHTree<IDrawable> tree = new();

        private bool isLoaded;

        public string Name { get; } = "Render System";

        public SystemFlags Flags => SystemFlags.Awake | SystemFlags.Destroy | SystemFlags.Load | SystemFlags.Unload;

        public RenderSystem(IGraphicsDevice device, LightManager lights)
        {
            this.device = device;

            drawables.OnAdded += DrawableOnAdded;
            drawables.OnRemoved += DrawableOnRemoved;
            this.lights = lights;
            culling = new(device);
        }

        public BVHTree<IDrawable> DrawablesTree => tree;

        public IReadOnlyList<IDrawable> Drawables => drawables;

        public IReadOnlyList<IDrawable> BackgroundQueue => backgroundQueue;

        public IReadOnlyList<IDrawable> GeometryQueue => geometryQueue;

        public IReadOnlyList<IDrawable> AlphaTestQueue => alphaTestQueue;

        public IReadOnlyList<IDrawable> GeometryLastQueue => geometryLastQueue;

        public IReadOnlyList<IDrawable> TransparencyQueue => transparencyQueue;

        public IReadOnlyList<IDrawable> OverlayQueue => overlayQueue;

        public void Invalidate(RenderQueueIndexFlags flags)
        {
        }

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(drawables);
        }

        public void Load(IGraphicsDevice device)
        {
            if (!isLoaded)
            {
                Parallel.For(0, drawables.Count, i =>
                {
                    drawables[i].Load(device);
                });

                isLoaded = true;
            }
        }

        public void Unload()
        {
            if (isLoaded)
            {
                foreach (IDrawable drawable in drawables)
                {
                    drawable.Unload();

                    if (drawable.LeafId != -1)
                    {
                        tree.RemoveLeaf(drawable.LeafId);
                    }
                }
                isLoaded = false;
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

        private static void DrawDepthList(IGraphicsContext context, List<IDrawable> renderers)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].DrawDepth(context);
            }
        }

        private static void DrawList(IGraphicsContext context, List<IDrawable> renderers, RenderPath path)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].Draw(context, path);
            }
        }

        private static void VisibilityTestList(CullingContext context, List<IDrawable> renderers)
        {
            var cam = CameraManager.Current;
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].VisibilityTest(context);
            }
        }

        private void GameObjectTransformed(GameObject sender, Transform transform)
        {
            var queue = lights.DrawableUpdateQueue;
            var components = sender.Components;
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is IDrawable drawable)
                {
                    queue.Enqueue(drawable);
                    if (drawable.LeafId != -1)
                    {
                        drawable.LeafId = tree.UpdateLeaf(drawable.LeafId, drawable.BoundingBox);
                    }
                    else
                    {
                        drawable.LeafId = tree.InsertLeaf(drawable, drawable.BoundingBox);
                    }
                }
            }
        }

        private void QueueIndexChanged(IDrawable sender, uint oldIndex, uint newIndex)
        {
            if (!RemoveFromQueue(sender))
            {
                return;
            }

            AddToQueue(sender);
        }

        [Profiling.Profile]
        public void Update(IGraphicsContext context)
        {
            for (int i = 0; i < drawables.Count; i++)
            {
                drawables[i].Update(context);
            }
        }

        public void Destroy()
        {
            for (int i = 0; i < drawables.Count; i++)
            {
                drawables[i].Destroy();
            }

            backgroundQueue.Clear();
            geometryQueue.Clear();
            alphaTestQueue.Clear();
            geometryLastQueue.Clear();
            transparencyQueue.Clear();
            overlayQueue.Clear();

            culling.Release();
        }

        private void DrawableOnRemoved(GameObject gameObject, IDrawable drawable)
        {
            if (isLoaded)
            {
                drawable.Unload();
            }

            gameObject.TransformUpdated -= GameObjectTransformed;
            drawable.QueueIndexChanged -= QueueIndexChanged;
            if (drawable.LeafId != -1)
            {
                tree.RemoveLeaf(drawable.LeafId);
            }

            RemoveFromQueue(drawable);
        }

        private void DrawableOnAdded(GameObject gameObject, IDrawable drawable)
        {
            if (isLoaded)
            {
                Task.Factory.StartNew(device =>
                {
                    drawable.Load((IGraphicsDevice)device!);
                }, device);

                tree.InsertLeaf(drawable, drawable.BoundingBox);
            }

            gameObject.TransformUpdated += GameObjectTransformed;
            drawable.QueueIndexChanged += QueueIndexChanged;
            AddToQueue(drawable);
        }

        public void AddToQueue(IDrawable renderer)
        {
            if (renderer.QueueIndex < (uint)RenderQueueIndex.Geometry)
            {
                backgroundQueue.Add(renderer);
                backgroundQueue.Sort(SortRendererAscending.Instance);
                return;
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.AlphaTest)
            {
                geometryQueue.Add(renderer);
                geometryQueue.Sort(SortRendererAscending.Instance);
                return;
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.GeometryLast)
            {
                alphaTestQueue.Add(renderer);
                alphaTestQueue.Sort(SortRendererAscending.Instance);
                return;
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.Transparency)
            {
                geometryLastQueue.Add(renderer);
                geometryLastQueue.Sort(SortRendererAscending.Instance);
                return;
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.Overlay)
            {
                transparencyQueue.Add(renderer);
                transparencyQueue.Sort(SortRendererAscending.Instance);
                return;
            }

            overlayQueue.Add(renderer);
            overlayQueue.Sort(SortRendererAscending.Instance);
        }

        public bool RemoveFromQueue(IDrawable renderer)
        {
            if (renderer.QueueIndex < (uint)RenderQueueIndex.Geometry)
            {
                return backgroundQueue.Remove(renderer);
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.AlphaTest)
            {
                return geometryQueue.Remove(renderer);
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.GeometryLast)
            {
                return alphaTestQueue.Remove(renderer);
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.Transparency)
            {
                return geometryLastQueue.Remove(renderer);
            }
            if (renderer.QueueIndex < (uint)RenderQueueIndex.Overlay)
            {
                return transparencyQueue.Remove(renderer);
            }

            return overlayQueue.Remove(renderer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteGroupVisibilityTest(IReadOnlyList<IDrawable> renderers, CullingContext context, ICPUProfiler? profiler, string groupDebugName)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                profiler?.Begin($"{groupDebugName}.{renderer.DebugName}");
                renderer.VisibilityTest(context);
                profiler?.End($"{groupDebugName}.{renderer.DebugName}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteGroup(IReadOnlyList<IDrawable> renderers, IGraphicsContext context, ICPUProfiler? profiler, string groupDebugName, RenderPath path)
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
        public static void ExecuteGroupDepth(IReadOnlyList<IDrawable> renderers, IGraphicsContext context, ICPUProfiler? profiler, string groupDebugName)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                profiler?.Begin($"{groupDebugName}.{renderer.DebugName}");
                renderer.DrawDepth(context);
                profiler?.End($"{groupDebugName}.{renderer.DebugName}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ExecuteGroup(IReadOnlyList<IDrawable> renderers, IGraphicsContext context, ICPUProfiler? profiler, string groupDebugName, RenderPath path, void** rtvs, uint rtvCount, IDepthStencilView dsv)
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