namespace HexaEngine.Editor
{
    using System;

    public interface IPropertyEditor
    {
        string Name { get; }

        Type Type { get; }

        void Draw();
    }
}