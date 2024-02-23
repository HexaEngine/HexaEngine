namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using System.Numerics;

    public class TerrainRenderer : IDisposable
    {
        private readonly ConstantBuffer<Matrix4x4> worldBuffer;
        private readonly ISamplerState linearClampSampler;

        private TerrainGrid? grid;

        private bool initialized;
        private bool disposedValue;

        public TerrainRenderer(IGraphicsDevice device)
        {
            worldBuffer = new(device, CpuAccessFlags.Write);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
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

        public unsafe void Update(Matrix4x4 transform)
        {
            if (!initialized)
                return;

            for (int i = 0; i < grid.Count; i++)
            {
                Matrix4x4 global = transform * Matrix4x4.CreateTranslation(grid[i].Offset);
                Matrix4x4.Invert(global, out var globalInverse);
                grid[i].Transform = global;
                grid[i].TransformInv = globalInverse;
            }
        }

        public void DrawForward(IGraphicsContext context)
        {
            if (!initialized)
                return;

            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                unsafe
                {
                    *worldBuffer.Local = Matrix4x4.Transpose(cell.Transform);
                    worldBuffer.Update(context);
                }

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                    continue;

                context.VSSetConstantBuffer(0, worldBuffer);
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
        }

        public void DrawDeferred(IGraphicsContext context)
        {
            if (!initialized)
                return;

            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                unsafe
                {
                    *worldBuffer.Local = Matrix4x4.Transpose(cell.Transform);
                    worldBuffer.Update(context);
                }

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                    continue;

                context.VSSetConstantBuffer(0, worldBuffer);

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
        }

        public void DrawDepth(IGraphicsContext context)
        {
            if (!initialized)
                return;
            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                unsafe
                {
                    *worldBuffer.Local = Matrix4x4.Transpose(cell.Transform);
                    worldBuffer.Update(context);
                }

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                    continue;

                context.VSSetConstantBuffer(0, worldBuffer);
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
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(1, light);
            for (int i = 0; i < grid.Count; i++)
            {
                var cell = grid[i];
                unsafe
                {
                    *worldBuffer.Local = Matrix4x4.Transpose(cell.Transform);
                    worldBuffer.Update(context);
                }

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                    continue;

                context.VSSetConstantBuffer(0, worldBuffer);

                cell.Bind(context);
                for (int j = 0; j < cell.DrawLayers.Count; j++)
                {
                    TerrainDrawLayer layer = cell.DrawLayers[j];
                    TerrainMaterial material = layer.Material;

                    material.DrawIndexedInstanced(context, type.ToString(), cell.IndexCount, 1, 0, 0, 0);
                    break;
                }
                cell.Unbind(context);
            }
            context.VSSetConstantBuffer(1, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                worldBuffer?.Dispose();
                linearClampSampler?.Dispose();
                disposedValue = true;
            }
        }

        ~TerrainRenderer()
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
}