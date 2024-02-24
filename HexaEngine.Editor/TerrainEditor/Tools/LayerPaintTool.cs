namespace HexaEngine.Editor.TerrainEditor.Tools
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Editors;
    using HexaEngine.Editor.MaterialEditor;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using System.Numerics;

    public class LayerPaintTool : TerrainTool
    {
        private IGraphicsPipelineState maskEdit;

        private IComputePipeline occupationCheckPipeline;
        private IComputePipeline channelRemapPipeline;

        private Texture2D maskTexBuffer;
        private ISamplerState maskSampler;

        private ConstantBuffer<CBBrush> brushBuffer;
        private ConstantBuffer<CBColorMask> maskBuffer;
        private ConstantBuffer<CBChannelRemap> remapBuffer;

        private StructuredUavBuffer<UPoint4> channelBuffer;

        private int selectedLayerIndex;

        public override string Name { get; } = "Layer Paint";

        protected override void InitializeTool(IGraphicsDevice device)
        {
            maskEdit = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "tools/terrain/mask/ps.hlsl",
            }, new()
            {
                DepthStencil = new()
                {
                    DepthEnable = true,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthFunc = ComparisonFunction.Less,
                    StencilEnable = false,
                    StencilReadMask = 255,
                    StencilWriteMask = 255,
                    FrontFace = DepthStencilOperationDescription.DefaultFront,
                    BackFace = DepthStencilOperationDescription.DefaultBack
                },
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleStrip,
            });

            occupationCheckPipeline = device.CreateComputePipeline(new()
            {
                Path = "tools/terrain/mask/occupation.hlsl",
            });

            channelRemapPipeline = device.CreateComputePipeline(new()
            {
                Path = "tools/terrain/mask/remap.hlsl",
            });

            maskTexBuffer = new(device, Format.R16G16B16A16UNorm, 1024, 1024, 1, 1, CpuAccessFlags.None, GpuAccessFlags.Read | GpuAccessFlags.UA);
            maskSampler = device.CreateSamplerState(SamplerStateDescription.PointClamp);
            maskBuffer = new(device, CpuAccessFlags.Write);
            brushBuffer = new(device, CpuAccessFlags.Write);
            remapBuffer = new(device, CpuAccessFlags.Write);
            channelBuffer = new(device, 1, CpuAccessFlags.Read);
        }

        protected override void DisposeCore()
        {
            maskEdit.Dispose();
            occupationCheckPipeline.Dispose();
            channelRemapPipeline.Dispose();
            maskTexBuffer.Dispose();
            maskSampler.Dispose();
            maskBuffer.Dispose();
            brushBuffer.Dispose();
            remapBuffer.Dispose();
            channelBuffer.Dispose();
        }

        private unsafe UPoint4 ChannelOccupation(IGraphicsContext context, Texture2D texture)
        {
            Vector2 size = texture.Viewport.Size;

            remapBuffer.Update(context, new(size, 0, 0, Vector4.Zero));

            channelBuffer.Clear(context);

            context.CSSetConstantBuffer(0, remapBuffer);
            context.CSSetShaderResource(0, texture.SRV);
            context.CSSetUnorderedAccessView(0, (void*)channelBuffer.UAV.NativePointer);

            context.SetComputePipeline(occupationCheckPipeline);

            uint numGroupsX = (uint)Math.Ceiling(size.X / 32);
            uint numGroupsY = (uint)Math.Ceiling(size.Y / 32);
            context.Dispatch(numGroupsX, numGroupsY, 1);

            context.SetComputePipeline(null);

            context.CSSetConstantBuffer(0, null);
            context.CSSetShaderResource(0, null);
            context.CSSetUnorderedAccessView(0, null);

            channelBuffer.Read(context);

            return channelBuffer[0];
        }

        private unsafe void MoveChannel(IGraphicsContext context, Texture2D texture, uint source, uint destination, Vector4 factor)
        {
            Vector2 size = texture.Viewport.Size;
            remapBuffer.Update(context, new(size, source, destination, factor));

            context.CSSetConstantBuffer(0, remapBuffer);
            context.CSSetShaderResource(0, texture.SRV);
            context.CSSetUnorderedAccessView(0, (void*)maskTexBuffer.UAV.NativePointer);

            context.SetComputePipeline(channelRemapPipeline);

            uint numGroupsX = (uint)Math.Ceiling(size.X / 32);
            uint numGroupsY = (uint)Math.Ceiling(size.Y / 32);
            context.Dispatch(numGroupsX, numGroupsY, 1);

            context.SetComputePipeline(null);

            context.CSSetConstantBuffer(0, null);
            context.CSSetShaderResource(0, null);
            context.CSSetUnorderedAccessView(0, null);

            maskTexBuffer.CopyTo(context, texture);
        }

        private void CheckDrawLayer(IGraphicsContext context, TerrainCell cell, TerrainDrawLayer drawLayer, ref bool updated)
        {
            Texture2D mask = drawLayer.Mask;
            UPoint4 occupationPixel = ChannelOccupation(context, mask);
            for (int i = drawLayer.LayerCount - 1; i >= 0; i--)
            {
                TerrainLayer layer = drawLayer.LayerGroup[i];
                uint occupationChannel = occupationPixel[i];

                if (occupationChannel <= 0)
                {
                    int source = i + 1;
                    int destination = i;
                    if (source >= drawLayer.LayerCount)
                    {
                        Vector4 factor = Vector4.One;
                        factor[i] = 0;
                        MoveChannel(context, mask, (uint)destination, (uint)destination, factor);
                    }
                    else
                    {
                        MoveChannel(context, mask, (uint)source, (uint)destination, Vector4.One);
                    }

                    drawLayer.RemoveLayer(layer);
                    updated = true;
                }
            }

            if (updated)
            {
                if (drawLayer.LayerCount == 0)
                {
                    cell.RemoveDrawLayer(drawLayer);
                    updated = false;
                }
            }
        }

        public override bool DrawSettings(TerrainToolContext toolContext)
        {
            bool hasChanged = false;
            var grid = toolContext.Grid;
            if (ImGui.CollapsingHeader("Layers"))
            {
                if (ImGui.Button("Add Layer"))
                {
                    grid.Layers.Add(new("New Layer"));
                    hasChanged = true;
                }

                ImGui.BeginListBox("##LayerBox");
                for (int i = 0; i < grid.Layers.Count; i++)
                {
                    var layer = grid.Layers[i];
                    if (ImGui.MenuItem(layer.Name, i == selectedLayerIndex))
                    {
                        selectedLayerIndex = i;
                    }
                }
                ImGui.EndListBox();

                if (selectedLayerIndex > -1 && selectedLayerIndex < grid.Layers.Count)
                {
                    var layer = grid.Layers[selectedLayerIndex];
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

            return hasChanged;
        }

        public override bool Modify(IGraphicsContext context, TerrainToolContext toolContext)
        {
            var grid = toolContext.Grid;
            var cell = toolContext.Cell;
            var position = toolContext.Position;

            Vector3 local = Vector3.Transform(position, cell.TransformInv);
            Vector2 pos = new Vector2(local.X, local.Z) / new Vector2(cell.BoundingBox.Size.X, cell.BoundingBox.Size.Z);

            Viewport vp = maskTexBuffer.Viewport;

            float sizeFactor = vp.Size.X / cell.BoundingBox.Size.X;

            CBBrush brush = new(new(pos.X, 0, pos.Y), Size / sizeFactor, BlendStart, BlendEnd);

            brushBuffer.Update(context, brush);

            TerrainLayer layerToDraw = grid.Layers[selectedLayerIndex];

            TerrainDrawLayer? updatedDrawLayer = null;
            bool updated = false;
            if (!cell.ContainsLayer(layerToDraw))
            {
                updatedDrawLayer = cell.AddLayer(layerToDraw);
            }

            for (int i = 0; i < cell.DrawLayers.Count; i++)
            {
                var layer = cell.DrawLayers[i];
                var maskTex = layer.Mask;
                var channelMask = layer.GetChannelMask(layerToDraw);

                CBColorMask colorMask = new(channelMask);

                colorMask.Mask *= Strength * Time.Delta;
                maskBuffer.Update(context, colorMask);

                context.CopyResource(maskTexBuffer, maskTex);

                context.PSSetShaderResource(0, maskTexBuffer.SRV);
                context.PSSetSampler(0, maskSampler);
                context.PSSetConstantBuffer(0, brushBuffer);
                context.PSSetConstantBuffer(1, maskBuffer);
                context.SetRenderTarget(maskTex.RTV, null);
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

                CheckDrawLayer(context, cell, layer, ref updated);

                if (updated || updatedDrawLayer == layer)
                {
                    layer.UpdateLayer();
                }
            }

            return false;
        }
    }
}