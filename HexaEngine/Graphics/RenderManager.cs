#pragma warning disable CS0618 // Type or member is obsolete

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

    public enum QueueGroupFlags
    {
        None,
        Dynamic
    }

    public class RenderQueueGroup : IDisposable
    {
        private ICommandList? commandList;
        private readonly RenderQueueIndex index;
        private QueueGroupFlags flags;
        private readonly List<IDrawable> renderers = [];
        private readonly string pass;
        private uint baseIndex;
        private uint lastIndex;
        private bool disposedValue;

        public RenderQueueGroup(RenderQueueIndex index, QueueGroupFlags flags, string pass)
        {
            this.index = index;
            this.flags = flags;
            this.pass = pass;
        }

        public RenderQueueIndex Index => index;

        public uint BaseIndex => baseIndex;

        public uint LastIndex => lastIndex;

        public bool IsDynamic
        {
            get => (flags & QueueGroupFlags.Dynamic) != 0;
            set
            {
                if (value)
                {
                    flags |= QueueGroupFlags.Dynamic;
                }
                else
                {
                    flags &= ~QueueGroupFlags.Dynamic;
                }
            }
        }

        public int Count => renderers.Count;

        public void Add(IDrawable renderer)
        {
            renderers.Add(renderer);
            renderers.Sort(SortRendererAscending.Instance);
            lastIndex = renderers[0].QueueIndex;
            baseIndex = renderers[^1].QueueIndex;
        }

        public void Remove(IDrawable renderer)
        {
            if (renderers.Remove(renderer))
            {
                if (renderers.Count > 0)
                {
                    lastIndex = renderers[0].QueueIndex;
                    baseIndex = renderers[^1].QueueIndex;
                }
                else
                {
                    lastIndex = (uint)index;
                }
            }
        }

        public void Clear()
        {
            renderers.Clear();
            lastIndex = (uint)index;
        }

        public void Invalidate()
        {
            var tmp = commandList;
            commandList = null;
            tmp?.Dispose();
        }

        public void ExecuteGroup(IGraphicsContext context, IGraphicsContext deferredContext)
        {
            if (IsDynamic)
            {
                RecordList(context);
            }

            if (commandList == null)
            {
                RecordList(deferredContext);
                commandList = deferredContext.FinishCommandList(false);
            }
            else
            {
                context.ExecuteCommandList(commandList, false);
            }
        }

        private void RecordList(IGraphicsContext deferred)
        {
            for (var i = 0; i < renderers.Count; i++)
            {
                renderers[i].Draw(deferred, pass);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                renderers.Clear();
                commandList?.Dispose();
                commandList = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class RenderQueue
    {
        private readonly List<RenderQueueGroup> groups = new();
        private readonly List<IDrawable> renderers = new();
        private readonly IGraphicsContext deferred;
        private RenderQueueIndex queueIndex;
        private string pass = null!;

        public RenderQueue(IGraphicsContext deferred)
        {
            this.deferred = deferred;
        }

        public void Execute(IGraphicsContext context)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                groups[i].ExecuteGroup(context, deferred);
            }
        }

        public void Add(IDrawable component)
        {
            renderers.Add(component);

            uint index = component.QueueIndex;
            RendererFlags flags = component.Flags;
            bool isDynamic = (flags & RendererFlags.Dynamic) != 0;
            RenderQueueGroup? groupLast = null;
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                RenderQueueGroup? next = i + 1 < groups.Count ? groups[i + 1] : null;

                if (group.BaseIndex > index)
                {
                    continue;
                }

                if ((next?.BaseIndex ?? uint.MaxValue) <= index)
                {
                    break;
                }

                if (group.IsDynamic == isDynamic)
                {
                    groupLast = group;
                }
            }

            if (groupLast == null)
            {
                groupLast = new(queueIndex, isDynamic ? QueueGroupFlags.Dynamic : QueueGroupFlags.None, pass);
                groups.Add(groupLast);
            }

            groupLast.Add(component);
        }

        public void Remove(IDrawable component)
        {
        }
    }

    public class RenderManager : ISceneSystem
    {
        private readonly IGraphicsDevice device;
        private readonly IGraphicsContext deferredContext;
        private readonly ComponentTypeQuery<IDrawable> drawables = new();
        private readonly List<IDrawable> backgroundQueue = new();
        private readonly List<IDrawable> geometryQueue = new();
        private readonly List<IDrawable> alphaTestQueue = new();
        private readonly List<IDrawable> geometryLastQueue = new();
        private readonly List<IDrawable> transparencyQueue = new();
        private readonly List<IDrawable> overlayQueue = new();
        private readonly SortRendererAscending comparer = new();
        private readonly LightManager lights;
        private readonly CullingManager culling;

        private readonly BVHTree<IDrawable> tree = new();

        private bool isLoaded;

        public string Name => "Renderers";

        public SystemFlags Flags => SystemFlags.Awake | SystemFlags.Destroy | SystemFlags.Load | SystemFlags.Unload;

        public RenderManager(IGraphicsDevice device, LightManager lights)
        {
            this.device = device;
            deferredContext = device.CreateDeferredContext();
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
            deferredContext.Dispose();
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

        private int currentObjectId = 0;

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

#pragma warning restore CS0618 // Type or member is obsolete