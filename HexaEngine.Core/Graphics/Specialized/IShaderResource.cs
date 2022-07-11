namespace HexaEngine.Core.Graphics.Specialized
{
    using HexaEngine.Core.Graphics;
    using System;

    public interface IShaderResource : IDisposable
    {
        void AddBinding(ShaderBinding binding);

        void AddBinding(ShaderStage stage, int slot);

        void Bind(IGraphicsContext context);

        void Bind(IGraphicsContext context, int slot);

        void Bind(IGraphicsContext context, int slot, ShaderStage stage);

        void ClearBindings();

        void RemoveBinding(ShaderBinding binding);

        void RemoveBinding(ShaderStage stage, int slot);
    }
}