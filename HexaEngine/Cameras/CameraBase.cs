namespace HexaEngine.Cameras
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System.ComponentModel;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class CameraBase : IView, INotifyPropertyChanged
    {
        protected Vector3 position;
        protected Viewport viewport = new(1, 1);

        public Matrix4x4 View { get; protected set; }

        public Matrix4x4 Projection { get; protected set; }

        public BoundingFrustum Frustum { get; protected set; }

        public Vector3 Position
        { get => position; set { position = value; OnPropertyChanged(); } }

        public Viewport Viewport => viewport;

        public Transform Transform { get; }
        CameraTransform IView.Transform { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new(name));
        }
    }
}