namespace HexaEngine.Objects.Components
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Scenes;
    using System;

    public class RendererComponent : IComponent
    {
        public IPropertyEditor? Editor { get; }

        public void Awake(IGraphicsDevice device, GameObject node)
        {
            throw new NotImplementedException();
        }

        public void Destory()
        {
            throw new NotImplementedException();
        }
    }
}