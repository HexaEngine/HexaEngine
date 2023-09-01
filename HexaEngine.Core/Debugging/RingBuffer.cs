namespace HexaEngine.Core.Debugging
{
    using System.Numerics;

    public unsafe class RingBuffer<T> where T : struct, INumber<T>
    {
        private readonly T[] rawValues;
        private readonly T[] avgValues;
        private readonly int length;
        private int head = 0;
        private int tail;
        private T sum;
        private T countT;
        private int count = 0;
        private bool averageValues = true;

        public RingBuffer(int length)
        {
            rawValues = new T[length];
            avgValues = new T[length];
            this.length = length;
        }

        public T[] Raw => rawValues;

        public T[] Values
        {
            get
            {
                return averageValues ? avgValues : rawValues;
            }
        }

        public int Length => length;

        public int Tail => tail;

        public int Head => head;

        public T TailValue => Values[tail];

        public T HeadValue => Values[head];

        public bool AverageValues { get => averageValues; set => averageValues = value; }

        public void Add(T value)
        {
            if (value < default(T))
            {
                value = default;
            }

            // Subtract the oldest _value from the sum if the buffer is full
            if (count == length)
            {
                sum -= rawValues[tail];
            }
            else
            {
                count++;
                countT++;
            }

            // Add the new _value to the sum
            sum += value;

            avgValues[head] = CalculateAverage();
            rawValues[head] = value;

            head = (head + 1) % length;
            tail = (head - count + length) % length;
        }

        public T CalculateAverage()
        {
            if (count == 0)
            {
                // The buffer is empty, return the default _value of T
                return default;
            }

            // Calculate and return the average
            return sum / countT;
        }
    }

    public unsafe struct UnsafeRingBuffer<T> where T : unmanaged, INumber<T>
    {
        private T* rawValues;
        private T* avgValues;
        private int length;
        private int head = 0;
        private int tail;
        private T sum;
        private T countT;
        private int count = 0;
        private bool averageValues = true;

        public UnsafeRingBuffer(int length)
        {
            rawValues = AllocT<T>(length);
            avgValues = AllocT<T>(length);
            this.length = length;
        }

        public readonly T* Raw => rawValues;

        public readonly T* Values
        {
            get
            {
                return averageValues ? avgValues : rawValues;
            }
        }

        public readonly int Length => length;

        public readonly int Tail => tail;

        public readonly int Head => head;

        public readonly T TailValue => Values[tail];

        public readonly T HeadValue => Values[head - 1];

        public bool AverageValues { readonly get => averageValues; set => averageValues = value; }

        public void Add(T value)
        {
            if (value < default(T))
            {
                value = default;
            }

            // Subtract the oldest _value from the sum if the buffer is full
            if (count == length)
            {
                sum -= rawValues[tail];
            }
            else
            {
                count++;
                countT++;
            }

            // Add the new _value to the sum
            sum += value;

            avgValues[head] = CalculateAverage();
            rawValues[head] = value;

            head = (head + 1) % length;
            tail = (head - count + length) % length;
        }

        public readonly T CalculateAverage()
        {
            if (count == 0)
            {
                // The buffer is empty, return the default _value of T
                return default;
            }

            // Calculate and return the average
            return sum / countT;
        }

        public void Release()
        {
            Free(rawValues);
            Free(avgValues);
            this = default;
        }
    }
}