namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Terrains;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Editor.Properties.Editors;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class TerrainLayer
    {
        public string Name;
        private readonly MaterialData material;
        public TerrainMaterial Material;

        public TerrainLayer(string name, MaterialData material)
        {
            Name = name;
            this.material = material;
        }

        public void Load()
        {
            Material = ResourceManager.LoadTerrainMaterial(material);
        }

        public async Task LoadAsync()
        {
            Material = await ResourceManager.LoadTerrainMaterialAsync(material);
        }

        public ShaderMacro[] GetShaderMacros(string baseName)
        {
            var macros = material.GetShaderMacros();
            for (int i = 0; i < macros.Length; i++)
            {
                macros[i].Name += baseName;
            }
            return macros;
        }
    }

    public class TerrainLayerCollection
    {
        private readonly Terrain terrain;
        public RWTexture2D LayerMask;
        public TerrainLayer[] Layers;
        public TerrainShader Shader;

        public TerrainLayerCollection(IGraphicsDevice device, Terrain terrain, int maskSize, bool allowEditMask)
        {
            LayerMask = new RWTexture2D(device, Format.R8G8B8A8UInt, maskSize, maskSize, 1, 1, allowEditMask ? CpuAccessFlags.Write : CpuAccessFlags.None);
            Layers = new TerrainLayer[4];
            this.terrain = terrain;
        }

        public void Reload(IGraphicsDevice device)
        {
            List<ShaderMacro> macros = new();
            for (var i = 0; i < Layers.Length; i++)
            {
                macros.AddRange(Layers[i].GetShaderMacros(i.ToString()));
            }
            Shader = new(device, terrain, macros.ToArray(), true);
        }
    }

    public class TerrainCell : IDisposable
    {
        private readonly bool writeable;
        private readonly IGraphicsDevice device;
        public HeightMap HeightMap;
        public Terrain Terrain;
        public IBuffer VertexBuffer;
        public IBuffer IndexBuffer;
        public uint Stride;
        public BoundingBox BoundingBox;
        public Point2 ID;
        public Vector3 Offset;
        public Matrix4x4 Transform;

        public TerrainShader DefaultShader;

        public TerrainCell? Left;
        public TerrainCell? Right;
        public TerrainCell? Top;
        public TerrainCell? Bottom;

        private bool disposedValue;

        public TerrainCell(IGraphicsDevice device, HeightMap heightMap, bool writeable)
        {
            this.writeable = writeable;
            this.device = device;
            HeightMap = heightMap;
            heightMap = new(32, 32);
            heightMap.GenerateEmpty();
            Terrain = new(heightMap);
            BoundingBox = Terrain.Box;
            VertexBuffer = Terrain.CreateVertexBuffer(device, writeable ? Usage.Dynamic : Usage.Immutable, writeable ? CpuAccessFlags.Write : CpuAccessFlags.None);
            IndexBuffer = Terrain.CreateIndexBuffer(device);
            Stride = Terrain.GetStride();
            DefaultShader = new(device, Terrain, MaterialData.Empty.GetShaderMacros(), false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                VertexBuffer.Dispose();
                IndexBuffer.Dispose();
                disposedValue = true;
            }
        }

        ~TerrainCell()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class TerrainGrid
    {
        private readonly List<TerrainCell> cells = new();
        private readonly List<TerrainLayer> layers = new();

        public TerrainCell this[int index]
        {
            get => cells[index];
            set => cells[index] = value;
        }

        public List<TerrainLayer> Layers => layers;

        public int Count => cells.Count;

        public void Add(TerrainCell cell)
        {
            cells.Add(cell);
            FindNeighbors(cell);
        }

        public void Remove(TerrainCell cell)
        {
            cells.Remove(cell);
        }

        public void Clear()
        {
            cells.Clear();
        }

        public TerrainCell? Find(Point2 id)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell.ID == id)
                    return cell;
            }
            return null;
        }

        public void FindNeighbors(TerrainCell cell)
        {
            cell.Top = Find(cell.ID + new Point2(0, 1));
            cell.Bottom = Find(cell.ID + new Point2(0, -1));
            cell.Right = Find(cell.ID + new Point2(1, 0));
            cell.Left = Find(cell.ID + new Point2(-1, 0));
        }

        public void FindNeighbors()
        {
            for (int i = 0; i < cells.Count; ++i)
            {
                FindNeighbors(cells[i]);
            }
        }
    }

    [EditorComponent<TerrainRenderer>("Terrain", false, true)]
    public class TerrainRenderer : IRendererComponent
    {
        private GameObject gameObject;
        private readonly TerrainGrid grid = new();
        private HeightMap heightMap;
        private ConstantBuffer<Matrix4x4> WorldBuffer;
        private ResourceRef<IBuffer> camera;
        private IGraphicsPipeline geometry;
        private IGraphicsPipeline depth;
        private IGraphicsPipeline psm;
        private IGraphicsPipeline osm;
        private IGraphicsPipeline csm;
        private BoundingBox boundingBox;

        private bool drawable;

        static TerrainRenderer()
        {
            ObjectEditorFactory.RegisterEditor(typeof(TerrainRenderer), new TerrainEditor());
        }

        [JsonIgnore]
        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Geometry;

        [JsonIgnore]
        public RendererFlags Flags { get; } = RendererFlags.All;

        [JsonIgnore]
        public BoundingBox BoundingBox { get => BoundingBox.Transform(boundingBox, gameObject.Transform); }

        [JsonIgnore]
        public TerrainGrid Grid => grid;

        public async void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;
            heightMap = new(32, 32);
            heightMap.GenerateEmpty();
            grid.Add(new(device, heightMap, true));

            WorldBuffer = new(device, CpuAccessFlags.Write);
            camera = ResourceManager2.Shared.GetBuffer("CBCamera");

            var inputElements = grid[0].Terrain.GetInputElements();
            var macros = grid[0].Terrain.GetShaderMacros();

            GraphicsPipelineDesc pipelineDesc = new()
            {
                VertexShader = "deferred/terrain/vs.hlsl",
                PixelShader = "deferred/terrain/ps.hlsl"
            };

            GraphicsPipelineState pipelineState = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            geometry = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, inputElements, macros);

            pipelineDesc.PixelShader = null;
            depth = await device.CreateGraphicsPipelineAsync(pipelineDesc, pipelineState, inputElements, macros);

            var csmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "forward/terrain/csm/vs.hlsl",
                GeometryShader = "forward/terrain/csm/gs.hlsl",
            };
            var csmPipelineState = new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            var osmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "forward/terrain/osm/vs.hlsl",
                GeometryShader = "forward/terrain/osm/gs.hlsl",
                PixelShader = "forward/terrain/osm/ps.hlsl",
            };
            var osmPipelineState = new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            var psmPipelineDesc = new GraphicsPipelineDesc()
            {
                VertexShader = "forward/terrain/psm/vs.hlsl",
                PixelShader = "forward/terrain/psm/ps.hlsl",
            };
            var psmPipelineState = new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullFront,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            csm = await device.CreateGraphicsPipelineAsync(csmPipelineDesc, csmPipelineState, inputElements, macros);
            osm = await device.CreateGraphicsPipelineAsync(osmPipelineDesc, osmPipelineState, inputElements, macros);
            psm = await device.CreateGraphicsPipelineAsync(psmPipelineDesc, psmPipelineState, inputElements, macros);

            Volatile.Write(ref drawable, true);
        }

        public void Destory()
        {
            Volatile.Write(ref drawable, false);
            for (int i = 0; i < grid.Count; i++)
            {
                grid[i].Dispose();
            }
            grid.Clear();
            WorldBuffer?.Dispose();
            geometry?.Dispose();
            depth?.Dispose();
            csm?.Dispose();
            osm?.Dispose();
            psm?.Dispose();
        }

        public unsafe void Update(IGraphicsContext context)
        {
            if (!drawable)
                return;
            for (int i = 0; i < grid.Count; i++)
            {
                grid[i].Transform = gameObject.Transform.Global * Matrix4x4.CreateTranslation(grid[i].Offset);
            }
        }

        public void DrawDepth(IGraphicsContext context)
        {
            if (!drawable)
                return;
            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                context.SetVertexBuffer(cell.VertexBuffer, cell.Stride);
                context.SetIndexBuffer(cell.IndexBuffer, Format.R32UInt, 0);
                context.VSSetConstantBuffer(WorldBuffer, 0);
                context.VSSetConstantBuffer(camera.Value, 1);
                context.SetGraphicsPipeline(depth);
                context.DrawIndexedInstanced(cell.Terrain.IndicesCount, 1, 0, 0, 0);
            }
        }

        public void DrawShadows(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!drawable)
                return;
            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                unsafe
                {
                    *WorldBuffer.Local = Matrix4x4.Transpose(cell.Transform);
                    WorldBuffer.Update(context);
                }

                context.SetVertexBuffer(cell.VertexBuffer, cell.Stride);
                context.SetIndexBuffer(cell.IndexBuffer, Format.R32UInt, 0);
                context.VSSetConstantBuffer(WorldBuffer, 0);
                context.DSSetConstantBuffer(light, 1);
                context.VSSetConstantBuffer(light, 1);
                context.GSSetConstantBuffer(light, 1);
                switch (type)
                {
                    case ShadowType.Perspective:
                        if (!psm.IsValid)
                        {
                            return;
                        }
                        context.SetGraphicsPipeline(psm);
                        break;

                    case ShadowType.Cascaded:
                        if (!csm.IsValid)
                        {
                            return;
                        }
                        context.SetGraphicsPipeline(csm);
                        break;

                    case ShadowType.Omni:
                        if (!osm.IsValid)
                        {
                            return;
                        }
                        context.SetGraphicsPipeline(osm);
                        break;
                }
                context.DrawIndexedInstanced(cell.Terrain.IndicesCount, 1, 0, 0, 0);
            }
        }

        public void Draw(IGraphicsContext context)
        {
            if (!drawable)
                return;

            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                unsafe
                {
                    *WorldBuffer.Local = Matrix4x4.Transpose(cell.Transform);
                    WorldBuffer.Update(context);
                }

                context.SetVertexBuffer(cell.VertexBuffer, cell.Stride);
                context.SetIndexBuffer(cell.IndexBuffer, Format.R32UInt, 0);
                context.VSSetConstantBuffer(WorldBuffer, 0);
                context.VSSetConstantBuffer(camera.Value, 1);
                context.SetGraphicsPipeline(geometry);
                context.DrawIndexedInstanced(cell.Terrain.IndicesCount, 1, 0, 0, 0);
            }
        }

        public void VisibilityTest(IGraphicsContext context)
        {
        }
    }
}