﻿using HexaEngine.Editor.Editors;

namespace HexaEngine.Editor.TerrainEditor
{
    using Hexa.NET.DebugDraw;
    using Hexa.NET.ImGui;
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using Hexa.NET.Mathematics.Noise;
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Editor.TerrainEditor.Shapes;
    using HexaEngine.Editor.TerrainEditor.Tools;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using System.Text;

    public class TerrainObjectEditor : IObjectEditor
    {
        internal static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(TerrainEditor));
        private IGraphicsPipelineState brushOverlay = null!;
        private ConstantBuffer<CBBrush> brushBuffer = null!;
        private ConstantBuffer<Matrix4x4> worldBuffer = null!;

        private readonly List<TerrainTool> tools = [];
        private TerrainTool? activeTool;
        private readonly TerrainToolContext toolContext = new();

        private bool isDown;

        private bool init;

        private bool editTerrain = false;

        private bool hoversOverTerrain;
        private Vector3 position;
        private Vector3 uv;
        private bool unsavedChanges;
        private bool unsavedDataDialogIsOpen;
        private Vector2 lastMousePosition;
        private readonly Queue<TerrainCell> updateQueue = new();

        public TerrainObjectEditor()
        {
            tools.Add(new RaiseLowerTool());
            tools.Add(new SmoothTool());
            tools.Add(new FlattenTool());
            tools.Add(new LayerPaintTool());
            tools.Add(new NoiseTool());

            toolContext.Shape = new CircleToolShape();

            Application.MainWindow.Closing += MainWindowClosing;
        }

        private void MainWindowClosing(object? sender, CloseEventArgs e)
        {
            if (unsavedChanges)
            {
                e.Handled = true;
                if (!unsavedDataDialogIsOpen)
                {
                    MessageBox.Show("(Material Editor) Unsaved changes", $"Do you want to save the changes in terrain?", this, (messageBox, state) =>
                    {
                        if (state is not TerrainObjectEditor terrainEditor)
                        {
                            return;
                        }

                        if (messageBox.Result == MessageBoxResult.Yes)
                        {
                            terrainEditor.signalSave = true;
                        }

                        if (messageBox.Result == MessageBoxResult.No)
                        {
                            terrainEditor.unsavedChanges = false;
                            Application.MainWindow.Close();
                        }

                        terrainEditor.unsavedDataDialogIsOpen = false;
                    }, MessageBoxType.YesNoCancel);
                    unsavedDataDialogIsOpen = true;
                }
            }
        }

        public string Name => "Terrain";

        public Type Type => typeof(TerrainRendererComponent);

        public object? Instance { get; set; }

        public bool IsEmpty => false;

        public bool IsHidden => false;

        public bool NoTable { get; set; }

        public string Symbol { get; } = "?";

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

            Vector2 mousePosition = Mouse.Position;
            if (editTerrain)
            {
                Vector3 rayDir = Mouse.ScreenToWorld(camera.Transform.Projection, camera.Transform.ViewInv, Application.MainWindow.WindowViewport);
                Vector3 rayPos = camera.Transform.GlobalPosition;
                Ray ray = new(rayPos, Vector3.Normalize(rayDir));
                toolContext.Ray = ray;
                for (int i = 0; i < terrain.Count; i++)
                {
                    var cell = terrain[i];
                    var localRay = Ray.Transform(ray, cell.TransformInv);
                    if (cell.IntersectRay(localRay, out var pointInTerrain))
                    {
                        pointInTerrain = Vector3.Transform(pointInTerrain, cell.Transform);
                        toolContext.Position = position = pointInTerrain;
                        toolContext.UV = uv = position / cell.BoundingBox.Size;
                        hoversOverTerrain = true;
                    }
                }
            }

            toolContext.Grid = terrain;

            ToolsMenu();
            CellsMenu(terrain);
            GenerationMenu(terrain);
            DrawEdit(context, component, terrain);

            if (updateQueue.Count > 0)
            {
                terrain.GenerateBoundingBox();
                component.GameObject.SendUpdateTransformed();
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

            if (signalSave || ImGui.Button("Save"))
            {
                Save(context, component, terrain);
                if (signalSave)
                {
                    signalSave = false;
                    Application.MainWindow.Close();
                }
            }

            return changed;
        }

