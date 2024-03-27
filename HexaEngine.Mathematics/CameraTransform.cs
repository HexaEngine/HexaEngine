namespace HexaEngine.Mathematics
{
    using System.Numerics;

    /// <summary>
    /// Represents a camera transform with projection properties.
    /// </summary>
    public class CameraTransform : Transform
    {
        /// <summary>
        /// The projection matrix of the camera.
        /// </summary>
        protected Matrix4x4 projection;

        /// <summary>
        /// The inverse projection matrix of the camera.
        /// </summary>
        protected Matrix4x4 projectionInv;

        /// <summary>
        /// The view projection matrix of the camera.
        /// </summary>
        protected Matrix4x4 viewProjection;

        /// <summary>
        /// The inverse view projection matrix of the camera.
        /// </summary>
        protected Matrix4x4 viewProjectionInv;

        /// <summary>
        /// The previous view projection matrix of the camera.
        /// </summary>
        protected Matrix4x4 prevViewProjection;

        /// <summary>
        /// The projection type of the camera.
        /// </summary>
        protected ProjectionType projectionType;

        /// <summary>
        /// The width of the camera.
        /// </summary>
        protected float width = 16;

        /// <summary>
        /// The height of the camera.
        /// </summary>
        protected float height = 9;

        /// <summary>
        /// The aspect ratio of the camera.
        /// </summary>
        protected float aspectRatio;

        /// <summary>
        /// The fov of the camera.
        /// </summary>
        protected float fov = 90f.ToRad();

        /// <summary>
        /// The near plane distance of the camera.
        /// </summary>
        protected float near = 0.01f;

        /// <summary>
        /// The far plane distance of the camera.
        /// </summary>
        protected float far = 100f;

        /// <summary>
        /// The frustum of the camera.
        /// </summary>
        protected BoundingFrustum frustum = new();

        /// <summary>
        /// The frustum of the camera (normalized).
        /// </summary>
        protected BoundingFrustum normalizedFrustum = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraTransform"/> class.
        /// </summary>
        public CameraTransform()
        {
            Recalculate();
        }

        /// <summary>
        /// Gets the projection matrix of the camera.
        /// </summary>
        public Matrix4x4 Projection => projection;

        /// <summary>
        /// Gets the inverse of the projection matrix of the camera.
        /// </summary>
        public Matrix4x4 ProjectionInv => projectionInv;

        /// <summary>
        /// Gets the view projection matrix of the camera.
        /// </summary>
        public Matrix4x4 ViewProjection => viewProjection;

        /// <summary>
        /// Gets the inverse view projection matrix of the camera.
        /// </summary>
        public Matrix4x4 ViewProjectionInv => viewProjectionInv;

        /// <summary>
        /// Gets the previous view projection matrix of the camera.
        /// </summary>
        public Matrix4x4 PrevViewProjection => prevViewProjection;

        /// <summary>
        /// Gets or sets the type of the projection of camera.
        /// </summary>
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
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
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
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
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
                OnChanged();
            }
        }

        /// <summary>
        /// Gets the camera's aspect ratio.
        /// </summary>
        public float AspectRatio => aspectRatio;

        /// <summary>
        /// Gets or sets the fov of the camera.
        /// </summary>
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
                if (projectionType == ProjectionType.Orthographic)
                {
                    return;
                }

                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the near plane distance.
        /// </summary>
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
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the far plane distance.
        /// </summary>
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
                OnChanged();
            }
        }

        /// <summary>
        /// Gets the range of the plane in the view frustum.
        /// </summary>
        /// <remarks>
        /// The plane range is calculated as the difference between the far and near clipping planes.
        /// </remarks>
        /// <value>The range of the plane in the view frustum.</value>
        public float ClipRange => Far - Near;

        /// <summary>
        /// Gets the frustum of the camera's view.
        /// </summary>
        public BoundingFrustum Frustum => frustum;

        /// <summary>
        /// Gets the normalized frustum of the camera's view.
        /// </summary>
        public BoundingFrustum NormalizedFrustum => normalizedFrustum;

        /// <inheritdoc/>
        public override bool Recalculate()
        {
            base.Recalculate();
            aspectRatio = width / height;
            switch (projectionType)
            {
                case ProjectionType.Orthographic:
                    projection = MathUtil.OrthoLH(width, height, near, far);
                    break;

                case ProjectionType.Perspective:
                    projection = MathUtil.PerspectiveFovLH(fov, aspectRatio, near, far);
                    break;
            }
            Matrix4x4.Invert(projection, out projectionInv);

            OnUpdated();
            IsDirty = true;
            return true;
        }

        /// <inheritdoc/>
        protected override void OnUpdated()
        {
            prevViewProjection = viewProjection;
            viewProjection = view * projection;
            frustum.Update(viewProjection);
            Matrix4x4.Invert(viewProjection, out viewProjectionInv);

            const float zNear = 0.001f;
            const float zFar = 0.5f;
            float q = zFar / (zFar - zNear);
            Matrix4x4 nProjection = projection;
            nProjection.M33 = q;
            nProjection.M43 = -q * zNear;
            normalizedFrustum.Update(view * nProjection);

            base.OnUpdated();
        }

        /// <inheritdoc/>
        public override void CopyTo(Transform other)
        {
            base.CopyTo(other);
            if (other is CameraTransform camera)
            {
                camera.width = width;
                camera.height = height;
                camera.prevViewProjection = prevViewProjection;
                camera.viewProjection = viewProjection;
                camera.viewProjectionInv = viewProjectionInv;
                camera.frustum = frustum;
                camera.projection = projection;
                camera.projectionInv = projectionInv;
                camera.projectionType = projectionType;
                camera.aspectRatio = aspectRatio;
                camera.near = near;
                camera.far = far;
                camera.fov = fov;
            }
        }
    }
}