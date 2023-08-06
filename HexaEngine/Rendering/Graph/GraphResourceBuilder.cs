namespace HexaEngine.Rendering.Graph
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;

    public class GraphResourceBuilder
    {
        private readonly IGraphicsDevice device;
        private Viewport viewport;
        private Viewport outputViewport;

        public GraphResourceBuilder(IGraphicsDevice device)
        {
            this.device = device;
        }

        private readonly List<IStructuredUavBuffer> structuredUavBuffers = new();
        private readonly Dictionary<string, int> nameToStructuredUavBuffer = new();

        private readonly List<IStructuredBuffer> structuredBuffers = new();
        private readonly Dictionary<string, int> nameToStructuredBuffer = new();

        private readonly List<IConstantBuffer> constantBuffers = new();
        private readonly Dictionary<string, int> nameToConstantBuffer = new();

        private readonly List<ShadowAtlasDescription> shadowAtlasDescriptions = new();
        private readonly List<ShadowAtlas> shadowAtlas = new();
        private readonly Dictionary<string, int> nameToShadowAtlas = new();

        private readonly List<DepthStencilBufferDescription> depthMipChainDescriptions = new();
        private readonly List<DepthMipChain> depthMipChains = new();
        private readonly Dictionary<string, int> nameToDepthMipChain = new();

        private readonly List<DepthStencilBufferDescription> depthStencilBufferDescriptions = new();
        private readonly List<DepthStencil> depthStencilBuffers = new();
        private readonly Dictionary<string, int> nameToDepthStencilBuffer = new();

        private readonly List<GBufferDescription> gBufferDescriptions = new();
        private readonly List<GBuffer> gBuffers = new();
        private readonly Dictionary<string, int> nameToGBuffer = new();

        private readonly List<Texture1DDescription> texture1DDescriptions = new();
        private readonly List<Texture1D> textures1d = new();
        private readonly Dictionary<string, int> nameToTexture1d = new();

        private readonly List<Texture2DDescription> texture2DDescriptions = new();
        private readonly List<Texture2D> textures2d = new();
        private readonly Dictionary<string, int> nameToTexture2d = new();

        private readonly List<Texture3DDescription> texture3DDescriptions = new();
        private readonly List<Texture3D> textures3d = new();
        private readonly Dictionary<string, int> nameToTexture3d = new();

        private readonly List<SamplerStateDescription> samplerStateDescriptions = new();
        private readonly List<ISamplerState> samplerStates = new();
        private readonly Dictionary<string, int> nameToSamplerState = new();

        public IReadOnlyList<Texture2D> Textures => textures2d;

        public IReadOnlyList<ShadowAtlas> ShadowAtlas => shadowAtlas;

        public IReadOnlyList<GBuffer> GBuffers => gBuffers;

        internal void CreateResources()
        {
            int gid = 0;
            for (int i = 0; i < shadowAtlasDescriptions.Count; i++)
            {
                shadowAtlas.Add(new(device, shadowAtlasDescriptions[i], lineNumber: gid++));
            }

            for (int i = 0; i < depthMipChainDescriptions.Count; i++)
            {
                depthMipChains.Add(new(device, depthMipChainDescriptions[i], lineNumber: gid++));
            }

            for (int i = 0; i < depthStencilBufferDescriptions.Count; i++)
            {
                depthStencilBuffers.Add(new(device, depthStencilBufferDescriptions[i], lineNumber: gid++));
            }

            for (int i = 0; i < gBufferDescriptions.Count; i++)
            {
                gBuffers.Add(new(device, gBufferDescriptions[i], lineNumber: gid++));
            }

            for (int i = 0; i < texture1DDescriptions.Count; i++)
            {
                textures1d.Add(new(device, texture1DDescriptions[i], lineNumber: gid++));
            }

            for (int i = 0; i < texture2DDescriptions.Count; i++)
            {
                textures2d.Add(new(device, texture2DDescriptions[i], lineNumber: gid++));
            }

            for (int i = 0; i < texture3DDescriptions.Count; i++)
            {
                textures3d.Add(new(device, texture3DDescriptions[i], lineNumber: gid++));
            }
        }

        internal void ReleaseResources()
        {
            for (int i = 0; i < structuredUavBuffers.Count; i++)
            {
                structuredUavBuffers[i].Dispose();
            }
            structuredUavBuffers.Clear();
            nameToStructuredUavBuffer.Clear();

            for (int i = 0; i < structuredBuffers.Count; i++)
            {
                structuredBuffers[i].Dispose();
            }
            structuredBuffers.Clear();
            nameToStructuredBuffer.Clear();

            for (int i = 0; i < constantBuffers.Count; i++)
            {
                constantBuffers[i].Dispose();
            }
            constantBuffers.Clear();
            nameToConstantBuffer.Clear();

            for (int i = 0; i < shadowAtlas.Count; i++)
            {
                shadowAtlas[i].Dispose();
            }
            shadowAtlas.Clear();
            shadowAtlasDescriptions.Clear();
            nameToShadowAtlas.Clear();

            for (int i = 0; i < depthMipChains.Count; i++)
            {
                depthMipChains[i].Dispose();
            }
            depthMipChains.Clear();
            depthMipChainDescriptions.Clear();
            nameToDepthMipChain.Clear();

            for (int i = 0; i < depthStencilBuffers.Count; i++)
            {
                depthStencilBuffers[i].Dispose();
            }
            depthStencilBuffers.Clear();
            depthStencilBufferDescriptions.Clear();
            nameToDepthStencilBuffer.Clear();

            for (int i = 0; i < gBuffers.Count; i++)
            {
                gBuffers[i].Dispose();
            }
            gBuffers.Clear();
            gBufferDescriptions.Clear();
            nameToGBuffer.Clear();

            for (int i = 0; i < textures1d.Count; i++)
            {
                textures1d[i].Dispose();
            }
            textures1d.Clear();
            texture2DDescriptions.Clear();
            nameToTexture1d.Clear();

            for (int i = 0; i < textures2d.Count; i++)
            {
                textures2d[i].Dispose();
            }
            textures2d.Clear();
            texture2DDescriptions.Clear();
            nameToTexture2d.Clear();

            for (int i = 0; i < textures3d.Count; i++)
            {
                textures3d[i].Dispose();
            }
            textures3d.Clear();
            texture3DDescriptions.Clear();
            nameToTexture3d.Clear();

            for (int i = 0; i < samplerStates.Count; i++)
            {
                samplerStates[i].Dispose();
            }
            samplerStates.Clear();
            samplerStateDescriptions.Clear();
            nameToSamplerState.Clear();
        }

        /// <summary>
        /// The static renderer viewport
        /// </summary>
        public Viewport Viewport { get => viewport; internal set => viewport = value; }

        /// <summary>
        /// The dynamic window viewport
        /// </summary>
        public Viewport OutputViewport { get => outputViewport; internal set => outputViewport = value; }

        public IRenderTargetView? Output { get; internal set; }

        public ITexture2D OutputTex { get; internal set; }

        public StructuredUavBuffer<T> CreateStructuredUavBuffer<T>(string name, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None) where T : unmanaged
        {
            int index = structuredUavBuffers.Count;
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredUavBuffer<T> structuredBuffer = new(device, accessFlags, uavFlags, srvFlags, name, index);
            structuredUavBuffers.Add(structuredBuffer);
            nameToStructuredUavBuffer.Add(name, index);
            return structuredBuffer;
        }

        public StructuredUavBuffer<T> CreateStructuredUavBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None) where T : unmanaged
        {
            int index = structuredUavBuffers.Count;
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredUavBuffer<T> structuredBuffer = new(device, initialCapacity, accessFlags, uavFlags, srvFlags, name, index);
            structuredUavBuffers.Add(structuredBuffer);
            nameToStructuredUavBuffer.Add(name, index);
            return structuredBuffer;
        }

        public int GetIndexOfStructuredUavBuffer(string name)
        {
            return nameToStructuredUavBuffer[name];
        }

        public StructuredUavBuffer<T> GetStructuredUavBuffer<T>(int index) where T : unmanaged
        {
            return (StructuredUavBuffer<T>)structuredUavBuffers[index];
        }

        public StructuredUavBuffer<T> GetStructuredUavBuffer<T>(string name) where T : unmanaged
        {
            return (StructuredUavBuffer<T>)structuredUavBuffers[nameToStructuredUavBuffer[name]];
        }

        public StructuredBuffer<T> CreateStructuredBuffer<T>(string name, CpuAccessFlags accessFlags) where T : unmanaged
        {
            int index = structuredBuffers.Count;
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredBuffer<T> structuredBuffer = new(device, accessFlags, name, index);
            structuredBuffers.Add(structuredBuffer);
            nameToStructuredBuffer.Add(name, index);
            return structuredBuffer;
        }

        public StructuredBuffer<T> CreateStructuredBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags) where T : unmanaged
        {
            int index = structuredBuffers.Count;
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredBuffer<T> structuredBuffer = new(device, initialCapacity, accessFlags, name, index);
            structuredBuffers.Add(structuredBuffer);
            nameToStructuredBuffer.Add(name, index);
            return structuredBuffer;
        }

        public int GetIndexOfStructuredBuffer(string name)
        {
            return nameToStructuredBuffer[name];
        }

        public StructuredBuffer<T> GetStructuredBuffer<T>(int index) where T : unmanaged
        {
            return (StructuredBuffer<T>)structuredBuffers[index];
        }

        public StructuredBuffer<T> GetStructuredBuffer<T>(string name) where T : unmanaged
        {
            return (StructuredBuffer<T>)structuredBuffers[nameToStructuredBuffer[name]];
        }

        public ConstantBuffer<T> CreateConstantBuffer<T>(string name, CpuAccessFlags accessFlags) where T : unmanaged
        {
            int index = constantBuffers.Count;
            // creation of constant buffers cannot be deferred, because of generics.
            ConstantBuffer<T> constantBuffer = new(device, accessFlags, name, index);
            constantBuffers.Add(constantBuffer);
            nameToConstantBuffer.Add(name, index);
            return constantBuffer;
        }

        public int GetIndexOfConstantBuffer(string name)
        {
            return nameToConstantBuffer[name];
        }

        public ConstantBuffer<T> GetConstantBuffer<T>(int index) where T : unmanaged
        {
            return (ConstantBuffer<T>)constantBuffers[index];
        }

        public ConstantBuffer<T> GetConstantBuffer<T>(string name) where T : unmanaged
        {
            return (ConstantBuffer<T>)constantBuffers[nameToConstantBuffer[name]];
        }

        public int CreateShadowAtlas(string name, ShadowAtlasDescription description)
        {
            int index = shadowAtlasDescriptions.Count;
            shadowAtlasDescriptions.Add(description);
            nameToShadowAtlas.Add(name, index);
            return index;
        }

        public int GetIndexOfShadowAtlas(string name)
        {
            return nameToShadowAtlas[name];
        }

        public ShadowAtlas GetShadowAtlas(int index)
        {
            return shadowAtlas[index];
        }

        public ShadowAtlas GetShadowAtlas(string name)
        {
            return shadowAtlas[nameToShadowAtlas[name]];
        }

        public int CreateDepthMipChain(string name, DepthStencilBufferDescription description)
        {
            int index = depthMipChainDescriptions.Count;
            depthMipChainDescriptions.Add(description);
            nameToDepthMipChain.Add(name, index);
            return index;
        }

        public int GetIndexOfDepthMipChain(string name)
        {
            return nameToDepthMipChain[name];
        }

        public DepthMipChain GetDepthMipChain(int index)
        {
            return depthMipChains[index];
        }

        public DepthMipChain GetDepthMipChain(string name)
        {
            return depthMipChains[nameToDepthMipChain[name]];
        }

        public int CreateDepthStencilBuffer(string name, DepthStencilBufferDescription description)
        {
            int index = depthStencilBufferDescriptions.Count;
            depthStencilBufferDescriptions.Add(description);
            nameToDepthStencilBuffer.Add(name, index);
            return index;
        }

        public int GetIndexOfDepthStencilBuffer(string name)
        {
            return nameToDepthStencilBuffer[name];
        }

        public DepthStencil GetDepthStencilBuffer(int index)
        {
            return depthStencilBuffers[index];
        }

        public DepthStencil GetDepthStencilBuffer(string name)
        {
            return depthStencilBuffers[nameToDepthStencilBuffer[name]];
        }

        public int CreateGBuffer(string name, GBufferDescription description)
        {
            int index = gBufferDescriptions.Count;
            gBufferDescriptions.Add(description);
            nameToGBuffer.Add(name, index);
            return index;
        }

        public int GetIndexOfGBuffer(string name)
        {
            return nameToGBuffer[name];
        }

        public GBuffer GetGBuffer(int index)
        {
            return gBuffers[index];
        }

        public GBuffer GetGBuffer(string name)
        {
            return gBuffers[nameToGBuffer[name]];
        }

        public int CreateTexture1D(string name, Texture1DDescription description)
        {
            int index = texture1DDescriptions.Count;
            texture1DDescriptions.Add(description);
            nameToTexture1d.Add(name, index);
            return index;
        }

        public int GetIndexOfTexture1D(string name)
        {
            return nameToTexture1d[name];
        }

        public Texture1D GetTexture1D(int index)
        {
            return textures1d[index];
        }

        public Texture1D GetTexture1D(string name)
        {
            return textures1d[nameToTexture1d[name]];
        }

        public int CreateTexture2D(string name, Texture2DDescription description)
        {
            int index = texture2DDescriptions.Count;
            texture2DDescriptions.Add(description);
            nameToTexture2d.Add(name, index);
            return index;
        }

        public int GetIndexOfTexture2D(string name)
        {
            return nameToTexture2d[name];
        }

        public Texture2D GetTexture2D(int index)
        {
            return textures2d[index];
        }

        public Texture2D GetTexture2D(string name)
        {
            return textures2d[nameToTexture2d[name]];
        }

        public int CreateTexture3D(string name, Texture3DDescription description)
        {
            int index = texture3DDescriptions.Count;
            texture3DDescriptions.Add(description);
            nameToTexture3d.Add(name, index);
            return index;
        }

        public int GetIndexOfTexture3D(string name)
        {
            return nameToTexture3d[name];
        }

        public Texture3D GetTexture3D(int index)
        {
            return textures3d[index];
        }

        public ISamplerState CreateSamplerState(string name, SamplerStateDescription description)
        {
            if (nameToSamplerState.TryGetValue(name, out var samplerStateIndex))
            {
                return samplerStates[samplerStateIndex];
            }
            int index = samplerStateDescriptions.Count;
            var samplerState = device.CreateSamplerState(description);
            samplerStateDescriptions.Add(description);
            nameToSamplerState.Add(name, index);
            samplerStates.Add(samplerState);
            return samplerState;
        }

        public int GetIndexOfSamplerState(string name)
        {
            return nameToSamplerState[name];
        }

        public ISamplerState GetSamplerState(int index)
        {
            return samplerStates[index];
        }

        public ISamplerState GetSamplerState(string name)
        {
            return samplerStates[nameToSamplerState[name]];
        }
    }
}