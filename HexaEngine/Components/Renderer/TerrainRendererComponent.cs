namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Terrains;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Culling;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Editor.Properties.Editors;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.Renderers;

    [EditorComponent<TerrainRendererComponent>("TerrainCellData", false, true)]
    public class TerrainRendererComponent : BaseRendererComponent
    {
        private StaticTerrainRenderer renderer;
        private readonly StaticTerrainGrid grid = new();
        private HeightMap heightMap;

        private BoundingBox boundingBox;

        [JsonIgnore]
        public override string DebugName { get; protected set; } = nameof(StaticTerrainRenderer);

        [JsonIgnore]
        public override uint QueueIndex { get; } = (uint)RenderQueueIndex.Geometry;

        [JsonIgnore]
        public override RendererFlags Flags { get; } = RendererFlags.All | RendererFlags.Forward | RendererFlags.Deferred | RendererFlags.Clustered;

        [JsonIgnore]
        public override BoundingBox BoundingBox { get => BoundingBox.Transform(boundingBox, GameObject.Transform); }

        [JsonIgnore]
        public StaticTerrainGrid Grid => grid;

        public override void Load(IGraphicsDevice device)
        {
            heightMap = new(32, 32);
            heightMap.GenerateEmpty();
            grid.Add(new(device, heightMap, true));
            renderer = new(device);
            renderer.Initialize(grid);
        }

        public override void Unload()
        {
            for (int i = 0; i < grid.Count; i++)
            {
                grid[i].Dispose();
            }
            grid.Clear();
            renderer.Dispose();
        }

        public override void Update(IGraphicsContext context)
        {
            renderer.Update(GameObject.Transform.Global);
        }

        public override void DrawDepth(IGraphicsContext context)
        {
            renderer.DrawDepth(context);
        }

        public override void Draw(IGraphicsContext context, RenderPath path)
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

        public override void Bake(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public override void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            renderer.DrawShadowMap(context, light, type);
        }

        public override void VisibilityTest(CullingContext context)
        {
        }
    }
}