using System.Runtime.CompilerServices;

public class Program
{
    /// <summary>To be documented.</summary>

    public static unsafe void Main()
    {
        Console.WriteLine("Hello, World!");
        ID3DInclude include;

        delegate*<ID3DInclude*, D3DIncludeType, byte*, void*, void**, uint*, int> pOpen = &Open;
        delegate*<ID3DInclude*, void*, int> pClose = &Close;

        void*[] callbacks = new void*[] { pOpen, pClose };

        fixed (void** pCallbacks = callbacks)
        {
            include = new ID3DInclude(pCallbacks);
        }

        include.Open(D3DIncludeType.None, null, null, null, null);
        include.Close(null);
    }

    public enum D3DIncludeType
    {
        None,
    }

    private static unsafe int Open(ID3DInclude* pInclude, D3DIncludeType IncludeType, byte* pFileName, void* pParentData, void** ppData, uint* pBytes)
    {
        Console.WriteLine("Hello from Open!");
        return -1;
    }

    private static unsafe int Close(ID3DInclude* pInclude, void* pData)
    {
        Console.WriteLine("Hello from Close!");
        return -1;
    }

    internal unsafe struct ID3DInclude
    {
        public void** LpVtbl;

        public ID3DInclude(void** lpVtbl)
        {
            LpVtbl = lpVtbl;
        }

        /// <summary>To be documented.</summary>
        public readonly unsafe int Open(D3DIncludeType IncludeType, byte* pFileName, void* pParentData, void** ppData, uint* pBytes)
        {
            var @this = (ID3DInclude*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            int ret = default;
            ret = ((delegate* unmanaged[Stdcall]<ID3DInclude*, D3DIncludeType, byte*, void*, void**, uint*, int>)@this->LpVtbl[0])(@this, IncludeType, pFileName, pParentData, ppData, pBytes);
            return ret;
        }

        /// <summary>To be documented.</summary>
        public readonly unsafe int Close(void* pData)
        {
            var @this = (ID3DInclude*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
            int ret = default;
            ret = ((delegate* unmanaged[Stdcall]<ID3DInclude*, void*, int>)@this->LpVtbl[1])(@this, pData);
            return ret;
        }
    }
}