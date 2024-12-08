namespace HexaEngine.Graphics
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public readonly struct SortRendererAscending : IComparer<IDrawable>
    {
        public static readonly SortRendererAscending Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly int IComparer<IDrawable>.Compare(IDrawable? a, IDrawable? b)
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