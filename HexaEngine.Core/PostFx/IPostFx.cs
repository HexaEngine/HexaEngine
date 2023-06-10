namespace HexaEngine.Core.PostFx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;

    public interface IPostFx : IDisposable
    {
        public string Name { get; }

        public PostFxFlags Flags { get; }

        public bool Enabled { get; set; }

        public int Priority { get; set; }

        public event Action<bool>? OnEnabledChanged;

        public event Action<int>? OnPriorityChanged;

        Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros);

        void Resize(int width, int height);

        void Draw(IGraphicsContext context);

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport);

        public void SetInput(IShaderResourceView view, ITexture2D resource);

        public void Update(IGraphicsContext context);
    }
}