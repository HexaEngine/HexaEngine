namespace HexaEngine.UI.Animation
{
    public abstract class Animatable : DependencyObject, IAnimatable
    {
        private Type? type;

        public override Type DependencyObjectType => type ??= GetType();
    }
}