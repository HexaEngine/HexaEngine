#nullable disable

using HexaEngine;

namespace HexaEngine.Filters
{
    using HexaEngine.Core.Graphics;
    using System;

    public interface IFilter : IDisposable
    {
        void Draw(IGraphicsContext context);

        void DrawSlice(IGraphicsContext context, int i, int x, int y, int xsize, int ysize);
    }
}