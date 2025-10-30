namespace HexaEngine.UI.Animation
{
    public class Storyboard : Animatable
    {
        private readonly List<IAnimatable> animations = new();

        public bool IsComplete { get; internal set; }

        public override Type DependencyObjectType { get; } = typeof(Storyboard);

        internal void Update(float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}