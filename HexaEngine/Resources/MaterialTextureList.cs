namespace HexaEngine.Resources
{
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.Utilities;
    using System.Collections;

    public unsafe class MaterialTextureList : IList<MaterialTexture?>, IDisposable
    {
        private readonly List<MaterialTexture?> textures = [];

        public MaterialTextureList()
        {
        }

        private bool disposedValue;
        private MaterialShader shader = null!;

        public int Count => textures.Count;

        public bool IsReadOnly => ((ICollection<MaterialTexture>)textures).IsReadOnly;

        public MaterialTexture? this[int index]
        {
            get => textures[index];
            set => textures[index] = value;
        }

        public void Add(MaterialTexture? texture)
        {
            textures.Add(texture);
        }

        public void Remove(MaterialTexture texture)
        {
            textures.Remove(texture);
        }

        public void Update(MaterialShader shader)
        {
            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];

                if (texture == null || !texture.Initialized)
                {
                    continue;
                }

                var type = texture.Desc.Type;

                var name = EnumHelper<MaterialTextureType>.GetName(type).AsSpan();

                foreach (var pass in shader.Passes)
                {
                    pass.Bindings.SetSRV($"{char.ToLower(name[0])}{name[1..]}Texture", texture.ShaderResourceView);
                    pass.Bindings.SetSampler($"{char.ToLower(name[0])}{name[1..]}TextureSampler", texture.Sampler);
                    pass.Bindings.SetSRV($"Srv{texture.Desc.Name}", texture.ShaderResourceView);
                    pass.Bindings.SetSampler($"Sampler{texture.Desc.Name}", texture.Sampler);
                }
            }
            this.shader = shader;
        }

        public bool Contains(MaterialTexture? texture)
        {
            return textures.Contains(texture);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                textures.Clear();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public int IndexOf(MaterialTexture? item)
        {
            return textures.IndexOf(item);
        }

        public void Insert(int index, MaterialTexture? item)
        {
            textures.Insert(index, item);
        }

        public void DisposeResources()
        {
            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];

                if (texture == null || !texture.Initialized)
                {
                    continue;
                }

                var type = texture.Desc.Type;

                var name = EnumHelper<MaterialTextureType>.GetName(type).AsSpan();

                foreach (var pass in shader.Passes)
                {
                    pass.Bindings.SetSRV($"{char.ToLower(name[0])}{name[1..]}Texture", null);
                    pass.Bindings.SetSampler($"{char.ToLower(name[0])}{name[1..]}TextureSampler", null);
                }
            }

            for (int i = 0; i < textures.Count; i++)
            {
                textures[i]?.Dispose();
            }
            Clear();
        }

        public void Clear()
        {
            textures.Clear();
        }

        public void CopyTo(MaterialTexture?[] array, int arrayIndex)
        {
            textures.CopyTo(array, arrayIndex);
        }

        bool ICollection<MaterialTexture?>.Remove(MaterialTexture? item)
        {
            return textures.Remove(item);
        }

        public IEnumerator<MaterialTexture?> GetEnumerator()
        {
            return textures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)textures).GetEnumerator();
        }

        public void RemoveAt(int index)
        {
        }
    }
}