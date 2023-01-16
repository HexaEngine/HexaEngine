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

    public class PostFxManager
    {
        private IGraphicsDevice device;
        private IGraphicsContext deferredContext;
        private List<IEffect> effects;
        private List<FxDependency> dependencies = new();

        private List<IEffect> sorted = new();
        private bool dirty;

        public PostFxManager(IGraphicsDevice device)
        {
            this.device = device;
            deferredContext = device.CreateDeferredContext();
        }

        public void AddEffect(IEffect effect)
        {
            effects.Add(effect);
        }

        public void RemoveEffect(IEffect effect)
        {
            effects.Remove(effect);
            dependencies.RemoveAll(x => x.Effect == effect);
        }

        public void Clear()
        {
            effects.Clear();
            dependencies.Clear();
        }

        public void AddDependency(FxDependency dependency)
        {
        }
    }
}