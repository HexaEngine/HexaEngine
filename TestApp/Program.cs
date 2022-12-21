namespace TestApp
{
    using HexaEngine.Mathematics;
    using System.Collections.Concurrent;

    public class Program
    {
        private static List<Tes> e = new List<Tes>();

        public static unsafe void Main()
        {
            /*
            List<string> strings = new List<string>();

            while (true)
            {
                string? input = Console.ReadLine();
                if (input == null) continue;
                if (input == "e") break;
                strings.Add(input);
            }
            string inp = string.Join(string.Empty, strings.ToArray());
            string[] nums = inp.Replace(Environment.NewLine, "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            int[] ints = nums.Select(int.Parse).ToArray();
            StringBuilder sb = new();
            for (int i = 0; i < ints.Length; i += 2)
            {
                sb.AppendLine($"{ints[i] - 1},{ints[i + 1] - 1},");
            }

            Console.WriteLine(sb.ToString());*/

            for (int i = 0; i < 16; i++)
            {
                Do(i);
            }
        }

        private static async void Do(int i)
        {
            Tes tes = new();
            e.Add(tes);
            for (int j = 0; j < 16; j++)
            {
                await tes.CreateInstance(i + ":" + j);
            }
        }

        private class Tes
        {
            private readonly ConcurrentDictionary<string, Archtype> t = new();
            private List<Archtype> s = new();
            private SemaphoreSlim semaphore = new(1);
            private int counter;

            public async Task<Archtype> CreateArchtype(string name)
            {
                await semaphore.WaitAsync();

                if (!t.TryGetValue(name, out var type))
                {
                    type = new(Interlocked.Increment(ref counter));
                    t.TryAdd(name, type);
                    s.Add(type);
                }

                semaphore.Release();
                return type;
            }

            public async Task<Instance> CreateInstance(string name)
            {
                await semaphore.WaitAsync();

                if (!t.TryGetValue(name, out var type))
                {
                    type = new(Interlocked.Increment(ref counter));
                    t.TryAdd(name, type);
                    s.Add(type);
                }

                var instance = type.CreateInstance();

                semaphore.Release();
                return instance;
            }
        }

        private class Archtype
        {
            private List<Instance> s = new();
            private int id;
            private int counter;

            public Archtype(int id)
            {
                this.id = id;
            }

            public Instance CreateInstance()
            {
                Instance instance = new(this, Interlocked.Increment(ref counter));
                s.Add(instance);
                return instance;
            }

            public override string ToString()
            {
                return id.ToString();
            }
        }

        private class Instance
        {
            private readonly Archtype archtype;
            private int id;

            public Instance(Archtype archtype, int id)
            {
                this.archtype = archtype;
                this.id = id;
            }

            public override string ToString()
            {
                return $"{archtype}:{id}";
            }
        }
    }
}