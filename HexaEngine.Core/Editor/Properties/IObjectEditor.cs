namespace HexaEngine.Core.Editor.Properties
{
    using HexaEngine.Core.Graphics;
    using System;

    public interface IObjectEditor : IDisposable
    {
        string Name { get; }

        Type Type { get; }

        object? Instance { get; set; }

        bool IsEmpty { get; }
        bool NoTable { get; set; }

        void Draw(IGraphicsContext context);
    }
}