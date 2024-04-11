namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Jobs;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes.Managers;
    using System.Text;

    [EditorCategory("Renderer")]
    [EditorComponent<TerrainRendererComponent>("Terrain Renderer", false, true)]
    public class TerrainRendererComponent : BaseRendererComponent, ISelectableRayTest
    {
        private TerrainRenderer renderer;
        private TerrainGrid? terrain;
        private AssetRef terrainAsset;

        public TerrainRendererComponent()
        {
            QueueIndex = (uint)RenderQueueIndex.Transparency;
        }

        [EditorProperty("Terrain File", AssetType.Terrain)]
        public AssetRef TerrainAsset
        {
            get => terrainAsset;
            set
            {
                terrainAsset = value;
                UpdateTerrain();
            }
        }

        [JsonIgnore]
        public override string DebugName { get; protected set; } = nameof(TerrainRenderer);

        [JsonIgnore]
        public override RendererFlags Flags { get; } = RendererFlags.All | RendererFlags.Forward | RendererFlags.Deferred | RendererFlags.Clustered;

        [JsonIgnore]
        public override BoundingBox BoundingBox { get => BoundingBox.Transform(terrain?.BoundingBox ?? BoundingBox.Empty, GameObject.Transform); }

        [JsonIgnore]
        public TerrainGrid? Terrain => terrain;

        public void CreateNew()
        {
            var name = SourceAssetsDatabase.GetFreeName("Terrain.terrain");

            TerrainFile terrain = new();
            TerrainCellData cell = new(default);
            cell.GenerateLOD();
            terrain.Cells.Add(cell);
            terrain.Save(SourceAssetsDatabase.GetFullPath(name), Encoding.UTF8, Endianness.LittleEndian, Compression.LZ4);

            var meta = SourceAssetsDatabase.CreateFile(name);
            var artifact = ArtifactDatabase.GetArtifact(meta.Guid);
            terrainAsset = artifact?.Guid ?? Guid.Empty;
            UpdateTerrain();
        }

        private static bool AssetExists(TerrainRendererComponent terrain)
        {
            return terrain.terrainAsset.Exists();
        }

        protected override void LoadCore(IGraphicsDevice device)
        {
            renderer = new(device);
            UpdateTerrain();
        }

        protected override void UnloadCore()
        {
            terrain?.Dispose();
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

        private Job UpdateTerrain()
        {
            Loaded = false;
            renderer?.Uninitialize();
            var tmpTerrain = terrain;
            terrain = null;
            tmpTerrain?.Dispose();

            return Job.Run("Terrain Load Job", this, state =>
            {
                if (state is not TerrainRendererComponent component)
                {
                    return;
                }

                if (component.GameObject == null)
                {
                    return;
                }

                if (component.terrainAsset.Guid == Guid.Empty)
                {
                    return;
                }

                var stream = component.terrainAsset.OpenReadReusable();
                if (stream != null)
                {
                    component.terrain = new(stream, true);

                    component.renderer.Initialize(component.terrain);
                    component.Loaded = true;
                    component.GameObject.SendUpdateTransformed();
                }
                else
                {
                    LoggerFactory.General.Error($"Couldn't load terrain {terrainAsset}");
                }
            }, JobPriority.Normal, JobFlags.BlockOnSceneLoad);
        }

        public bool SelectRayTest(Ray ray, ref float depth)
        {
            if (terrain == null)
            {
                return false;
            }

            if (!BoundingBox.Intersects(ray).HasValue)
            {
                return false;
            }

            for (int i = 0; i < terrain.Count; i++)
            {
                var cell = terrain[i];
                Ray cellSpaceRay = Ray.Transform(ray, cell.TransformInv);
                var result = cell.LODData.Intersect(cellSpaceRay);
                if (result != null)
                {
                    depth = result.Value;
                    return true;
                }
            }

            return false;
        }
    }
}