namespace HexaEngine.Core.Rendering
{
    using HexaEngine.Core.Resources;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MeshRenderer
    {
        private Queue<Model> models = new();

        public void Draw(Model model)
        {
            models.Enqueue(model);
        }

        public void RenderToTarget()
        {
        }
    }
}