namespace HexaEngine.PerformanceTests
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }

    public unsafe class UniformScale
    {
        private Vector4[] vectors = new Vector4[WorkSize];

        public const int WorkSize = 1024 * 16;

        public UniformScale()
        {
        }

        private struct Particle
        {
            public float X, Y, Z, W;

            public Particle(float x, float y, float z, float w)
            {
                X = x;
                Y = y;
                Z = z;
                W = w;
            }

            public Particle(float x)
            {
                X = x;
                Y = x;
                Z = x;
                W = x;
            }
        }

        [Benchmark(Baseline = true)]
        public void Baseline()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                Particle particle = new(i);
                Particle* p = &particle;
                Vector4* v = (Vector4*)(float*)p;
                vectors[i] = *v * i;
            }
        }

        [Benchmark]
        public void Load()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                Particle particle = new(i);
                Particle* p = &particle;
                float* f = (float*)p;
                Vector4 myVec = new(f[0], f[1], f[2], f[3]);
                vectors[i] = myVec * i;
            }
        }

        [Benchmark]
        public void Load2()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                Particle particle = new(i);
                Particle* p = &particle;
                float* f = (float*)p;
                Vector4 myVec = Extensions.ToVec4(f);
                vectors[i] = myVec * i;
            }
        }
    }

    public static unsafe class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ToVec4(float* ptr)
        {
            return new(ptr[0], ptr[1], ptr[2], ptr[3]);
        }
    }
}