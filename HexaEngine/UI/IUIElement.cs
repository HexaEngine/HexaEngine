namespace HexaEngine.UI
{
    using System.Numerics;

    public interface IUIElement : IInputElement, IUINavigationElement
    {
        public Vector2 Position { get; set; }

        public Vector2 Size { get; }

        public Vector2 Midpoint => Position + Size / 2;

        public Vector2 ComputeDirection(IUIElement to, out float weight)
        {
            var midpoint = Midpoint;
            var dir = to.Midpoint - midpoint;
            weight = dir.Length();
            return Vector2.Normalize(dir);
        }
    }
}