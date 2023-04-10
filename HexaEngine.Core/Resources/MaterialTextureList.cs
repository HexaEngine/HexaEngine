namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using System.Collections;

    public unsafe class MaterialTextureList : IList<ResourceInstance<MaterialTexture>?>, IDisposable
    {
        private const uint DefaultCapacity = 4;
        private readonly List<ResourceInstance<MaterialTexture>?> textures = new();
        private uint capacity;
        private int count;
        private int startSlot;

        public MaterialTextureList()
        {
            capacity = DefaultCapacity;
            shaderResourceViews = AllocArray(capacity);
            samplers = AllocArray(capacity);
            Zero(samplers, (uint)(sizeof(nint) * capacity));
            Zero(shaderResourceViews, (uint)(sizeof(nint) * capacity));
        }

        public void** shaderResourceViews;
        public void** samplers;
        private bool disposedValue;

        public int Count => count;

        public int StartSlot => startSlot;

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
                Zero(samplers, (uint)(sizeof(nint) * capacity));
                Zero(shaderResourceViews, (uint)(sizeof(nint) * capacity));
                MemoryCopy(tmpShaderResourceViews, shaderResourceViews, capacity * sizeof(nint), oldCapacity * sizeof(nint));
                MemoryCopy(tmpSamplers, samplers, capacity * sizeof(nint), oldCapacity * sizeof(nint));
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
            Zero(samplers, (uint)(sizeof(nint) * capacity));
            Zero(shaderResourceViews, (uint)(sizeof(nint) * capacity));
            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];

                if (texture == null || texture.Value == null)
                    continue;

                var slot = GetIndexFor(texture.Value.Desc.Type);

                if (slot == -1)
                    continue;

                EnsureCapacity((uint)(slot + 1));
                shaderResourceViews[slot] = (void*)texture.Value.ShaderResourceView.NativePointer;
                samplers[slot] = (void*)texture.Value.Sampler.NativePointer;
                count = Math.Max(slot + 1, count);
                startSlot = Math.Min(slot + 1, startSlot);
            }
        }

        public void Bind(IGraphicsContext context)
        {
            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];

                if (texture == null || texture.Value == null)
                    continue;

                var slot = GetIndexFor(texture.Value.Desc.Type);

                if (slot == -1)
                    continue;

                context.PSSetShaderResource(texture.Value.ShaderResourceView, slot);
                context.PSSetSampler(texture.Value.Sampler, slot);
            }
        }

        public bool Contains(ResourceInstance<MaterialTexture>? texture)
        {
            return textures.Contains(texture);
        }

        public static int GetIndexFor(TextureType type)
        {
            return type switch
            {
                TextureType.None => -1,
                TextureType.Diffuse => 0,
                TextureType.Specular => -1,
                TextureType.Ambient => -1,
                TextureType.Emissive => 4,
                TextureType.Height => -1,
                TextureType.Normals => 1,
                TextureType.Shininess => -1,
                TextureType.Opacity => -1,
                TextureType.Displacement => -1,
                TextureType.Lightmap => -1,
                TextureType.Reflection => -1,
                TextureType.BaseColor => 0,
                TextureType.NormalCamera => -1,
                TextureType.EmissionColor => -1,
                TextureType.Metalness => 3,
                TextureType.Roughness => 2,
                TextureType.AmbientOcclusion => 5,
                TextureType.Sheen => -1,
                TextureType.Clearcoat => -1,
                TextureType.Transmission => -1,
                TextureType.RoughnessMetalness => 6,
                TextureType.Unknown => -1,
                _ => -1,
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                textures.Clear();
                Free(shaderResourceViews);
                Free(samplers);
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
        }

        public void Clear()
        {
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