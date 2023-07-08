using HexaEngine.Scenes.Components.Renderer;

namespace HexaEngine.Rendering
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Resources;
    using System.Numerics;

    public class TerrainRenderer : IDisposable
    {
        private readonly ConstantBuffer<Matrix4x4> worldBuffer;
        private readonly ResourceRef<IBuffer> camera;

        private TerrainGrid? grid;

        private bool initialized;
        private bool disposedValue;

        public TerrainRenderer(IGraphicsDevice device)
        {
            worldBuffer = new(device, CpuAccessFlags.Write);
            camera = ResourceManager2.Shared.GetBuffer("CBCamera");
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
                grid[i].Transform = transform * Matrix4x4.CreateTranslation(grid[i].Offset);
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
                    var layer = cell.DrawLayers[j];
                    if (!layer.BeginDrawForward(context, camera.Value))
                        continue;
                    context.DrawIndexedInstanced(cell.IndexCount, 1, 0, 0, 0);
                    layer.EndDrawForward(context);
                }
                cell.Unbind(context);
            }
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
                    var layer = cell.DrawLayers[j];
                    if (!layer.BeginDrawDeferred(context, camera.Value))
                        continue;
                    context.DrawIndexedInstanced(cell.IndexCount, 1, 0, 0, 0);
                    layer.EndDrawDeferred(context);
                }
                cell.Unbind(context);
            }
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
                    var layer = cell.DrawLayers[j];
                    if (!layer.BeginDrawDepth(context, camera.Value))
                        continue;
                    context.DrawIndexedInstanced(cell.IndexCount, 1, 0, 0, 0);
                    layer.EndDrawDepth(context);
                }
                cell.Unbind(context);
            }
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
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
                    var layer = cell.DrawLayers[j];
                    if (!layer.BeginDrawShadow(context, light, type))
                        continue;
                    context.DrawIndexedInstanced(cell.IndexCount, 1, 0, 0, 0);
                    layer.EndDrawShadow(context);
                }
                cell.Unbind(context);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                worldBuffer?.Dispose();
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