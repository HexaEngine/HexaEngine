namespace HexaEngine.UI
{
    public interface ICompositionTarget : IDependencyElement
    {
        public void Invalidate()
        {
            if (Parent is ICompositionTarget target)
            {
                target.Invalidate();
            }
        }

        protected void OnComposeTarget();
    }
}