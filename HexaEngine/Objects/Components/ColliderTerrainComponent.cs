namespace HexaEngine.Objects.Components
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using System;

    public class ColliderTerrainComponent : IComponent
    {
        public bool IsVisible { get; set; }

        public void Initialize(IGraphicsDevice device, SceneNode node)
        {
        }

        public void Uninitialize()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}