namespace D3D12Testing.Unsafes
{
    using System.Collections;

    public unsafe class UnsafeListEnumerator<T> : IEnumerator<T> where T : unmanaged
    {
        public UnsafeList<T> values;
        private int position = -1;

        public UnsafeListEnumerator(UnsafeList<T> values)
        {
            this.values = values;
        }

        public T Current => values[position];

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            position++;
            return position < values.Size;
        }

        public void Reset()
        {
            position = -1;
        }

        public void Dispose()
        {
        }
    }
}