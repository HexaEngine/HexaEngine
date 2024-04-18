namespace HexaEngine.PerformanceTests
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;
    using HexaEngine.Core.Unsafes;

    public class Program
    {
        public static void Main()
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }

    public class TypeObject
    {
        public int Value;
    }

    public unsafe class SSETests
    {
        private readonly ConcurrentNativeToManagedMapper mapper = new();

        public const int WorkSize = 1024 * 1024;

        public SSETests()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                mapper.AddMapping(i, new TypeObject());
            }
        }

        [Benchmark(Baseline = true)]
        public void Access()
        {
            var obj = (TypeObject)mapper.GetManagedObject(Random.Shared.Next(0, WorkSize));
            obj.Value = Random.Shared.Next(0, WorkSize);
        }
    }
}