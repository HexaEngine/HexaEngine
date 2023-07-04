#nullable disable

namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;

    public interface IAmbientOcclusion : IDisposable
    {
        string Name { get; }

        Task Initialize(IGraphicsDevice device, int width, int height);

        void Draw(IGraphicsContext context);

        void Resize(int width, int height);
    }
}