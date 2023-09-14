namespace D3D12Testing.Unsafes
{
    public struct Iterator<T> where T : struct
    {
        private readonly List<T> values;
        private int index;

        public Iterator(List<T> values)
        {
            this.values = values;
            index = 0;
        }

        public readonly T Current => values[index];

        public readonly bool End => index == values.Count;

        public static Iterator<T> operator ++(Iterator<T> iterator)
        {
            iterator.index++;
            return iterator;
        }

        public static Iterator<T> operator --(Iterator<T> iterator)
        {
            iterator.index--;
            return iterator;
        }

        public static Iterator<T> operator +(Iterator<T> iterator, int value)
        {
            iterator.index += value;
            return iterator;
        }

        public static Iterator<T> operator -(Iterator<T> iterator, int value)
        {
            iterator.index -= value;
            return iterator;
        }

        public static Iterator<T> operator +(Iterator<T> iterator, uint value)
        {
            iterator.index += (int)value;
            return iterator;
        }

        public static Iterator<T> operator -(Iterator<T> iterator, uint value)
        {
            iterator.index -= (int)value;
            return iterator;
        }
    }
}