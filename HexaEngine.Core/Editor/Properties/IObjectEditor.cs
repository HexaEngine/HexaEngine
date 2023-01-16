namespace HexaEngine.Core.Editor.Properties
{
    using System;

    public interface IObjectEditor
    {
        string Name { get; }

        Type Type { get; }
        object? Instance { get; set; }

        void Draw();
    }
}