namespace HexaEngine.Core.Unsafes
{
    using HexaEngine.Core.Debugging;
    using System.Runtime.InteropServices;

    public unsafe class AllocationCallbacks : IAllocationCallbacks
    {
#if TRACELEAK
        private readonly List<Allocation> allocations = [];

        public IReadOnlyList<Allocation> Allocations => allocations;
#endif
#if TRACELEAK

        public struct Allocation
        {
            public void* Ptr;
            public nint Size;
            public string Caller;

            public Allocation(void* ptr, nint size, string caller)
            {
                Ptr = ptr;
                Size = size;
                Caller = caller;
            }
        }

#endif

#if TRACELEAK

        private void Remove(void* ptr)
        {
            if (ptr == null)
            {
                return;
            }

            lock (allocations)
            {
                for (int i = 0; i < allocations.Count; i++)
                {
                    Allocation allocation = allocations[i];
                    if (allocation.Ptr == ptr)
                    {
                        allocations.RemoveAt(i);
                        return;
                    }
                }
            }
        }

#endif

        public void ReportInstances()
        {
#if TRACELEAK
            lock (allocations)
            {
                foreach (Allocation allocation in allocations)
                {
                    Logger.Warn($"*** Live allocation ({allocation.Caller}):\n\t{(nint)allocation.Ptr:X}\n\tSize: {allocation.Size}\n");
                }
            }
#endif
        }

        public void ReportDuplicateInstances()
        {
#if TRACELEAK
            lock (allocations)
            {
                foreach (Allocation allocation in allocations)
                {
                    foreach (Allocation allocationCmp in allocations)
                    {
                        if (allocation.Caller == allocationCmp.Caller && allocation.Size == allocationCmp.Size)
                        {
                            Logger.Warn($"*** Possible duplicate instance ({allocation.Caller}):\n\t{(nint)allocation.Ptr:X}\n\tSize: {allocation.Size}\n");
                        }
                    }
                }
            }
#endif
        }

        public void* Alloc(nint size
#if TRACELEAK
            , string name
#endif
            )
        {
            void* ptr = (void*)Marshal.AllocHGlobal(size); ;
#if TRACELEAK
            Allocation allocation = new(ptr, size, name);
            lock (allocations)
            {
                allocations.Add(allocation);
            }
#endif
            return ptr;
        }

        public void* ReAlloc(void* ptr, nint size
#if TRACELEAK
            , string name
#endif
            )
        {
#if TRACELEAK
            Remove(ptr);
#endif
            ptr = (void*)Marshal.ReAllocHGlobal((nint)ptr, size); ;
#if TRACELEAK
            Allocation allocation = new(ptr, size, name);
            lock (allocations)
            {
                allocations.Add(allocation);
            }
#endif
            return ptr;
        }

        public void Free(void* ptr)
        {
#if TRACELEAK
            Remove(ptr);
#endif
            Marshal.FreeHGlobal((nint)ptr);
        }
    }
}