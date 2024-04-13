namespace TestApp
{
    using HexaEngine.Core.Assets.Importer;
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Intrinsics;

    public static unsafe partial class Program
    {
        public static void Main()
        {
            Vector4D left = new(1, 2, 3, 4);
            Vector4D right = new(5, 6, 7, 8);

            double dot = Vector4D.Dot(left, right);

            double doot = MathUtil.Dot(left, right);

            var normalizedD = Vector4D.Normalize(left);
            var normalizedD0 = MathUtil.Normalize(left);

            var normalized = MathUtil.Normalize(new Vector3(1, 0, 0));
            var normalized0 = MathUtil.Normalize(new Vector3(0, 0, 0));
        }
    }

    public unsafe class UniformScale
    {
        private Vector4[] vectors = new Vector4[WorkSize];

        public const int WorkSize = 1024;

        public UniformScale()
        {
        }

        private struct Particle
        {
            public float X, Y, Z, W;
        }

        public void Baseline()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                Particle particle = new() { X = i, Y = i, Z = i, W = i };
                Particle* p = &particle;
                Vector4* myVec = (Vector4*)p;
                vectors[i] = *myVec;
            }
        }

        public void BitwiseMask()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                Particle particle = new() { X = i, Y = i, Z = i, W = i };
                Particle* p = &particle;
                float* f = (float*)p;
                Vector4 myVec = new(f[0], f[1], f[2], f[3]);
                vectors[i] = myVec;
            }
        }
    }
}