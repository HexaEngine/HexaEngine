namespace HexaEngine.Core.Fx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;

    public interface IPostFx : IDisposable
    {
        public string Name { get; }

        public PostFxFlags Flags { get; }

        public bool Enabled { get; set; }

        public int Priority { get; set; }

        Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros);

        void Resize(int width, int height);

        void Draw(IGraphicsContext context);

        public void SetOutput(IRenderTargetView view, Viewport viewport);

        public void SetInput(IShaderResourceView view);

        public void Update(IGraphicsContext context);
    }
}