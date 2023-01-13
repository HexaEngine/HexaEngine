namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Graphics;

    public interface IPostProcessManager
    {
        public void AddEffect(IEffect effect);

        public void RemoveEffect(IEffect effect);

        public void Draw();
    }
}