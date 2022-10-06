namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

    /// <summary>
    /// <see cref="Transform"/> is used for hierachical matrix calculation.
    /// </summary>
    public class Transform
    {
        protected Transform? initial;
        protected Transform? parent;
        protected Vector3 position;
        protected Vector3 rotation;
        protected Vector3 scale = Vector3.One;
        protected Quaternion orientation;
        protected Vector3 globalPosition;
        protected Quaternion globalOrientation;
        protected Vector3 globalScale;
        protected Vector3 forward;
        protected Vector3 backward;
        protected Vector3 left;
        protected Vector3 right;
        protected Vector3 up;
        protected Vector3 down;
        protected Matrix4x4 global;
        protected Matrix4x4 globalInverse;
        protected Matrix4x4 local;
        protected Matrix4x4 localInverse;
        protected Matrix4x4 view;
        protected Matrix4x4 viewInv;
        protected Vector3 velocity;
        protected Vector3 oldpos;

        public Transform()
        {
            Recalculate();
        }

        public Transform? Parent
        {
            get => parent;
            set
            {
                parent = value;
                if (parent != null)
                    parent.Updated += ParentUpdated;
                Recalculate();
            }
        }

        private void ParentUpdated(object? sender, EventArgs e)
        {
            Recalculate();
        }

        /// <summary>
        /// Gets or sets the local position.
        /// </summary>
        public Vector3 Position
        {
            get => position;
            set
            {
                oldpos = position;
                position = value;
                velocity = position - oldpos;
                Recalculate();
            }
        }

        /// <summary>
        /// Gets or sets the local rotation.
        /// </summary>
        /// <remarks>The rotation is in space euler from 0° to 360°(359°)</remarks>
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

        /// <summary>
        /// Gets or sets the local scale.
        /// </summary>
        public Vector3 Scale
        {
            get => scale;
            set
            {
                scale = value;
                Recalculate();
            }
        }

        /// <summary>
        /// Gets or sets the local orienataion.
        /// </summary>

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

        /// <summary>
        /// Gets or sets the global (world space) position.
        /// </summary>

        public Vector3 GlobalPosition
        {
            get => globalPosition;
            set
            {
                if (parent == null)
                    position = value;
                else
                    // Transform because the rotation could modify the position of the child.
                    position = Vector3.Transform(value, parent.globalInverse);

                Recalculate();
            }
        }

        /// <summary>
        /// Gets or sets the global (world space) orientation.
        /// </summary>

        public Quaternion GlobalOrientation
        {
            get => globalOrientation;
            set
            {
                if (parent == null)
                    orientation = value;
                else
                    // Divide because quaternions are like matrices.
                    orientation = value / parent.globalOrientation;

                Recalculate();
            }
        }

        /// <summary>
        /// Gets or sets the global (world space) scale.
        /// </summary>

        public Vector3 GlobalScale
        {
            get => globalScale;
            set
            {
                if (parent == null)
                    scale = value;
                else
                    // divide because scale is a factor.
                    scale = value / parent.globalScale;

                Recalculate();
            }
        }

        /// <summary>
        /// Gets or sets the position and orientation.
        /// </summary>

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

        /// <summary>
        /// Gets or sets the local the position orientation and scale.
        /// </summary>

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

        /// <summary>
        /// The forward vector in global orientation space
        /// </summary>

        public Vector3 Forward => forward;

        /// <summary>
        /// The backward vector in global orientation space
        /// </summary>

        public Vector3 Backward => backward;

        /// <summary>
        /// The left vector in global orientation space
        /// </summary>

        public Vector3 Left => left;

        /// <summary>
        /// The right vector in global orientation space
        /// </summary>

        public Vector3 Right => right;

        /// <summary>
        /// The up vector in global orientation space
        /// </summary>

        public Vector3 Up => up;

        /// <summary>
        /// The down vector in global orientation space
        /// </summary>

        public Vector3 Down => down;

        /// <summary>
        /// The global transformation matrix
        /// </summary>

        public Matrix4x4 Global { get => global; }

        /// <summary>
        /// The inverse global transformation matrix
        /// </summary>

        public Matrix4x4 GlobalInverse => globalInverse;

        /// <summary>
        /// The local transformation matrix
        /// </summary>

        public Matrix4x4 Local { get => local; set => SetMatrix(value); }

        /// <summary>
        /// The local inverse transformation matrix
        /// </summary>

        public Matrix4x4 LocalInverse => localInverse;

        /// <summary>
        /// The view matrix in world space
        /// </summary>

        public Matrix4x4 View => view;

        /// <summary>
        /// The inverse view matrix in world space
        /// </summary>

        public Matrix4x4 ViewInv => viewInv;

        /// <summary>
        /// The velocity of the object only useful for 3d sound.
        /// </summary>

        public Vector3 Velocity => velocity;

        /// <summary>
        /// Notifies that the transform has changed. Used internally for parent child updates.
        /// </summary>
        public event EventHandler? Updated;

        /// <summary>
        /// Invokes the <see cref="Updated"/> event.
        /// </summary>
        protected void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Recalculates all values of the <see cref="Transform"/>.
        /// </summary>
        protected virtual void Recalculate()
        {
            local = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(position);
            Matrix4x4.Invert(local, out localInverse);
            if (parent == null)
                global = local;
            else
                global = local * parent.global;
            Matrix4x4.Invert(global, out globalInverse);

            Matrix4x4.Decompose(global, out globalScale, out globalOrientation, out globalPosition);
            forward = Vector3.Transform(Vector3.UnitZ, globalOrientation);
            backward = Vector3.Transform(-Vector3.UnitZ, globalOrientation);
            right = Vector3.Transform(Vector3.UnitX, globalOrientation);
            left = Vector3.Transform(-Vector3.UnitX, globalOrientation);
            up = Vector3.Transform(Vector3.UnitY, globalOrientation);
            down = Vector3.Transform(-Vector3.UnitY, globalOrientation);

            view = MathUtil.LookAtLH(globalPosition, forward + globalPosition, up);
            Matrix4x4.Invert(view, out viewInv);
            OnUpdated();
        }

        /// <summary>
        /// Reverse calculates the local params from a local transform matrix.
        /// </summary>
        /// <param name="matrix">Local space transform matrix</param>
        private void SetMatrix(Matrix4x4 matrix)
        {
            local = matrix;
            Matrix4x4.Invert(local, out localInverse);
            Matrix4x4.Decompose(local, out scale, out orientation, out position);
            rotation = orientation.GetRotation().ToDeg();
            if (parent == null)
                global = local;
            else
                global = local * parent.global;
            Matrix4x4.Invert(global, out globalInverse);
            Matrix4x4.Decompose(global, out globalScale, out globalOrientation, out globalPosition);

            forward = Vector3.Transform(Vector3.UnitZ, globalOrientation);
            backward = Vector3.Transform(-Vector3.UnitZ, globalOrientation);
            right = Vector3.Transform(Vector3.UnitX, globalOrientation);
            left = Vector3.Transform(-Vector3.UnitX, globalOrientation);
            up = Vector3.Transform(Vector3.UnitY, globalOrientation);
            down = Vector3.Transform(-Vector3.UnitY, globalOrientation);
            view = MathUtil.LookAtLH(globalPosition, forward + globalPosition, up);
            Matrix4x4.Invert(view, out viewInv);

            OnUpdated();
        }

        /// <summary>
        /// Stores the current state. Used internally for the editor.
        /// </summary>
        public void StoreInitial()
        {
            initial = Clone();
        }

        /// <summary>
        /// Stores the current state. Used internally for the editor.
        /// </summary>
        public void RestoreInitial()
        {
            if (initial == null) return;
            backward = initial.backward;
            down = initial.down;
            forward = initial.forward;
            left = initial.left;
            global = initial.global;
            globalInverse = initial.globalInverse;
            oldpos = initial.oldpos;
            orientation = initial.orientation;
            parent = initial.parent;
            position = initial.position;
            right = initial.right;
            rotation = initial.rotation;
            scale = initial.scale;
            up = initial.up;
            velocity = initial.velocity;
            view = initial.view;
            viewInv = initial.viewInv;
        }

        /// <summary>
        /// Clones this <see cref="Transform"/> instance.
        /// </summary>
        /// <returns>Returns a new instance of <see cref="Transform"/> with the values of this <see cref="Transform"/></returns>
        public Transform Clone()
        {
            return new Transform() { backward = backward, down = down, forward = forward, initial = initial, left = left, global = global, globalInverse = globalInverse, oldpos = oldpos, orientation = orientation, parent = parent, position = position, right = right, rotation = rotation, scale = scale, up = up, velocity = velocity, view = view, viewInv = viewInv };
        }

        public static implicit operator Matrix4x4(Transform transform) => transform.global;
    }
}