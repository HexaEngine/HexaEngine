namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;

    public interface IView
    {
        public CameraTransform Transform { get; }
    }
}