namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

    public class Transform
    {
        protected Vector3 position;
        protected Vector3 rotation;
        protected Vector3 scale = Vector3.One;
        protected Quaternion orientation;
        protected Vector3 forward;
        protected Vector3 backward;
        protected Vector3 left;
        protected Vector3 right;
        protected Vector3 up;
        protected Vector3 down;
        protected Matrix4x4 matrix;
        protected Matrix4x4 matrixInv;
        protected Matrix4x4 view;
        protected Matrix4x4 viewInv;
        protected Vector3 velocity;
        protected Vector3 oldpos;

        public Transform()
        {
            Recalculate();
        }

        public Vector3 Position
        { get => position; set { oldpos = position; position = value; velocity = position - oldpos; Recalculate(); } }

        public Vector3 Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                orientation = value.NormalizeEulerAngleDegrees().ToRad().GetQuaternion();
                Recalculate();
            }
        }

        public Vector3 Scale
        { get => scale; set { scale = value; Recalculate(); } }

        public Quaternion Orientation
        {
            get => orientation;
            set
            {
                orientation = value;
                rotation = value.GetRotation().ToDeg();
                Recalculate();
            }
        }

        public (Vector3, Quaternion) PositionRotation
        {
            get => (position, orientation);
            set
            {
                oldpos = position;
                (position, orientation) = value;
                velocity = position - oldpos;
                Recalculate();
            }
        }

        public (Vector3, Quaternion, Vector3) PositionRotationScale
        {
            get => (position, orientation, scale);
            set
            {
                oldpos = position;
                (position, orientation, scale) = value;
                velocity = position - oldpos;
                Recalculate();
            }
        }

        public Vector3 Forward => forward;

        public Vector3 Backward => backward;

        public Vector3 Left => left;

        public Vector3 Right => right;

        public Vector3 Up => up;

        public Vector3 Down => down;

        public Matrix4x4 Matrix { get => matrix; set => SetMatrix(value); }

        public Matrix4x4 MatrixInv => matrixInv;

        public Matrix4x4 View => view;

        public Matrix4x4 ViewInv => viewInv;

        public Vector3 Velocity => velocity;

        public event EventHandler? Updated;

        protected void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Recalculate()
        {
            forward = Vector3.Transform(Vector3.UnitZ, orientation);
            backward = Vector3.Transform(-Vector3.UnitZ, orientation);
            right = Vector3.Transform(Vector3.UnitX, orientation);
            left = Vector3.Transform(-Vector3.UnitX, orientation);
            up = Vector3.Transform(Vector3.UnitY, orientation);
            down = Vector3.Transform(-Vector3.UnitY, orientation);
            matrix = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(position);
            view = Extensions.LookAtLH(position, forward + position, up);
            Matrix4x4.Invert(matrix, out matrixInv);
            Matrix4x4.Invert(view, out viewInv);
            OnUpdated();
        }

        private void SetMatrix(Matrix4x4 matrix)
        {
            this.matrix = matrix;
            Matrix4x4.Decompose(matrix, out scale, out orientation, out position);
            rotation = orientation.GetRotation().ToDeg();
            forward = Vector3.Transform(Vector3.UnitZ, orientation);
            backward = Vector3.Transform(-Vector3.UnitZ, orientation);
            right = Vector3.Transform(Vector3.UnitX, orientation);
            left = Vector3.Transform(-Vector3.UnitX, orientation);
            up = Vector3.Transform(Vector3.UnitY, orientation);
            down = Vector3.Transform(-Vector3.UnitY, orientation);
            view = Extensions.LookAtLH(position, forward + position, up);
            OnUpdated();
        }

        public static implicit operator Matrix4x4(Transform transform) => transform.matrix;
    }
}