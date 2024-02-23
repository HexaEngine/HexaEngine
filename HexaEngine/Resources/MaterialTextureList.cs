namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using System.Collections;

    public unsafe class MaterialTextureList : IList<MaterialTexture?>, IDisposable
    {
        private const uint DefaultCapacity = 4;
        private readonly List<MaterialTexture?> textures = [];
        private uint capacity;
        private uint count;
        private uint startTextureSlot;
        private uint startSamplerSlot;

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

        public uint StartTextureSlot { get => startTextureSlot; set => startTextureSlot = value; }

        public uint StartSamplerSlot { get => startSamplerSlot; set => startSamplerSlot = value; }

        public uint SlotCount => count;

        public void** ShaderResourceViews => shaderResourceViews;

        public void** Samplers => samplers;

        public bool IsReadOnly => ((ICollection<MaterialTexture>)textures).IsReadOnly;

        public MaterialTexture? this[int index]
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

        public void Add(MaterialTexture? texture)
        {
            textures.Add(texture);
        }

        public void Remove(MaterialTexture texture)
        {
            textures.Remove(texture);
        }

        public void Update()
        {
            ZeroMemory(samplers, (uint)(sizeof(nint) * capacity));
            ZeroMemory(shaderResourceViews, (uint)(sizeof(nint) * capacity));

            textures.Sort(TextureIndexSorter.Instance);

            count = 0;

            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];

                if (texture == null || !texture.Initialized)
                {
                    continue;
                }

                EnsureCapacity(count + 1);
                shaderResourceViews[count] = (void*)texture.ShaderResourceView.NativePointer;
                samplers[count] = (void*)texture.Sampler.NativePointer;
                count++;
            }
        }

        public void BindPS(IGraphicsContext context)
        {
            if (count == 0) return;
            context.PSSetShaderResources(startTextureSlot, count, shaderResourceViews);
            context.PSSetSamplers(startSamplerSlot, count, samplers);
        }

        public void UnbindPS(IGraphicsContext context)
        {
            if (count == 0) return;
            nint* empty = stackalloc nint[(int)count];
            context.PSSetShaderResources(startTextureSlot, count, (void**)empty);
            context.PSSetSamplers(startSamplerSlot, count, (void**)empty);
        }

        public void BindGS(IGraphicsContext context)
        {
            if (count == 0) return;
            context.GSSetShaderResources(startTextureSlot, count, shaderResourceViews);
            context.GSSetSamplers(startSamplerSlot, count, samplers);
        }

        public void UnbindGS(IGraphicsContext context)
        {
            if (count == 0) return;
            nint* empty = stackalloc nint[(int)count];
            context.GSSetShaderResources(startTextureSlot, count, (void**)empty);
            context.GSSetSamplers(startSamplerSlot, count, (void**)empty);
        }

        public void BindDS(IGraphicsContext context)
        {
            if (count == 0) return;
            context.DSSetShaderResources(startTextureSlot, count, shaderResourceViews);
            context.DSSetSamplers(startSamplerSlot, count, samplers);
        }

        public void UnbindDS(IGraphicsContext context)
        {
            if (count == 0) return;
            nint* empty = stackalloc nint[(int)count];
            context.DSSetShaderResources(startTextureSlot, count, (void**)empty);
            context.DSSetSamplers(startSamplerSlot, count, (void**)empty);
        }

        public void BindHS(IGraphicsContext context)
        {
            if (count == 0) return;
            context.HSSetShaderResources(startTextureSlot, count, shaderResourceViews);
            context.HSSetSamplers(startSamplerSlot, count, samplers);
        }

        public void UnbindHS(IGraphicsContext context)
        {
            if (count == 0) return;
            nint* empty = stackalloc nint[(int)count];
            context.HSSetShaderResources(startTextureSlot, count, (void**)empty);
            context.HSSetSamplers(startSamplerSlot, count, (void**)empty);
        }

        public void BindVS(IGraphicsContext context)
        {
            if (count == 0) return;
            context.VSSetShaderResources(startTextureSlot, count, shaderResourceViews);
            context.VSSetSamplers(startSamplerSlot, count, samplers);
        }

        public void UnbindVS(IGraphicsContext context)
        {
            if (count == 0) return;
            nint* empty = stackalloc nint[(int)count];
            context.VSSetShaderResources(startTextureSlot, count, (void**)empty);
            context.VSSetSamplers(startSamplerSlot, count, (void**)empty);
        }

        public bool Contains(MaterialTexture? texture)
        {
            return textures.Contains(texture);
        }

        public class TextureIndexSorter : IComparer<MaterialTexture?>
        {
            public static readonly TextureIndexSorter Instance = new();

            public int Compare(MaterialTexture? x, MaterialTexture? y)
            {
                if (x == null || y == null || !x.Initialized || !y.Initialized)
                {
                    return 0;
                }

                var xPriority = GetPriorityFor(x.Desc.Type);
                var yPriority = GetPriorityFor(y.Desc.Type);

                return xPriority.CompareTo(yPriority);
            }

            public static uint GetPriorityFor(MaterialTextureType type)
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

        public int IndexOf(MaterialTexture? item)
        {
            return textures.IndexOf(item);
        }

        public void Insert(int index, MaterialTexture? item)
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
            count = 0;
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
    }
}