namespace HexaEngine.PerformanceTests
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;
    using System.Numerics;

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }

    public class UniformScale
    {
        public UniformScale()
        {
        }

        [Benchmark]
        public void BitwiseMask()
        {
            Vector3 scale = new(2, 1, 1);
            Vector3 scaleSrc = new(1);
            float x = BitConverter.UInt32BitsToSingle(BitConverter.SingleToUInt32Bits(scale.X) & ~BitConverter.SingleToUInt32Bits(scaleSrc.X));
            float y = BitConverter.UInt32BitsToSingle(BitConverter.SingleToUInt32Bits(scale.Y) & ~BitConverter.SingleToUInt32Bits(scaleSrc.Y));
            float z = BitConverter.UInt32BitsToSingle(BitConverter.SingleToUInt32Bits(scale.Z) & ~BitConverter.SingleToUInt32Bits(scaleSrc.Z));
            scaleSrc = new(x != 0 ? x : y != 0 ? y : z);
        }

        [Benchmark(Baseline = true)]
        public void Baseline()
        {
            Vector3 scale = new(2, 1, 1);
            Vector3 scaleSrc = new(1);
            scaleSrc = new(scale.X != scaleSrc.X ? scale.X : scale.Y != scaleSrc.Y ? scale.Y : scale.Z);
        }
    }
}