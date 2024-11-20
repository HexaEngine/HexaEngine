namespace HexaEngine.D3D12
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;

    public unsafe class D3D12GraphicsPipeline
    {
        internal Shader* vertexShaderBlob;
        internal Shader* hullShaderBlob;
        internal Shader* domainShaderBlob;
        internal Shader* geometryShaderBlob;
        internal Shader* pixelShaderBlob;
        internal InputLayoutDesc inputLayoutDesc;

        internal Blob? signature;

        internal ComPtr<ID3D12RootSignature> rootSignature;
    }
}