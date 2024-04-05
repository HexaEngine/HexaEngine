namespace HexaEngine.Core.Security.Cryptography
{
    using System;
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
            return $"{L0:X8}{L1:X8}{L2:X8}{L3:X8}";
        }
    }
}