        private void DrawEdit(IGraphicsContext context, TerrainRendererComponent component, TerrainGrid terrain)
        {
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
                    *worldBuffer.Local = Matrix4x4.Transpose(cell.Transform);
                    worldBuffer.Update(context);
                }
                if (editTerrain)
                {
                    if (hoversOverTerrain)
                    {
                        int activeLOD = cell.CurrentLODLevel;

                        DrawBrushOverlay(context, cell);

                        bool wasDown = false;

                        if (Mouse.IsDown(MouseButton.Left) && SceneWindow.IsHovered)
                        {
                            bool first = !isDown;
                            isDown = true;
                            bool hasAffected = false;

                            if (first)
                            {
                                activeTool.OnMouseDown(position);
                            }

                            activeTool.OnMouseMove(position);

                            if (editTerrain)
                            {
                                hasAffected |= activeTool.Modify(context, toolContext);
                                unsavedChanges = true;
                            }

                            if (editTerrain && hasAffected)
                            {
                                cell.GenerateLevel(activeLOD);

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
                        else
                        {
                            if (isDown)
                            {
                                wasDown = true;
                                isDown = false;
                            }
                        }

                        if (wasDown)
                        {
                            GenerateInactiveLOD(cell, activeLOD);
                        }
                    }
                }
            }
        }

        private void DrawBrushOverlay(IGraphicsContext context, TerrainCell cell)
        {
            var swapChain = Application.MainWindow.SwapChain;
            CBBrush brush = new(position, activeTool!.Size, activeTool.BlendStart, activeTool.BlendEnd);
            brushBuffer.Update(context, brush);

            cell.Bind(context);
            context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
            context.SetViewport(Application.MainWindow.WindowViewport);

            context.SetGraphicsPipelineState(brushOverlay);

            context.DrawIndexedInstanced(cell.Mesh.IndexCount, 1, 0, 0, 0);

            context.SetRenderTarget(null, null);
            context.SetViewport(default);

            context.SetGraphicsPipelineState(null);
        }

