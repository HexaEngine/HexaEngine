namespace HexaEngine.Core.Security.Cryptography
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public class SHA256SignatureGenerator
    {
        public static SHA256Signature GenerateSHA256Signature(string input)
        {
            Span<SHA256Signature> signature = stackalloc SHA256Signature[1];
            SHA256.HashData(MemoryMarshal.AsBytes(input.AsSpan()), MemoryMarshal.AsBytes(signature));
            return signature[0];
        }

        public static unsafe SHA256Signature GenerateSHA256SignatureUnsafe(string input)
        {
            SHA256Signature signature;

            fixed (char* pString = input)
            {
                SHA256.HashData(new Span<byte>(pString, input.Length * sizeof(char)), new Span<byte>(&signature, sizeof(SHA256Signature)));
            }

            return signature;
        }
    }

    public struct SHA256Signature : IEquatable<SHA256Signature>
    {
        public ulong L0;
        public ulong L1;
        public ulong L2;
        public ulong L3;

        public SHA256Signature(ulong l0, ulong l1, ulong l2, ulong l3)
        {
            L0 = l0;
            L1 = l1;
            L2 = l2;
            L3 = l3;
        }

        public SHA256Signature(ReadOnlySpan<byte> source, bool bigEndian)
        {
            if (bigEndian)
            {
                L0 = BinaryPrimitives.ReadUInt64BigEndian(source);
                L1 = BinaryPrimitives.ReadUInt64BigEndian(source[8..]);
                L2 = BinaryPrimitives.ReadUInt64BigEndian(source[16..]);
                L3 = BinaryPrimitives.ReadUInt64BigEndian(source[24..]);
            }
            else
            {
                L0 = BinaryPrimitives.ReadUInt64LittleEndian(source);
                L1 = BinaryPrimitives.ReadUInt64LittleEndian(source[8..]);
                L2 = BinaryPrimitives.ReadUInt64LittleEndian(source[16..]);
                L3 = BinaryPrimitives.ReadUInt64LittleEndian(source[24..]);
            }
        }

        public readonly bool TryWriteBytes(Span<byte> destination)
        {
            if (destination.Length < 32)
            {
                return false;
            }

            BinaryPrimitives.WriteUInt64LittleEndian(destination, L0);
            BinaryPrimitives.WriteUInt64LittleEndian(destination[8..], L1);
            BinaryPrimitives.WriteUInt64LittleEndian(destination[16..], L2);
            BinaryPrimitives.WriteUInt64LittleEndian(destination[24..], L3);
            return true;
        }

        public readonly bool TryWriteBytes(Span<byte> destination, bool bigEndian)
        {
            if (destination.Length < 32)
            {
                return false;
            }

            if (bigEndian)
            {
                BinaryPrimitives.WriteUInt64BigEndian(destination, L0);
                BinaryPrimitives.WriteUInt64BigEndian(destination[8..], L1);
                BinaryPrimitives.WriteUInt64BigEndian(destination[16..], L2);
                BinaryPrimitives.WriteUInt64BigEndian(destination[24..], L3);
            }
            else
            {
                BinaryPrimitives.WriteUInt64LittleEndian(destination, L0);
                BinaryPrimitives.WriteUInt64LittleEndian(destination[8..], L1);
                BinaryPrimitives.WriteUInt64LittleEndian(destination[16..], L2);
                BinaryPrimitives.WriteUInt64LittleEndian(destination[24..], L3);
            }

            return true;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is SHA256Signature signature && Equals(signature);
        }

        public readonly bool Equals(SHA256Signature other)
        {
            return L0 == other.L0 &&
                   L1 == other.L1 &&
                   L2 == other.L2 &&
                   L3 == other.L3;
        }

        public static SHA256Signature operator ^(SHA256Signature left, SHA256Signature right)
        {
            return new SHA256Signature(left.L0 ^ right.L0, left.L1 ^ right.L1, left.L2 ^ right.L2, left.L3 ^ right.L3);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(L0, L1, L2, L3);
        }

        public static bool operator ==(SHA256Signature left, SHA256Signature right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SHA256Signature left, SHA256Signature right)
        {
            return !(left == right);
        }

        public override readonly string ToString()
        {
            return $"{L0:X16}{L1:X16}{L2:X16}{L3:X16}";
        }
    }
}