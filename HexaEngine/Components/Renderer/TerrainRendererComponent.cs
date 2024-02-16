﻿namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;

    [EditorCategory("Renderer")]
    [EditorComponent<TerrainRendererComponent>("Terrain Renderer", false, true)]
    public class TerrainRendererComponent : BaseRendererComponent
    {
        private StaticTerrainRenderer renderer;
        private readonly StaticTerrainGrid grid = new();
        private HeightMap heightMap;

        private BoundingBox boundingBox;

        public TerrainRendererComponent()
        {
            QueueIndex = (uint)RenderQueueIndex.Transparency;
        }

        [JsonIgnore]
        public override string DebugName { get; protected set; } = nameof(StaticTerrainRenderer);

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