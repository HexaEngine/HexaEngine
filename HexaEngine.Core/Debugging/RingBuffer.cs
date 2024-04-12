namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// A generic ring buffer for storing and calculating averages of a specified numeric type.
    /// </summary>
    /// <typeparam name="T">The type of values to store in the buffer. Must be a numeric struct type implementing <see cref="INumber{T}"/>.</typeparam>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="RingBuffer{T}"/> class with the specified length.
        /// </summary>
        /// <param name="length">The maximum length of the ring buffer.</param>
        public RingBuffer(int length)
        {
            rawValues = new T[length];
            avgValues = new T[length];
            this.length = length;
        }

        /// <summary>
        /// Gets the raw values stored in the ring buffer.
        /// </summary>
        public T[] Raw => rawValues;

        /// <summary>
        /// Gets the values stored in the ring buffer. If <see cref="AverageValues"/> is set to true, it returns the averaged values; otherwise, it returns the raw values.
        /// </summary>
        public T[] Values
        {
            get
            {
                return averageValues ? avgValues : rawValues;
            }
        }

        /// <summary>
        /// Gets the maximum length of the ring buffer.
        /// </summary>
        public int Length => length;

        /// <summary>
        /// Gets the tail position in the ring buffer.
        /// </summary>
        public int Tail => tail;

        /// <summary>
        /// Gets the head position in the ring buffer.
        /// </summary>
        public int Head => head;

        /// <summary>
        /// Gets the value at the tail position in the ring buffer.
        /// </summary>
        public T TailValue => Values[tail];

        /// <summary>
        /// Gets the value at the head position in the ring buffer.
        /// </summary>
        public T HeadValue => Values[head];

        /// <summary>
        /// Gets or sets a value indicating whether the values in the ring buffer should be averaged.
        /// </summary>
        public bool AverageValues { get => averageValues; set => averageValues = value; }

        /// <summary>
        /// Adds a value to the ring buffer, updating the calculated average if necessary.
        /// </summary>
        /// <param name="value">The value to add to the ring buffer.</param>
        public void Add(T value)
        {
            if (value < default(T))
            {
                value = default;
            }

            // Subtract the oldest value from the sum if the buffer is full
            if (count == length)
            {
                sum -= rawValues[tail];
            }
            else
            {
                count++;
                countT++;
            }

            // Add the new value to the sum
            sum += value;

            avgValues[head] = CalculateAverage();
            rawValues[head] = value;

            head = (head + 1) % length;
            tail = (head - count + length) % length;
        }

        /// <summary>
        /// Calculates the average of the values in the ring buffer.
        /// </summary>
        /// <returns>The calculated average value.</returns>
        public T CalculateAverage()
        {
            if (count == 0)
            {
                // The buffer is empty, return the default value of T
                return default;
            }

            // Calculate and return the average
            return sum / countT;
        }
    }

    /// <summary>
    /// An unsafe generic ring buffer for storing and calculating averages of a specified unmanaged numeric type.
    /// </summary>
    /// <typeparam name="T">The type of values to store in the buffer. Must be an unmanaged type implementing <see cref="INumber{T}"/>.</typeparam>
    public unsafe struct UnsafeRingBuffer<T> : IEquatable<UnsafeRingBuffer<T>> where T : unmanaged, INumber<T>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeRingBuffer{T}"/> struct with the specified length.
        /// </summary>
        /// <param name="length">The maximum length of the ring buffer.</param>
        public UnsafeRingBuffer(int length)
        {
            rawValues = AllocT<T>(length);
            avgValues = AllocT<T>(length);
            this.length = length;
        }

        /// <summary>
        /// Gets the raw values stored in the ring buffer.
        /// </summary>
        public readonly T* Raw => rawValues;

        /// <summary>
        /// Gets the values stored in the ring buffer. If <see cref="AverageValues"/> is set to true, it returns the averaged values; otherwise, it returns the raw values.
        /// </summary>
        public readonly T* Values
        {
            get
            {
                return averageValues ? avgValues : rawValues;
            }
        }

        /// <summary>
        /// Gets the maximum length of the ring buffer.
        /// </summary>
        public readonly int Length => length;

        /// <summary>
        /// Gets the tail position in the ring buffer.
        /// </summary>
        public readonly int Tail => tail;

        /// <summary>
        /// Gets the head position in the ring buffer.
        /// </summary>
        public readonly int Head => head;

        /// <summary>
        /// Gets the value at the tail position in the ring buffer.
        /// </summary>
        public readonly T TailValue => Values[tail];

        /// <summary>
        /// Gets the value at the head position in the ring buffer.
        /// </summary>
        public readonly T HeadValue => Values[head - 1];

        /// <summary>
        /// Gets or sets a value indicating whether the values in the ring buffer should be averaged.
        /// </summary>
        public bool AverageValues { readonly get => averageValues; set => averageValues = value; }

        /// <summary>
        /// Adds a value to the ring buffer, updating the calculated average if necessary.
        /// </summary>
        /// <param name="value">The value to add to the ring buffer.</param>
        public void Add(T value)
        {
            if (value < default(T))
            {
                value = default;
            }

            // Subtract the oldest value from the sum if the buffer is full
            if (count == length)
            {
                sum -= rawValues[tail];
            }
            else
            {
                count++;
                countT++;
            }

            // Add the new value to the sum
            sum += value;

            avgValues[head] = CalculateAverage();
            rawValues[head] = value;

            head = (head + 1) % length;
            tail = (head - count + length) % length;
        }

        /// <summary>
        /// Calculates the average of the values in the ring buffer.
        /// </summary>
        /// <returns>The calculated average value.</returns>
        public readonly T CalculateAverage()
        {
            if (count == 0)
            {
                // The buffer is empty, return the default value of T
                return default;
            }

            // Calculate and return the average
            return sum / countT;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is UnsafeRingBuffer<T> buffer && Equals(buffer);
        }

        public readonly bool Equals(UnsafeRingBuffer<T> other)
        {
            return rawValues == other.rawValues;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine((nint)rawValues);
        }

        /// <summary>
        /// Releases the allocated memory for raw and average values and resets the buffer to its default state.
        /// </summary>
        public void Release()
        {
            Free(rawValues);
            Free(avgValues);
            this = default;
        }

        public static bool operator ==(UnsafeRingBuffer<T> left, UnsafeRingBuffer<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UnsafeRingBuffer<T> left, UnsafeRingBuffer<T> right)
        {
            return !(left == right);
        }
    }
}