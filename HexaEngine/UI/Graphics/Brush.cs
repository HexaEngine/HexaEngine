namespace HexaEngine.UI.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public abstract class Brush : IDisposable
    {
        private bool disposedValue;

        public abstract void Apply(UIDrawCommand command, IGraphicsContext context);

        protected abstract void DisposeCore();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposeCore();
                disposedValue = true;
            }
        }

        ~Brush()
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

    public sealed class SolidColorBrush : Brush
    {
        private readonly ConstantBuffer<Vector4> constantBuffer;

        public SolidColorBrush(IGraphicsDevice device, Color color)
        {
            constantBuffer = new(device, color.ToVector4(), CpuAccessFlags.Write);
        }

        public unsafe Color Color { get => new(*constantBuffer.Local); set => *constantBuffer.Local = value; }

        public override void Apply(UIDrawCommand command, IGraphicsContext context)
        {
            constantBuffer.Update(context);
            context.PSSetConstantBuffer(0, constantBuffer);
        }

        protected override void DisposeCore()
        {
            constantBuffer.Dispose();
        }
    }

    public class ImageBrush : Brush
    {
        public override void Apply(UIDrawCommand command, IGraphicsContext context)
        {
        }

        protected override void DisposeCore()
        {
            throw new NotImplementedException();
        }
    }
}