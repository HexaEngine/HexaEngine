namespace HexaEngine.Core.Unsafes
{
    public interface IAllocationCallbacks
    {
        unsafe void* Alloc(nint size
#if TRACELEAK
            , string name
#endif
            );

        unsafe void* Alloc(int size
#if TRACELEAK
         , string name
#endif
         )
        {
            return Alloc((nint)size
#if TRACELEAK
                , name
#endif
                );
        }

        unsafe void* ReAlloc(void* ptr, nint size
#if TRACELEAK
    , string name
#endif
    );

        unsafe void* ReAlloc(void* ptr, int size
#if TRACELEAK
         , string name
#endif
         )
        {
            return ReAlloc(ptr, (nint)size
#if TRACELEAK
                , name
#endif
                );
        }

        unsafe void Free(void* ptr);
    }
}