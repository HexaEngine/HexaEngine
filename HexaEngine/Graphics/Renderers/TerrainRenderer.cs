namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Utilities;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using System.Numerics;

    public class TerrainRenderer : IDisposable
    {
        private ConstantBuffer<UPoint4> offsetBuffer;
        private StructuredBuffer<Matrix4x4> transformNoBuffer;
        private StructuredBuffer<uint> transformNoOffsetBuffer;
        private StructuredUavBuffer<Matrix4x4> transformBuffer;
        private StructuredUavBuffer<uint> transformOffsetBuffer;
        private readonly ISamplerState linearClampSampler;

        private TerrainGrid? grid;

        private bool initialized;
        private bool disposedValue;

        public TerrainRenderer(IGraphicsDevice device)
        {
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            offsetBuffer = new(CpuAccessFlags.Write);
            transformNoBuffer = new(CpuAccessFlags.Write);
            transformNoOffsetBuffer = new(CpuAccessFlags.Write);
            transformBuffer = new(CpuAccessFlags.Write);
            transformOffsetBuffer = new(CpuAccessFlags.Write);
        }

        public void Initialize(TerrainGrid grid)
        {
            this.grid = grid;

            initialized = true;
        }

        public void Uninitialize()
        {
            initialized = false;

            grid = null;
        }

        public unsafe void Update(IGraphicsContext context, Matrix4x4 transform)
        {
            if (!initialized)
            {
                return;
            }

            transformBuffer.ResetCounter();
            transformOffsetBuffer.ResetCounter();

            transformNoBuffer.ResetCounter();
            transformNoOffsetBuffer.ResetCounter();

            for (int i = 0; i < grid.Count; i++)
            {
                TerrainCell cell = grid[i];
                Matrix4x4 global = transform * Matrix4x4.CreateTranslation(cell.Offset);
                Matrix4x4.Invert(global, out var globalInverse);
                cell.Transform = global;
                cell.TransformInv = globalInverse;
                Matrix4x4 globalTransposed = Matrix4x4.Transpose(global);

                transformOffsetBuffer.Add((uint)i);
                transformNoOffsetBuffer.Add((uint)i);
                transformBuffer.Add(globalTransposed);
                transformNoBuffer.Add(globalTransposed);
            }

            transformBuffer.Update(context);
            transformOffsetBuffer.Update(context);

            transformNoBuffer.Update(context);
            transformNoOffsetBuffer.Update(context);
        }

        public void DrawForward(IGraphicsContext context)
        {
            if (!initialized)
            {
                return;
            }

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                {
                    continue;
                }

                unsafe
                {
                    offsetBuffer.Update(context, new((uint)i));
                }

                cell.Bind(context);
                for (int j = 0; j < cell.DrawLayers.Count; j++)
                {
                    TerrainDrawLayer layer = cell.DrawLayers[j];
                    TerrainMaterial material = layer.Material;

                    context.PSSetShaderResource(11, layer.Mask.SRV);
                    context.DSSetSampler(0, linearClampSampler);
                    context.DSSetShaderResource(0, layer.Mask.SRV);
                    material.DrawIndexedInstanced(context, "Forward", cell.IndexCount, 1, 0, 0, 0);
                }
                cell.Unbind(context);
            }
            context.PSSetShaderResource(11, null);
            context.DSSetShaderResource(0, null);
            context.DSSetSampler(0, null);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public void DrawDeferred(IGraphicsContext context)
        {
            if (!initialized)
            {
                return;
            }

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                {
                    continue;
                }

                unsafe
                {
                    offsetBuffer.Update(context, new((uint)i));
                }

                cell.Bind(context);
                for (int j = 0; j < cell.DrawLayers.Count; j++)
                {
                    TerrainDrawLayer layer = cell.DrawLayers[j];
                    TerrainMaterial material = layer.Material;

                    context.PSSetShaderResource(11, layer.Mask.SRV);
                    context.DSSetSampler(0, linearClampSampler);
                    context.DSSetShaderResource(0, layer.Mask.SRV);
                    material.DrawIndexedInstanced(context, "Deferred", cell.IndexCount, 1, 0, 0, 0);
                }
                cell.Unbind(context);
            }

            context.PSSetShaderResource(11, null);
            context.DSSetShaderResource(0, null);
            context.DSSetSampler(0, null);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public void DrawDepth(IGraphicsContext context)
        {
            if (!initialized)
            {
                return;
            }

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformNoBuffer.SRV);
            context.VSSetShaderResource(1, transformNoOffsetBuffer.SRV);

            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                {
                    continue;
                }

                unsafe
                {
                    offsetBuffer.Update(context, new((uint)i));
                }

                cell.Bind(context);
                for (int j = 0; j < cell.DrawLayers.Count; j++)
                {
                    TerrainDrawLayer layer = cell.DrawLayers[j];
                    TerrainMaterial material = layer.Material;

                    context.PSSetSampler(0, linearClampSampler);
                    context.PSSetShaderResource(0, layer.Mask.SRV);
                    context.DSSetSampler(0, linearClampSampler);
                    context.DSSetShaderResource(0, layer.Mask.SRV);
                    material.DrawIndexedInstanced(context, "DepthOnly", cell.IndexCount, 1, 0, 0, 0);
                }
                cell.Unbind(context);
            }
            context.PSSetShaderResource(0, null);
            context.PSSetSampler(0, null);
            context.DSSetShaderResource(0, null);
            context.DSSetSampler(0, null);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!initialized)
            {
                return;
            }

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformNoBuffer.SRV);
            context.VSSetShaderResource(1, transformNoOffsetBuffer.SRV);

            var name = EnumHelper<ShadowType>.GetName(type);

            context.VSSetConstantBuffer(1, light);
            context.GSSetConstantBuffer(0, light);
            context.PSSetConstantBuffer(0, light);
            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                {
                    continue;
                }

                unsafe
                {
                    offsetBuffer.Update(context, new((uint)i));
                }

                cell.Bind(context);
                for (int j = 0; j < cell.DrawLayers.Count; j++)
                {
                    TerrainDrawLayer layer = cell.DrawLayers[j];
                    TerrainMaterial material = layer.Material;

                    material.DrawIndexedInstanced(context, name, cell.IndexCount, 1, 0, 0, 0);
                    break;
                }
                cell.Unbind(context);
            }
            context.PSSetConstantBuffer(0, null);
            context.GSSetConstantBuffer(0, null);
            context.VSSetConstantBuffer(1, null);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                linearClampSampler?.Dispose();
                offsetBuffer?.Dispose();
                transformBuffer?.Dispose();
                transformOffsetBuffer?.Dispose();
                transformNoBuffer?.Dispose();
                transformNoOffsetBuffer?.Dispose();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}