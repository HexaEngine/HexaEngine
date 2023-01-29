namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Rendering;
    using HexaEngine.Core.Resources;
    using System;

    public class MeshRenderer : IRenderer
    {
        private readonly Queue<Model> drawQueue = new();

        public void DrawModel(Model model)
        {
            drawQueue.Enqueue(model);
        }

        public void Init(IGraphicsDevice device)
        {
            throw new NotImplementedException();
        }

        public void Update(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void Draw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}