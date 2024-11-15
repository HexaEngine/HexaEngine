namespace HexaEngine.Editor.UI
{
    using System.Numerics;

    public interface ITitleBarElement
    {
        public void Draw(TitleBarContext context);

        public Vector2 Size { get; }

        public string Label { get; }

        bool IsVisible { get; }
    }
}