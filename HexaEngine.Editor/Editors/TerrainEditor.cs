namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Numerics;

    public struct CBColorMask
    {
        public Vector4 Mask;

        public CBColorMask(ChannelMask mask)
        {
            Mask = default;
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
        private readonly Queue<StaticTerrainCell> updateQueue = new();

        public void Draw(IGraphicsContext context)
        {
            if (Instance is not TerrainRendererComponent component)
            {
                return;
            }

            var grid = component.Grid;

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

                maskEdit = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
                {
                    VertexShader = "quad.hlsl",
                    PixelShader = "tools/terrain/edit/mask/ps.hlsl",
                }, new()
                {
                    DepthStencil = depthStencilD,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = BlendDescription.AlphaBlend,
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

            hoversOverTerrain = false;

            var ca = CameraManager.Current;

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

                    var scene = SceneManager.Current;

                    if (scene is null)
                    {
                        current = -1;
                        return;
                    }

                    var manager = scene.MaterialManager;

                    if (manager.Count == 0)
                    {
                        current = -1;
                    }

                    if (layer.Data == null)
                    {
                        if (ImGui.Button("Create new"))
                        {
                            layer.Data = new("New Material");
                        }

                        lock (manager.Materials)
                        {
                            ImGui.PushItemWidth(200);
                            //ComboHelper.ComboForAssetRef("##Mat", ref, AssetType.Material);

                            if (ImGui.BeginCombo("##Materials", string.Empty))
                            {
                                for (int i = 0; i < manager.Count; i++)
                                {
                                    var material = manager.Materials[i];
                                    if (ImGui.MenuItem(material.Name))
                                    {
                                        current = i;
                                    }
                                }
                                ImGui.EndCombo();
                            }
                            ImGui.PopItemWidth();
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
                    if (editMask)
                    {
                        var tuple = cell.GetLayerMask(grid.Layers[maskChannel]);
                        if (tuple != null)
                        {
                            context.PSSetShaderResource(0, tuple.Value.Item2.SRV);
                            context.PSSetSampler(0, maskSampler);
                            context.SetPipelineState(maskOverlay);
                            context.DrawIndexedInstanced(cell.Terrain.IndicesCount, 1, 0, 0, 0);
                        }
                    }

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

                        if (Mouse.IsDown(MouseButton.Left))
                        {
                            isDown = true;
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

                            if (editMask)
                            {
                                var tuple = cell.GetLayerMask(grid.Layers[maskChannel]);
                                tuple ??= cell.AddLayer(grid.Layers[maskChannel]);
                                CBColorMask colorMask = new(tuple.Value.Item1);
                                colorMask.Mask *= strength * Time.Delta;
                                maskBuffer.Update(context, colorMask);

                                context.PSSetShaderResource(0, null);
                                context.PSSetConstantBuffer(0, maskBuffer);
                                context.SetRenderTarget(tuple.Value.Item2.RTV, depthStencil.DSV);
                                context.SetPipelineState(maskEdit);
                                context.SetViewport(tuple.Value.Item2.Viewport);

                                Matrix4x4.Invert(cell.Transform, out var inverse);
                                var tlSize = tuple.Value.Item2.Viewport.Size / new Vector2(cell.BoundingBox.Size.X, cell.BoundingBox.Size.Z);
                                var vpSize = new Vector2(size) * tlSize;
                                var local = Vector3.Transform(position, inverse);
                                var pos = new Vector2(local.X, local.Z) / tlSize * tuple.Value.Item2.Viewport.Size - vpSize / 2f;

                                context.SetViewport(new(pos, vpSize));
                                context.DrawInstanced(4, 1, 0, 0);

                                hasAffected = true;
                                isEdited = true;
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