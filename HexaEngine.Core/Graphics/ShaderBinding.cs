namespace HexaEngine.Core.Graphics
{
    public struct ShaderBinding
    {
        public ShaderBinding(ShaderStage stage, int slot)
        {
            Stage = stage;
            Slot = slot;
        }

        public ShaderStage Stage;
        public int Slot;
    }

    public struct BoundResource
    {
        public IShaderResourceView Resource;
        public List<ShaderBinding> Bindings;

        public BoundResource(IShaderResourceView resource, params ShaderBinding[] bindings)
        {
            Resource = resource;
            Bindings = new(bindings);
        }

        public BoundResource(IShaderResourceView resource, ShaderStage stage, int slot)
        {
            Resource = resource;
            Bindings = new List<ShaderBinding>();
            Bindings.Add(new ShaderBinding(stage, slot));
        }

        public void Bind(IGraphicsContext context)
        {
            for (int i = 0; i < Bindings.Count; i++)
            {
                context.SetShaderResource(Resource, Bindings[i].Stage, Bindings[i].Slot);
            }
        }
    }

    public struct BoundSampler
    {
        public ISamplerState Sampler;
        public List<ShaderBinding> Bindings;

        public BoundSampler(ISamplerState sampler, params ShaderBinding[] bindings)
        {
            Sampler = sampler;
            Bindings = new(bindings);
        }

        public BoundSampler(ISamplerState sampler, ShaderStage stage, int slot)
        {
            Sampler = sampler;
            Bindings = new List<ShaderBinding>();
            Bindings.Add(new ShaderBinding(stage, slot));
        }

        public void Bind(IGraphicsContext context)
        {
            for (int i = 0; i < Bindings.Count; i++)
            {
                context.SetSampler(Sampler, Bindings[i].Stage, Bindings[i].Slot);
            }
        }
    }

    public struct BoundConstant
    {
        public IBuffer Constant;
        public List<ShaderBinding> Bindings;

        public BoundConstant(IBuffer constant, params ShaderBinding[] bindings)
        {
            Constant = constant;
            Bindings = new(bindings);
        }

        public BoundConstant(IBuffer constant, ShaderStage stage, int slot)
        {
            Constant = constant;
            Bindings = new List<ShaderBinding>();
            Bindings.Add(new ShaderBinding(stage, slot));
        }

        public void Bind(IGraphicsContext context)
        {
            for (int i = 0; i < Bindings.Count; i++)
            {
                context.SetConstantBuffer(Constant, Bindings[i].Stage, Bindings[i].Slot);
            }
        }
    }
}