        private static void GenerateInactiveLOD(TerrainCell cell, int activeLevel)
        {
            for (int i = 0; i < cell.LODLevels; i++)
            {
                if (activeLevel == i)
                {
                    continue;
                }
                cell.GenerateLevel(i);
            }
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
                EdgeFadeMode edgeFadeMode = activeTool.EdgeFadeMode;
                if (ComboEnumHelper<EdgeFadeMode>.Combo("Fade Mode", ref edgeFadeMode))
                {
                    activeTool.EdgeFadeMode = edgeFadeMode;
                }

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

                activeTool.DrawSettings(toolContext);
            }
        }

        private static void DrawCellOverlay(TerrainCell cell)
        {
            if (ImGui.IsItemHovered())
            {
                DebugDraw.DrawBox(cell.Offset + new Vector3(16, 0, 16), Quaternion.Identity, 16, 1, 16, Colors.Yellow);
            }
        }

        private void CellsMenu(TerrainGrid grid)
        {
            if (ImGui.CollapsingHeader("Info"))
            {
                for (int i = 0; i < grid.Count; i++)
                {
                    var cell = grid[i];
                    if (ImGui.TreeNodeEx($"{cell.ID}"))
                    {
                        DrawCellOverlay(cell);
                        for (int j = 0; j < cell.DrawLayers.Count; j++)
                        {
                            var drawLayer = cell.DrawLayers[j];
                            for (var k = 0; k < drawLayer.LayerCount; k++)
                            {
                                var layer = drawLayer.LayerGroup[k];
                                ImGui.TreeNodeEx($"{layer.Name}", ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                            }
                        }
                        ImGui.TreePop();
                    }
                    else
                    {
                        DrawCellOverlay(cell);
                    }
                }
            }

            if (ImGui.CollapsingHeader("Cells"))
            {
                if (!ImGui.BeginTable("##T", 2, ImGuiTableFlags.SizingFixedFit))
                {
                    return;
                }

                ImGui.TableSetupColumn("");
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
                for (int i = 0; i < grid.Count; i++)
                {
                    var cell = grid[i];

                    if (cell.Right != null && cell.Top != null)
                    {
                        continue;
                    }

                    ImGui.TableNextRow();

                    if (cell.Right == null)
                    {
                        ImGui.TableSetColumnIndex(0);
                        if (ImGui.Button($"{cell.ID} Add Tile X+"))
                        {
                            TerrainCell terrainCell = grid.CreateCell(cell.ID + new Point2(1, 0));
                            grid.Add(terrainCell);
                            grid.FindNeighbors();
                            unsavedChanges = true;
                        }

                        if (ImGui.IsItemHovered())
                        {
                            DebugDraw.DrawBox(cell.Offset + new Vector3(48, 0, 16), Quaternion.Identity, 16, 1, 16, Colors.Yellow);
                        }
                    }

                    if (cell.Top == null)
                    {
                        ImGui.TableSetColumnIndex(1);
                        if (ImGui.Button($"{cell.ID} Add Tile Z+"))
                        {
                            TerrainCell terrainCell = grid.CreateCell(cell.ID + new Point2(0, 1));
                            grid.Add(terrainCell);
                            grid.FindNeighbors();
                            unsavedChanges = true;
                        }

                        if (ImGui.IsItemHovered())
                        {
                            DebugDraw.DrawBox(cell.Offset + new Vector3(16, 0, 48), Quaternion.Identity, 16, 1, 16, Colors.Yellow);
                        }
                    }
                }
                ImGui.EndTable();
            }
        }

        private void Save(IGraphicsContext context, TerrainRendererComponent component, TerrainGrid terrain)
        {
            unsavedChanges = false;

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

            metadata.Lock();
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
                metadata.ReleaseLock();
                stream?.Dispose();
            }
        }

        private void Init(IGraphicsContext context)
        {
            if (!init)
            {
                var device = context.Device;
                var inputElements = TerrainCellData.InputElements;

                brushBuffer = new(CpuAccessFlags.Write);
                worldBuffer = new(CpuAccessFlags.Write);

                brushOverlay = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
                {
                    VertexShader = "tools/terrain/overlay/vs.hlsl",
                    PixelShader = "tools/terrain/overlay/ps.hlsl",
                }, new()
                {
                    DepthStencil = DepthStencilDescription.None,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = BlendDescription.AlphaBlend,
                    Topology = PrimitiveTopology.TriangleList,
                    InputElements = inputElements
                });

                brushOverlay.Bindings.SetCBV("WorldBuffer", worldBuffer);
                brushOverlay.Bindings.SetCBV("CBBrush", brushBuffer);

                for (int i = 0; i < tools.Count; i++)
                {
                    tools[i].Initialize(device);
                }

                init = true;
            }
        }

        private PerlinNoise perlinNoise = new();
        private int seed = 0;
        private int octaves = 10;
        private float persistence = 0.4f;
        private float amplitude = 2;
        private Vector2 scale = new(0.002f);
        private float min = 0f;
        private float max = 10f;
        private bool signalSave;

        private void GenerationMenu(TerrainGrid terrain)
        {
            if (ImGui.CollapsingHeader("Generation"))
            {
                if (ImGui.InputInt("Seed", ref seed))
                {
                    perlinNoise = new(seed);
                }
                ImGui.InputInt("Octaves", ref octaves);
                ImGui.InputFloat("Persistence", ref persistence);
                ImGui.InputFloat("Amplitude", ref amplitude);
                ImGui.InputFloat2("Scale", ref scale, "%.8g");
                ImGui.InputFloat("Min", ref min);
                ImGui.InputFloat("Max", ref max);
                if (ImGui.Button("Generate"))
                {
                    for (int i = 0; i < terrain.Count; i++)
                    {
                        var cell = terrain[i];
                        var heightMap = cell.CellData.HeightMap;
                        var height = heightMap.Height;
                        var width = heightMap.Width;
                        var offset = (Vector2)cell.ID * new Vector2(width - 1, height - 1);

                        for (uint y = 0; y < height; y++)
                        {
                            for (uint x = 0; x < width; x++)
                            {
                                uint index = height * y + x;

                                heightMap[index] = MathUtil.Lerp(min, max, (perlinNoise.OctaveNoise((x + offset.X) * scale.X, (y + offset.Y) * scale.Y, octaves, persistence, amplitude) + 1) / 2);
                            }
                        }
                    }

                    for (int i = 0; i < terrain.Count; i++)
                    {
                        terrain[i].FixHeightMap();
                    }

                    for (int i = 0; i < terrain.Count; i++)
                    {
                        terrain[i].GenerateLOD();
                    }

                    for (int i = 0; i < terrain.Count; i++)
                    {
                        terrain[i].FixNormals();
                        updateQueue.Enqueue(terrain[i]);
                    }
                }
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < tools.Count; i++)
            {
                tools[i].Dispose();
            }
            brushOverlay.Dispose();
            brushBuffer.Dispose();
            worldBuffer.Dispose();
        }
    }
}