namespace HexaEngine.UI
{
    public interface IChildContainer : IDependencyElement
    {
        UIElementCollection Children { get; }
    }
}