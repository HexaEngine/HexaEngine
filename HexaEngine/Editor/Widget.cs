namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using ImGuiNET;

    public abstract class Widget
    {
        protected bool IsShown;
        protected bool IsDocked;

        //protected abstract string Name { get; }
        protected ImGuiWindowFlags Flags;

        public abstract void Init(IGraphicsDevice device);

        public abstract void Draw(IGraphicsContext context);

        public abstract void DrawMenu();

        public abstract void Dispose();
    }
}