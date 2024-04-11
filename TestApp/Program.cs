namespace TestApp
{
    using HexaEngine.Core.Assets.Importer;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public static unsafe partial class Program
    {
        public static void Main()
        {
            UniformScale uniformScale = new UniformScale();
            uniformScale.Baseline();
            uniformScale.BitwiseMask();
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