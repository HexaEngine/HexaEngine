namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Terrains;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Editor.Properties.Editors;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering;

    [EditorComponent<TerrainRendererComponent>("Terrain", false, true)]
    public class TerrainRendererComponent : IRendererComponent
    {
        private GameObject gameObject;
        private TerrainRenderer renderer;
        private readonly TerrainGrid grid = new();
        private HeightMap heightMap;

        private BoundingBox boundingBox;

        private bool drawable;

        static TerrainRendererComponent()
        {
            ObjectEditorFactory.RegisterEditor(typeof(TerrainRendererComponent), new TerrainEditor());
        }

        [JsonIgnore]
        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Geometry;

        [JsonIgnore]
        public RendererFlags Flags { get; } = RendererFlags.All | RendererFlags.Forward | RendererFlags.Deferred | RendererFlags.Clustered;

        [JsonIgnore]
        public BoundingBox BoundingBox { get => BoundingBox.Transform(boundingBox, gameObject.Transform); }

        [JsonIgnore]
        public TerrainGrid Grid => grid;

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;
            heightMap = new(32, 32);
            heightMap.GenerateEmpty();
            grid.Add(new(device, heightMap, true));
            renderer = new(device);
            renderer.Initialize(grid);
        }

        public void Destory()
        {
            Volatile.Write(ref drawable, false);
            for (int i = 0; i < grid.Count; i++)
            {
                grid[i].Dispose();
            }
            grid.Clear();
            renderer.Dispose();
        }

        public unsafe void Update(IGraphicsContext context)
        {
            renderer.Update(gameObject.Transform.Global);
        }

        public void DrawDepth(IGraphicsContext context)
        {
            renderer.DrawDepth(context);
        }

        public void Draw(IGraphicsContext context, RenderPath path)
        {
            if (path == RenderPath.Deferred)
            {
                renderer.DrawDeferred(context);
            }
            else
            {
                renderer.DrawForward(context);
            }
        }

        public void Bake(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            renderer.DrawShadowMap(context, light, type);
        }

        public void VisibilityTest(IGraphicsContext context, Camera camera)
        {
        }
    }
}