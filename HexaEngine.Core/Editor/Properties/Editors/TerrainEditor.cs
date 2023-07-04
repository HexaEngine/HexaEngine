namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.IO.Terrains;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.ImGuiNET;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes.Components.Renderer;
    using System;
    using System.Numerics;

    internal class TerrainEditor : IObjectEditor
    {
        private IGraphicsPipeline brushOverlay;
        private ConstantBuffer<CBBrush> brushBuffer;
        private ConstantBuffer<Matrix4x4> WorldBuffer;
        private ResourceRef<IBuffer> camera;

        private IGraphicsPipeline maskOverlay;
        private RWTexture2D<Vector4> mask;
        private ISamplerState maskSampler;

        private bool init;

        public string Name => "Terrain";

        public Type Type => typeof(TerrainRendererComponent);

        public object? Instance { get; set; }

        public bool IsEmpty => false;

        public bool NoTable { get; set; }

        private bool editTerrain = false;
        private float size = 2f;
        private float strength = 10;
        private bool raise = true;

        private bool editMask = false;
        private int maskChannel = 0;

        private bool hoversOverTerrain;
        private Vector3 position;
        private readonly Queue<TerrainCell> updateQueue = new();

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
                var inputElements = Terrain.InputElements;

                brushOverlay = device.CreateGraphicsPipeline(new GraphicsPipelineDesc()
                {
                    VertexShader = "tools/terrain/brush/vs.hlsl",
                    PixelShader = "tools/terrain/brush/ps.hlsl",
                }, new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.None,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                }, inputElements);

                maskOverlay = device.CreateGraphicsPipeline(new GraphicsPipelineDesc()
                {
                    VertexShader = "tools/terrain/mask/vs.hlsl",
                    PixelShader = "tools/terrain/mask/ps.hlsl",
                }, new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.None,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                }, inputElements);
                maskSampler = device.CreateSamplerState(SamplerDescription.PointClamp);

                brushBuffer = new(device, CpuAccessFlags.Write);
                WorldBuffer = new(device, CpuAccessFlags.Write);
                camera = ResourceManager2.Shared.GetBuffer("CBCamera");

                mask = new(device, Format.R32G32B32A32Float, 32, 32, 1, 1, CpuAccessFlags.Write);

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
                    hoversOverTerrain = true;
                }
            }

            ImGui.Checkbox("Edit Terrain", ref editTerrain);
            ImGui.InputFloat("Size", ref size);
            ImGui.InputFloat("Strength", ref strength);
            ImGui.Checkbox("Raise", ref raise);

            ImGui.Checkbox("Edit Mask", ref editMask);
            ImGui.InputInt("Mask Channel", ref maskChannel);

            var swapChain = Application.MainWindow.SwapChain;
            context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
            context.SetViewport(Application.MainWindow.WindowViewport);

            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                unsafe
                {
                    *WorldBuffer.Local = Matrix4x4.Transpose(cell.Transform);
                    WorldBuffer.Update(context);
                }

                if (ImGui.CollapsingHeader(($"{cell.ID}")))
                {
                    if (cell.Right == null && ImGui.Button($"{cell.ID} Add Tile X+"))
                    {
                        HeightMap heightMap = new(32, 32);
                        heightMap.GenerateEmpty();
                        TerrainCell terrainCell = new(context.Device, heightMap, true);
                        terrainCell.ID = cell.ID + new Point2(1, 0);
                        terrainCell.Offset = cell.Offset + new Vector3(cell.HeightMap.Width - 1, 0, 0);

                        grid.Add(terrainCell);
                        grid.FindNeighbors();
                    }

                    if (cell.Top == null && ImGui.Button($"{cell.ID} Add Tile Z+"))
                    {
                        HeightMap heightMap = new(32, 32);
                        heightMap.GenerateEmpty();
                        TerrainCell terrainCell = new(context.Device, heightMap, true);
                        terrainCell.ID = cell.ID + new Point2(0, 1);
                        terrainCell.Offset = cell.Offset + new Vector3(0, 0, cell.HeightMap.Height - 1);

                        grid.Add(terrainCell);
                        grid.FindNeighbors();
                    }
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
                        context.PSSetShaderResource(0, mask.SRV);
                        context.PSSetSampler(0, maskSampler);
                        context.SetGraphicsPipeline(maskOverlay);
                        context.DrawIndexedInstanced(cell.Terrain.IndicesCount, 1, 0, 0, 0);
                    }

                    if (hoversOverTerrain)
                    {
                        unsafe
                        {
                            brushBuffer.Local->Pos = position;
                            brushBuffer.Local->Radius = size;
                        }
                        brushBuffer.Update(context);

                        context.SetGraphicsPipeline(brushOverlay);
                        context.DrawIndexedInstanced(cell.Terrain.IndicesCount, 1, 0, 0, 0);

                        if (Mouse.IsDown(MouseButton.Left))
                        {
                            bool hasAffected = false;
                            for (int j = 0; j < cell.Terrain.VerticesCount; j++)
                            {
                                var vertex = Vector3.Transform(cell.Terrain.Positions[j], cell.Transform);
                                Vector3 p1 = new(vertex.X, 0, vertex.Z);
                                Vector3 p2 = new(position.X, 0, position.Z);
                                Vector2 uv = new(p1.X / cell.Terrain.Width, p1.Z / cell.Terrain.Height);
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
                                    else if (editMask)
                                    {
                                        var vec = mask[uv];
                                        if (raise)
                                        {
                                            vec[maskChannel] += temp * Time.Delta;
                                        }
                                        else
                                        {
                                            vec[maskChannel] -= temp * Time.Delta;
                                        }
                                        vec[maskChannel] = Math.Clamp(vec[maskChannel], 0, 1);
                                        vec[3] = 0;
                                        mask[uv] = vec;
                                    }

                                    hasAffected = true;
                                }
                            }

                            if (editMask && hasAffected)
                            {
                                mask.Write(context);
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
                    }
                }
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
        }
    }
}