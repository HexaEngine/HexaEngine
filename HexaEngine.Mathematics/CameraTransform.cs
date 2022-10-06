namespace HexaEngine.Mathematics
{
    using System.Numerics;

    public class CameraTransform : Transform
    {
        protected Matrix4x4 projection;
        protected Matrix4x4 projectionInv;
        protected ProjectionType projectionType;
        protected float width = 16;
        protected float height = 9;
        protected float aspectRatio;
        protected float fov = 90;
        protected float near = 0.01f;
        protected float far = 100f;
        protected BoundingFrustum frustum;

        public CameraTransform()
        {
            Recalculate();
        }

        public Matrix4x4 Projection { get => projection; }

        public Matrix4x4 ProjectionInv => projectionInv;

        public ProjectionType ProjectionType
        {
            get => projectionType;
            set
            {
                if (value == projectionType) return;
                projectionType = value;
                Recalculate();
            }
        }

        public float Width
        {
            get => width;
            set
            {
                if (value == width) return;
                width = value;
                Recalculate();
            }
        }

        public float Height
        {
            get => height;
            set
            {
                if (value == height) return;
                height = value;
                Recalculate();
            }
        }

        public float AspectRatio => aspectRatio;

        public float Fov
        {
            get => fov;
            set
            {
                if (value == fov) return;
                fov = value;
                if (projectionType == ProjectionType.Othro) return;
                Recalculate();
            }
        }

        public float Near
        {
            get => near;
            set
            {
                if (value == near) return;
                near = value;
                Recalculate();
            }
        }

        public float Far
        {
            get => far;
            set
            {
                if (value == far) return;
                far = value;
                Recalculate();
            }
        }

        public BoundingFrustum Frustum => frustum;

        protected override void Recalculate()
        {
            base.Recalculate();
            aspectRatio = width / height;
            switch (projectionType)
            {
                case ProjectionType.Othro:
                    projection = MathUtil.OrthoLH(width, height, near, far);
                    break;

                case ProjectionType.Perspective:
                    projection = MathUtil.PerspectiveFovLH(fov, aspectRatio, near, far);
                    break;
            }
            Matrix4x4.Invert(projection, out projectionInv);
            frustum = new(view * projection);
            OnUpdated();
        }
    }
}