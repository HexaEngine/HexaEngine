#define MoveAndReplace

namespace HexaEngine.PerformanceTests
{
    using BenchmarkDotNet.Attributes;
    using Hexa.NET.Mathematics;
    using HexaEngine.Graphics;
    using HexaEngine.Scenes;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public class Program
    {
        public static void Main()
        {
            Octree<int> octree = new(new BoundingBox(new(-10000, -10000, -10000), new(10000, 10000, 10000)));

            const int objectCount = 10000;
            Sphere[] spheres = new Sphere[objectCount];
            BoundingBox[] boxes = new BoundingBox[objectCount];
            Random random = new(1237896);

            for (int i = 0; i < objectCount; i++)
            {
                Sphere sphere = new();
                sphere.Randomize(random);
                boxes[i] = new(sphere);
                spheres[i] = sphere;
            }

            // warmup.
            {
                for (int j = 0; j < 100; j++)
                {
                    for (int i = 0; i < objectCount; i++)
                    {
                        octree.AddObject(i, spheres[i]);
                    }

                    for (int i = 0; i < objectCount; i++)
                    {
                        octree.UpdateObject(i, spheres[i]);
                    }

                    for (int i = 0; i < objectCount; i++)
                    {
                        octree.RemoveObject(i);
                    }
                }

                octree.Clear();
            }

            const int iterations = 1024;
            double[] samples = new double[iterations];
            {
                for (int ix = 0; ix < iterations; ix++)
                {
                    long start = Stopwatch.GetTimestamp();
                    for (int i = 0; i < objectCount; i++)
                    {
                        octree.AddObject(i, spheres[i]);
                    }
                    octree.Clear();
                    long end = Stopwatch.GetTimestamp();
                    samples[ix] = (end - start) / (double)Stopwatch.Frequency;
                }

                double mean = samples.Sum();
                double avg = samples.Average();
                double stddev = samples.StdDev();
                Console.WriteLine($"({objectCount} objects) ({iterations} Iterations)");
                Console.WriteLine($"(Octree) Insert: Mean: {mean.FormatTime()}, Avg: {avg.FormatTime()}, StdDev: {stddev.FormatTime()}, Avg per Obj:{(avg / objectCount).FormatTime()}");
            }
            octree.Clear();
            Array.Clear(samples);
            {
                for (int ix = 0; ix < iterations; ix++)
                {
                    // prepare test.
                    for (int i = 0; i < objectCount; i++)
                    {
                        octree.AddObject(i, spheres[i]);
                    }

                    var start = Stopwatch.GetTimestamp();

                    for (int i = 0; i < objectCount; i++)
                    {
                        octree.RemoveObject(i);
                    }

                    var end = Stopwatch.GetTimestamp();

                    samples[ix] = (end - start) / (double)Stopwatch.Frequency;
                }

                double mean = samples.Sum();
                double avg = samples.Average();
                double stddev = samples.StdDev();

                Console.WriteLine($"({objectCount} objects) ({iterations} Iterations)");
                Console.WriteLine($"(Octree) Delete: Mean: {mean.FormatTime()}, Avg: {avg.FormatTime()}, StdDev: {stddev.FormatTime()}, Avg per Obj:{(avg / objectCount).FormatTime()}");
            }
            octree.Clear();
            Array.Clear(samples);
            {
                // prepare test.
                for (int i = 0; i < objectCount; i++)
                {
                    octree.AddObject(i, spheres[i]);
                }

                for (int ix = 0; ix < iterations; ix++)
                {
                    for (int i = 0; i < objectCount; i++)
                    {
                        spheres[i].Move(random, 1f);
                    }

                    var start = Stopwatch.GetTimestamp();

                    for (int i = 0; i < objectCount; i++)
                    {
                        octree.RemoveObject(i);
                        octree.AddObject(i, spheres[i]);
                    }

                    var end = Stopwatch.GetTimestamp();

                    samples[ix] = (end - start) / (double)Stopwatch.Frequency;
                }

                double mean = samples.Sum();
                double avg = samples.Average();
                double stddev = samples.StdDev();
                Console.WriteLine($"({objectCount} objects) ({iterations} Iterations)");
                Console.WriteLine($"(Octree) Reinsert (Remove Insert): Mean: {mean.FormatTime()}, Avg: {avg.FormatTime()}, StdDev: {stddev.FormatTime()}, Avg per Obj:{(avg / objectCount).FormatTime()}");
            }
            octree.Clear();
            Array.Clear(samples);
            {
                // prepare test.
                for (int i = 0; i < objectCount; i++)
                {
                    octree.AddObject(i, spheres[i]);
                }

                for (int ix = 0; ix < iterations; ix++)
                {
                    for (int i = 0; i < objectCount; i++)
                    {
                        spheres[i].Move(random, 1f);
                    }

                    var start = Stopwatch.GetTimestamp();

                    for (int i = 0; i < objectCount; i++)
                    {
                        octree.UpdateObject(i, spheres[i]);
                    }

                    var end = Stopwatch.GetTimestamp();

                    samples[ix] = (end - start) / (double)Stopwatch.Frequency;
                }

                double mean = samples.Sum();
                double avg = samples.Average();
                double stddev = samples.StdDev();
                Console.WriteLine($"({objectCount} objects) ({iterations} Iterations)");
                Console.WriteLine($"(Octree) Reinsert (Optimized): Mean: {mean.FormatTime()}, Avg: {avg.FormatTime()}, StdDev: {stddev.FormatTime()}, Avg per Obj:{(avg / objectCount).FormatTime()}");
            }

            return;
            Test test = new Test();

            const int numRuns = 10;
            double singleThreadedTotal = 0;
            double multiThreadedTotal = 0;
            // Warmup
            for (int i = 0; i < 1024; i++)
            {
                test.TestSpheres();
            }

            // Single-threaded
            for (int j = 0; j < numRuns; j++)
            {
                var start = Stopwatch.GetTimestamp();
                test.TestSpheres();
                var end = Stopwatch.GetTimestamp();
                var elapsed = (end - start) / (double)Stopwatch.Frequency;
                singleThreadedTotal += elapsed;
            }
            Console.WriteLine($"Average Single-threaded: {(singleThreadedTotal / numRuns) * 1000}ms");

            // Warmup
            for (int i = 0; i < 1024; i++)
            {
                test.TestSpheresParallel();
            }

            // Multi-threaded
            for (int j = 0; j < numRuns; j++)
            {
                var start = Stopwatch.GetTimestamp();
                test.TestSpheresParallel();
                var end = Stopwatch.GetTimestamp();
                var elapsed = (end - start) / (double)Stopwatch.Frequency;
                multiThreadedTotal += elapsed;
            }
            Console.WriteLine($"Average Multi-threaded: {(multiThreadedTotal / numRuns) * 1000}ms");
        }
    }

