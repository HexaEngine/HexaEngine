using HexaEngine.Editor.Editors;

namespace HexaEngine.Editor.TerrainEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Editor.MaterialEditor;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Editor.TerrainEditor.Shapes;
    using HexaEngine.Editor.TerrainEditor.Tools;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using System.Text;

    public class TerrainObjectEditor : IObjectEditor
    {
        private IGraphicsPipelineState brushOverlay;
        private ConstantBuffer<CBBrush> brushBuffer;
        private ConstantBuffer<CBColorMask> maskBuffer;
        private ConstantBuffer<Matrix4x4> WorldBuffer;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private DepthStencil depthStencil;
        private Texture2D texture;

        private readonly List<TerrainTool> tools = new();
        private TerrainTool? activeTool;
        private TerrainToolContext toolContext = new();

        private IGraphicsPipelineState maskEdit;
        private ISamplerState maskSampler;

        private bool isDown;
        private bool isEdited;

        private bool init;

        public TerrainObjectEditor()
        {
            tools.Add(new RaiseLowerTool());
            tools.Add(new SmoothTool());
            tools.Add(new FlattenTool());
            toolContext.Shape = new CircleToolShape();
        }

        public string Name => "Terrain";

        public Type Type => typeof(TerrainRendererComponent);

        public object? Instance { get; set; }

        public bool IsEmpty => false;

        public bool IsHidden => false;

        public bool NoTable { get; set; }

        private bool editTerrain = false;
        private float size = 2f;
        private float strength = 10;
        private float edgeStart = 0;
        private float edgeEnd = 1;
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

        private readonly Queue<TerrainCell> updateQueue = new();

        public bool Draw(IGraphicsContext context)
        {
            if (Instance is not TerrainRendererComponent component)
            {
                return false;
            }

            bool changed = false;

            var terrain = component.Terrain;

            var asset = component.TerrainAsset;
            if (ComboHelper.ComboForAssetRef("Terrain", ref asset, AssetType.Terrain))
            {
                changed = true;
                component.TerrainAsset = asset;
            }

            if (terrain == null)
            {
                if (ImGui.Button("Create new"))
                {
                    changed = true;
                    component.CreateNew();
                }

                return changed;
            }

            var camera = CameraManager.Current;

            if (camera == null)
            {
                return changed;
            }

            Init(context);

            hoversOverTerrain = false;

            var mousePosition = Mouse.Position;
            bool mouseMoved = mousePosition != lastMousePosition;

            Vector3 rayDir = Mouse.ScreenToWorld(camera.Transform.Projection, camera.Transform.ViewInv, Application.MainWindow.WindowViewport);
            Vector3 rayPos = camera.Transform.GlobalPosition;
            Ray ray = new(rayPos, Vector3.Normalize(rayDir));
            toolContext.Ray = ray;
            for (int i = 0; i < terrain.Count; i++)
            {
                var cell = terrain[i];
                if (cell.IntersectRay(ray, cell.Transform, out var pointInTerrain))
                {
                    toolContext.Position = position = pointInTerrain;
                    toolContext.UV = uv = position / cell.BoundingBox.Size;
                    hoversOverTerrain = true;
                }
            }

            ToolsMenu();

            LayersMenu(terrain);

            CellsMenu(terrain);

            var shape = toolContext.Shape;
            for (int i = 0; i < terrain.Count; i++)
            {
                var cell = terrain[i];

                if (activeTool == null)
                {
                    break;
                }

                if (!shape.TestCell(toolContext, activeTool, cell))
                {
                    continue;
                }

                toolContext.Cell = cell;

                unsafe
                {
                    *WorldBuffer.Local = Matrix4x4.Transpose(cell.Transform);
                    WorldBuffer.Update(context);
                }
                if (editTerrain || editMask)
                {
                    if (hoversOverTerrain)
                    {
                        var swapChain = Application.MainWindow.SwapChain;

                        CBBrush brush = new(position, activeTool.Size, activeTool.BlendStart, activeTool.BlendEnd);
                        brushBuffer.Update(context, brush);

                        cell.Bind(context);
                        context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
                        context.SetViewport(Application.MainWindow.WindowViewport);
                        context.VSSetConstantBuffer(0, WorldBuffer);
                        context.VSSetConstantBuffer(1, this.camera.Value);
                        context.PSSetConstantBuffer(0, brushBuffer);
                        context.PSSetConstantBuffer(1, this.camera.Value);
                        context.SetPipelineState(brushOverlay);

                        context.DrawIndexedInstanced(cell.Mesh.IndexCount, 1, 0, 0, 0);

                        context.SetRenderTarget(null, null);
                        context.SetViewport(default);
                        context.VSSetConstantBuffer(0, null);
                        context.VSSetConstantBuffer(1, null);
                        context.PSSetConstantBuffer(0, null);
                        context.PSSetConstantBuffer(1, null);
                        context.SetPipelineState(null);

                        if (Mouse.IsDown(MouseButton.Left) && SceneWindow.IsHovered)
                        {
                            bool first = !isDown;
                            isDown = true;
                            bool hasAffected = false;

                            if (first)
                            {
                                activeTool.OnMouseDown(position);
                            }

                            if (editTerrain)
                            {
                                hasAffected |= activeTool.Modify(toolContext);
                                hasChanged = true;
                            }

                            if (editMask)
                            {
                                hasAffected |= EditMask(context, terrain, cell);
                                hasChanged = true;
                            }

                            if (editTerrain && hasAffected)
                            {
                                cell.Generate();

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
                cell.UpdateVertexBuffer(context);
            }

            lastMousePosition = mousePosition;

            if (ImGui.Button("Regenerate All"))
            {
                terrain.RegenerateAll(context);
            }

            if (ImGui.Button("Save"))
            {
                Save(context, component, terrain);
            }

            return changed;
        }

        private void ToolsMenu()
        {
            ImGui.Checkbox("Edit Terrain", ref editTerrain);

            if (ImGui.BeginListBox("Tools"))
            {
                for (int i = 0; i < tools.Count; i++)
                {
                    var tool = tools[i];
                    bool isSelected = activeTool == tool;
                    if (ImGui.Selectable(tool.Name, isSelected))
                    {
                        activeTool = tool;
                    }
                }
                ImGui.EndListBox();
            }

            if (activeTool != null)
            {
                float size = activeTool.Size;
                if (ImGui.InputFloat("Size", ref size))
                {
                    activeTool.Size = size;
                }

                float strength = activeTool.Strength;
                if (ImGui.InputFloat("Strength", ref strength))
                {
                    activeTool.Strength = strength;
                }

                float blendStart = activeTool.BlendStart;
                if (ImGui.InputFloat("Blend Start", ref blendStart))
                {
                    activeTool.BlendStart = blendStart;
                }

                float blendEnd = activeTool.BlendEnd;
                if (ImGui.InputFloat("Blend End", ref blendEnd))
                {
                    activeTool.BlendEnd = blendEnd;
                }

                activeTool.DrawSettings();
            }
        }

        private void CellsMenu(TerrainGrid grid)
        {
            if (ImGui.CollapsingHeader("Cells"))
            {
                for (int i = 0; i < grid.Count; i++)
                {
                    var cell = grid[i];

                    if (ImGui.TreeNode($"{cell.ID}"))
                    {
                        if (cell.Right == null && ImGui.Button($"{cell.ID} Add Tile X+"))
                        {
                            TerrainCell terrainCell = grid.CreateCell(cell.ID + new Point2(1, 0));
                            grid.Add(terrainCell);
                            grid.FindNeighbors();
                            hasChanged = true;
                        }

                        if (cell.Top == null && cell.Right == null)
                        {
                            ImGui.SameLine();
                        }

                        if (cell.Top == null && ImGui.Button($"{cell.ID} Add Tile Z+"))
                        {
                            TerrainCell terrainCell = grid.CreateCell(cell.ID + new Point2(0, 1));
                            grid.Add(terrainCell);
                            grid.FindNeighbors();
                            hasChanged = true;
                        }

                        ImGui.TreePop();
                    }
                }
            }
        }

        private void LayersMenu(TerrainGrid grid)
        {
            if (ImGui.CollapsingHeader("Layers"))
            {
                if (ImGui.Button("Add Layer"))
                {
                    grid.Layers.Add(new("New Layer"));
                    hasChanged = true;
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
                        hasChanged = true;
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
                        hasChanged = true;
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
        }

        private void Save(IGraphicsContext context, TerrainRendererComponent component, TerrainGrid terrain)
        {
            hasChanged = false;

            for (int i = 0; i < terrain.Count; i++)
            {
                var cell = terrain[i];
                for (int j = 0; j < cell.DrawLayers.Count; j++)
                {
                    var layer = cell.DrawLayers[j];
                    layer.LayerGroup.Mask.ReadLayerMask(context, layer.Mask);
                }
            }

            var asset = component.TerrainAsset;
            var data = terrain.TerrainData;
            var metadata = asset.GetSourceMetadata();

            if (metadata == null)
            {
                return;
            }

            var path = metadata.GetFullPath();
            Stream? stream = null;

            try
            {
                stream = File.Create(path);
                data.Save(stream, Encoding.UTF8, Endianness.LittleEndian, Compression.LZ4);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Error($"Failed to save terrain, {path}");
                if (Debugger.IsAttached)
                {
                    throw;
                }
            }
            finally
            {
                stream?.Dispose();
            }
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
                    Blend = BlendDescription.AlphaBlend,
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

        private bool EditTerrain(TerrainCell cell, CBBrush brush)
        {
            bool hasAffected = false;
            for (int j = 0; j < cell.Mesh.VertexCount; j++)
            {
                var vertex = cell.LODData.Positions[j];
                var vertexWS = Vector3.Transform(vertex, cell.Transform);
                Vector3 p1 = new(vertexWS.X, 0, vertexWS.Z);
                Vector3 p2 = new(position.X, 0, position.Z);

                float distance = Vector3.Distance(p2, p1);

                if (distance > size)
                {
                    continue;
                }

                float edgeFade = brush.ComputeEdgeFade(distance);
                float value = strength * edgeFade * Time.Delta;

                var heightMap = cell.CellData.HeightMap;
                Vector2 cTex = new Vector2(vertex.X, vertex.Z) / new Vector2(32);
                Vector2 pos = cTex * heightMap.Size;

                uint index = heightMap.GetIndexFor((uint)pos.X, (uint)pos.Y);

                if (raise)
                {
                    heightMap[index] += value;
                }
                else
                {
                    heightMap[index] -= value;
                }

                hasAffected = true;
            }

            return hasAffected;
        }

        private bool EditMask(IGraphicsContext context, TerrainGrid grid, TerrainCell cell)
        {
            Viewport vp = texture.Viewport;

            Vector3 local = Vector3.Transform(position, cell.TransformInv);
            Vector2 pos = new Vector2(local.X, local.Z) / new Vector2(cell.BoundingBox.Size.X, cell.BoundingBox.Size.Z);

            BoundingSphere sphere = new(new Vector3(pos.X, 0, pos.Y) * cell.BoundingBox.Size, size);

            ImGui.Text($"{cell.ID}, {pos}");

            if (!cell.BoundingBox.Intersects(sphere))
            {
                return false;
            }

            bool hasAffected;
            Texture2D? maskTex = cell.GetLayerMask(grid.Layers[maskChannel], out ChannelMask mask);
            maskTex ??= cell.AddLayer(grid.Layers[maskChannel], out mask);

            ImGui.Text($"{cell.ID}, {mask}");

            float sizeFactor = vp.Size.X / cell.BoundingBox.Size.X;

            CBBrush brush = new(new(pos.X, 0, pos.Y), size / sizeFactor, edgeStart, edgeEnd);

            brushBuffer.Update(context, brush);

            CBColorMask colorMask = new(mask);

            colorMask.Mask *= strength * Time.Delta;
            maskBuffer.Update(context, colorMask);

            context.CopyResource(texture, maskTex);

            context.PSSetShaderResource(0, texture.SRV);
            context.PSSetSampler(0, maskSampler);
            context.PSSetConstantBuffer(0, brushBuffer);
            context.PSSetConstantBuffer(1, maskBuffer);
            context.SetRenderTarget(maskTex.RTV, depthStencil.DSV);
            context.SetViewport(vp);

            context.SetPipelineState(maskEdit);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);

            context.SetViewport(default);
            context.SetRenderTarget(null, null);
            context.PSSetShaderResource(0, null);
            context.PSSetSampler(0, null);
            context.PSSetConstantBuffer(0, null);
            context.PSSetConstantBuffer(1, null);

            hasAffected = true;
            isEdited = true;
            return hasAffected;
        }

        public void Dispose()
        {
            brushOverlay.Dispose();
            brushBuffer.Dispose();
            WorldBuffer.Dispose();
            maskSampler.Dispose();
            maskEdit.Dispose();
            maskBuffer.Dispose();
        }
    }
}