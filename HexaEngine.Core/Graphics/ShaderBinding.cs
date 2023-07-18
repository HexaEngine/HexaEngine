namespace HexaEngine.Core.Graphics
{
    public struct ShaderBinding
    {
        public ShaderBinding(ShaderStage stage, uint slot)
        {
            Stage = stage;
            Slot = slot;
        }

        public ShaderStage Stage;
        public uint Slot;
    }

    public struct BoundResource
    {
        public IShaderResourceView? Resource;
        public List<ShaderBinding> Bindings;

        public BoundResource(IShaderResourceView? resource, params ShaderBinding[] bindings)
        {
            Resource = resource;
            Bindings = new(bindings);
        }

        public BoundResource(IShaderResourceView? resource, ShaderStage stage, uint slot)
        {
            Resource = resource;
            Bindings = new List<ShaderBinding>();
            Bindings.Add(new ShaderBinding(stage, slot));
        }

        public void Bind(IGraphicsContext context)
        {
            for (int i = 0; i < Bindings.Count; i++)
            {
                var binding = Bindings[i];
                switch (binding.Stage)
                {
                    case ShaderStage.Vertex:
                        context.VSSetShaderResource(Bindings[i].Slot, Resource);
                        break;

                    case ShaderStage.Hull:
                        context.HSSetShaderResource(Bindings[i].Slot, Resource);
                        break;

                    case ShaderStage.Domain:
                        context.DSSetShaderResource(Bindings[i].Slot, Resource);
                        break;

                    case ShaderStage.Geometry:
                        context.GSSetShaderResource(Bindings[i].Slot, Resource);
                        break;

                    case ShaderStage.Pixel:
                        context.PSSetShaderResource(Bindings[i].Slot, Resource);
                        break;

                    case ShaderStage.Compute:
                        context.CSSetShaderResource(Bindings[i].Slot, Resource);
                        break;
                }
            }
        }
    }

    public struct BoundSampler
    {
        public ISamplerState? Sampler;
        public List<ShaderBinding> Bindings;

        public BoundSampler(ISamplerState? sampler, params ShaderBinding[] bindings)
        {
            Sampler = sampler;
            Bindings = new(bindings);
        }

        public BoundSampler(ISamplerState? sampler, ShaderStage stage, uint slot)
        {
            Sampler = sampler;
            Bindings = new List<ShaderBinding>();
            Bindings.Add(new ShaderBinding(stage, slot));
        }

        public void Bind(IGraphicsContext context)
        {
            for (int i = 0; i < Bindings.Count; i++)
            {
                var binding = Bindings[i];
                switch (binding.Stage)
                {
                    case ShaderStage.Vertex:
                        context.VSSetSampler(Bindings[i].Slot, Sampler);
                        break;

                    case ShaderStage.Hull:
                        context.HSSetSampler(Bindings[i].Slot, Sampler);
                        break;

                    case ShaderStage.Domain:
                        context.DSSetSampler(Bindings[i].Slot, Sampler);
                        break;

                    case ShaderStage.Geometry:
                        context.GSSetSampler(Bindings[i].Slot, Sampler);
                        break;

                    case ShaderStage.Pixel:
                        context.PSSetSampler(Bindings[i].Slot, Sampler);
                        break;

                    case ShaderStage.Compute:
                        context.CSSetSampler(Bindings[i].Slot, Sampler);
                        break;
                }
            }
        }
    }

    public struct BoundConstant
    {
        public IBuffer? Constant;
        public List<ShaderBinding> Bindings;

        public BoundConstant(IBuffer? constant, params ShaderBinding[] bindings)
        {
            Constant = constant;
            Bindings = new(bindings);
        }

        public BoundConstant(IBuffer? constant, ShaderStage stage, uint slot)
        {
            Constant = constant;
            Bindings = new List<ShaderBinding>();
            Bindings.Add(new ShaderBinding(stage, slot));
        }

        public void Bind(IGraphicsContext context)
        {
            for (int i = 0; i < Bindings.Count; i++)
            {
                var binding = Bindings[i];
                switch (binding.Stage)
                {
                    case ShaderStage.Vertex:
                        context.VSSetConstantBuffer(Bindings[i].Slot, Constant);
                        break;

                    case ShaderStage.Hull:
                        context.HSSetConstantBuffer(Bindings[i].Slot, Constant);
                        break;

                    case ShaderStage.Domain:
                        context.DSSetConstantBuffer(Bindings[i].Slot, Constant);
                        break;

                    case ShaderStage.Geometry:
                        context.GSSetConstantBuffer(Bindings[i].Slot, Constant);
                        break;

                    case ShaderStage.Pixel:
                        context.PSSetConstantBuffer(Bindings[i].Slot, Constant);
                        break;

                    case ShaderStage.Compute:
                        context.CSSetConstantBuffer(Bindings[i].Slot, Constant);
                        break;
                }
            }
        }
    }
}