    public static class Helper
    {
        public static string FormatTime(this double value)
        {
            int i = 0;
            while (value < 1)
            {
                value *= 1000;
                i++;
            }

            if (i == 0)
            {
                return $"{value:n3}s";
            }

            char prefix = i switch
            {
                0 => ' ',
                1 => 'm',
                2 => 'µ',
                3 => 'n',
                _ => throw new Exception()
            };

            return $"{value:n3}{prefix}s";
        }

        public static double StdDev(this IList<double> values)
        {
            double standardDeviation = 0;
            if (values.Any())
            {
                // Compute the average.
                double avg = values.Average();

                // Perform the Sum of (value-avg)_2_2.
                double sum = SumStdDev(values, avg);

                // Put it all together.
                standardDeviation = Math.Sqrt(sum / (values.Count - 1));
            }
            return standardDeviation;
        }

        public static double SumStdDev(IList<double> values, double avg)
        {
            double sum = 0;
            foreach (var value in values)
            {
                sum += Math.Pow(value - avg, 2);
            }
            return sum;
        }
    }

    public struct Sphere : IEquatable<Sphere>
    {
        public Vector3 Center;
        public float Radius;

        public Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is Sphere sphere && Equals(sphere);
        }

        public readonly bool Equals(Sphere other)
        {
            return Center.Equals(other.Center) &&
                   Radius == other.Radius;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Center, Radius);
        }

        public void Randomize()
        {
            Center = new(Random.Shared.Next(-9000, 9000), Random.Shared.Next(-9000, 9000), Random.Shared.Next(-9000, 9000));
            Radius = Random.Shared.Next(1, 4);
        }

        public void Randomize(Random random)
        {
            Center = new(random.Next(-8000, 8000), random.Next(-8000, 8000), random.Next(-8000, 8000));
            Radius = random.Next(1, 4);
        }

        public void Move(Random random, float deltaTime)
        {
            Vector3 velocity = new(random.Next(-10, 10), random.Next(-10, 10), random.Next(-10, 10));
            Center += velocity * deltaTime;
        }

        public static bool operator ==(Sphere left, Sphere right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Sphere left, Sphere right)
        {
            return !(left == right);
        }

        public static implicit operator BoundingSphere(Sphere sphere) => new(sphere.Center, sphere.Radius);
    }

    public class Test : IDisposable
    {
        private readonly Camera _camera = new();
        private Vector4 Frustum;
        private float CamNear;
        private float CamFar;
        private readonly Sphere[] spheres = new Sphere[1024 * 1024];
        private int counter;
        private readonly Thread[] threads;
        private readonly ManualResetEventSlim[] handles;
        private readonly Barrier barrier;
        private bool running = true;
        private int count;

        public Test()
        {
            _camera.Transform.Recalculate();
            ComputePlanes(_camera);
            for (int i = 0; i < spheres.Length; i++)
            {
                spheres[i].Randomize();
            }

            threads = new Thread[Environment.ProcessorCount];
            handles = new ManualResetEventSlim[threads.Length];
            barrier = new(threads.Length + 1);
            for (int i = 0; i < threads.Length; i++)
            {
                handles[i] = new ManualResetEventSlim(false);
                threads[i] = new Thread(WorkerVoid);
                threads[i].Start(i);
            }
        }

        public void ComputePlanes(Camera camera)
        {
            Matrix4x4 projection = camera.Transform.Projection;

            Matrix4x4 projectionT = Matrix4x4.Transpose(projection);

            Vector4 frustumX = MathUtil.NormalizePlane(projectionT.GetRow(3) + projectionT.GetRow(0)); // x + w < 0
            Vector4 frustumY = MathUtil.NormalizePlane(projectionT.GetRow(3) + projectionT.GetRow(1)); // y + w < 0

            Frustum = new(frustumX.X, frustumX.Z, frustumY.Y, frustumY.Z);
            CamNear = camera.Near;
            CamFar = camera.Far;
        }

        private unsafe int* globalActives = (int*)Marshal.AllocHGlobal(1024 * 1024 * sizeof(int));

        private unsafe void WorkerVoid(object? param)
        {
            int id = (int)param!;
            var handle = handles[id];
            const int localBufferSize = 8192;
            int* localActives = stackalloc int[localBufferSize];
            while (running)
            {
                handle.Wait(); // wait for work.
                handle.Reset();

                if (!running) // early exit if signal was for exiting.
                {
                    return;
                }

                int batchSize = count / threads.Length;
                int start = batchSize * id;

                // instead of doing Interlocked.Increment every time defer it and batch add it when the thread local buffer is full, minimizes time for syncronisation.

                int active = 0;
                for (int i = 0; i < batchSize; i++)
                {
                    var sphere = spheres[start + i];
                    if (IsVisible(sphere.Center, sphere.Radius))
                    {
                        localActives[active] = start + i;
                        active++;
                        if (active == localBufferSize)
                        {
                            Offload(localActives, &active);
                        }
                    }
                }

                Offload(localActives, &active);

                barrier.SignalAndWait(); // signal that work is done to the dispatcher thread.
            }

            void Offload(int* localActives, int* active)
            {
                int count = *active;
                int location = Interlocked.Add(ref counter, count) - count;
                var size = sizeof(int) * count;
                Buffer.MemoryCopy(localActives, globalActives + location, size, size);
                *active = 0;
            }
        }

        [Benchmark()]
        public unsafe void TestSpheresParallel()
        {
            counter = 0;
            count = spheres.Length;
            for (int i = 0; i < threads.Length; i++)
            {
                handles[i].Set();
            }
            int batchSize = count / threads.Length;
            int rem = count % threads.Length;
            int start = batchSize * threads.Length;

            barrier.SignalAndWait();

            for (int i = 0; i < rem; i++)
            {
                var sphere = spheres[start + i];
                if (IsVisible(sphere.Center, sphere.Radius))
                {
                    globalActives[counter] = i;
                    counter++;
                }
            }
        }

        [Benchmark()]
        public unsafe void TestSpheres()
        {
            counter = 0;
            for (int i = 0; i < spheres.Length; i++)
            {
                var sphere = spheres[i];
                if (IsVisible(sphere.Center, sphere.Radius))
                {
                    globalActives[counter] = i;
                    counter++;
                }
            }
        }

        public bool IsVisible(Vector3 center, float radius)
        {
            bool visible = true;

            // the left/top/right/bottom plane culling utilizes frustum symmetry to cull against two planes at the same time
            visible = visible && center.Z * Frustum[1] - Math.Abs(center.X) * Frustum[0] > -radius;
            visible = visible && center.Z * Frustum[3] - Math.Abs(center.Y) * Frustum[2] > -radius;
            // the near/far plane culling uses camera space Z directly
            visible = visible && center.Z + radius > CamNear && center.Z - radius < CamFar;

            return visible;
        }

        public unsafe void Dispose()
        {
            running = false;
            for (int i = 0; i < threads.Length; i++)
            {
                handles[i].Set();
                threads[i].Join();
            }

            if (globalActives != null)
            {
                Marshal.FreeHGlobal((nint)globalActives);
                globalActives = null;
            }
        }
    }
}