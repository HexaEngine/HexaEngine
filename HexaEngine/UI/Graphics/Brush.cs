namespace HexaEngine.UI.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using System.Collections.Generic;
    using System.Numerics;

    public abstract class Brush : UIResource
    {
        public abstract void Apply(UIDrawCommand command, IGraphicsContext context);
    }

    public sealed class SolidColorBrush : Brush, IEquatable<SolidColorBrush?>
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

        public override bool Equals(object? obj)
        {
            return Equals(obj as SolidColorBrush);
        }

        public bool Equals(SolidColorBrush? other)
        {
            return other is not null && Color.Equals(other.Color);
        }

        protected override void DisposeCore()
        {
            constantBuffer.Dispose();
        }

        public static bool operator ==(SolidColorBrush? left, SolidColorBrush? right)
        {
            return EqualityComparer<SolidColorBrush>.Default.Equals(left, right);
        }

        public static bool operator !=(SolidColorBrush? left, SolidColorBrush? right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"Color: {Color}";
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