namespace HexaEngine.Core.Editor.Properties
{
    public interface IPropertyEditor
    {
        bool Draw(object instance, ref object? value);
    }
}