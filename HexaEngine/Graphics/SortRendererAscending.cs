namespace HexaEngine.Graphics
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public readonly struct SortRendererAscending : IComparer<IRendererComponent>
    {
        public static readonly SortRendererAscending Instance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly int IComparer<IRendererComponent>.Compare(IRendererComponent? a, IRendererComponent? b)
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