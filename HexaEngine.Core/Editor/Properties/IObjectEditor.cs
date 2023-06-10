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

        void Draw(IGraphicsContext context);
    }
}