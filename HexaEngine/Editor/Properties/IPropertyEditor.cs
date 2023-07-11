namespace HexaEngine.Editor.Properties
{
    using HexaEngine.Core.Graphics;
    using System.Reflection;

    public interface IPropertyEditor
    {
        string Name { get; }

        PropertyInfo Property { get; }

        bool Draw(IGraphicsContext context, object instance, ref object? value);
    }
}