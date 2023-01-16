namespace HexaEngine.Editor.Properties
{
    public interface IPropertyEditor
    {
        bool Draw(object instance, ref object? value);
    }
}