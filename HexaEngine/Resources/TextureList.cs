namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using System.Collections;

    public unsafe class TextureList : IList<MaterialTexture>, IDisposable
    {
        private const uint DefaultCapacity = 4;
        private readonly List<MaterialTexture> textures = new();
        private uint capacity;
        private uint count;
        private uint startSlot;

        public TextureList()
        {
            capacity = DefaultCapacity;
            shaderResourceViews = AllocArray(capacity);
            samplers = AllocArray(capacity);
            ZeroMemory(samplers, (uint)(sizeof(nint) * capacity));
            ZeroMemory(shaderResourceViews, (uint)(sizeof(nint) * capacity));
        }

        public void** shaderResourceViews;
        public void** samplers;
        private bool disposedValue;

        public int Count => textures.Count;

        public uint StartSlot => startSlot;

        public uint SlotCount => count;

        public void** ShaderResourceViews => shaderResourceViews;

        public void** Samplers => samplers;

        public bool IsReadOnly => false;

        public MaterialTexture this[int index]
        {
            get => textures[index];
            set => textures[index] = value;
        }

        private void EnsureCapacity(uint newCapacity)
        {
            if (newCapacity > capacity)
            {
                var oldCapacity = capacity;
                capacity = (uint)(newCapacity * 1.5f);
                var tmpShaderResourceViews = shaderResourceViews;
                var tmpSamplers = samplers;
                shaderResourceViews = AllocArray(capacity);
                samplers = AllocArray(capacity);
                ZeroMemory(samplers, (uint)(sizeof(nint) * capacity));
                ZeroMemory(shaderResourceViews, (uint)(sizeof(nint) * capacity));
                Memcpy(tmpShaderResourceViews, shaderResourceViews, capacity * sizeof(nint), oldCapacity * sizeof(nint));
                Memcpy(tmpSamplers, samplers, capacity * sizeof(nint), oldCapacity * sizeof(nint));
                Free(tmpShaderResourceViews);
                Free(tmpSamplers);
            }
        }

        public void Add(MaterialTexture texture)
        {
            textures.Add(texture);
        }

        public void Remove(MaterialTexture texture)
        {
            textures.Remove(texture);
        }

        public void Update()
        {
            startSlot = int.MaxValue;
            count = 0;
            ZeroMemory(samplers, (uint)(sizeof(nint) * capacity));
            ZeroMemory(shaderResourceViews, (uint)(sizeof(nint) * capacity));
            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];

                if (texture == null)
                {
                    continue;
                }

                var slot = GetIndexFor(texture.Desc.Type);

                if (slot == uint.MaxValue)
                {
                    continue;
                }

                EnsureCapacity(slot + 1);
                shaderResourceViews[slot] = (void*)texture.ShaderResourceView.NativePointer;
                samplers[slot] = (void*)texture.Sampler.NativePointer;
                count = Math.Max(slot + 1, count);
                startSlot = Math.Min(slot, startSlot);
            }
        }

        public void Bind(IGraphicsContext context)
        {
            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];

                if (texture == null)
                {
                    continue;
                }

                var slot = GetIndexFor(texture.Desc.Type);

                if (slot == uint.MaxValue)
                {
                    continue;
                }

                context.PSSetShaderResource(slot, texture.ShaderResourceView);
                context.PSSetSampler(slot, texture.Sampler);
            }
        }

        public bool Contains(MaterialTexture texture)
        {
            return textures.Contains(texture);
        }

        public static uint GetIndexFor(MaterialTextureType type)
        {
            return type switch
            {
                MaterialTextureType.None => uint.MaxValue,
                MaterialTextureType.Diffuse => 0,
                MaterialTextureType.Specular => uint.MaxValue,
                MaterialTextureType.Ambient => uint.MaxValue,
                MaterialTextureType.Emissive => 4,
                MaterialTextureType.Height => uint.MaxValue,
                MaterialTextureType.Normal => 1,
                MaterialTextureType.Shininess => uint.MaxValue,
                MaterialTextureType.Opacity => uint.MaxValue,
                MaterialTextureType.Displacement => uint.MaxValue,
                MaterialTextureType.AmbientOcclusionRoughnessMetalness => 7,
                MaterialTextureType.Reflection => uint.MaxValue,
                MaterialTextureType.BaseColor => 0,
                MaterialTextureType.NormalCamera => uint.MaxValue,
                MaterialTextureType.EmissionColor => uint.MaxValue,
                MaterialTextureType.Metalness => 3,
                MaterialTextureType.Roughness => 2,
                MaterialTextureType.AmbientOcclusion => 5,
                MaterialTextureType.Sheen => uint.MaxValue,
                MaterialTextureType.Clearcoat => uint.MaxValue,
                MaterialTextureType.Transmission => uint.MaxValue,
                MaterialTextureType.RoughnessMetalness => 6,
                MaterialTextureType.Unknown => uint.MaxValue,
                _ => uint.MaxValue,
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                textures.Clear();
                Free(shaderResourceViews);
                Free(samplers);
                count = 0;
                capacity = 0;
                shaderResourceViews = null;
                samplers = null;

                disposedValue = true;
            }
        }

        ~TextureList()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public int IndexOf(MaterialTexture item)
        {
            return ((IList<MaterialTexture>)textures).IndexOf(item);
        }

        public void Insert(int index, MaterialTexture item)
        {
            ((IList<MaterialTexture>)textures).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<MaterialTexture>)textures).RemoveAt(index);
            Update();
        }

        public void Clear()
        {
            startSlot = 0;
            count = 0;
            ((ICollection<MaterialTexture>)textures).Clear();
        }

        public void CopyTo(MaterialTexture[] array, int arrayIndex)
        {
            ((ICollection<MaterialTexture>)textures).CopyTo(array, arrayIndex);
        }

        bool ICollection<MaterialTexture>.Remove(MaterialTexture item)
        {
            return ((ICollection<MaterialTexture>)textures).Remove(item);
        }

        public IEnumerator<MaterialTexture> GetEnumerator()
        {
            return ((IEnumerable<MaterialTexture>)textures).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)textures).GetEnumerator();
        }
    }
}