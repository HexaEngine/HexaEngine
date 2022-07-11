namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using System;

    public interface IConstantBuffer<T> : IConstantBuffer where T : unmanaged
    {
        void Write(IGraphicsContext context, T value);

        void Write(IGraphicsContext context, T[] value);
    }

    public interface IConstantBuffer : IDisposable
    {
        void Bind(IGraphicsContext context);
    }
}