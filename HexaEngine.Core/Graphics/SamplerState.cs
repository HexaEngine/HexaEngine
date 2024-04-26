namespace HexaEngine.Core.Graphics
{
    public class SamplerState : DisposableBase, ISamplerState
    {
        private readonly ISamplerState samplerState;

        public SamplerState(SamplerStateDescription description)
        {
            samplerState = Application.GraphicsDevice.CreateSamplerState(description);
        }

        public SamplerStateDescription Description => samplerState.Description;

        public string? DebugName { get => samplerState.DebugName; set => samplerState.DebugName = value; }

        public nint NativePointer => samplerState.NativePointer;

        protected override void DisposeCore()
        {
            samplerState.Dispose();
        }
    }
}