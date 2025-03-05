namespace HexaEngine.Components.Renderer
{
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Jobs;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes.Managers;
    using System.Text;

    [EditorCategory("Renderer")]
    [EditorComponent<TerrainRendererComponent>("Terrain Renderer", false, true, Icon = "\xf5ee")]
    public class TerrainRendererComponent : BaseDrawableComponent, ISelectableHitTest
    {
        private TerrainGrid? terrain;
        private AssetRef terrainAsset;

        private static TerrainRenderer renderer1 = null!;
        private static int instances;
        private int currentLODLevel;
        private int maxLODLevel;
        private int minLODLevel;

        public TerrainRendererComponent()
        {
            QueueIndex = (uint)RenderQueueIndex.GeometryLast;
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
        public override RendererFlags Flags { get; } = RendererFlags.All | RendererFlags.Clustered | RendererFlags.Forward;

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

        [JsonIgnore]
        public int LODLevel => currentLODLevel;

        [EditorCategory("LOD")]
        [EditorProperty("Max Level")]
        public int MaxLODLevel
        {
            get => maxLODLevel;
            set
            {
                if (terrain != null)
                {
                    value = MathUtil.Clamp(value, 0, 4);
                }

                maxLODLevel = value;
            }
        }

        [EditorCategory("LOD")]
        [EditorProperty("Min Level")]
        public int MinLODLevel
        {
            get => minLODLevel;
            set
            {
                if (terrain != null)
                {
                    value = MathUtil.Clamp(value, 0, 4);
                }

                minLODLevel = value;
            }
        }

        private static bool AssetExists(TerrainRendererComponent terrain)
        {
            return terrain.terrainAsset.Exists();
        }

        protected override void LoadCore(IGraphicsDevice device)
        {
            if (Interlocked.Increment(ref instances) == 1)
            {
                renderer1 = new();
                ((IRenderer1)renderer1).Initialize(device, CullingManager.Current.Context);
            }

            UpdateTerrain();
        }

        protected override void UnloadCore()
        {
            if (Interlocked.Decrement(ref instances) == 0)
            {
                renderer1.Dispose();
            }

            terrain?.Dispose();
        }

        public override void Update(IGraphicsContext context)
        {
            if (terrain == null)
            {
                return;
            }

            renderer1.Update(context, GameObject.Transform.Global, terrain);
        }

        public override void DrawDepth(IGraphicsContext context)
        {
            if (terrain == null)
            {
                return;
            }

            renderer1.DrawDepth(context, terrain);
        }

        public override void Draw(IGraphicsContext context, RenderPath path)
        {
            if (terrain == null)
            {
                return;
            }

            if (path == RenderPath.Deferred)
            {
                renderer1.DrawDeferred(context, terrain);
            }
            else
            {
                renderer1.DrawForward(context, terrain);
            }
        }

        public override void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (terrain == null)
            {
                return;
            }

            renderer1.DrawShadowMap(context, terrain, light, type);
        }

        public override void VisibilityTest(CullingContext context)
        {
            if (terrain == null)
            {
                return;
            }

            renderer1.VisibilityTest(context, terrain);
        }

        public override void Bake(IGraphicsContext context)
        {
            if (terrain == null)
            {
                return;
            }

            renderer1.Bake(context, terrain);
        }

        private Job UpdateTerrain()
        {
            Loaded = false;
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

                    component.Loaded = true;
                    component.GameObject.SendUpdateTransformed();
                }
                else
                {
                    LoggerFactory.General.Error($"Couldn't load terrain {terrainAsset}");
                }
            }, JobPriority.Normal, JobFlags.BlockOnSceneLoad);
        }

        public override bool SelectRayTest(Ray ray, ref float depth)
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