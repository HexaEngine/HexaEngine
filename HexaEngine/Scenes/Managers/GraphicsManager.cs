namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Objects;
    using System.Collections.Generic;

    public interface IResourceManager
    {
        public Mesh LoadMesh(string name);

        public Mesh CreateMesh(string name);

        public Material LoadMaterial(string name);

        public Material CreateMaterial(string name);

        public Texture LoadTexture(string name);

        public Texture CreateTexture(string name);

        public Effect LoadEffect(string name);

        public Effect CreateEffect(string name);
    }

    public interface ILightManager
    {
        public IReadOnlyList<Light> Lights { get; }

        public RenderQueue Queue { get; set; }

        public void AddLight(Light light);

        public void RemoveLight(Light light);

        public void Update();

        public void Draw();
    }

    public interface IPostProcessManager
    {
        public void AddEffect(Effect effect);

        public void RemoveEffect(Effect effect);

        public void Draw();
    }

    public enum RenderQueueIndex
    {
        OpaqueNoCull,
        Opaque,
        Terrain,
        Billboard,
        BackgroundNoCull,
        Background,
        Opacity,
        Wireframe,
        Effect,
        EffectOverlay,
        Overlay,
        Debug,
    }

    public interface IDrawable
    {
        void Draw(IGraphicsContext context);

        void DrawDepth(IGraphicsContext context);
    }

    public struct RenderQueueItem
    {
        public RenderQueueIndex Index;
        public IDrawable Drawable;
    }

    public unsafe class RenderQueue
    {
        private readonly RenderQueueIndex[] keys;
        private readonly Dictionary<RenderQueueIndex, List<RenderQueueItem>> queues = new();
        private RenderQueueItem item;

        public RenderQueue()
        {
            keys = Enum.GetValues<RenderQueueIndex>();
            foreach (var value in keys)
            {
                queues.Add(value, new List<RenderQueueItem>());
            }
        }

        public virtual void Clear()
        {
            for (int i = 0; i < keys.Length; i++)
            {
                queues[keys[i]].Clear();
            }
        }

        public virtual void Enqueue(RenderQueueIndex index, IDrawable drawable)
        {
            item.Index = index;
            item.Drawable = drawable;
            queues[index].Add(item);
        }

        public virtual IReadOnlyList<RenderQueueItem> GetQueue(RenderQueueIndex index)
        {
            return queues[index];
        }
    }

    public static class GraphicsManager
    {
        private static IGraphicsDevice graphicsDevice;
        private static RenderQueue queue;
        private static IResourceManager resources;
        private static ILightManager lights;
        private static IPostProcessManager postProcess;

        public static void Initialize(IGraphicsDevice device)
        {
            graphicsDevice = device;
            queue = new();
        }

        public static RenderQueue Queue => queue;

        public static IResourceManager Resources => resources;

        public static ILightManager Lights => lights;

        public static IPostProcessManager PostProcess => postProcess;
    }
}