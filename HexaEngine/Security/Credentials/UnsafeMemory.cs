namespace HexaEngine.Security.Credentials
{
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public unsafe struct SecureMemory<T> : IDisposable where T : unmanaged
    {
        public T* Data;
        public nuint Length;

        public SecureMemory(nuint length, nuint alignment = 1)
        {
            Data = (T*)NativeMemory.AlignedAlloc((nuint)sizeof(T) * length, alignment);
            Length = length;
        }

        public SecureMemory(int length, nuint alignment = 1) : this((nuint)length, alignment)
        {
        }

        public ref T this[nuint index]
        {
            get => ref Data[index];
        }

        public ref T this[int index]
        {
            get => ref Data[index];
        }

        public readonly UnsafeSpan<T> this[Range range]
        {
            get
            {
                var (offset, length) = range.GetOffsetAndLength((int)Length);
                return new UnsafeSpan<T>(Data + offset, (nuint)length);
            }
        }

        public readonly UnsafeSpan<T> Slice(nuint start)
        {
            return start > Length
                ? throw new ArgumentOutOfRangeException(nameof(start))
                : new UnsafeSpan<T>(Data + start, Length - start);
        }

        public readonly UnsafeSpan<T> Slice(int start)
        {
            if (start < 0 || (nuint)start > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            return new UnsafeSpan<T>(Data + start, Length - (nuint)start);
        }

        public readonly UnsafeSpan<T> Slice(nuint start, nuint length)
        {
            if (start + length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            return new UnsafeSpan<T>(Data + start, length);
        }

        public readonly UnsafeSpan<T> Slice(int start, int length)
        {
            if (start < 0 || length < 0 || (nuint)(start + length) > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            return new UnsafeSpan<T>(Data + start, (nuint)length);
        }

        public readonly Span<T> AsSpan() => new(Data, (int)Length);
        public readonly UnsafeSpan<T> AsUnsafeSpan() => new(Data, Length);

        public readonly ReadOnlySpan<T> AsReadOnlySpan() => new(Data, (int)Length);

        public static implicit operator Span<T>(SecureMemory<T> memory) => memory.AsSpan();

        public static implicit operator UnsafeSpan<T>(SecureMemory<T> memory) => memory.AsUnsafeSpan();

        public static implicit operator ReadOnlySpan<T>(SecureMemory<T> memory) => memory.AsReadOnlySpan();

        public static implicit operator T*(SecureMemory<T> memory) => memory.Data;

        public static implicit operator void*(SecureMemory<T> memory) => memory.Data;

        public void Dispose()
        {
            if (Data != null)
            {
                Clear();
                NativeMemory.AlignedFree(Data);
                Data = null;
                Length = 0;
            }
        }

        public readonly void Clear()
        {
            CryptographicOperations.ZeroMemory(new Span<byte>(Data, (int)((nuint)sizeof(T) * Length)));
        }
    }
}