namespace HexaEngine.UI.Markup
{
    public interface INameScope
    {
        public object? FindName(string name);

        public void RegisterName(string name, object scopedElement);

        public void UnregisterName(string name);
    }
}