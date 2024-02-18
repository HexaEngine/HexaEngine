namespace HexaEngine.Graphics
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public readonly struct SortRendererInstanceAscending<T> : IComparer<T> where T : IRendererInstance
    {
        public static readonly SortRendererInstanceAscending<T> Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly int IComparer<T>.Compare(T? a, T? b)
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