namespace HexaEngine.Core.Graphics.Shaders.Reflection
{
    public struct ConstantBuffer
    {
        public string? Name;
        public uint Slot;
        public uint Size;
        public uint PaddedSize;
        public ConstantBufferMemberInfo[] Members;

        public ConstantBuffer(string? name, uint slot, uint size, uint paddedSize, ConstantBufferMemberInfo[] members)
        {
            Name = name;
            Slot = slot;
            Size = size;
            PaddedSize = paddedSize;
            Members = members;
        }

        public void Release()
        {
            if (Members != null)
            {
                for (int i = 0; i < Members.Length; i++)
                {
                    Members[i].Release();
                }
            }

            this = default;
        }
    }

    public class ShaderReflection : IDisposable
    {
        public readonly ConstantBuffer[] ConstantBuffers;

        private bool disposedValue;

        public ShaderReflection(ConstantBuffer[] constantBuffers)
        {
            ConstantBuffers = constantBuffers;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    for (var i = 0; i < ConstantBuffers.Length; i++)
                    {
                        ConstantBuffers[i].Release();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}