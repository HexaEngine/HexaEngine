namespace HexaEngine.PerformanceTests
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Intrinsics.X86;
    using System.Runtime.Intrinsics;
    using HexaEngine.Mathematics;
    using System.Runtime.InteropServices;

    public class Program
    {
        public static void Main()
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }

    public unsafe class SSETests
    {
        private readonly Vector3[] vectors = new Vector3[WorkSize];

        public const int WorkSize = 1024 * 1024;

        public SSETests()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                vectors[i] = new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());
            }
        }

        [Benchmark(Baseline = true)]
        public void NonSSE()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                vectors[i] = Normalize(vectors[i]);
            }
        }

        [Benchmark()]
        public void SSEOnly()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                vectors[i] = NormalizeSSEOnly(vectors[i]);
            }
        }

        [Benchmark()]
        public void Combined()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                vectors[i] = NormalizeBoth(vectors[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 NormalizeSSEOnly(Vector3 vector)
        {
            Vector128<float> vec = vector.AsVector128();
            Vector128<float> vLengthSq = Sse41.DotProduct(vec, vec, 0x7f);
            // Prepare for the division
            Vector128<float> vResult = Sse.Sqrt(vLengthSq);
            // Create zero with a single instruction
            Vector128<float> vZeroMask = Vector128<float>.Zero;
            // Test for a divide by zero (Must be FP to detect -0.0)
            vZeroMask = Sse.CompareNotEqual(vZeroMask, vResult);
            // Failsafe on zero (Or epsilon) length planes
            // If the length is infinity, set the elements to zero
            vLengthSq = Sse.CompareNotEqual(vLengthSq, MathUtil.Infinity.AsVector128());
            // Divide to perform the normalization
            vResult = Sse.Divide(vec, vResult);
            // Any that are infinity, set to zero
            vResult = Sse.And(vResult, vZeroMask);
            // Select qnan or result based on infinite length
            Vector128<float> vTemp1 = Sse.AndNot(vLengthSq, MathUtil.QNaN.AsVector128());
            Vector128<float> vTemp2 = Sse.And(vResult, vLengthSq);
            vResult = Sse.Or(vTemp1, vTemp2);
            return vResult.AsVector3();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 NormalizeBoth(Vector3 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                Vector128<float> vLengthSq = Sse41.DotProduct(vec, vec, 0x7f);
                // Prepare for the division
                Vector128<float> vResult = Sse.Sqrt(vLengthSq);
                // Create zero with a single instruction
                Vector128<float> vZeroMask = Vector128<float>.Zero;
                // Test for a divide by zero (Must be FP to detect -0.0)
                vZeroMask = Sse.CompareNotEqual(vZeroMask, vResult);
                // Failsafe on zero (Or epsilon) length planes
                // If the length is infinity, set the elements to zero
                vLengthSq = Sse.CompareNotEqual(vLengthSq, MathUtil.Infinity.AsVector128());
                // Divide to perform the normalization
                vResult = Sse.Divide(vec, vResult);
                // Any that are infinity, set to zero
                vResult = Sse.And(vResult, vZeroMask);
                // Select qnan or result based on infinite length
                Vector128<float> vTemp1 = Sse.AndNot(vLengthSq, MathUtil.QNaN.AsVector128());
                Vector128<float> vTemp2 = Sse.And(vResult, vLengthSq);
                vResult = Sse.Or(vTemp1, vTemp2);
                return vResult.AsVector3();
            }

            {
                Vector4 vec = new(vector.X, vector.Y, vector.Z, 0);
                float lengthSq = Vector4.Dot(vec, vec);
                float length = MathF.Sqrt(lengthSq);
                if (length != 0)
                {
                    vec = Vector4.Divide(vec, length);
                }
                else
                {
                    vec = Vector4.Zero;
                }

                return new(vec.X, vec.Y, vec.Z);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(Vector3 vector)
        {
            Vector4 vec = new(vector.X, vector.Y, vector.Z, 0);
            float lengthSq = Vector4.Dot(vec, vec);
            float length = MathF.Sqrt(lengthSq);
            if (length != 0)
            {
                vec = Vector4.Divide(vec, length);
            }
            else
            {
                vec = Vector4.Zero;
            }

            return new(vec.X, vec.Y, vec.Z);
        }
    }
}