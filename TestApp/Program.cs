namespace TestApp
{
    using HexaEngine.Audio.Common.Flac;
    using HexaEngine.Core.Unsafes;
    using System;

    public unsafe struct BufferHandle
    {
        public void* Buffer;
        public int Start;
        public int Length;

        public readonly Span<byte> Span => new(Buffer, Length);

        public readonly int End => Start + Length;
    }

    public unsafe class NativeBuffer(int size)
    {
        private readonly List<Pointer<BufferHandle>> handles = new();
        private readonly void* buffer = Alloc(size);
        private readonly int size = size;
        private int totalAvailable = size;
        private int totalFragmented;

        private int head;

        public IReadOnlyList<Pointer<BufferHandle>> Handles => handles;

        public int TotalAvailable => totalAvailable;

        public int TotalFragmented => totalFragmented;

        /// <summary>
        /// Rents an buffer from the pool.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Pointer<BufferHandle> Rent(int size)
        {
            if (totalAvailable < size)
            {
                throw new Exception("Buffer is full");
            }

            if (head + size > this.size)
            {
                Consolidate();
            }

            if (head + size > this.size)
            {
                throw new Exception("Could not defragment");
            }

            Pointer<BufferHandle> handle = new();
            handle.Data->Buffer = (byte*)buffer + head;
            handle.Data->Start = head;
            handle.Data->Length = size;

            totalAvailable -= size;
            head += size;

            handles.Add(handle);

            return handle;
        }

        /// <summary>
        /// Returns an buffer back to the pool
        /// </summary>
        /// <param name="handle"></param>
        public void Return(Pointer<BufferHandle> handle)
        {
            var start = handle.Data->Start;
            var end = handle.Data->End;
            var len = handle.Data->Length;

            if (end == head)
            {
                head -= len;
            }
            else
            {
                totalFragmented += len;
            }

            handles.Remove(handle);
            totalAvailable += handle.Data->Length;
            handle.Free();

            if (totalAvailable == size)
            {
                head = 0;
                totalFragmented = 0;
            }
        }

        /// <summary>
        /// Defragments the buffer
        /// </summary>
        public void Consolidate()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("### Consolidate Start");
            Console.ForegroundColor = ConsoleColor.White;

            int last = 0;
            foreach (var handle in handles.OrderBy(x => x.Data->Start))
            {
                if (handle.Data->Start == 0)
                {
                    last = handle.Data->End;
                    continue;
                }

                if (handle.Data->Start > last)
                {
                    MoveTo(handle, last);
                }

                last = handle.Data->End;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("### Consolidate End");
            Console.ForegroundColor = ConsoleColor.White;

            head = last;
            totalFragmented = 0;
        }

        private void MoveTo(BufferHandle* handle, int position)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Moved Block: Start: {handle->Start}, End: {handle->End}, Length: {handle->Length} to Start: {position}");
            Console.ForegroundColor = ConsoleColor.White;
            Span<byte> span = new(buffer, size);
            handle->Span.CopyTo(span[position..]);
            handle->Buffer = (byte*)buffer + position;
            handle->Start = position;
        }
    }

    public static partial class Program
    {
        public static void Main()
        {
            var fs = File.OpenRead("Sample_BeeMoved_96kHz24bit.flac");
            FlacHeader header = default;
            header.Read(fs);

            using BitReader reader = new(fs);

            while (!reader.EndOfStream)
            {
                reader.BytePosition = header.DataBegin;
                Frame frame = default;
                frame.Read(reader, default);
            }

            StdString str0 = "Test 123 123!";
            StdString str1 = "Test 123 123!";
            StdString str2 = "Test 123 123!";

            // same
            str0.Replace("123", "312");
            // smoller
            str1.Replace("123", "0");
            // lorger
            str2.Replace("123", "Hello World");

            NativeBuffer buffer = new(8192 * 16);

            int idx = 0;
            while ((idx = str0.Find("312", idx + 1)) != -1)
            {
            }

            RandomizedTest(buffer, 1837819);
            RandomizedTest(buffer, 46463);
            RandomizedTest(buffer, 534534);
            RandomizedTest(buffer, 7567);
            RandomizedTest(buffer, 234234);
            RandomizedTest(buffer, 75673);
            RandomizedTest(buffer, 34537);
        }

        private static void RandomizedTest(NativeBuffer buffer, int seed)
        {
            List<Pointer<BufferHandle>> handles = new();

            Random random = new(seed);

            AllocateRandom(random, buffer, handles);

            PrintMemoryUsage(buffer);

            FreeRandom(random, buffer, handles);

            PrintMemoryUsage(buffer);

            AllocateRandom(random, buffer, handles);

            PrintMemoryUsage(buffer);

            FreeAll(buffer, handles);

            PrintMemoryUsage(buffer);
        }

        private static void AllocateRandom(Random random, NativeBuffer buffer, List<Pointer<BufferHandle>> handles)
        {
            while (buffer.TotalAvailable > 0)
            {
                handles.Add(buffer.Rent(random.Next(Math.Min(256, buffer.TotalAvailable), buffer.TotalAvailable)));
            }
        }

        private static void FreeRandom(Random random, NativeBuffer buffer, List<Pointer<BufferHandle>> handles)
        {
            var toRemove = handles.Count / 2;
            for (int i = 0; i < toRemove; i++)
            {
                var idx = random.Next(0, handles.Count);
                var handle = handles[idx];
                buffer.Return(handle);
                handles.RemoveAt(idx);
            }
        }

        private static void FreeAll(NativeBuffer buffer, List<Pointer<BufferHandle>> handles)
        {
            for (int i = 0; i < handles.Count; i++)
            {
                buffer.Return(handles[i]);
            }
            handles.Clear();
        }

        private static unsafe void PrintMemoryUsage(NativeBuffer manager)
        {
            Console.WriteLine($"Memory Usage: Total Free: {manager.TotalAvailable}, Fragmented: {manager.TotalFragmented}");
            foreach (var handle in manager.Handles)
            {
                Console.WriteLine($"Block: Start: {handle.Data->Start}, End: {handle.Data->End}, Length: {handle.Data->Length}");
            }
            Console.WriteLine();
        }
    }
}