namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;

    public struct FxDependency
    {
        public IEffect Effect;
        public List<IEffect> Dependencies;

        public FxDependency(IEffect effect)
        {
            Effect = effect;
            Dependencies = new List<IEffect>();
        }
    }

    public interface IEffect2 : IDisposable
    {
        IRenderTargetView Output { get; }

        string Name { get; }

        Task Initialize(IGraphicsDevice device, PostFxManager manager, int width, int height);

        void Draw(IGraphicsContext context);

        Task BeginResize();

        Task EndResize(int width, int height);
    }

    public class PostFxManager
    {
        private IGraphicsDevice device;
        private IGraphicsContext deferredContext;
        private List<IEffect2> effects = new();
        private Dictionary<IEffect2, ICommandList> cmdlists = new();
        private List<FxDependency> dependencies = new();

        private List<IEffect2> sorted = new();
        private bool dirty;

        public PostFxManager(IGraphicsDevice device)
        {
            this.device = device;
            deferredContext = device.CreateDeferredContext();
        }

        public void AddEffect(IEffect2 effect)
        {
            effects.Add(effect);
        }

        public void RemoveEffect(IEffect2 effect)
        {
            effects.Remove(effect);
            cmdlists.Remove(effect);
            dependencies.RemoveAll(x => x.Effect == effect);
        }

        public void Clear()
        {
            effects.Clear();
            dependencies.Clear();
        }
    }
}