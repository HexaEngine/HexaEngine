namespace HexaEngine.Core.Renderers
{
    using System.Collections.Generic;

    public class SortRendererAscending : IComparer<IRendererComponent>
    {
        int IComparer<IRendererComponent>.Compare(IRendererComponent? a, IRendererComponent? b)
        {
            if (a == null || b == null)
            {
                return 0;
            }

            if (a.QueueIndex < b.QueueIndex)
            {
                return -1;
            }

            if (a.QueueIndex > b.QueueIndex)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}