namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Core.Graphics;

    public enum ToolFlags
    {
        Default,
        NoEdit
    }

    public abstract class Tool : IDisposable
    {
        public abstract string Icon { get; }

        public abstract string Name { get; }

        public virtual ToolFlags Flags { get; } = ToolFlags.Default;

        public abstract void Init(IGraphicsDevice device);

        public abstract void DrawSettings();

        public abstract void DrawPreview(IGraphicsContext context, ToolContext toolContext);

        public abstract void Draw(IGraphicsContext context, ToolContext toolContext);

        public abstract void Dispose();
    }
}