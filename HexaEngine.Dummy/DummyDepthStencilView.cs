namespace HexaEngine.Dummy
{
    using HexaEngine.Core.Graphics;

    public class DummyDepthStencilView : DummyObject, IDepthStencilView
    {
        public DummyDepthStencilView(IResource resource, DepthStencilViewDescription description)
        {
            Resource = resource;
            Description = description;
        }

        public IResource Resource { get; }

        public DepthStencilViewDescription Description { get; }
    }
}