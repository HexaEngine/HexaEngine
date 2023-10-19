namespace HexaEngine.UI
{
    using System.Numerics;

    public abstract class UIElement : UINavigationElement, IUIElement
    {
        public Vector2 Position { get; set; }

        public abstract Vector2 Size { get; }
    }
}