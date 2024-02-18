namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.MaterialEditor;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Mathematics;
    using HexaEngine.Mathematics.Noise;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Numerics;

    public struct CBColorMask
    {
        public Vector4 Mask;
        public Vector2 Location;
        public float Range;
        public float padding;

        public CBColorMask(ChannelMask mask, Vector2 location, float range)
        {
            Mask = new(-1);
            if ((mask & ChannelMask.R) != 0)
            {
                Mask.X = 1;
            }
            if ((mask & ChannelMask.G) != 0)
            {
                Mask.Y = 1;
            }
            if ((mask & ChannelMask.B) != 0)
            {
                Mask.Z = 1;
            }
            if ((mask & ChannelMask.A) != 0)
            {
                Mask.W = 1;
            }
            Location = location;
            Range = range;
        }
    }

    internal class TerrainEditor : IObjectEditor
    {
        private IGraphicsPipelineState brushOverlay;
        private ConstantBuffer<CBBrush> brushBuffer;
        private ConstantBuffer<CBColorMask> maskBuffer;
        private ConstantBuffer<Matrix4x4> WorldBuffer;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private DepthStencil depthStencil;
        private Texture2D texture;

        private IGraphicsPipelineState maskOverlay;
        private IGraphicsPipelineState maskEdit;
        private ISamplerState maskSampler;

        private bool isDown;
        private bool isEdited;

        private bool init;

        public string Name => "Terrain";

        public Type Type => typeof(TerrainRendererComponent);

        public object? Instance { get; set; }

        public bool IsEmpty => false;

        public bool IsHidden => false;

        public bool NoTable { get; set; }

        private bool editTerrain = false;
        private float size = 2f;
        private float strength = 10;
        private bool raise = true;

        private bool editMask = false;
        private int maskChannel = 0;

        private bool hoversOverTerrain;
        private Vector3 position;
        private Vector3 uv;
        private bool hasChanged;
        private bool isActive;
        private bool hasFileSaved;
        private int current;
        private Vector2 lastMousePosition;

        private int seed = 0;
        private int octaves = 2;
        private float persistence = 0.5f;
        private float amplitude = 16;
        private float maxHeight = 10;
        private float minHeight = 0;
        private Vector2 scale = new(0.001f);

        private readonly Queue<StaticTerrainCell> updateQueue = new();

        public bool Draw(IGraphicsContext context)
        {
            if (Instance is not TerrainRendererComponent component)
            {
                return false;
            }
            bool changed = false;

            var grid = component.Grid;

            Init(context);

            hoversOverTerrain = false;

            var ca = CameraManager.Current;

            var mousePosition = Mouse.Position;
            bool mouseMoved = mousePosition != lastMousePosition;

            Vector3 rayDir = Mouse.ScreenToWorld(ca.Transform.Projection, ca.Transform.ViewInv, Application.MainWindow.WindowViewport);
            Vector3 rayPos = ca.Transform.GlobalPosition;
            Ray ray = new(rayPos, Vector3.Normalize(rayDir));
            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                if (cell.Terrain.IntersectRay(ray, cell.Transform, out var pointInTerrain))
                {
                    position = pointInTerrain;
                    uv = position / cell.BoundingBox.Size;
                    hoversOverTerrain = true;
                }
            }

            ImGui.Checkbox("Edit Terrain", ref editTerrain);
            ImGui.InputFloat("Size", ref size);
            ImGui.InputFloat("Strength", ref strength);
            ImGui.Checkbox("Raise", ref raise);

            var swapChain = Application.MainWindow.SwapChain;
            context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
            context.SetViewport(Application.MainWindow.WindowViewport);

            if (ImGui.CollapsingHeader("Layers"))
            {
                if (ImGui.Button("Add Layer"))
                {
                    grid.Layers.Add(new("New Layer"));
                }
                ImGui.Checkbox("Edit Mask", ref editMask);
                ImGui.BeginListBox("##LayerBox");
                for (int i = 0; i < grid.Layers.Count; i++)
                {
                    var layer = grid.Layers[i];
                    if (ImGui.MenuItem(layer.Name, i == maskChannel))
                    {
                        maskChannel = i;
                    }
                }
                ImGui.EndListBox();

                if (maskChannel > -1 && maskChannel < grid.Layers.Count)
                {
                    var layer = grid.Layers[maskChannel];
                    var name = layer.Name;
                    if (ImGui.InputText("Name", ref name, 256))
                    {
                        layer.Name = name;
                    }

                    ImGui.SeparatorText("Material");

                    var assetRef = layer.Material;

                    if (ComboHelper.ComboForAssetRef("##Mat", ref assetRef, AssetType.Material))
                    {
                        layer.Material = assetRef;
                        for (int i = 0; i < grid.Count; i++)
                        {
                            grid[i].UpdateLayer(layer);
                        }
                    }

                    ImGui.SameLine();

                    if (ImGui.SmallButton($"\xE70F"))
                    {
                        if (WindowManager.TryGetWindow<MaterialEditorWindow>(out var materialEditor))
                        {
                            materialEditor.Material = assetRef;
                            materialEditor.Focus();
                        }
                    }
                }
            }

            if (ImGui.CollapsingHeader("Cells"))
            {
                for (int i = 0; i < grid.Count; i++)
                {
                    var cell = grid[i];

                    if (ImGui.TreeNode($"{cell.ID}"))
                    {
                        if (cell.Right == null && ImGui.Button($"{cell.ID} Add Tile X+"))
                        {
                            HeightMap heightMap = new(32, 32);
                            heightMap.GenerateEmpty();
                            StaticTerrainCell terrainCell = new(context.Device, heightMap, true);
                            terrainCell.ID = cell.ID + new Point2(1, 0);
                            terrainCell.Offset = cell.Offset + new Vector3(cell.HeightMap.Width - 1, 0, 0);

                            grid.Add(terrainCell);
                            grid.FindNeighbors();
                        }

                        if (cell.Top == null && cell.Right == null)
                        {
                            ImGui.SameLine();
                        }

                        if (cell.Top == null && ImGui.Button($"{cell.ID} Add Tile Z+"))
                        {
                            HeightMap heightMap = new(32, 32);
                            heightMap.GenerateEmpty();
                            StaticTerrainCell terrainCell = new(context.Device, heightMap, true);
                            terrainCell.ID = cell.ID + new Point2(0, 1);
                            terrainCell.Offset = cell.Offset + new Vector3(0, 0, cell.HeightMap.Height - 1);

                            grid.Add(terrainCell);
                            grid.FindNeighbors();
                        }

                        ImGui.TreePop();
                    }
                }
            }

            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                unsafe
                {
                    *WorldBuffer.Local = Matrix4x4.Transpose(cell.Transform);
                    WorldBuffer.Update(context);
                }
                if (editTerrain || editMask)
                {
                    context.SetVertexBuffer(cell.VertexBuffer, cell.Stride);
                    context.SetIndexBuffer(cell.IndexBuffer, Format.R32UInt, 0);
                    context.VSSetConstantBuffer(0, WorldBuffer);
                    context.VSSetConstantBuffer(1, camera.Value);
                    context.PSSetConstantBuffer(0, brushBuffer);
                    context.PSSetConstantBuffer(1, camera.Value);

                    if (hoversOverTerrain)
                    {
                        unsafe
                        {
                            brushBuffer.Local->Pos = position;
                            brushBuffer.Local->Radius = size;
                        }
                        brushBuffer.Update(context);

                        context.SetPipelineState(brushOverlay);
                        context.DrawIndexedInstanced(cell.Terrain.IndicesCount, 1, 0, 0, 0);

                        if (Mouse.IsDown(MouseButton.Left) && SceneWindow.IsHovered)
                        {
                            bool first = !isDown;
                            isDown = true;
                            bool hasAffected = false;

                            if (editTerrain)
                            {
                                hasAffected |= EditTerrain(cell);
                            }

                            if (editMask)
                            {
                                hasAffected |= EditMask(context, grid, cell);
                            }

                            if (editTerrain && hasAffected)
                            {
                                cell.Terrain.Recalculate();
                                cell.Top?.Terrain.AverageEdge(Edge.ZNeg, cell.Terrain);
                                cell.Bottom?.Terrain.AverageEdge(Edge.ZPos, cell.Terrain);
                                cell.Right?.Terrain.AverageEdge(Edge.XNeg, cell.Terrain);
                                cell.Left?.Terrain.AverageEdge(Edge.XPos, cell.Terrain);
                                if (!updateQueue.Contains(cell))
                                {
                                    updateQueue.Enqueue(cell);
                                }
                                if (cell.Top != null && !updateQueue.Contains(cell.Top))
                                {
                                    updateQueue.Enqueue(cell.Top);
                                }
                                if (cell.Bottom != null && !updateQueue.Contains(cell.Bottom))
                                {
                                    updateQueue.Enqueue(cell.Bottom);
                                }
                                if (cell.Right != null && !updateQueue.Contains(cell.Right))
                                {
                                    updateQueue.Enqueue(cell.Right);
                                }
                                if (cell.Left != null && !updateQueue.Contains(cell.Left))
                                {
                                    updateQueue.Enqueue(cell.Left);
                                }
                            }
                        }

                        isDown = false;
                    }
                }
            }

            if (!isDown && isEdited)
            {
                context.ClearDepthStencilView(depthStencil.DSV, DepthStencilClearFlags.All, 1, 0);
                isEdited = false;
            }

            while (updateQueue.TryDequeue(out var cell))
            {
                cell.Terrain.WriteVertexBuffer(context, cell.VertexBuffer);
            }

            if (ImGui.CollapsingHeader("Procedural"))
            {
                ImGui.InputInt("Seed", ref seed);
                ImGui.InputFloat2("Noise Scale", ref scale);
                ImGui.InputInt("Octaves", ref octaves);
                ImGui.InputFloat("Persistence", ref persistence);
                ImGui.InputFloat("Amplitude", ref amplitude);

                ImGui.InputFloat("Max Height", ref maxHeight);
                ImGui.InputFloat("Min Height", ref minHeight);
                if (ImGui.Button("Generate"))
                {
                    PerlinNoise noise = new(seed);
                    for (int i = 0; i < grid.Count; i++)
                    {
                        var cell = grid[i];
                        Generate(noise, cell);

                        cell.Terrain.Recalculate();
                    }
                    for (int i = 0; i < grid.Count; i++)
                    {
                        var cell = grid[i];
                        cell.Top?.Terrain.AverageEdge(Edge.ZNeg, cell.Terrain);
                        cell.Bottom?.Terrain.AverageEdge(Edge.ZPos, cell.Terrain);
                        cell.Right?.Terrain.AverageEdge(Edge.XNeg, cell.Terrain);
                        cell.Left?.Terrain.AverageEdge(Edge.XPos, cell.Terrain);
                        cell.Terrain.WriteVertexBuffer(context, cell.VertexBuffer);
                    }
                }
            }

            lastMousePosition = mousePosition;

            return changed;
        }

        private void Init(IGraphicsContext context)
        {
            if (!init)
            {
                var device = context.Device;
                var inputElements = TerrainCellData.InputElements;

                depthStencil = new(device, Format.D32Float, 1024, 1024);

                brushOverlay = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
                {
                    VertexShader = "tools/terrain/overlay/brush/vs.hlsl",
                    PixelShader = "tools/terrain/overlay/brush/ps.hlsl",
                }, new()
                {
                    DepthStencil = DepthStencilDescription.None,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                    InputElements = inputElements
                });

                maskOverlay = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
                {
                    VertexShader = "tools/terrain/overlay/mask/vs.hlsl",
                    PixelShader = "tools/terrain/overlay/mask/ps.hlsl",
                }, new()
                {
                    DepthStencil = DepthStencilDescription.None,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                    InputElements = inputElements
                });
                DepthStencilDescription depthStencilD = new()
                {
                    DepthEnable = true,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthFunc = ComparisonFunction.Less,
                    StencilEnable = false,
                    StencilReadMask = 255,
                    StencilWriteMask = 255,
                    FrontFace = DepthStencilOperationDescription.DefaultFront,
                    BackFace = DepthStencilOperationDescription.DefaultBack
                };

                texture = new(device, Format.R16G16B16A16UNorm, 1024, 1024, 1, 1, CpuAccessFlags.None, GpuAccessFlags.Read);

                maskEdit = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
                {
                    VertexShader = "quad.hlsl",
                    PixelShader = "tools/terrain/edit/mask/ps.hlsl",
                }, new()
                {
                    DepthStencil = depthStencilD,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleStrip,
                    InputElements = inputElements
                });

                maskSampler = device.CreateSamplerState(SamplerStateDescription.PointClamp);

                brushBuffer = new(device, CpuAccessFlags.Write);
                WorldBuffer = new(device, CpuAccessFlags.Write);

                camera = SceneRenderer.Current.ResourceBuilder.GetConstantBuffer<CBCamera>("CBCamera");
                maskBuffer = new(device, CpuAccessFlags.Write);

                init = true;
            }
        }

        private bool EditTerrain(StaticTerrainCell cell)
        {
            bool hasAffected = false;
            for (int j = 0; j < cell.Terrain.VerticesCount; j++)
            {
                var vertex = Vector3.Transform(cell.Terrain.Positions[j], cell.Transform);
                Vector3 p1 = new(vertex.X, 0, vertex.Z);
                Vector3 p2 = new(position.X, 0, position.Z);
                Vector3 uv = cell.Terrain.UVs[j];
                //
                float distance = (p2 - p1).Length();
                //
                float cosValue = MathF.Cos(MathF.PI / 2 * distance / size);
                float temp = strength * MathF.Max(0, cosValue);
                //
                if (distance <= size)
                {
                    if (editTerrain)
                    {
                        if (raise)
                        {
                            cell.Terrain.Positions[j].Y += temp * Time.Delta;
                        }
                        else
                        {
                            cell.Terrain.Positions[j].Y -= temp * Time.Delta;
                        }
                    }

                    hasAffected = true;
                }
            }

            return hasAffected;
        }

        private bool EditMask(IGraphicsContext context, StaticTerrainGrid grid, StaticTerrainCell cell)
        {
            bool hasAffected;
            Texture2D? maskTex = cell.GetLayerMask(grid.Layers[maskChannel], out ChannelMask mask);
            maskTex ??= cell.AddLayer(grid.Layers[maskChannel], out mask);

            context.CopyResource(texture, maskTex);

            context.PSSetShaderResource(0, texture.SRV);
            context.PSSetSampler(0, maskSampler);
            context.PSSetConstantBuffer(0, maskBuffer);
            context.SetRenderTarget(maskTex.RTV, depthStencil.DSV);
            context.SetViewport(maskTex.Viewport);

            Matrix4x4.Invert(cell.Transform, out var inverse);
            var tlSize = maskTex.Viewport.Size / new Vector2(cell.BoundingBox.Size.X, cell.BoundingBox.Size.Z);
            var local = Vector3.Transform(position, inverse);
            var pos = new Vector2(local.X, local.Z) / tlSize;

            CBColorMask colorMask = new(mask, pos, size / (maskTex.Viewport.Size.X / cell.BoundingBox.Size.X));

            colorMask.Mask *= strength * Time.Delta;
            maskBuffer.Update(context, colorMask);

            context.SetPipelineState(maskEdit);
            context.DrawInstanced(4, 1, 0, 0);

            hasAffected = true;
            isEdited = true;
            return hasAffected;
        }

        private void Generate(PerlinNoise noise, StaticTerrainCell cell)
        {
            for (int j = 0; j < cell.Terrain.VerticesCount; j++)
            {
                var vertex = Vector3.Transform(cell.Terrain.Positions[j], cell.Transform);
                cell.Terrain.Positions[j].Y = MathUtil.Lerp(minHeight, maxHeight, noise.OctaveNoise(vertex.X * scale.X, vertex.Z * scale.Y, octaves, persistence, amplitude));
            }
        }

        public void Dispose()
        {
            brushOverlay.Dispose();
            brushBuffer.Dispose();
            WorldBuffer.Dispose();
            maskOverlay.Dispose();
            maskSampler.Dispose();
        }
    }
}