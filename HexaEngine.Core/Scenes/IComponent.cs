﻿namespace HexaEngine.Core.Scenes
{
    using HexaEngine.Core.Graphics;

    public interface IComponent
    {
        public void Awake(IGraphicsDevice device, GameObject gameObject);

        public void Destory();

        public void FixedUpdate()
        {
        }

        public void Update()
        {
        }
    }

    public interface IScriptComponent : IComponent
    {
        public void Awake()
        {
        }

        public void Destroy()
        {
        }
    }

    public interface IRenderComponent : IComponent
    {
        public void Draw();
    }
}