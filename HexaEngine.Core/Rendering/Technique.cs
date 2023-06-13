namespace HexaEngine.Core.Rendering
{
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;

    public class Technique
    {
        private readonly List<IRenderPass> passes = new();

        public void Add(IRenderPass pass)
        {
            passes.Add(pass);
        }

        public bool Remove(IRenderPass pass)
        {
            return passes.Remove(pass);
        }

        public void RemoveAt(int index)
        {
            passes.RemoveAt(index);
        }

        public void Clear()
        {
            passes.Clear();
        }

        public bool Contains(IRenderPass pass)
        {
            return passes.Contains(pass);
        }

        public int IndexOf(IRenderPass pass)
        {
            return passes.IndexOf(pass);
        }

        public void Draw(IGraphicsContext context)
        {
            for (int i = 0; i < passes.Count; i++)
            {
                passes[i].BeginDraw(context);
            }

            for (int i = 0; i < passes.Count; i++)
            {
                passes[i].Draw(context);
            }

            for (int i = 0; i < passes.Count; i++)
            {
                passes[i].EndDraw(context);
            }
        }
    }
}