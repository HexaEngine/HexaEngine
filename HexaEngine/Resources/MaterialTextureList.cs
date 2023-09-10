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
                MaterialTextureType.AmbientOcclusionRoughnessMetallic => 7,
                MaterialTextureType.Reflection => uint.MaxValue,
                MaterialTextureType.BaseColor => 0,
                MaterialTextureType.NormalCamera => uint.MaxValue,
                MaterialTextureType.EmissionColor => uint.MaxValue,
                MaterialTextureType.Metallic => 3,
                MaterialTextureType.Roughness => 2,
                MaterialTextureType.AmbientOcclusion => 5,
                MaterialTextureType.Sheen => uint.MaxValue,
                MaterialTextureType.Clearcoat => uint.MaxValue,
                MaterialTextureType.Transmission => uint.MaxValue,
                MaterialTextureType.RoughnessMetallic => 6,
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
            return textures.IndexOf(item);
        }

        public void Insert(int index, ResourceInstance<MaterialTexture>? item)
        {
            textures.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            textures.RemoveAt(index);
            Update();
        }

        public void Clear()
        {
            startSlot = 0;
            count = 0;
            textures.Clear();
        }

        public void CopyTo(ResourceInstance<MaterialTexture>?[] array, int arrayIndex)
        {
            textures.CopyTo(array, arrayIndex);
        }

        bool ICollection<ResourceInstance<MaterialTexture>?>.Remove(ResourceInstance<MaterialTexture>? item)
        {
            return textures.Remove(item);
        }

        public IEnumerator<ResourceInstance<MaterialTexture>?> GetEnumerator()
        {
            return textures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)textures).GetEnumerator();
        }
    }
}