namespace HexaEngine.Cameras
{
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using MathUtil = Mathematics.MathUtil;

    public class SurfaceCamera : CameraBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        private SurfaceCamera()
        {
            UpdateView();
        }

        public static readonly SurfaceCamera Instance = new();
        private float width;
        private float height;

        public float Width { get => width; set => width = value; }

        public float Height { get => height; set => height = value; }

        /// <summary>
        /// Updates the projection.
        /// </summary>
        public void UpdateProjection()
        {
            Projection = MathUtil.OrthoLH(Width, Height, viewport.MinDepth, viewport.MaxDepth);
        }

        /// <summary>
        /// Updates the view.
        /// </summary>
        public void UpdateView()
        {
            UpdateProjection();
            View = MathUtil.LookAtLH(-Vector3.UnitZ, Vector3.UnitZ + -Vector3.UnitZ, Vector3.UnitY);
            Frustum = new(Projection * View);
        }

        protected override void OnPropertyChanged([CallerMemberName] string name = "")
        {
            UpdateView();
            base.OnPropertyChanged(name);
        }
    }
}