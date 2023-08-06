namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using System.Collections;

    public unsafe class MaterialTextureList : IList<ResourceInstance<MaterialTexture>?>, IDisposable
    {
        private const uint DefaultCapacity = 4;
        private readonly List<ResourceInstance<MaterialTexture>?> textures = new();
        private uint capacity;
        private uint count;
        private uint startSlot;

        public MaterialTextureList()
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

        public bool IsReadOnly => ((ICollection<ResourceInstance<MaterialTexture>>)textures).IsReadOnly;

        public ResourceInstance<MaterialTexture>? this[int index]
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

        public void Add(ResourceInstance<MaterialTexture>? texture)
        {
            textures.Add(texture);
        }

        public void Remove(ResourceInstance<MaterialTexture> texture)
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

                if (texture == null || texture.Value == null)
                {
                    continue;
                }

                var slot = GetIndexFor(texture.Value.Desc.Type);

                if (slot == uint.MaxValue)
                {
                    continue;
                }

                EnsureCapacity(slot + 1);
                shaderResourceViews[slot] = (void*)texture.Value.ShaderResourceView.NativePointer;
                samplers[slot] = (void*)texture.Value.Sampler.NativePointer;
                count = Math.Max(slot + 1, count);
                startSlot = Math.Min(slot, startSlot);
            }
        }

        public void Bind(IGraphicsContext context)
        {
            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];

                if (texture == null || texture.Value == null)
                {
                    continue;
                }

                var slot = GetIndexFor(texture.Value.Desc.Type);

                if (slot == uint.MaxValue)
                {
                    continue;
                }

                context.PSSetShaderResource(slot, texture.Value.ShaderResourceView);
                context.PSSetSampler(slot, texture.Value.Sampler);
            }
        }

        public bool Contains(ResourceInstance<MaterialTexture>? texture)
        {
            return textures.Contains(texture);
        }

        public static uint GetIndexFor(TextureType type)
        {
            return type switch
            {
                TextureType.None => uint.MaxValue,
                TextureType.Diffuse => 0,
                TextureType.Specular => uint.MaxValue,
                TextureType.Ambient => uint.MaxValue,
                TextureType.Emissive => 4,
                TextureType.Height => uint.MaxValue,
                TextureType.Normal => 1,
                TextureType.Shininess => uint.MaxValue,
                TextureType.Opacity => uint.MaxValue,
                TextureType.Displacement => uint.MaxValue,
                TextureType.AmbientOcclusionRoughnessMetalness => 7,
                TextureType.Reflection => uint.MaxValue,
                TextureType.BaseColor => 0,
                TextureType.NormalCamera => uint.MaxValue,
                TextureType.EmissionColor => uint.MaxValue,
                TextureType.Metalness => 3,
                TextureType.Roughness => 2,
                TextureType.AmbientOcclusion => 5,
                TextureType.Sheen => uint.MaxValue,
                TextureType.Clearcoat => uint.MaxValue,
                TextureType.Transmission => uint.MaxValue,
                TextureType.RoughnessMetalness => 6,
                TextureType.Unknown => uint.MaxValue,
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

        ~MaterialTextureList()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public int IndexOf(ResourceInstance<MaterialTexture>? item)
        {
            return ((IList<ResourceInstance<MaterialTexture>?>)textures).IndexOf(item);
        }

        public void Insert(int index, ResourceInstance<MaterialTexture>? item)
        {
            ((IList<ResourceInstance<MaterialTexture>?>)textures).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<ResourceInstance<MaterialTexture>?>)textures).RemoveAt(index);
            Update();
        }

        public void Clear()
        {
            startSlot = 0;
            count = 0;
            ((ICollection<ResourceInstance<MaterialTexture>?>)textures).Clear();
        }

        public void CopyTo(ResourceInstance<MaterialTexture>[] array, int arrayIndex)
        {
            ((ICollection<ResourceInstance<MaterialTexture>?>)textures).CopyTo(array, arrayIndex);
        }

        bool ICollection<ResourceInstance<MaterialTexture>?>.Remove(ResourceInstance<MaterialTexture>? item)
        {
            return ((ICollection<ResourceInstance<MaterialTexture>?>)textures).Remove(item);
        }

        public IEnumerator<ResourceInstance<MaterialTexture>?> GetEnumerator()
        {
            return ((IEnumerable<ResourceInstance<MaterialTexture>?>)textures).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)textures).GetEnumerator();
        }
    }
}