#nullable disable

namespace HexaEngine.Graphics.Filters
{
    using HexaEngine.Core.Graphics;
    using System;

    public interface IFilter : IDisposable
    {
        void Draw(IGraphicsContext context);
    }
}