namespace HexaEngine.Core.Graphics
{
    public interface IResourceBindingList : IDisposable
    {
        IPipeline Pipeline { get; }

        IEnumerable<BindingValuePair> SRVs { get; }

        IEnumerable<BindingValuePair> CBVs { get; }

        IEnumerable<BindingValuePair> UAVs { get; }

        IEnumerable<BindingValuePair> Samplers { get; }

        void SetCBV(string name, IBuffer? cbv);

        void SetCBV(string name, ShaderStage stage, IBuffer? cbv);

        void SetSampler(string name, ISamplerState? sampler);

        void SetSampler(string name, ShaderStage stage, ISamplerState? sampler);

        void SetSRV(string name, IShaderResourceView? srv);

        void SetSRV(string name, ShaderStage stage, IShaderResourceView? srv);

        void SetUAV(string name, IUnorderedAccessView? uav, uint initialCount = unchecked((uint)-1));

        void SetUAV(string name, ShaderStage stage, IUnorderedAccessView? uav, uint initialCount = unchecked((uint)-1));
    }

    public enum ShaderParameterType
    {
        SRV,
        UAV,
        CBV,
        Sampler,
    }

    public unsafe struct BindingValuePair
    {
        public string Name;
        public ShaderStage Stage;
        public ShaderParameterType Type;
        public void* Value;
    }
}