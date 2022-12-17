namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Graphics;

    public interface IPostProcessManager
    {
        public void AddEffect(Effect effect);

        public void RemoveEffect(Effect effect);

        public void Draw();
    }
}