namespace HexaEngine.Mathematics
{
    using System.Numerics;

    public class CameraTransform : Transform
    {
        protected Matrix4x4 projection;
        protected Matrix4x4 projectionInv;
        protected Matrix4x4 viewProjection;
        protected Matrix4x4 viewProjectionInv;
        protected Matrix4x4 prevViewProjection;
        protected ProjectionType projectionType;
        protected float width = 16;
        protected float height = 9;
        protected float aspectRatio;
        protected float fov = 90;
        protected float near = 0.01f;
        protected float far = 100f;
        protected BoundingFrustum frustum = new();

        public CameraTransform()
        {
            Recalculate();
        }

        public Matrix4x4 Projection => projection;

        public Matrix4x4 ProjectionInv => projectionInv;

        public Matrix4x4 ViewProjection => viewProjection;

        public Matrix4x4 ViewProjectionInv => viewProjectionInv;

        public Matrix4x4 PrevViewProjection => prevViewProjection;

        public ProjectionType ProjectionType
        {
            get => projectionType;
            set
            {
                if (value == projectionType)
                {
                    return;
                }

                projectionType = value;
                dirty = true;
            }
        }

        public float Width
        {
            get => width;
            set
            {
                if (value == width)
                {
                    return;
                }

                width = value;
                dirty = true;
            }
        }

        public float Height
        {
            get => height;
            set
            {
                if (value == height)
                {
                    return;
                }

                height = value;
                dirty = true;
            }
        }

        public float AspectRatio => aspectRatio;

        public float Fov
        {
            get => fov;
            set
            {
                if (value == fov)
                {
                    return;
                }

                fov = value;
                if (projectionType == ProjectionType.Othro)
                {
                    return;
                }

                dirty = true;
            }
        }

        public float Near
        {
            get => near;
            set
            {
                if (value == near)
                {
                    return;
                }

                near = value;
                dirty = true;
            }
        }

        public float Far
        {
            get => far;
            set
            {
                if (value == far)
                {
                    return;
                }

                far = value;
                dirty = true;
            }
        }

        public BoundingFrustum Frustum => frustum;

        public override bool Recalculate()
        {
            base.Recalculate();
            aspectRatio = width / height;
            switch (projectionType)
            {
                case ProjectionType.Othro:
                    projection = MathUtil.OrthoLH(width, height, near, far);
                    break;

                case ProjectionType.Perspective:
                    projection = MathUtil.PerspectiveFovLH(fov.ToRad(), aspectRatio, near, far);
                    break;
            }
            Matrix4x4.Invert(projection, out projectionInv);

            OnUpdated();
            dirty = true;
            return true;
        }

        protected override void OnUpdated()
        {
            prevViewProjection = viewProjection;
            viewProjection = view * projection;
            frustum.Initialize(viewProjection);
            Matrix4x4.Invert(viewProjection, out viewProjectionInv);
            base.OnUpdated();
        }
    }
}