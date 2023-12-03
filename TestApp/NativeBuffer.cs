namespace TestApp
{
    using System;

    public unsafe class NativeBuffer(int size)
    {
        private readonly List<Pointer<BufferHandle>> handles = new();
        private readonly void* buffer = AllocT(size);
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
}