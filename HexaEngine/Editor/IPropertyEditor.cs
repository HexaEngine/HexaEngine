namespace HexaEngine.Editor
{
    using System;
    using System.Reflection;

    public interface IPropertyEditor
    {
        PropertyInfo[] Properties { get; }
        Type Type { get; }
        string Name { get; }

        void Draw();
    }
}