﻿namespace HexaEngine.Components.Renderer
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
    using HexaEngine.Scenes;

    [EditorComponent<TerrainRendererComponent>("TerrainCellData", false, true)]
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
        public string DebugName { get; private set; } = nameof(TerrainRenderer);

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
            DebugName = gameObject.Name + DebugName;
            this.gameObject = gameObject;
            heightMap = new(32, 32);
            heightMap.GenerateEmpty();
            grid.Add(new(device, heightMap, true));
            renderer = new(device);
            renderer.Initialize(grid);
        }

        public void Destroy()
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

        public void VisibilityTest(CullingContext context)
        {
        }
    }
}