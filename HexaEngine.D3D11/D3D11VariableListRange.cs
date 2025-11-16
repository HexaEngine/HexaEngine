namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using System;

    public unsafe struct D3D11VariableListRange
    {
        public ShaderStage Stage;
        public UnsafeList<D3D11VariableList> Lists;

        public D3D11VariableListRange(ShaderStage stage, UnsafeList<D3D11VariableList> lists)
        {
            Stage = stage;
            Lists = lists;
        }

        public void TrySetByName<T>(string name, in T value) where T : unmanaged
        {
            for (int i = 0; i < Lists.Count; i++)
            {
                Lists.GetPointer(i)->TrySetByName(name, in value);
            }
        }

        public void TrySetByName<T>(string name, T* values, uint count) where T : unmanaged
        {
            for (int i = 0; i < Lists.Count; i++)
            {
                Lists.GetPointer(i)->TrySetByName(name, values, count);
            }
        }

        public void TrySetByName<T>(string name, ReadOnlySpan<T> span) where T : unmanaged
        {
            fixed (T* ptr = span)
            {
                TrySetByName(name, ptr, (uint)span.Length);
            }
        }

        public void TrySetUnbound<T>(in T value) where T : unmanaged
        {
            for (int i = 0; i < Lists.Count; i++)
            {
                Lists.GetPointer(i)->TrySetUnbound(in value);
            }
        }

        public void Upload(IGraphicsContext context)
        {
            for (int i = 0; i < Lists.Count; i++)
            {
                Lists.GetPointer(i)->Upload(context);
            }
        }

        public void Release()
        {
            for (int i = 0; i < Lists.Count; i++)
            {
                Lists[i].Release();
            }
            Lists.Release();
        }
    }
}